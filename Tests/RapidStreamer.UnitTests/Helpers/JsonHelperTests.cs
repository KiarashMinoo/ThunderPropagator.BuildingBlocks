using Xunit;
using RapidStreamer.BuildingBlocks.Application.Helpers;

namespace RapidStreamer.UnitTests.Helpers
{
    public class JsonHelperTests
    {
        private class TestObject
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 42;
        }

        [Fact]
        public void ToJson_ShouldSerializeObject()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var json = obj.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Test", json);
            Assert.Contains("42", json);
        }

        [Fact]
        public void ToJsonBytes_ShouldSerializeToBytes()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var bytes = obj.ToJsonBytes();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void ToJsonBase64_ShouldSerializeToBase64()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var base64 = obj.ToJsonBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);
        }

        [Fact]
        public void FromJson_ShouldDeserializeObject()
        {
            // Arrange
            var obj = new TestObject();
            var json = obj.ToJson();

            // Act
            var deserialized = json.FromJson<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromJsonBytes_ShouldDeserializeFromBytes()
        {
            // Arrange
            var obj = new TestObject();
            var bytes = obj.ToJsonBytes();

            // Act
            var deserialized = bytes.FromJsonBytes<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromJsonBase64_ShouldDeserializeFromBase64()
        {
            // Arrange
            var obj = new TestObject();
            var base64 = obj.ToJsonBase64();

            // Act
            var deserialized = base64.FromJsonBase64<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }
    }
}