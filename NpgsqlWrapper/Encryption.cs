using System.Security.Cryptography;
using System.Text;

// Inspired of AES (Advanced Encryption Standard): https://www.c-sharpcorner.com/article/best-algorithm-for-encrypting-and-decrypting-a-string-in-c-sharp/
namespace NpgsqlWrapper
{
    /// <summary>
    /// This class encrypts and decrypts a string with AES (Advanced Encryption Standard). It encrypts data in fixed-size blocks of 128 bits, using a key size of 256 bits.
    /// </summary>
    public class Encryption
    {
        /// <summary>
        /// Create an Aes object with the specified key and IV to encrypt message in plain text.
        /// </summary>
        /// <param name="plainText">The message to encrypt.</param>
        /// <param name="key">Encryption secret key.</param>
        /// <param name="iv">Encryption initialisation vector.</param>
        /// <returns>Bytes array.</returns>
        public static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using MemoryStream memoryStream = new MemoryStream();
                using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                encrypted = memoryStream.ToArray();
            }

            return encrypted;
        }

        /// <summary>
        /// // Create an Aes object with the specified key and IV to decrypt the byte array.
        /// </summary>
        /// <param name="cipherText">The message to encrypt.</param>
        /// <param name="key">Encryption secret ke.y</param>
        /// <param name="iv">Encryption initialisation vector.</param>
        /// <returns>String.</returns>
        public static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            string decrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using MemoryStream memoryStream = new MemoryStream(cipherText);
                using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using StreamReader streamReader = new StreamReader(cryptoStream);
                decrypted = streamReader.ReadToEnd();
            }

            return decrypted;
        }

        /// <summary>
        /// Generat key and initialisation vector to be used for encryption.
        /// </summary>
        /// <param name="key">Encryption secret key.</param>
        /// <param name="iv">Encryption initialisation vector.</param>
        public static void GenerateKeyPair(out byte[] key, out byte[] iv)
        {
            using Aes aes = Aes.Create();

            key = aes.Key;
            iv = aes.IV;
        }
    }
}
