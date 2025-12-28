using Xunit;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.UnitTests.Helpers
{
    public class NetJsonHelperTests
    {
        private class TestObject
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 42;
        }

        [Fact]
        public void ToNetJson_ShouldSerializeObject()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var json = obj.ToNetJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Test", json);
            Assert.Contains("42", json);
        }

        [Fact]
        public void ToNetJsonBytes_ShouldSerializeToBytes()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var bytes = obj.ToNetJsonBytes();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void ToNetJsonBase64_ShouldSerializeToBase64()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var base64 = obj.ToNetJsonBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);
        }

        [Fact]
        public void FromNetJson_ShouldDeserializeObject()
        {
            // Arrange
            var obj = new TestObject();
            var json = obj.ToNetJson();

            // Act
            var deserialized = json.FromNetJson<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromNetJsonBytes_ShouldDeserializeFromBytes()
        {
            // Arrange
            var obj = new TestObject();
            var bytes = obj.ToNetJsonBytes();

            // Act
            var deserialized = bytes.FromNetJsonBytes<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromNetJsonBase64_ShouldDeserializeFromBase64()
        {
            // Arrange
            var obj = new TestObject();
            var base64 = obj.ToNetJsonBase64();

            // Act
            var deserialized = base64.FromNetJsonBase64<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }
    }
}