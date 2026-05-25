using System.Security.Cryptography;

namespace ThunderPropagator.BuildingBlocks.Application.Ciphering
{
    public static class EncryptionService
    {
        private const int AesIvSizeInBytes = 16;
        private const int SaltSizeInBytes = 16;

        public static string Encrypt(string plainText, byte[] encryptionKeyBytes)
        {
            var iv = RandomNumberGenerator.GetBytes(AesIvSizeInBytes);

            using var aes = Aes.Create();
            aes.Key = encryptionKeyBytes;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
                streamWriter.Write(plainText);

            var cipherBytes = memoryStream.ToArray();
            var encryptedBytes = new byte[iv.Length + cipherBytes.Length];
            Buffer.BlockCopy(iv, 0, encryptedBytes, 0, iv.Length);
            Buffer.BlockCopy(cipherBytes, 0, encryptedBytes, iv.Length, cipherBytes.Length);

            return Convert.ToBase64String(encryptedBytes).TrimEnd('=');
        }

        public static string Decrypt(string cipherText, byte[] encryptionKeyBytes)
        {
            var buffer = Convert.FromBase64String(AddBase64Padding(cipherText));
            if (buffer.Length <= AesIvSizeInBytes)
            {
                throw new FormatException("Cipher text must include an IV and encrypted payload.");
            }

            var iv = buffer[..AesIvSizeInBytes];
            var cipherBytes = buffer[AesIvSizeInBytes..];

            using var aes = Aes.Create();
            aes.Key = encryptionKeyBytes;
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(cipherBytes);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        ///     Derives an AES key from a password using PBKDF2 with a freshly generated random salt.
        ///     The returned salt must be stored alongside the encrypted data and supplied to the
        ///     <see cref="CreateKey(string,byte[],int,int,System.Security.Cryptography.HashAlgorithmName?)"/>
        ///     overload when re-deriving the key for decryption.
        /// </summary>
        /// <param name="password">The password to derive the key from.</param>
        /// <param name="keyBytes">The desired key length in bytes. Default is 32 (256-bit).</param>
        /// <param name="iterations">
        ///     The number of PBKDF2 iterations. Must be at least 100 000.
        ///     Default is 600 000 per NIST SP 800-132 (2023) guidance for SHA-256/SHA3-256.
        /// </param>
        /// <param name="algorithmName">The hash algorithm to use. Default is SHA3-256.</param>
        /// <returns>A tuple containing the derived key bytes and the randomly generated salt.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="iterations"/> is less than 100 000.
        /// </exception>
        /// <remarks>
        ///     The default was raised from 300 to 600 000. Data encrypted with the old default
        ///     must be re-derived using the original iteration count passed explicitly.
        /// </remarks>
        public static (byte[] Key, byte[] Salt) CreateKey(string password, int keyBytes = 32, int iterations = 600_000, HashAlgorithmName? algorithmName = null)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);
            var key = DeriveKey(password, salt, keyBytes, iterations, algorithmName);
            return (key, salt);
        }

        /// <summary>
        ///     Re-derives an AES key from a password and a previously stored salt using PBKDF2.
        ///     Use this overload during decryption with the salt that was saved at encryption time.
        /// </summary>
        /// <param name="password">The password to derive the key from.</param>
        /// <param name="salt">The salt that was generated and stored at encryption time.</param>
        /// <param name="keyBytes">The desired key length in bytes. Default is 32 (256-bit).</param>
        /// <param name="iterations">
        ///     The number of PBKDF2 iterations. Must be at least 100 000.
        ///     Default is 600 000 per NIST SP 800-132 (2023) guidance for SHA-256/SHA3-256.
        /// </param>
        /// <param name="algorithmName">The hash algorithm to use. Default is SHA3-256.</param>
        /// <returns>The derived key bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="iterations"/> is less than 100 000.
        /// </exception>
        public static byte[] CreateKey(string password, byte[] salt, int keyBytes = 32, int iterations = 600_000, HashAlgorithmName? algorithmName = null)
        {
            return DeriveKey(password, salt, keyBytes, iterations, algorithmName);
        }

        private static byte[] DeriveKey(string password, byte[] salt, int keyBytes, int iterations, HashAlgorithmName? algorithmName)
        {
            if (iterations < 100_000)
                throw new ArgumentOutOfRangeException(nameof(iterations), iterations, "PBKDF2 iteration count must be at least 100 000 to meet minimum security requirements (NIST SP 800-132).");
#if NET10_0_OR_GREATER
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, algorithmName ?? HashAlgorithmName.SHA3_256, keyBytes);
#else
            var keyGenerator = new Rfc2898DeriveBytes(password, salt, iterations, algorithmName ?? HashAlgorithmName.SHA3_256);
            return keyGenerator.GetBytes(keyBytes);
#endif
        }

        private static string AddBase64Padding(string value)
        {
            var padding = value.Length % 4;
            return padding == 0 ? value : value.PadRight(value.Length + 4 - padding, '=');
        }
    }
}
