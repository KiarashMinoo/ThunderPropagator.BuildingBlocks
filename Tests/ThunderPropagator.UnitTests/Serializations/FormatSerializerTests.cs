using ThunderPropagator.BuildingBlocks.Application.Serializations;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Json;

namespace ThunderPropagator.UnitTests.Serializations
{
    // Public POCO required by XML serializer; reused across JSON/NJson/NetJson/Yaml tests too
    public class SimpleTestObject
    {
        public string Name { get; set; } = "Test";
        public int Value { get; set; } = 42;
    }

    // XML requires default constructor and public settable properties (satisfied by SimpleTestObject)

    public class JsonFormatSerializerTests
    {
        private readonly JsonFormatSerializer _serializer = new();

        [Fact]
        public void Serialize_ShouldProduceJsonString()
        {
            var obj = new SimpleTestObject();
            var result = _serializer.Serialize(obj);
            Assert.NotNull(result);
            Assert.Contains("name", result);
            Assert.Contains("42", result);
        }

        [Fact]
        public void SerializeToBytes_ShouldProduceUtf8Bytes()
        {
            var obj = new SimpleTestObject();
            var bytes = _serializer.SerializeToBytes(obj);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void Deserialize_String_ShouldRoundTrip()
        {
            var obj = new SimpleTestObject();
            var json = _serializer.Serialize(obj);
            var result = _serializer.Deserialize<SimpleTestObject>(json);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void Deserialize_Bytes_ShouldRoundTrip()
        {
            var obj = new SimpleTestObject();
            var bytes = _serializer.SerializeToBytes(obj);
            var result = _serializer.Deserialize<SimpleTestObject>(bytes);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void Deserialize_EmptyString_ShouldReturnDefault()
        {
            var result = _serializer.Deserialize<SimpleTestObject>(string.Empty);
            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_EmptyBytes_ShouldReturnDefault()
        {
            var result = _serializer.Deserialize<SimpleTestObject>(Array.Empty<byte>());
            Assert.Null(result);
        }

        [Fact]
        public void SerializerType_ShouldBeJson()
        {
            Assert.Equal(JsonFormatSerializer.Json, _serializer.SerializerType);
        }
    }

    public class NJsonFormatSerializerTests
    {
        private readonly NJsonFormatSerializer _serializer = new();

        [Fact]
        public void Serialize_ShouldProduceJsonString()
        {
            var obj = new SimpleTestObject();
            var result = _serializer.Serialize(obj);
            Assert.NotNull(result);
            Assert.Contains("42", result);
        }

        [Fact]
        public void Deserialize_String_ShouldRoundTrip()
        {
            var obj = new SimpleTestObject();
            var json = _serializer.Serialize(obj);
            var result = _serializer.Deserialize<SimpleTestObject>(json);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void Deserialize_Bytes_ShouldRoundTrip()
        {
            var obj = new SimpleTestObject();
            var bytes = _serializer.SerializeToBytes(obj);
            var result = _serializer.Deserialize<SimpleTestObject>(bytes);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void SerializerType_ShouldBeNJson()
        {
            Assert.Equal(NJsonFormatSerializer.NJson, _serializer.SerializerType);
        }
    }
}
