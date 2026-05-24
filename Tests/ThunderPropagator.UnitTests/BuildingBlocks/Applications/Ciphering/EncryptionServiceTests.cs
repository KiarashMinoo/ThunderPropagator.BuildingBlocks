using ThunderPropagator.BuildingBlocks.Application.Ciphering;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Ciphering
{
    public
#if !DEBUG
        sealed
#endif
        class EncryptionServiceTests
    {
        private const string TestPassword = "securePassword123!";
        private const string TestPlainText = "Hello, World!";
        private readonly byte[] _key;

        public EncryptionServiceTests()
        {
            // Create a key using the password
            _key = EncryptionService.CreateKey(TestPassword);
        }

        [Fact]
        public void Encrypt_ShouldReturnNonEmptyCiphertext()
        {
            // Act
            var cipherText = EncryptionService.Encrypt(TestPlainText, _key);

            // Assert
            Assert.False(string.IsNullOrEmpty(cipherText)); // Ciphertext should not be empty
        }

        [Fact]
        public void Decrypt_ShouldReturnOriginalPlainText()
        {
            // Arrange
            var cipherText = EncryptionService.Encrypt(TestPlainText, _key);

            // Act
            var decryptedText = EncryptionService.Decrypt(cipherText, _key);

            // Assert
            Assert.Equal(TestPlainText, decryptedText); // Decrypted text should match the original plaintext
        }

        [Fact]
        public void Encrypt_ShouldReturnDifferentCiphertext_ForSamePlainTextAndKey()
        {
            // Act
            var firstCipherText = EncryptionService.Encrypt(TestPlainText, _key);
            var secondCipherText = EncryptionService.Encrypt(TestPlainText, _key);

            // Assert
            Assert.NotEqual(firstCipherText, secondCipherText);
        }

        [Fact]
        public void Encrypt_ShouldPrependInitializationVectorToCiphertext()
        {
            // Act
            var cipherText = EncryptionService.Encrypt(TestPlainText, _key);
            var encryptedBytes = Convert.FromBase64String(AddBase64Padding(cipherText));

            // Assert
            Assert.True(encryptedBytes.Length > 16);
        }

        [Fact]
        public void CreateKey_ShouldGenerateSecureKey()
        {
            // Arrange
            const int expectedKeySize = 32; // 256 bits

            // Act
            var key = EncryptionService.CreateKey(TestPassword, expectedKeySize);

            // Assert
            Assert.Equal(expectedKeySize, key.Length); // Key length should match the expected size
        }

        [Fact]
        public void Decrypt_InvalidCipherText_ShouldThrowFormatException()
        {
            // Arrange
            var invalidCipherText = "InvalidCipherText";

            // Act & Assert
            Assert.Throws<FormatException>(() => EncryptionService.Decrypt(invalidCipherText, _key));
        }

        // Additional tests for edge cases and security considerations can be added here

        private static string AddBase64Padding(string value)
        {
            var padding = value.Length % 4;
            return padding == 0 ? value : value.PadRight(value.Length + 4 - padding, '=');
        }
    }
}
