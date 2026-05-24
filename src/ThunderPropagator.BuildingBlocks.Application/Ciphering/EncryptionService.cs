using System.Security.Cryptography;

namespace ThunderPropagator.BuildingBlocks.Application.Ciphering
{
    public static class EncryptionService
    {
        private const int AesIvSizeInBytes = 16;

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

        private static readonly byte[] Salt = [10, 20, 30, 40, 50, 60, 70, 80];

        public static byte[] CreateKey(string password, int keyBytes = 32, int iterations = 300, HashAlgorithmName? algorithmName = null)
        {
            // Pbkdf2
#if NET10_0_OR_GREATER
            return Rfc2898DeriveBytes.Pbkdf2(password, Salt, iterations, algorithmName ?? HashAlgorithmName.SHA3_256, keyBytes);
#else
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, iterations, algorithmName ?? HashAlgorithmName.SHA3_256);
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
