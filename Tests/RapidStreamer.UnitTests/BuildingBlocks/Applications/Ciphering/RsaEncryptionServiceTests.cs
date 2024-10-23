using System.Security.Cryptography;
using RapidStreamer.BuildingBlocks.Application.Ciphering;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Ciphering
{
    public
#if !DEBUG
        sealed
#endif
        class RsaEncryptionServiceTests
    {
        private const string TestPlainText = "Hello, World!";
        private readonly RsaEncryptionService _rsaService;
        private readonly (RSAParameters privateKey, RSAParameters publicKey) _keys;

        public RsaEncryptionServiceTests()
        {
            _rsaService = new RsaEncryptionService(2048); // Using a stronger key size
            _keys = RsaEncryptionService.GenerateKeys(2048);
        }

        [Fact]
        public void Encrypt_ShouldReturnCipherText()
        {
            // Act
            var cipherText = _rsaService.Encrypt(TestPlainText);

            // Assert
            Assert.False(string.IsNullOrEmpty(cipherText));
        }

        [Fact]
        public void Decrypt_ShouldReturnOriginalPlainText()
        {
            // Arrange
            var cipherText = _rsaService.Encrypt(TestPlainText);

            // Act
            var decryptedText = _rsaService.Decrypt(cipherText);

            // Assert
            Assert.Equal(TestPlainText, decryptedText);
        }

        [Fact]
        public void Encrypt_WithCustomPublicKey_ShouldReturnCipherText()
        {
            // Act
            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, _keys.publicKey);

            // Assert
            Assert.False(string.IsNullOrEmpty(cipherText));
        }

        [Fact]
        public void Decrypt_WithCustomPrivateKey_ShouldReturnOriginalPlainText()
        {
            // Arrange
            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, _keys.publicKey);

            // Act
            var decryptedText = RsaEncryptionService.Decrypt(cipherText, _keys.privateKey);

            // Assert
            Assert.Equal(TestPlainText, decryptedText);
        }

        [Fact]
        public void Encrypt_WithInvalidKey_ShouldThrowException()
        {
            // Arrange
            var invalidKey = new byte[0]; // Empty key

            // Act & Assert
            Assert.Throws<CryptographicException>(() => RsaEncryptionService.Encrypt(TestPlainText, invalidKey));
        }

        [Fact]
        public void Decrypt_WithInvalidCipherText_ShouldThrowFormatException()
        {
            // Arrange
            var invalidCipherText = "InvalidCipherText";

            // Act & Assert
            Assert.Throws<FormatException>(() => _rsaService.Decrypt(invalidCipherText));
        }

        // Additional tests can be added for edge cases and specific error conditions
    }
}