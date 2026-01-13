using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AttandanceSyncApp.Helpers
{
    /// <summary>
    /// Helper class for AES encryption/decryption of sensitive data
    /// </summary>
    public static class EncryptionHelper
    {
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("AttandanceSyncSalt2024");

        /// <summary>
        /// Encrypts a plain text string using AES encryption
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            var key = GetEncryptionKey();

            using (var aes = Aes.Create())
            {
                var keyBytes = new Rfc2898DeriveBytes(key, Salt, 1000);
                aes.Key = keyBytes.GetBytes(32);
                aes.IV = keyBytes.GetBytes(16);

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts an AES encrypted string
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            var key = GetEncryptionKey();

            using (var aes = Aes.Create())
            {
                var keyBytes = new Rfc2898DeriveBytes(key, Salt, 1000);
                aes.Key = keyBytes.GetBytes(32);
                aes.IV = keyBytes.GetBytes(16);

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Hashes a password using SHA-256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            return HashPassword(password) == hashedPassword;
        }

        /// <summary>
        /// Generates a cryptographically secure random token
        /// </summary>
        public static string GenerateSecureToken(int length = 64)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        private static string GetEncryptionKey()
        {
            var key = ConfigurationManager.AppSettings["EncryptionKey"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("EncryptionKey is not configured in Web.config");
            }
            return key;
        }
    }
}
