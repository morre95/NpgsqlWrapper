using Microsoft.VisualStudio.TestTools.UnitTesting;
using NpgsqlWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpgsqlWrapper.Tests
{
    [TestClass()]
    public class EncryptionTests
    {
        [TestMethod()]
        public void EncryptStringToBytesTest()
        {
            string original = "secret message";
            byte[] encrypted;
            string decrypted;

            byte[] key = new byte[32];
            byte[] vector = new byte[32];
            Encryption.GenerateKeyPair(out key, out vector);

            // Encrypt the string
            encrypted = Encryption.EncryptStringToBytes(original, key, vector);

            
            Assert.AreNotEqual(original, encrypted);
            Assert.AreEqual(24, Convert.ToBase64String(encrypted).Length);
        }

        [TestMethod()]
        public void DecryptStringFromBytesTest()
        {
            string original = "secret message";
            byte[] encrypted;
            string decrypted;

            byte[] key = new byte[32];
            byte[] vector = new byte[32];
            Encryption.GenerateKeyPair(out key, out vector);

            // Encrypt the string
            encrypted = Encryption.EncryptStringToBytes(original, key, vector);

            // Decrypt the bytes
            decrypted = Encryption.DecryptStringFromBytes(encrypted, key, vector);

            Assert.AreEqual(original, decrypted);
        }

        [TestMethod()]
        public void GenerateKeyPairTest()
        {
            byte[] key = new byte[32];
            byte[] vector = new byte[32];

            Assert.AreEqual(key.Length, vector.Length);

            Encryption.GenerateKeyPair(out key, out vector);

            Assert.AreNotEqual(key.Length, vector.Length);

            Assert.AreEqual(16, vector.Length);
            Assert.AreEqual(32, key.Length);
        }
    }
}