using Xunit;
using RapidStreamer.BuildingBlocks.Application.Helpers;

namespace RapidStreamer.UnitTests.Helpers
{
    public class YamlHelperTests
    {
        private class TestObject
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 42;
        }

        [Fact]
        public void ToYaml_ShouldSerializeObject()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var yaml = obj.ToYaml();

            // Assert
            Assert.NotNull(yaml);
            Assert.Contains("Test", yaml);
            Assert.Contains("42", yaml);
        }

        [Fact]
        public void ToYamlBytes_ShouldSerializeToBytes()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var bytes = obj.ToYamlBytes();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void ToYamlBase64_ShouldSerializeToBase64()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var base64 = obj.ToYamlBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);
        }

        [Fact]
        public void FromYaml_ShouldDeserializeObject()
        {
            // Arrange
            var obj = new TestObject();
            var yaml = obj.ToYaml();

            // Act
            var deserialized = yaml.FromYaml<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromYamlBytes_ShouldDeserializeFromBytes()
        {
            // Arrange
            var obj = new TestObject();
            var bytes = obj.ToYamlBytes();

            // Act
            var deserialized = bytes.FromYamlBytes<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void FromYamlBase64_ShouldDeserializeFromBase64()
        {
            // Arrange
            var obj = new TestObject();
            var base64 = obj.ToYamlBase64();

            // Act
            var deserialized = base64.FromYamlBase64<TestObject>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }
    }
}