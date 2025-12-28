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
    }
}