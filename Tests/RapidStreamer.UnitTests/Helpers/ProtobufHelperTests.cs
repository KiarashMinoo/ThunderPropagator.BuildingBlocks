using Xunit;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using ProtoBuf;

namespace RapidStreamer.UnitTests.Helpers
{
    [ProtoContract]
    internal class TestObject
    {
        [ProtoMember(1)]
        public string Name { get; set; } = "Test";
        [ProtoMember(2)]
        public int Value { get; set; } = 42;
    }

    public class ProtobufHelperTests
    {
        [Fact]
        public void ToProtobuf_ShouldSerializeToStream()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            using var stream = obj.ToProtobuf();

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public void ToProtobufBase64_ShouldSerializeToBase64()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var base64 = obj.ToProtobufBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);
        }

        [Fact]
        public void FromProtobuf_ShouldDeserializeFromStream()
        {
            // Arrange
            var obj = new TestObject();
            using var stream = obj.ToProtobuf();

            // Act
            var deserialized = stream.FromProtobuf<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromProtobuf_ShouldDeserializeFromBytes()
        {
            // Arrange
            var obj = new TestObject();
            using var stream = obj.ToProtobuf();
            var bytes = stream.ToByteArray();

            // Act
            var deserialized = bytes.FromProtobuf<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromProtobufBase64_ShouldDeserializeFromBase64()
        {
            // Arrange
            var obj = new TestObject();
            var base64 = obj.ToProtobufBase64();

            // Act
            var deserialized = base64.FromProtobufBase64<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }
    }
}