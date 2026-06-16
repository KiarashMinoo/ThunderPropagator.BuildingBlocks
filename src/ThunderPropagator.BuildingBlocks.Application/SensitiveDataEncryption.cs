using ThunderPropagator.BuildingBlocks.Application.Ciphering;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// Central configuration point for at-rest encryption of properties marked with
    /// <see cref="Attributes.SensitiveDataAttribute"/>. Call <see cref="Configure"/> once at
    /// application startup before any serialization occurs. All serialization paths in this
    /// library (<see cref="ServiceConfiguration"/> JSON converter and <c>NJsonHelper</c>)
    /// automatically encrypt sensitive fields on write and decrypt on read while a key is active.
    /// Subsequent <see cref="Configure"/> calls are silently ignored, matching the semantics of
    /// <see cref="Telemetry.Configure"/>.
    /// </summary>
    public static class SensitiveDataEncryption
    {
        private static byte[]? _key;
        private static int _configured;

        /// <summary>Gets a value indicating whether an encryption key has been configured.</summary>
        public static bool IsConfigured => _configured == 1;

        /// <summary>
        /// Configures the AES key used to encrypt and decrypt
        /// <see cref="Attributes.SensitiveDataAttribute"/>-marked string properties during
        /// serialization. Must be a 16, 24, or 32-byte (128 / 192 / 256-bit AES) key.
        /// Use <see cref="EncryptionService.CreateKey(string, int, int, System.Security.Cryptography.HashAlgorithmName?)"/>
        /// to derive a key from a master password. Subsequent calls are silently ignored.
        /// </summary>
        /// <param name="key">The AES key bytes (16, 24, or 32 bytes).</param>
        public static void Configure(byte[] key)
        {
            ArgumentNullException.ThrowIfNull(key);
            if (Interlocked.CompareExchange(ref _configured, 1, 0) == 0)
                _key = (byte[])key.Clone();
        }

        internal static string Encrypt(string plaintext)
        {
            return _key is not null
                ? EncryptionService.Encrypt(plaintext, _key)
                : plaintext;
        }

        internal static string Decrypt(string ciphertext)
        {
            return _key is not null
                ? EncryptionService.Decrypt(ciphertext, _key)
                : ciphertext;
        }

        /// <summary>Clears the configured key. For unit-test infrastructure only.</summary>
        internal static void Reset()
        {
            _key = null;
            Volatile.Write(ref _configured, 0);
        }
    }
}
