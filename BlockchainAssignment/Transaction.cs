using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Transaction
    {
        public String Hash;
        public String Signature;
        public String SenderAddress;
        public String RecipientAddress;
        public DateTime timestamp;
        public float amnt;
        public float fee;

        public Transaction(String sAddress, String sPrivKey, String rAddress, float amount, float transactionFee)
        {
            // Assign given arguments
            SenderAddress = sAddress;
            RecipientAddress = rAddress;
            amnt = amount;
            fee = transactionFee;

            //Call functions to assign the other arguments
            timestamp = DateTime.Now;
            Hash = CreateHash();
            Signature = Wallet.Wallet.CreateSignature(SenderAddress, sPrivKey, Hash);
        }

        public String CreateHash()
        {
            SHA256 hasher;
            hasher = SHA256Managed.Create();
            String input = SenderAddress + RecipientAddress + timestamp.ToString() + amnt.ToString() + fee.ToString();
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes((input)));

            String hash = string.Empty;

            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }
            return hash;

        }

        public String ReturnInfo()
        {
            return "Transaction Hash: " + Hash +
                "\nDigital Signature: " + Signature +
                "\nTimestamp: " + timestamp +
                "\nTransferred: " + amnt.ToString() + " NotBitCoins" +
                "\nFees: " + fee.ToString() +
                "\nSender Address: " + SenderAddress +
                "\nReciever Address: " + RecipientAddress;
        }
    }
}
