using MessagePack;
using ProtoBuf;
using ThunderPropagator.BuildingBlocks.Application.Serializations;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Json;
using ThunderPropagator.BuildingBlocks.Application.Serializations.MessagePack;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Protobuf;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Xml;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml;
using Xunit;

namespace ThunderPropagator.UnitTests.Serializations
{
    // Public POCO required by XML serializer; reused across JSON/NJson/NetJson/Yaml tests too
    public class SimpleTestObject
    {
        public string Name { get; set; } = "Test";
        public int Value { get; set; } = 42;
    }

    // Protobuf requires ProtoContract
    [ProtoContract]
    internal class ProtobufTestObject
    {
        [ProtoMember(1)]
        public string Name { get; set; } = "Test";
        [ProtoMember(2)]
        public int Value { get; set; } = 42;
    }

    // MessagePack requires MessagePackObject
    [MessagePackObject(keyAsPropertyName: false, AllowPrivate = true)]
    internal class MsgPackTestObject
    {
        [Key(0)]
        public string Name { get; set; } = "Test";
        [Key(1)]
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
            Assert.Equal(SerializerType.Json, _serializer.SerializerType);
        }

        [Fact]
        public void MediaType_ShouldBeApplicationJson()
        {
            Assert.Equal(SerializerMediaTypes.Json, _serializer.MediaType);
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
            Assert.Equal(SerializerType.NJson, _serializer.SerializerType);
        }
    }

    public class NetJsonFormatSerializerTests
    {
        private readonly NetJsonFormatSerializer _serializer = new();

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
        public void SerializerType_ShouldBeNetJson()
        {
            Assert.Equal(SerializerType.NetJson, _serializer.SerializerType);
        }
    }

    public class ProtobufFormatSerializerTests
    {
        private readonly ProtobufFormatSerializer _serializer = new();

        [Fact]
        public void Serialize_ShouldProduceBase64String()
        {
            var obj = new ProtobufTestObject();
            var result = _serializer.Serialize(obj);
            Assert.NotNull(result);
            // Base64 string should be decodable
            var bytes = Convert.FromBase64String(result);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void SerializeToBytes_ShouldProduceBytes()
        {
            var obj = new ProtobufTestObject();
            var bytes = _serializer.SerializeToBytes(obj);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void Deserialize_String_ShouldRoundTrip()
        {
            var obj = new ProtobufTestObject();
            var base64 = _serializer.Serialize(obj);
            var result = _serializer.Deserialize<ProtobufTestObject>(base64);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void Deserialize_Bytes_ShouldRoundTrip()
        {
            var obj = new ProtobufTestObject();
            var bytes = _serializer.SerializeToBytes(obj);
            var result = _serializer.Deserialize<ProtobufTestObject>(bytes);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void Deserialize_EmptyBytes_ShouldReturnDefault()
        {
            var result = _serializer.Deserialize<ProtobufTestObject>(Array.Empty<byte>());
            Assert.Null(result);
        }

        [Fact]
        public void SerializerType_ShouldBeProtobuf()
        {
            Assert.Equal(SerializerType.Protobuf, _serializer.SerializerType);
        }

        [Fact]
        public void MediaType_ShouldBeProtobuf()
        {
            Assert.Equal(SerializerMediaTypes.Protobuf, _serializer.MediaType);
        }
    }

    public class MessagePackFormatSerializerTests
    {
        private readonly MessagePackFormatSerializer _serializer = new();

        [Fact]
        public void Serialize_ShouldProduceBase64String()
        {
            var obj = new MsgPackTestObject();
            var result = _serializer.Serialize(obj);
            Assert.NotNull(result);
            var bytes = Convert.FromBase64String(result);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void Deserialize_String_ShouldRoundTrip()
        {
            var obj = new MsgPackTestObject();
            var base64 = _serializer.Serialize(obj);
            var result = _serializer.Deserialize<MsgPackTestObject>(base64);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void Deserialize_Bytes_ShouldRoundTrip()
        {
            var obj = new MsgPackTestObject();
            var bytes = _serializer.SerializeToBytes(obj);
            var result = _serializer.Deserialize<MsgPackTestObject>(bytes);
            Assert.NotNull(result);
            Assert.Equal(obj.Name, result.Name);
            Assert.Equal(obj.Value, result.Value);
        }

        [Fact]
        public void SerializerType_ShouldBeMessagePack()
        {
            Assert.Equal(SerializerType.MessagePack, _serializer.SerializerType);
        }
    }

    public class XmlFormatSerializerTests
    {
        private readonly XmlFormatSerializer _serializer = new();

        [Fact]
        public void Serialize_ShouldProduceXmlString()
        {
            var obj = new SimpleTestObject();
            var result = _serializer.Serialize(obj);
            Assert.NotNull(result);
            Assert.Contains("Test", result);
            Assert.Contains("42", result);
        }

        [Fact]
        public void Deserialize_String_ShouldRoundTrip()
        {
            var obj = new SimpleTestObject();
            var xml = _serializer.Serialize(obj);
            var result = _serializer.Deserialize<SimpleTestObject>(xml);
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
        public void SerializerType_ShouldBeXml()
        {
            Assert.Equal(SerializerType.Xml, _serializer.SerializerType);
        }

        [Fact]
        public void MediaType_ShouldBeXml()
        {
            Assert.Equal(SerializerMediaTypes.Xml, _serializer.MediaType);
        }
    }

    public class YamlFormatSerializerTests
    {
        private readonly YamlFormatSerializer _serializer = new();

        [Fact]
        public void Serialize_ShouldProduceYamlString()
        {
            var obj = new SimpleTestObject();
            var result = _serializer.Serialize(obj);
            Assert.NotNull(result);
            Assert.Contains("name", result);
            Assert.Contains("42", result);
        }

        [Fact]
        public void Deserialize_String_ShouldRoundTrip()
        {
            var obj = new SimpleTestObject();
            var yaml = _serializer.Serialize(obj);
            var result = _serializer.Deserialize<SimpleTestObject>(yaml);
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
        public void SerializerType_ShouldBeYaml()
        {
            Assert.Equal(SerializerType.Yaml, _serializer.SerializerType);
        }

        [Fact]
        public void MediaType_ShouldBeYaml()
        {
            Assert.Equal(SerializerMediaTypes.Yaml, _serializer.MediaType);
        }
    }
}
