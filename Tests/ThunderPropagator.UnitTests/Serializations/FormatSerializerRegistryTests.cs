using ThunderPropagator.BuildingBlocks.Application.Serializations;
using ThunderPropagator.BuildingBlocks.Application.Serializations.Json;

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
            ];

            IFormatDeserializer[] deserializers =
            [
                new JsonFormatSerializer(),
                new NJsonFormatSerializer(),
            ];

            return new FormatSerializerRegistry(serializers, deserializers);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetSerializer_ByType_ShouldReturnMatchingSerializer(SerializerType type)
        {
            var registry = BuildRegistry();
            var serializer = registry.GetSerializer(type);
            Assert.NotNull(serializer);
            Assert.Equal(type, serializer.SerializerType);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetDeserializer_ByType_ShouldReturnMatchingDeserializer(SerializerType type)
        {
            var registry = BuildRegistry();
            var deserializer = registry.GetDeserializer(type);
            Assert.NotNull(deserializer);
            Assert.Equal(type, deserializer.SerializerType);
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
