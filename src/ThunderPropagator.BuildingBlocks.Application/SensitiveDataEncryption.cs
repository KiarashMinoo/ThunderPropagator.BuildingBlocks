using System.Collections.Concurrent;
using System.Reflection;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
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

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _sensitivePropsCache = new();

        private static PropertyInfo[] GetSensitiveProperties(Type type)
        {
            return _sensitivePropsCache.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite
                        && p.PropertyType == typeof(string)
                        && p.GetCustomAttribute<SensitiveDataAttribute>() is not null)
                    .ToArray());
        }

        /// <summary>
        /// Encrypts all <see cref="Attributes.SensitiveDataAttribute"/>-marked string properties
        /// on <paramref name="instance"/> in-place and returns the original values so the caller
        /// can revert after serialization. Returns <see langword="null"/> when encryption is not
        /// configured, the instance is <see langword="null"/>, or the type is a value type.
        /// </summary>
        internal static (PropertyInfo Prop, string? Original)[]? EncryptInPlace(object? instance)
        {
            if (!IsConfigured || instance is null || instance.GetType().IsValueType)
                return null;

            var props = GetSensitiveProperties(instance.GetType());
            if (props.Length == 0)
                return null;

            var originals = new (PropertyInfo Prop, string? Original)[props.Length];
            for (var i = 0; i < props.Length; i++)
            {
                var original = props[i].GetValue(instance) as string;
                originals[i] = (props[i], original);
                if (original is not null)
                    props[i].SetValue(instance, Encrypt(original));
            }

            return originals;
        }

        /// <summary>
        /// Restores the original (plain-text) property values that were encrypted by
        /// <see cref="EncryptInPlace"/>. Always call this in a <see langword="finally"/>
        /// block after serialization completes.
        /// </summary>
        internal static void RevertEncryption(object? instance, (PropertyInfo Prop, string? Original)[] originals)
        {
            if (instance is null)
                return;

            foreach (var (prop, original) in originals)
                prop.SetValue(instance, original);
        }

        /// <summary>
        /// Decrypts all <see cref="Attributes.SensitiveDataAttribute"/>-marked string properties
        /// on <paramref name="instance"/> in-place after deserialization. No-op when encryption is
        /// not configured, the instance is <see langword="null"/>, or the type is a value type.
        /// </summary>
        internal static void DecryptInPlace(object? instance)
        {
            if (!IsConfigured || instance is null || instance.GetType().IsValueType)
                return;

            var props = GetSensitiveProperties(instance.GetType());
            foreach (var prop in props)
            {
                if (prop.GetValue(instance) is string { Length: > 0 } encrypted)
                    prop.SetValue(instance, Decrypt(encrypted));
            }
        }

        /// <summary>Clears the configured key. For unit-test infrastructure only.</summary>
        internal static void Reset()
        {
            _key = null;
            Volatile.Write(ref _configured, 0);
        }
    }
}
