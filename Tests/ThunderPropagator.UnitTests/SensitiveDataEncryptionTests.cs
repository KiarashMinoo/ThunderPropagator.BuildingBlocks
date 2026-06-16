using MessagePack;
using Newtonsoft.Json;
using ProtoBuf;
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

        // ─── NetJsonHelper ────────────────────────────────────────────────────────

        [Fact]
        public void NetJsonHelper_SensitiveProperty_IsEncryptedInJson()
        {
            var obj = new SensitiveNetJsonObject { Secret = SecretValue, Public = PublicValue };

            var json = obj.ToNetJson();

            Assert.DoesNotContain(SecretValue, json, StringComparison.Ordinal);
            Assert.Contains(PublicValue, json, StringComparison.Ordinal);
        }

        [Fact]
        public void NetJsonHelper_SensitiveProperty_RoundTrips()
        {
            var original = new SensitiveNetJsonObject { Secret = SecretValue, Public = PublicValue };

            var json = original.ToNetJson();
            var restored = json.FromNetJson<SensitiveNetJsonObject>();

            Assert.Equal(SecretValue, restored?.Secret);
            Assert.Equal(PublicValue, restored?.Public);
        }

        // ─── YamlHelper ───────────────────────────────────────────────────────────

        [Fact]
        public void YamlHelper_SensitiveProperty_IsEncryptedInYaml()
        {
            var config = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var yaml = config.ToYaml();

            Assert.DoesNotContain(SecretValue, yaml, StringComparison.Ordinal);
            Assert.Contains("audience", yaml, StringComparison.Ordinal);
        }

        [Fact]
        public void YamlHelper_SensitiveProperty_RoundTrips()
        {
            var original = new TestJwtConfig
            {
                IssuerSigningKey = SecretValue,
                ValidAudience = "audience",
                ValidIssuer = "issuer"
            };

            var yaml = original.ToYaml();
            var restored = yaml.FromYaml<TestJwtConfig>();

            Assert.Equal(SecretValue, restored?.IssuerSigningKey);
            Assert.Equal("audience", restored?.ValidAudience);
        }

        // ─── ProtobufHelper ───────────────────────────────────────────────────────

        [Fact]
        public void ProtobufHelper_SensitiveProperty_IsEncryptedInBytes()
        {
            var obj = new SensitiveProtoObject { Secret = SecretValue, Public = PublicValue };

            var bytes = obj.ToProtobufBytes();
            var serialized = System.Text.Encoding.UTF8.GetString(bytes);

            Assert.DoesNotContain(SecretValue, serialized, StringComparison.Ordinal);
        }

        [Fact]
        public void ProtobufHelper_SensitiveProperty_RoundTrips()
        {
            var original = new SensitiveProtoObject { Secret = SecretValue, Public = PublicValue };

            var bytes = original.ToProtobufBytes();
            var restored = bytes.FromProtobuf<SensitiveProtoObject>();

            Assert.Equal(SecretValue, restored.Secret);
            Assert.Equal(PublicValue, restored.Public);
        }

        [Fact]
        public void ProtobufHelper_CallerInstanceNotMutatedAfterSerialize()
        {
            var obj = new SensitiveProtoObject { Secret = SecretValue, Public = PublicValue };

            _ = obj.ToProtobufBytes();

            Assert.Equal(SecretValue, obj.Secret);
        }

        // ─── MessagePackHelper ────────────────────────────────────────────────────

        [Fact]
        public void MessagePackHelper_SensitiveProperty_IsEncryptedInBytes()
        {
            var obj = new SensitiveMsgPackObject { Secret = SecretValue, Public = PublicValue };

            var bytes = obj.ToMessagePackBytes();
            var json = MessagePackSerializer.ConvertToJson(bytes);

            Assert.DoesNotContain(SecretValue, json, StringComparison.Ordinal);
        }

        [Fact]
        public void MessagePackHelper_SensitiveProperty_RoundTrips()
        {
            var original = new SensitiveMsgPackObject { Secret = SecretValue, Public = PublicValue };

            var bytes = original.ToMessagePackBytes();
            var restored = bytes.FromMessagePack<SensitiveMsgPackObject>();

            Assert.Equal(SecretValue, restored.Secret);
            Assert.Equal(PublicValue, restored.Public);
        }

        [Fact]
        public void MessagePackHelper_CallerInstanceNotMutatedAfterSerialize()
        {
            var obj = new SensitiveMsgPackObject { Secret = SecretValue, Public = PublicValue };

            _ = obj.ToMessagePackBytes();

            Assert.Equal(SecretValue, obj.Secret);
        }

        // ─── XmlHelper ────────────────────────────────────────────────────────────

        [Fact]
        public void XmlHelper_SensitiveProperty_IsEncryptedInXml()
        {
            var obj = new SensitiveXmlObject { Secret = SecretValue, Public = PublicValue };

            var xml = obj.ToXml();

            Assert.DoesNotContain(SecretValue, xml, StringComparison.Ordinal);
            Assert.Contains(PublicValue, xml, StringComparison.Ordinal);
        }

        [Fact]
        public void XmlHelper_SensitiveProperty_RoundTrips()
        {
            var original = new SensitiveXmlObject { Secret = SecretValue, Public = PublicValue };

            var xml = original.ToXml();
            var restored = xml.FromXml<SensitiveXmlObject>();

            Assert.Equal(SecretValue, restored?.Secret);
            Assert.Equal(PublicValue, restored?.Public);
        }

        [Fact]
        public void XmlHelper_CallerInstanceNotMutatedAfterSerialize()
        {
            var obj = new SensitiveXmlObject { Secret = SecretValue, Public = PublicValue };

            _ = obj.ToXml();

            Assert.Equal(SecretValue, obj.Secret);
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

        private sealed class TestJwtConfig : JwtConfiguration { }

        private sealed class TestBasicUserConfig : BasicUserConfiguration
        {
            public void SetCredentials(string username, string password)
            {
                Username = username;
                Password = password;
            }
        }

    }

    // ── Namespace-level test fixtures (public required by MessagePack/XmlSerializer) ──

    [ProtoContract]
    internal sealed class SensitiveProtoObject
    {
        [ProtoMember(1), SensitiveData]
        public string Secret { get; set; } = "";

        [ProtoMember(2)]
        public string Public { get; set; } = "";
    }

    [MessagePackObject]
    public sealed class SensitiveMsgPackObject
    {
        [Key(0), SensitiveData]
        public string Secret { get; set; } = "";

        [Key(1)]
        public string Public { get; set; } = "";
    }

    public sealed class SensitiveXmlObject
    {
        [SensitiveData]
        public string Secret { get; set; } = "";

        public string Public { get; set; } = "";
    }

    // CamelCase disabled: NetJSON's camelCase deserialization uses case-sensitive key matching,
    // so round-tripping "Secret" → "secret" → property lookup fails. PascalCase avoids this.
    [ThunderPropagator.BuildingBlocks.Application.Attributes.JsonSerialization(CamelCase = false)]
    public sealed class SensitiveNetJsonObject
    {
        [SensitiveData]
        public string Secret { get; set; } = "";

        public string Public { get; set; } = "";
    }
}
