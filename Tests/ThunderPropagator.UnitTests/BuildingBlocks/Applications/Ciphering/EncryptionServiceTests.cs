using System.Reflection;
using System.Security.Cryptography;
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

        // Use the minimum allowed iteration count so tests stay fast while still
        // exercising real PBKDF2 key derivation through the production code path.
        private const int TestIterations = 100_000;

        private readonly byte[] _key;
        private readonly byte[] _salt;

        public EncryptionServiceTests()
        {
            (_key, _salt) = EncryptionService.CreateKey(TestPassword, iterations: TestIterations);
        }

        [Fact]
        public void Encrypt_ShouldReturnNonEmptyCiphertext()
        {
            var cipherText = EncryptionService.Encrypt(TestPlainText, _key);

            Assert.False(string.IsNullOrEmpty(cipherText));
        }

        [Fact]
        public void Decrypt_ShouldReturnOriginalPlainText()
        {
            var cipherText = EncryptionService.Encrypt(TestPlainText, _key);

            var decryptedText = EncryptionService.Decrypt(cipherText, _key);

            Assert.Equal(TestPlainText, decryptedText);
        }

        [Fact]
        public void Encrypt_ShouldReturnDifferentCiphertext_ForSamePlainTextAndKey()
        {
            var firstCipherText = EncryptionService.Encrypt(TestPlainText, _key);
            var secondCipherText = EncryptionService.Encrypt(TestPlainText, _key);

            Assert.NotEqual(firstCipherText, secondCipherText);
        }

        [Fact]
        public void Encrypt_ShouldPrependInitializationVectorToCiphertext()
        {
            var cipherText = EncryptionService.Encrypt(TestPlainText, _key);
            var encryptedBytes = Convert.FromBase64String(AddBase64Padding(cipherText));

            Assert.True(encryptedBytes.Length > 16);
        }

        [Fact]
        public void CreateKey_ShouldGenerateSecureKey()
        {
            const int expectedKeySize = 32;

            var (key, salt) = EncryptionService.CreateKey(TestPassword, expectedKeySize, iterations: TestIterations);

            Assert.Equal(expectedKeySize, key.Length);
            Assert.Equal(16, salt.Length);
        }

        [Fact]
        public void CreateKey_ShouldGenerateDifferentSalt_EachCall()
        {
            var (_, firstSalt) = EncryptionService.CreateKey(TestPassword, iterations: TestIterations);
            var (_, secondSalt) = EncryptionService.CreateKey(TestPassword, iterations: TestIterations);

            Assert.False(firstSalt.SequenceEqual(secondSalt));
        }

        [Fact]
        public void CreateKey_WithExistingSalt_ShouldDeriveIdenticalKey()
        {
            var (originalKey, salt) = EncryptionService.CreateKey(TestPassword, iterations: TestIterations);

            var rederived = EncryptionService.CreateKey(TestPassword, salt, iterations: TestIterations);

            Assert.Equal(originalKey, rederived);
        }

        [Fact]
        public void CreateKey_WithExistingSalt_ShouldDecryptCipherTextEncryptedWithOriginalKey()
        {
            var (key, salt) = EncryptionService.CreateKey(TestPassword, iterations: TestIterations);
            var cipherText = EncryptionService.Encrypt(TestPlainText, key);

            var rederived = EncryptionService.CreateKey(TestPassword, salt, iterations: TestIterations);
            var decrypted = EncryptionService.Decrypt(cipherText, rederived);

            Assert.Equal(TestPlainText, decrypted);
        }

        [Fact]
        public void Decrypt_InvalidCipherText_ShouldThrowFormatException()
        {
            Assert.Throws<FormatException>(() => EncryptionService.Decrypt("InvalidCipherText", _key));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(300)]
        [InlineData(99_999)]
        public void CreateKey_IterationsBelowMinimum_ThrowsArgumentOutOfRangeException(int iterations)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => EncryptionService.CreateKey(TestPassword, iterations: iterations));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(300)]
        [InlineData(99_999)]
        public void CreateKey_WithSalt_IterationsBelowMinimum_ThrowsArgumentOutOfRangeException(int iterations)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            Assert.Throws<ArgumentOutOfRangeException>(() => EncryptionService.CreateKey(TestPassword, salt, iterations: iterations));
        }

        [Fact]
        public void CreateKey_DefaultIterations_IsAtLeast600000()
        {
            // Verify the default parameter value meets NIST SP 800-132 guidance.
            var method = typeof(EncryptionService).GetMethod(
                nameof(EncryptionService.CreateKey),
                [typeof(string), typeof(int), typeof(int), typeof(HashAlgorithmName?)])!;

            var iterationsParam = method.GetParameters().First(p => p.Name == "iterations");
            var defaultValue = (int)iterationsParam.DefaultValue!;

            Assert.True(defaultValue >= 600_000, $"Default iteration count {defaultValue} is below the NIST-recommended minimum of 600 000.");
        }

        // --- Issue #135 checklist tests: PBKDF2 algorithm hygiene ---

        [Fact]
        public void CreateKey_DefaultAlgorithm_ProducesSameResultAsSha3_256Explicit()
        {
            // Verifies the default algorithm is SHA3-256, not a weak hash like MD5 or SHA-1.
            // If the default drifted to a weaker algorithm, the two derived keys would differ.
            var salt = RandomNumberGenerator.GetBytes(16);

            var keyDefault = EncryptionService.CreateKey(TestPassword, salt, iterations: TestIterations);
            var keyExplicit = EncryptionService.CreateKey(TestPassword, salt, iterations: TestIterations, algorithmName: HashAlgorithmName.SHA3_256);

            Assert.Equal(keyExplicit, keyDefault);
        }

        [Fact]
        public void CreateKey_WithWeakAlgorithm_Sha1_ProducesDifferentKeyThanDefault()
        {
            // SHA-1 is explicitly NOT the default; this ensures the default is a stronger algorithm.
            var salt = RandomNumberGenerator.GetBytes(16);

            var keyDefault = EncryptionService.CreateKey(TestPassword, salt, iterations: TestIterations);
            var keySha1 = EncryptionService.CreateKey(TestPassword, salt, iterations: TestIterations, algorithmName: HashAlgorithmName.SHA1);

            Assert.NotEqual(keySha1, keyDefault);
        }

        private static string AddBase64Padding(string value)
        {
            var padding = value.Length % 4;
            return padding == 0 ? value : value.PadRight(value.Length + 4 - padding, '=');
        }
    }
}
