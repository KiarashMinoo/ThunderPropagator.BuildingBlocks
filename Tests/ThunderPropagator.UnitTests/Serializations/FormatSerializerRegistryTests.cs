using ThunderPropagator.BuildingBlocks.Application.Serializations;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Json;
using ThunderPropagator.BuildingBlocks.Application.Serializations.MessagePack;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Protobuf;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Xml;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml;
using Xunit;

namespace ThunderPropagator.UnitTests.Serializations
{
    public class FormatSerializerRegistryTests
    {
        private static FormatSerializerRegistry BuildRegistry()
        {
            IFormatSerializer[] serializers =
            [
                new JsonFormatSerializer(),
                new NJsonFormatSerializer(),
                new NetJsonFormatSerializer(),
                new ProtobufFormatSerializer(),
                new MessagePackFormatSerializer(),
                new XmlFormatSerializer(),
                new YamlFormatSerializer(),
            ];

            IFormatDeserializer[] deserializers =
            [
                new JsonFormatSerializer(),
                new NJsonFormatSerializer(),
                new NetJsonFormatSerializer(),
                new ProtobufFormatSerializer(),
                new MessagePackFormatSerializer(),
                new XmlFormatSerializer(),
                new YamlFormatSerializer(),
            ];

            return new FormatSerializerRegistry(serializers, deserializers);
        }

        [Theory]
        [InlineData(SerializerType.Json)]
        [InlineData(SerializerType.NJson)]
        [InlineData(SerializerType.NetJson)]
        [InlineData(SerializerType.Protobuf)]
        [InlineData(SerializerType.MessagePack)]
        [InlineData(SerializerType.Xml)]
        [InlineData(SerializerType.Yaml)]
        public void GetSerializer_ByType_ShouldReturnMatchingSerializer(SerializerType type)
        {
            var registry = BuildRegistry();
            var serializer = registry.GetSerializer(type);
            Assert.NotNull(serializer);
            Assert.Equal(type, serializer.SerializerType);
        }

        [Theory]
        [InlineData(SerializerType.Json)]
        [InlineData(SerializerType.NJson)]
        [InlineData(SerializerType.NetJson)]
        [InlineData(SerializerType.Protobuf)]
        [InlineData(SerializerType.MessagePack)]
        [InlineData(SerializerType.Xml)]
        [InlineData(SerializerType.Yaml)]
        public void GetDeserializer_ByType_ShouldReturnMatchingDeserializer(SerializerType type)
        {
            var registry = BuildRegistry();
            var deserializer = registry.GetDeserializer(type);
            Assert.NotNull(deserializer);
            Assert.Equal(type, deserializer.SerializerType);
        }

        [Theory]
        [InlineData(SerializerMediaTypes.Json)]
        [InlineData(SerializerMediaTypes.Protobuf)]
        [InlineData(SerializerMediaTypes.MessagePack)]
        [InlineData(SerializerMediaTypes.Xml)]
        [InlineData(SerializerMediaTypes.Yaml)]
        public void GetSerializer_ByMediaType_ShouldReturnSerializer(string mediaType)
        {
            var registry = BuildRegistry();
            var serializer = registry.GetSerializer(mediaType);
            Assert.NotNull(serializer);
            Assert.Equal(mediaType, serializer.MediaType, StringComparer.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(SerializerMediaTypes.Json)]
        [InlineData(SerializerMediaTypes.Protobuf)]
        [InlineData(SerializerMediaTypes.MessagePack)]
        [InlineData(SerializerMediaTypes.Xml)]
        [InlineData(SerializerMediaTypes.Yaml)]
        public void GetDeserializer_ByMediaType_ShouldReturnDeserializer(string mediaType)
        {
            var registry = BuildRegistry();
            var deserializer = registry.GetDeserializer(mediaType);
            Assert.NotNull(deserializer);
            Assert.Equal(mediaType, deserializer.MediaType, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetSerializer_ByJsonMediaType_ShouldReturnJsonFormatSerializer()
        {
            var registry = BuildRegistry();
            var serializer = registry.GetSerializer(SerializerMediaTypes.Json);
            Assert.IsType<JsonFormatSerializer>(serializer);
        }

        [Fact]
        public void GetDeserializer_ByJsonMediaType_ShouldReturnJsonFormatSerializer()
        {
            var registry = BuildRegistry();
            var deserializer = registry.GetDeserializer(SerializerMediaTypes.Json);
            Assert.IsType<JsonFormatSerializer>(deserializer);
        }

        [Fact]
        public void GetSerializer_UnknownType_ShouldThrow()
        {
            var registry = BuildRegistry();
            Assert.Throws<InvalidOperationException>(() => registry.GetSerializer((SerializerType)99));
        }

        [Fact]
        public void GetDeserializer_UnknownType_ShouldThrow()
        {
            var registry = BuildRegistry();
            Assert.Throws<InvalidOperationException>(() => registry.GetDeserializer((SerializerType)99));
        }

        [Fact]
        public void GetSerializer_UnknownMediaType_ShouldThrow()
        {
            var registry = BuildRegistry();
            Assert.Throws<InvalidOperationException>(() => registry.GetSerializer("application/unknown"));
        }

        [Fact]
        public void GetDeserializer_UnknownMediaType_ShouldThrow()
        {
            var registry = BuildRegistry();
            Assert.Throws<InvalidOperationException>(() => registry.GetDeserializer("application/unknown"));
        }

        [Fact]
        public void Constructor_NullSerializers_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FormatSerializerRegistry(null!, Array.Empty<IFormatDeserializer>()));
        }

        [Fact]
        public void Constructor_NullDeserializers_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FormatSerializerRegistry(Array.Empty<IFormatSerializer>(), null!));
        }
    }
}
