using System.Security.Cryptography;

namespace RapidStreamer.BuildingBlocks.Application.Ciphering
{
    public static class EncryptionService
    {
        public static string Encrypt(string plainText, byte[] encryptionKeyBytes)
        {
            var iv = new byte[16];

            using var aes = Aes.Create();
            aes.Key = encryptionKeyBytes;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
                streamWriter.Write(plainText);

            var array = memoryStream.ToArray();
            return Convert.ToBase64String(array)[..^2];
        }

        public static string Decrypt(string cipherText, byte[] encryptionKeyBytes)
        {
            var iv = new byte[16];
            var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = encryptionKeyBytes;
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            return streamReader.ReadToEnd();
        }

        private static readonly byte[] Salt = [10, 20, 30, 40, 50, 60, 70, 80];

        public static byte[] CreateKey(string password, int keyBytes = 32, int iterations = 300, HashAlgorithmName? algorithmName = null)
        {
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, iterations, algorithmName ?? HashAlgorithmName.SHA3_256);
            return keyGenerator.GetBytes(keyBytes);
        }
    }
}