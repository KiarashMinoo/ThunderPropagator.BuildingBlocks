using Newtonsoft.Json;
using ThunderPropagator.BuildingBlocks.Application;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using ThunderPropagator.BuildingBlocks.Application.Identity;

namespace ThunderPropagator.UnitTests
{
    /// <summary>
    /// Verifies that properties marked with <see cref="SensitiveDataAttribute"/> are encrypted
    /// when serialized and decrypted when deserialized, across all three serialization paths:
    /// <c>ServiceConfiguration</c> converter, <c>JwtConfiguration</c> via NJsonHelper,
    /// and <c>BasicUserConfiguration</c> via NJsonHelper.
    /// </summary>
    public
#if !DEBUG
        sealed
#endif
        class SensitiveDataEncryptionTests : IDisposable
    {
        // Any valid AES-256 key (32 bytes). Deterministic so all tests in one run share the same key.
        private static readonly byte[] TestKey =
            Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();

        private const string SecretValue = "super-secret-value-that-must-not-appear-in-json";
        private const string PublicValue = "visible-public-value";

        public SensitiveDataEncryptionTests()
        {
            SensitiveDataEncryption.Reset();
            SensitiveDataEncryption.Configure(TestKey);
        }

        public void Dispose() => SensitiveDataEncryption.Reset();

        // ─── ServiceConfiguration via JsonConverter ────────────────────────────────

        [Fact]
        public void ServiceConfiguration_SensitiveProperty_IsEncryptedInSerializedJson()
        {
            var config = ServiceConfiguration.CreateNew<SecureServiceConfig>(
            [
                new KeyValuePair<string, string>("Secret", SecretValue),
                new KeyValuePair<string, string>("Name", PublicValue)
            ]);

            var json = JsonConvert.SerializeObject(config);

            Assert.DoesNotContain(SecretValue, json, StringComparison.Ordinal);
            Assert.Contains(PublicValue, json, StringComparison.Ordinal);
        }

        [Fact]
        public void ServiceConfiguration_SensitiveProperty_RoundTrips()
        {
            var original = ServiceConfiguration.CreateNew<SecureServiceConfig>(
            [
                new KeyValuePair<string, string>("Secret", SecretValue),
                new KeyValuePair<string, string>("Name", PublicValue)
            ]);

            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<SecureServiceConfig>(json);

            Assert.Equal(SecretValue, restored?.Secret);
            Assert.Equal(PublicValue, restored?.Name);
        }

        [Fact]
        public void ServiceConfiguration_NonSensitiveProperty_IsPlaintext()
        {
            var config = ServiceConfiguration.CreateNew<SecureServiceConfig>(
            [
                new KeyValuePair<string, string>("Name", PublicValue)
            ]);

            var json = JsonConvert.SerializeObject(config);

            Assert.Contains(PublicValue, json, StringComparison.Ordinal);
        }

        [Fact]
        public void ServiceConfiguration_WhenNotConfigured_SensitivePropertyIsNotEncrypted()
        {
            SensitiveDataEncryption.Reset();

            var config = ServiceConfiguration.CreateNew<SecureServiceConfig>(
            [
                new KeyValuePair<string, string>("Secret", SecretValue)
            ]);

            var json = JsonConvert.SerializeObject(config);

            Assert.Contains(SecretValue, json, StringComparison.Ordinal);
        }

        // ─── JwtConfiguration via NJsonHelper ─────────────────────────────────────

        [Fact]
        public void JwtConfiguration_IssuerSigningKey_IsEncryptedInNJson()
        {
            var config = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var json = config.ToNJson();

            Assert.DoesNotContain(SecretValue, json, StringComparison.Ordinal);
            Assert.Contains("audience", json, StringComparison.Ordinal);
        }

        [Fact]
        public void JwtConfiguration_IssuerSigningKey_RoundTripsViaToFromNJson()
        {
            var original = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var json = original.ToNJson();
            var restored = json.FromNJson<TestJwtConfig>();

            Assert.Equal(SecretValue, restored?.IssuerSigningKey);
            Assert.Equal("audience", restored?.ValidAudience);
        }

        [Fact]
        public void JwtConfiguration_WhenNotConfigured_IssuerSigningKeyIsNotEncrypted()
        {
            SensitiveDataEncryption.Reset();

            var config = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var json = config.ToNJson();

            Assert.Contains(SecretValue, json, StringComparison.Ordinal);
        }

        // ─── BasicUserConfiguration via NJsonHelper ────────────────────────────────

        [Fact]
        public void BasicUserConfiguration_Password_IsEncryptedInNJson()
        {
            var config = new TestBasicUserConfig();
            config.SetCredentials("alice", SecretValue);

            var json = config.ToNJson();

            Assert.DoesNotContain(SecretValue, json, StringComparison.Ordinal);
            Assert.Contains("alice", json, StringComparison.Ordinal);
        }

        [Fact]
        public void BasicUserConfiguration_Password_RoundTripsViaToFromNJson()
        {
            var original = new TestBasicUserConfig();
            original.SetCredentials("alice", SecretValue);

            var json = original.ToNJson();
            var restored = json.FromNJson<TestBasicUserConfig>();

            Assert.Equal(SecretValue, restored?.Password);
            Assert.Equal("alice", restored?.Username);
        }

        // ─── JsonHelper (STJ) ─────────────────────────────────────────────────────

        [Fact]
        public void JsonHelper_SensitiveProperty_IsEncryptedInJson()
        {
            var config = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var json = config.ToJson();

            Assert.DoesNotContain(SecretValue, json, StringComparison.Ordinal);
            Assert.Contains("audience", json, StringComparison.Ordinal);
        }

        [Fact]
        public void JsonHelper_SensitiveProperty_RoundTrips()
        {
            var original = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var json = original.ToJson();
            var restored = json.FromJson<TestJwtConfig>();

            Assert.Equal(SecretValue, restored?.IssuerSigningKey);
            Assert.Equal("audience", restored?.ValidAudience);
        }

        // ─── Configure is idempotent ───────────────────────────────────────────────

        [Fact]
        public void Configure_CalledTwice_SecondCallIsIgnored()
        {
            var differentKey = new byte[32];
            for (var i = 0; i < 32; i++) differentKey[i] = (byte)(i + 100);

            SensitiveDataEncryption.Configure(differentKey); // second call — must be ignored

            var config = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var json = config.ToNJson();
            var restored = json.FromNJson<TestJwtConfig>();

            // If the second call had been honoured, decryption with the first key would fail.
            Assert.Equal(SecretValue, restored?.IssuerSigningKey);
        }

        // ─── Private test types ────────────────────────────────────────────────────

        private
#if !DEBUG
            sealed
#endif
            class SecureServiceConfig : ServiceConfiguration
        {
            [SensitiveData]
            public string? Secret
            {
                get => Get<string>();
                set => Set(value);
            }

            public string? Name
            {
                get => Get<string>();
                set => Set(value);
            }
        }

        private sealed class TestJwtConfig : JwtConfiguration
        {
        }

        private sealed class TestBasicUserConfig : BasicUserConfiguration
        {
            public void SetCredentials(string username, string password)
            {
                Username = username;
                Password = password;
            }
        }
    }
}
