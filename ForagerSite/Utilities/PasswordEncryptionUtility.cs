using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ForagerSite.Utilities
{
    public class PasswordEncryptionUtility
    {
        private readonly byte[] EncryptionKey;
        private readonly byte[] InitializationVector;

        public PasswordEncryptionUtility()
        {
            EncryptionKey = new byte[32]; // 32 bytes for AES-256
            InitializationVector = new byte[16]; // 16 bytes for AES

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(EncryptionKey);
                rng.GetBytes(InitializationVector);
            }
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = InitializationVector;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // Decrypt a string
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText));

            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = InitializationVector;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
