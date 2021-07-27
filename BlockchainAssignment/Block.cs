using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Block
    {
        public DateTime timestamp;
        public int index;
        public int nonce = 0;
        public int eNonce = -1;
        public String Hash = "";
        public String previousHash;
        public List<Transaction> transactionList = new List<Transaction>();
        public float difficulty;
        public int threadNumber = 2;
        public string minerAddress = String.Empty;
        public float reward = 255;
        public float cum_fees = 0;
        public String merkleRoot;
       
        private CancellationToken _cancellationToken;


        // Non Genesis Block Constructor
        public Block(Block lastBlock, List<Transaction> transList, string minerAddress, bool threaded, int diff, int threads=1)
        {
            this.timestamp = DateTime.Now;
            this.previousHash = lastBlock.Hash;
            this.index = lastBlock.index + 1;
            this.transactionList = transList;
            this.minerAddress = minerAddress;
            
            // Reward Logic
            transactionList.ForEach(t => cum_fees += t.fee);
            transactionList.Add(new Transaction("Mine Rewards", "", minerAddress, this.reward+cum_fees, 0));
            this.merkleRoot = MerkleRoot(transactionList);

            this.threadNumber = threads;
            this.difficulty = diff;

            var watch = new System.Diagnostics.Stopwatch();
            if (threaded)
            {
                watch.Start();
                ThreadedMine();
                watch.Stop();
            } else
            {
                watch.Start();
                Mine();
                watch.Stop();
            }
            Console.WriteLine($"Execution Time: {watch.ElapsedTicks} ticks");
        }

        // Genesis Block constructor
        public Block()
        {
            timestamp = DateTime.Now;
            previousHash = String.Empty;
            index = 0;
            this.merkleRoot = String.Empty;
            // CreateHash will default to eNonce = 1
            Hash = CreateHash();
            eNonce = 1;
        }

        public static String MerkleRoot(List<Transaction> transactionList)
        {
            List<String> hashes = transactionList.Select(t => t.Hash).ToList();
            if(hashes.Count == 0)
            {
                return String.Empty;
            }
            if(hashes.Count == 1)
            {
                return HashCode.HashTools.combineHash(hashes[0], hashes[0]);
            }
            while(hashes.Count != 1)
            {
                List<String> merkleLeaves = new List<string>();
                for (int i=0; i < hashes.Count; i += 2)
                {
                    if (i == hashes.Count - 1)
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i]));
                    }
                    else
                    {
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i + 1]));
                    }
                }
                hashes = merkleLeaves;
            }
            return hashes[0];
        }

        public void Mine()
        {
            String target_string = "";
            for (int i = 0; i<difficulty; i++)
            {
                target_string += "0";
            }

            while (!Hash.StartsWith(target_string))
            {
                this.nonce++;
                this.Hash = CreateHash();
            }
            this.eNonce = 1;
        }

        public void ThreadedMine()
        {
            // This is what's used to stop the threads if another thread has found a viable token
            var cancellationSource = new CancellationTokenSource();
            this._cancellationToken = cancellationSource.Token;

            // Defines a local variable for each of the threads, so it's threadSafe
            ThreadLocal<String> localHash = new ThreadLocal<String>(() =>
            {
                return "";
            });

            ThreadLocal<int> localNonce = new ThreadLocal<int>(() =>
            {
                return 0;
            });

            // Return values for the 'successful thread'
            object result = null;
            object threadNum = null;
            object threadNonce = null;

            int no_of_threads = this.threadNumber;

            String target_string = "";
            for (int i = 0; i < difficulty; i++)
            {
                target_string += "0";
            }

            Task[] ts = new Task[no_of_threads];
            for (int i = 0; i<no_of_threads; i++)
            {
                ts[i] = Task.Run(() =>
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        localNonce.Value++;
                        localHash.Value = CreateHash(Thread.CurrentThread.ManagedThreadId,localNonce.Value);
                        if (localHash.Value.StartsWith(target_string))
                        {
                            cancellationSource.Cancel();
                            result = localHash.Value;
                            threadNonce = localNonce.Value;
                            threadNum = Thread.CurrentThread.ManagedThreadId;
                        }
                    }
                });
            }
            Task.WaitAll(ts);
            Hash = result.ToString();
            this.nonce = (int)threadNonce;
            eNonce = (int)threadNum;
        }

        // Default parameter eNonce is so we can use the same function for the threaded mine and the non threaded mine.
        public String CreateHash(int eNonce=1, int nonce=-1)
        {
            // This is to ensure validation works (as we're passing CreateHash with no arguments with validation)
            if (this.eNonce != -1)
            {
                eNonce = this.eNonce;
            }
            if (nonce == -1)
            {
                nonce = this.nonce;
            }

            SHA256 hasher;
            hasher = SHA256Managed.Create();
            String input = index.ToString() + timestamp.ToString() + previousHash + nonce.ToString() + eNonce.ToString() + difficulty.ToString() + reward.ToString() + merkleRoot;
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            String hash = string.Empty;

            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }
            return hash;
        }

        public String ReturnInfo()
        {
            string str = "== BLOCK START ==" +
                "\nBlock index: " + index +
                " \t\tTimestamp: " + timestamp +
                "\nPrevious Hash: " + previousHash +
                "\nHash: " + this.Hash +
                "\nMerkleroot: " + merkleRoot +
                "\nNonce: " + nonce +
                "\nE-Nonce: " + this.eNonce + 
                "\nDifficulty: " + difficulty +
                "\nMiner Address: " + minerAddress +
                "\nReward: " + reward +
                "\t\tCumulative Fees: " + cum_fees +
                "\n\n= TRANSACTIONS =";

            foreach (Transaction t in transactionList)
            {
                str += "\n\n" + t.ReturnInfo();
            }

            str += "\n== BLOCK END ==";

            return str;
        }

    }
}
