using System.Reflection;
using System.Security.Cryptography;
using ThunderPropagator.BuildingBlocks.Application.Ciphering;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Ciphering
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
            var cipherText = _rsaService.Encrypt(TestPlainText);

            var decrypted = _rsaService.Decrypt(cipherText);

            Assert.Equal(TestPlainText, decrypted);
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
            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, _keys.publicKey);

            var decrypted = RsaEncryptionService.Decrypt(cipherText, _keys.privateKey);

            Assert.Equal(TestPlainText, decrypted);
        }

        [Fact]
        public void Encrypt_WithInvalidKey_ShouldThrowException()
        {
            // Arrange
            var invalidKey = new byte[0]; // Empty key

            // Act & Assert
            // Current implementation guards against empty keys and throws ArgumentException
            Assert.Throws<ArgumentException>(() => RsaEncryptionService.Encrypt(TestPlainText, invalidKey));
        }

        [Fact]
        public void Decrypt_WithInvalidCipherText_ShouldThrowFormatException()
        {
            // Arrange
            var invalidCipherText = "InvalidCipherText";

            // Act & Assert
            Assert.Throws<FormatException>(() => _rsaService.Decrypt(invalidCipherText));
        }

        [Theory]
        [InlineData(256)]
        [InlineData(512)]
        [InlineData(1024)]
        [InlineData(2047)]
        public void GenerateKeys_WithKeySize_BelowMinimum_ShouldThrow(int keySize)
        {
            Assert.Throws<ArgumentException>(() => RsaEncryptionService.GenerateKeys(keySize));
        }

        [Fact]
        public void GenerateKeys_WithKeySize_AboveMaximum_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => RsaEncryptionService.GenerateKeys(32768));
        }

        [Theory]
        [InlineData(256)]
        [InlineData(512)]
        [InlineData(1024)]
        [InlineData(2047)]
        public void Encrypt_WithKeySize_BelowMinimum_ShouldThrow(int keySize)
        {
            Assert.Throws<ArgumentException>(() => RsaEncryptionService.Encrypt(TestPlainText, _keys.publicKey, keySize));
        }

        [Fact]
        public void Encrypt_WithKeySize_AboveMaximum_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => RsaEncryptionService.Encrypt(TestPlainText, _keys.publicKey, 32768));
        }

        [Fact]
        public void GenerateKeys_DefaultKeySize_IsAtLeast2048()
        {
            var method = typeof(RsaEncryptionService).GetMethod(
                nameof(RsaEncryptionService.GenerateKeys),
                [typeof(int)])!;

            var defaultValue = (int)method.GetParameters()[0].DefaultValue!;

            Assert.True(defaultValue >= 2048, $"Default key size {defaultValue} is below the NIST-recommended minimum of 2048 bits.");
        }

        // --- Issue #135 checklist tests: RSA round-trips and OaepSHA256 padding ---

        [Fact]
        public void EncryptDecrypt_WithRsaParameters_RoundTrip()
        {
            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, _keys.publicKey);

            var decrypted = RsaEncryptionService.Decrypt(cipherText, _keys.privateKey);

            Assert.Equal(TestPlainText, decrypted);
        }

        [Fact]
        public void EncryptDecrypt_WithByteKeys_RoundTrip()
        {
            var (privateKeyBytes, publicKeyBytes) = RsaEncryptionHelper.GenerateRsaCodes();

            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, publicKeyBytes);
            var decrypted = RsaEncryptionService.Decrypt(cipherText, privateKeyBytes);

            Assert.Equal(TestPlainText, decrypted);
        }

        [Fact]
        public void EncryptDecrypt_WithPemKeys_RoundTrip()
        {
            var (privatePem, publicPem) = RsaEncryptionHelper.GeneratePemCodes();

            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, publicPem);
            var decrypted = RsaEncryptionService.Decrypt(cipherText, privatePem);

            Assert.Equal(TestPlainText, decrypted);
        }

        [Fact]
        public void Encrypt_WrongPrivateKey_ThrowsCryptographicException()
        {
            var (_, wrongPublicKey) = RsaEncryptionService.GenerateKeys();
            var (correctPrivateKey, _) = RsaEncryptionService.GenerateKeys();

            var cipherText = RsaEncryptionService.Encrypt(TestPlainText, wrongPublicKey);

            Assert.Throws<CryptographicException>(() =>
                RsaEncryptionService.Decrypt(cipherText, correctPrivateKey));
        }
    }
}