using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AttandanceSyncApp.Helpers
{
    /// <summary>
    /// Helper class for AES encryption/decryption of sensitive data.
    /// Uses AES-256 encryption with PBKDF2 key derivation for secure password storage
    /// and database credential protection.
    /// </summary>
    public static class EncryptionHelper
    {
        /// Static salt value used for key derivation in AES encryption.
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("AttandanceSyncSalt2024");

        /// <summary>
        /// Encrypts a plain text string using AES-256 encryption.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <returns>Base64 encoded encrypted string, or original value if null/empty.</returns>
        public static string Encrypt(string plainText)
        {
            // Return immediately if there's nothing to encrypt
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            // Retrieve encryption key from configuration
            var key = GetEncryptionKey();

            // Create AES encryptor with 256-bit key
            using (var aes = Aes.Create())
            {
                // Derive 256-bit key and 128-bit IV from password using PBKDF2
                var keyBytes = new Rfc2898DeriveBytes(key, Salt, 1000);
                aes.Key = keyBytes.GetBytes(32); // 256 bits
                aes.IV = keyBytes.GetBytes(16);  // 128 bits

                // Create encryptor and encrypt the plain text
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        // Write plain text to crypto stream
                        sw.Write(plainText);
                    }

                    // Convert encrypted bytes to Base64 string for storage
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts an AES-256 encrypted string.
        /// </summary>
        /// <param name="encryptedText">The Base64 encoded encrypted text.</param>
        /// <returns>Decrypted plain text, or original value if null/empty.</returns>
        public static string Decrypt(string encryptedText)
        {
            // Return immediately if there's nothing to decrypt
            if (string.IsNullOrEmpty(encryptedText))
                return encryptedText;

            // Retrieve encryption key from configuration
            var key = GetEncryptionKey();

            // Create AES decryptor with 256-bit key
            using (var aes = Aes.Create())
            {
                // Derive same key and IV used during encryption
                var keyBytes = new Rfc2898DeriveBytes(key, Salt, 1000);
                aes.Key = keyBytes.GetBytes(32); // 256 bits
                aes.IV = keyBytes.GetBytes(16);  // 128 bits

                // Create decryptor and decrypt the cipher text
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    // Read decrypted plain text from crypto stream
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Hashes a password using SHA-256 for secure storage.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>Base64 encoded hash, or null if password is empty.</returns>
        public static string HashPassword(string password)
        {
            // Return null for empty passwords
            if (string.IsNullOrEmpty(password))
                return null;

            // Compute SHA-256 hash of the password
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Return hash as Base64 string for storage
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Verifies a password against its stored hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hashedPassword">The stored hash to compare against.</param>
        /// <returns>True if password matches the hash, false otherwise.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Reject empty or null values
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            // Hash the provided password and compare with stored hash
            return HashPassword(password) == hashedPassword;
        }

        /// <summary>
        /// Generates a cryptographically secure random token for session management.
        /// </summary>
        /// <param name="length">Number of random bytes to generate (default 64).</param>
        /// <returns>Base64 encoded random token.</returns>
        public static string GenerateSecureToken(int length = 64)
        {
            // Use cryptographically secure random number generator
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Generate random bytes
                var bytes = new byte[length];
                rng.GetBytes(bytes);
                // Return as Base64 string
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Retrieves the encryption key from application configuration.
        /// </summary>
        /// <returns>The encryption key.</returns>
        /// <exception cref="InvalidOperationException">Thrown if key is not configured.</exception>
        private static string GetEncryptionKey()
        {
            // Retrieve key from Web.config appSettings
            var key = ConfigurationManager.AppSettings["EncryptionKey"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("EncryptionKey is not configured in Web.config");
            }
            return key;
        }
    }
}
