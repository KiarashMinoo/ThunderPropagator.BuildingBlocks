using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text;
using Xunit;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.UnitTests.Helpers
{
    public class NJsonHelperTests
    {
        private class TestObject
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 42;
        }

        private abstract class Animal
        {
            public string Kind { get; set; } = string.Empty;
        }

        private sealed class Dog : Animal
        {
            public Dog()
            {
                Kind = "dog";
            }
        }

        private sealed class Cat : Animal
        {
            public Cat()
            {
                Kind = "cat";
            }
        }

        // Binder that only allows Dog — used to verify FromNJsonPolymorphic.
        private sealed class DogOnlyBinder : ISerializationBinder
        {
            public Type BindToType(string? assemblyName, string typeName)
            {
                if (typeName.EndsWith(nameof(Dog), StringComparison.Ordinal))
                    return typeof(Dog);
                throw new JsonSerializationException($"Type '{typeName}' is not allowed.");
            }

            public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
            {
                assemblyName = null;
                typeName = serializedType.FullName;
            }
        }

        [Fact]
        public void ToNJson_ShouldSerializeObject()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var json = obj.ToNJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Test", json);
            Assert.Contains("42", json);
        }

        [Fact]
        public void ToNJsonBytes_ShouldSerializeToBytes()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var bytes = obj.ToNJsonBytes();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void ToNJsonBase64_ShouldSerializeToBase64()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var base64 = obj.ToNJsonBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);
        }

        [Fact]
        public void FromNJson_ShouldDeserializeObject()
        {
            // Arrange
            var obj = new TestObject();
            var json = obj.ToNJson();

            // Act
            var deserialized = json.FromNJson<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromNJsonBytes_ShouldDeserializeFromBytes()
        {
            // Arrange
            var obj = new TestObject();
            var bytes = obj.ToNJsonBytes();

            // Act
            var deserialized = bytes.FromNJsonBytes<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromNJsonBase64_ShouldDeserializeFromBase64()
        {
            // Arrange
            var obj = new TestObject();
            var base64 = obj.ToNJsonBase64();

            // Act
            var deserialized = base64.FromNJsonBase64<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        // --- Issue #131 security tests: $type field ignored by default deserialization paths ---

        [Fact]
        public void FromNJson_WithDollarTypePayload_IgnoresTypeField()
        {
            // JSON carries a $type annotation that, with TypeNameHandling.Objects, would
            // instantiate a Dog instead of a Cat. The default path must ignore it.
            var json = $"{{\"$type\":\"{typeof(Dog).AssemblyQualifiedName}\",\"kind\":\"dog\"}}";

            var result = json.FromNJson<Cat>();

            Assert.NotNull(result);
            Assert.Equal("dog", result.Kind);
        }

        [Fact]
        public void FromNJsonBytes_WithDollarTypePayload_IgnoresTypeField()
        {
            var json = $"{{\"$type\":\"{typeof(Dog).AssemblyQualifiedName}\",\"kind\":\"dog\"}}";
            var bytes = Encoding.UTF8.GetBytes(json);

            var result = bytes.FromNJsonBytes<Cat>();

            Assert.NotNull(result);
            Assert.Equal("dog", result.Kind);
        }

        [Fact]
        public void FromNJsonBase64_WithDollarTypePayload_IgnoresTypeField()
        {
            var json = $"{{\"$type\":\"{typeof(Dog).AssemblyQualifiedName}\",\"kind\":\"dog\"}}";
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            var result = base64.FromNJsonBase64<Cat>();

            Assert.NotNull(result);
            Assert.Equal("dog", result.Kind);
        }

        [Fact]
        public void FromNJsonPolymorphic_AllowedType_DeserializesCorrectly()
        {
            var dog = new Dog();
            var json = JsonConvert.SerializeObject(dog, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            var result = json.FromNJsonPolymorphic<Animal>(new DogOnlyBinder());

            Assert.NotNull(result);
            Assert.IsType<Dog>(result);
            Assert.Equal("dog", result.Kind);
        }

        [Fact]
        public void FromNJsonPolymorphic_DisallowedType_ThrowsJsonSerializationException()
        {
            var cat = new Cat();
            var json = JsonConvert.SerializeObject(cat, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            Assert.Throws<JsonSerializationException>(() => json.FromNJsonPolymorphic<Animal>(new DogOnlyBinder()));
        }

        [Fact]
        public void FromNJsonPolymorphic_NullBinder_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => "{\"kind\":\"dog\"}".FromNJsonPolymorphic<Animal>(null!));
        }
    }
}