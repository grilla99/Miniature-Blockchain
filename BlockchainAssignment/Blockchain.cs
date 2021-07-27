using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Blockchain
    {
        public List<Block> Blocks = new List<Block>();
        public List<Transaction> transactionPool = new List<Transaction>();

        // Constructor for Blockchain
        public Blockchain()
        {
            Blocks.Add(new Block());
        }

        public List<Transaction> GetTransactionList(int mode, int no_of_trans, String minerAddress)
        {
            List<Transaction> returnList = new List<Transaction>();

            // Faster way of returning the list, if we want all the items in the list.
            if (no_of_trans == transactionPool.Count)
            {
                return transactionPool;
            }

            switch (mode)
            {
                // Greedy Mode
                case 0:
                    // Gets transaction pool, sorted by the transaction fee
                    returnList = transactionPool.OrderBy(t => t.fee).Reverse().ToList();
                    // Remove the transactions from the list that we want 
                    returnList.RemoveRange(no_of_trans, returnList.Count - (no_of_trans));
                    break;
                // Random Mode
                case 1:
                    // Generate no_of_trans random numbers, referring to index
                    // Then add them to the return list.
                    var random = new Random();
                    List<Transaction> tempList = transactionPool.ToList();
                    for (int i = 0; i < no_of_trans; i++)
                    {
                        int rndIndex = random.Next(0, tempList.Count);
                        returnList.Add(tempList[rndIndex]);
                        tempList.RemoveAt(rndIndex);
                    }
                    break;
                // Altruistic Mode (Oldest)
                case 2:
                    returnList = transactionPool.OrderBy(t => t.timestamp).ToList();
                    returnList.RemoveRange(no_of_trans, returnList.Count - (no_of_trans));
                    break;
                // Address Based
                case 3:
                    foreach(Transaction t in transactionPool)
                    {
                        // If transaction is to do with the Miner (either sender or reciever)
                        if (t.RecipientAddress.Equals(minerAddress) || t.SenderAddress.Equals(minerAddress))
                        {
                            returnList.Add(t);
                        }
                        // Stop once we have reached the number of transactions
                        if (returnList.Count == no_of_trans)
                        {
                            break;
                        }
                    }
                    
                    // If we don't have the right number of transactions (i.e. too  little)
                    if (returnList.Count != no_of_trans)
                    {
                        //  Find the transactions in Transaction pool that aren't in returnList
                        List<Transaction> resultList = transactionPool.Except(returnList).ToList();
                        
                        // Add the remaining transactions we need
                        for (int i = 0; i <(no_of_trans - returnList.Count); i++)
                        {
                            returnList.Add(resultList[i]);
                        }
                    }
                    break;
            }
            foreach (Transaction t in returnList)
            {
                Console.WriteLine("\nAmount: " + t.amnt.ToString() +
                                  "\nFee: " + t.fee.ToString() + 
                                  "\nTimestamp: " + t.timestamp.ToString() +
                                  "\nSender: " + t.SenderAddress +
                                  "\nRecipient: " + t.RecipientAddress);
            }
            return returnList;
        }

        public String GetBlockOutput(int blockIndex)
        {
            try
            {
                return Blocks[blockIndex].ReturnInfo();
            } catch (ArgumentOutOfRangeException)
            {
                return "Block doesn't exist";
            }
        }

        public Block GetLastBlock()
        {
            return Blocks[Blocks.Count - 1];
        }

        public String ReturnInfo()
        {
            string str = String.Empty;
            foreach (Block curBlock in Blocks)
            {
                str += curBlock.ReturnInfo();
                str += "\n\n";
            }
            return str;
        }

        public bool ValidateHash(Block b)
        {
            String rehash = b.CreateHash();
            Console.WriteLine("Validate Hash: " + rehash.Equals(b.Hash).ToString());
            return rehash.Equals(b.Hash);
        }

        public bool ValidateMerkelRoot(Block b)
        {
            String reMerkle = Block.MerkleRoot(b.transactionList);
            Console.WriteLine("Validate MerkleRoot: " + reMerkle.Equals(b.merkleRoot).ToString());
            return reMerkle.Equals(b.merkleRoot);
        }

        public bool ValidateTransactions(Block b)
        {
            foreach (Transaction t in b.transactionList)
            {
                if (t.Signature == "null" || !Wallet.Wallet.ValidateSignature(t.SenderAddress, t.Hash, t.Signature))
                {
                    return false;
                }
            }
            return true;
        }

        public double GetBalance(String address)
        {
            // Iterate through each Block's Transaction List
            double balance = 0;
            foreach(Block b in Blocks)
            {
                foreach(Transaction t in b.transactionList)
                {
                    if (t.RecipientAddress.Equals(address))
                    {
                        balance += t.amnt;
                    }
                    if (t.SenderAddress.Equals(address))
                    {
                        balance -= (t.amnt + t.fee);
                    }
                }
            }
            // Iterate through each transaction in transactionPool
            foreach (Transaction t in this.transactionPool)
            {
                if (t.RecipientAddress.Equals(address))
                {
                    balance += t.amnt;
                }
                if (t.SenderAddress.Equals(address))
                {
                    balance -= (t.amnt + t.fee);
                }
            }

            return balance;
        }

        public String ReturnTransPoolInfo()
        {
            string str = String.Empty;
            foreach (Transaction t in transactionPool)
            {
                str += "\n\nIndex:" + transactionPool.IndexOf(t) +"\n" + t.ReturnInfo();
            }
            if (str == String.Empty)
            {
                str += "No Pending Transactions";
            }
            return str;
        }
    }
}
