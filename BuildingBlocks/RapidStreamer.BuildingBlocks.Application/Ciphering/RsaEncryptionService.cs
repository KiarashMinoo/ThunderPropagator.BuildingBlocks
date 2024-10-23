using Ardalis.GuardClauses;
using Newtonsoft.Json;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace RapidStreamer.BuildingBlocks.Application.Ciphering
{
    public static class RsaEncryptionHelper
    {
        public static (string PrivateKey, string PublicKey) GeneratePemCodes(int dwKeySize = 512)
        {
            using var rsaCryptoServiceProvider = RsaEncryptionService.GenerateProvider(dwKeySize);
            var privateKey = rsaCryptoServiceProvider.ExportRSAPrivateKeyPem();
            var publicKey = rsaCryptoServiceProvider.ExportRSAPublicKeyPem();
            return (privateKey, publicKey);
        }

        public static (byte[] PrivateKey, byte[] PublicKey) GenerateRsaCodes(int dwKeySize = 512)
        {
            using var rsaCryptoServiceProvider = RsaEncryptionService.GenerateProvider(dwKeySize);
            var privateKey = rsaCryptoServiceProvider.ExportRSAPrivateKey();
            var publicKey = rsaCryptoServiceProvider.ExportRSAPublicKey();
            return (privateKey, publicKey);
        }

        public static string ToXmlString(this RSAParameters key)
        {
            StringWriter writer = new();
            XmlSerializer serializer = new(typeof(RSAParameters));
            serializer.Serialize(writer, key);
            return writer.ToString();
        }

        public static string ToJsonString(this RSAParameters key)
        {
            StringWriter writer = new();
            JsonSerializer serializer = new();
            serializer.Serialize(writer, key);
            return writer.ToString();
        }
    }

    public
#if !DEBUG
        sealed
#endif
        class RsaEncryptionService
    {
        public RSAParameters PrivateKey { get; }
        public RSAParameters PublicKey { get; }

        public RsaEncryptionService(int dwKeySize = 512) => (PrivateKey, PublicKey) = GenerateKeys(dwKeySize);

        public RsaEncryptionService(RSAParameters privateKey, RSAParameters publicKey, int dwKeySize = 512)
            : this(dwKeySize)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        public RsaEncryptionService(string privateKey, string publicKey, int dwKeySize = 512)
            : this(Guard.Against.Null(privateKey.FromNJsonBase64<RSAParameters>()),
                Guard.Against.Null(publicKey.FromNJsonBase64<RSAParameters>()),
                dwKeySize)
        {
        }

        internal static RSACryptoServiceProvider GenerateProvider(int dwKeySize = 512) => new(Guard.Against.GreaterThanOrEqual(dwKeySize, 512, nameof(dwKeySize)));

        public static (RSAParameters PrivateKey, RSAParameters PublicKey) GenerateKeys(int dwKeySize = 512)
        {
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            var privateKey = rsaCryptoServiceProvider.ExportParameters(true);
            var publicKey = rsaCryptoServiceProvider.ExportParameters(false);
            return (privateKey, publicKey);
        }

        public static string Encrypt(string plainText, byte[] rsaPublicKey, int dwKeySize = 512)
        {
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            rsaCryptoServiceProvider.ImportRSAPublicKey(Guard.Against.NullOrEmpty(rsaPublicKey).ToArray(), out _);
            var dataBytes = Guard.Against.NullOrWhiteSpace(plainText).ToByteArray();
            var cipherData = rsaCryptoServiceProvider.Encrypt(dataBytes, false);
            return Convert.ToBase64String(cipherData)[..^2];
        }

        public static string Encrypt(string plainText, string pemPublicKey, int dwKeySize = 512)
        {
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            rsaCryptoServiceProvider.ImportFromPem(Guard.Against.NullOrWhiteSpace(pemPublicKey));
            var dataBytes = Guard.Against.NullOrWhiteSpace(plainText).ToByteArray();
            var cipherData = rsaCryptoServiceProvider.Encrypt(dataBytes, false);
            return Convert.ToBase64String(cipherData)[..^2];
        }

        public static string Encrypt(string plainText, RSAParameters publicKey, int dwKeySize = 512)
        {
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            rsaCryptoServiceProvider.ImportParameters(publicKey);
            var dataBytes = Guard.Against.NullOrWhiteSpace(plainText).ToByteArray();
            var cipherData = rsaCryptoServiceProvider.Encrypt(dataBytes, false);
            return Convert.ToBase64String(cipherData)[..^2];
        }

        public string Encrypt(string plainText) => Encrypt(Guard.Against.NullOrWhiteSpace(plainText), PublicKey);

        public static string Decrypt(string cipherText, byte[] rsaPrivateKey, int dwKeySize = 512)
        {
            var dataBytes = Convert.FromBase64String(Guard.Against.NullOrWhiteSpace(cipherText));
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            rsaCryptoServiceProvider.ImportRSAPrivateKey(Guard.Against.NullOrEmpty(rsaPrivateKey).ToArray(), out _);
            var plainTextBytes = rsaCryptoServiceProvider.Decrypt(dataBytes, false);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        public static string Decrypt(string cipherText, string pemPrivateKey, int dwKeySize = 512)
        {
            var dataBytes = Convert.FromBase64String(Guard.Against.NullOrWhiteSpace(cipherText));
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            rsaCryptoServiceProvider.ImportFromPem(Guard.Against.NullOrWhiteSpace(pemPrivateKey));
            var plainTextBytes = rsaCryptoServiceProvider.Decrypt(dataBytes, false);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        public static string Decrypt(string cipherText, RSAParameters privateKey, int dwKeySize = 512)
        {
            var dataBytes = Convert.FromBase64String(Guard.Against.NullOrWhiteSpace(cipherText));
            using var rsaCryptoServiceProvider = GenerateProvider(dwKeySize);
            rsaCryptoServiceProvider.ImportParameters(privateKey);
            var plainTextBytes = rsaCryptoServiceProvider.Decrypt(dataBytes, false);
            return Encoding.UTF8.GetString(plainTextBytes);
        }

        public string Decrypt(string cipherText) => Decrypt(Guard.Against.NullOrWhiteSpace(cipherText), PrivateKey);
    }
}