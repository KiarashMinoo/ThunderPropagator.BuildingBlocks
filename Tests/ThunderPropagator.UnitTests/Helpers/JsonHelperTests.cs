using Xunit;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using ThunderPropagator.BuildingBlocks.Application;
using System.Text.Json;

namespace ThunderPropagator.UnitTests.Helpers
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

        [Fact]
        public void ToJson_ShouldHandleException()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            // Act
            var json = exception.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("InvalidOperationException", json);
            Assert.Contains("Test exception", json);
        }

        [Fact]
        public void FromJson_ShouldHandleTypeParameter()
        {
            // Arrange
            var obj = new TestObject();
            var json = obj.ToJson();

            // Act
            var deserialized = json.FromJson(typeof(TestObject));

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<TestObject>(deserialized);
            var typedObj = (TestObject)deserialized;
            Assert.Equal(obj.Name, typedObj.Name);
            Assert.Equal(obj.Value, typedObj.Value);
        }

        [Fact]
        public void ToJson_WithCustomOptions_ShouldApplyOptions()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var json = obj.ToJson(options => { options.PropertyNamingPolicy = null; return options; });

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Name", json); // PascalCase
            Assert.Contains("Value", json);
        }

        [Fact]
        public void FromJsonBytes_EmptyBytes_ShouldReturnDefault()
        {
            // Arrange
            var emptyBytes = Array.Empty<byte>();

            // Act
            var result = emptyBytes.FromJsonBytes<TestObject>();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJsonBase64_EmptyString_ShouldReturnDefault()
        {
            // Arrange
            var emptyString = string.Empty;

            // Act
            var result = emptyString.FromJsonBase64<TestObject>();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJsonBase64_WhitespaceString_ShouldReturnDefault()
        {
            // Arrange
            var whitespaceString = "   ";

            // Act
            var result = whitespaceString.FromJsonBase64<TestObject>();

            // Assert
            Assert.Null(result);
        }

        [JsonSerialization(CamelCase = false)]
        private class TestObjectWithoutCamelCase
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 42;
        }

        [Fact]
        public void ToJson_WithJsonSerializationAttributeCamelCaseFalse_ShouldUsePascalCase()
        {
            // Arrange
            var obj = new TestObjectWithoutCamelCase();

            // Act
            var json = obj.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("Name", json); // PascalCase
            Assert.Contains("Value", json);
        }

        [Fact]
        public void FromJson_WithJsonSerializationAttributeCamelCaseFalse_ShouldDeserializeCorrectly()
        {
            // Arrange
            var obj = new TestObjectWithoutCamelCase();
            var json = obj.ToJson();

            // Act
            var deserialized = json.FromJson<TestObjectWithoutCamelCase>();

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(obj.Name, deserialized.Name);
            Assert.Equal(obj.Value, deserialized.Value);
        }

        [Fact]
        public void ToJsonBytes_WithException_ShouldSerializeExceptionInfo()
        {
            // Arrange
            var exception = new ArgumentException("Test argument exception");

            // Act
            var bytes = exception.ToJsonBytes();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);

            // Verify it can be deserialized back
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            Assert.Contains("ArgumentException", json);
        }

        [Fact]
        public void ToJsonBase64_WithException_ShouldSerializeExceptionInfo()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            var base64 = exception.ToJsonBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);

            // Verify it can be deserialized back
            var deserialized = base64.FromJsonBase64<ExceptionInfo>();
            Assert.NotNull(deserialized);
            Assert.Equal("System.Exception", deserialized.Type);
            Assert.Equal("Test exception", deserialized.Message);
        }

        [Fact]
        public void FromJson_WithCustomOptions_ShouldApplyOptions()
        {
            // Arrange
            var obj = new TestObject();
            var json = "{\"name\":\"Custom\",\"value\":100}";

            // Act
            var deserialized = json.FromJson<TestObject>(options => { options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; return options; });

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("Custom", deserialized.Name);
            Assert.Equal(100, deserialized.Value);
        }

        [Fact]
        public void FromJsonBytes_WithCustomOptions_ShouldApplyOptions()
        {
            // Arrange
            var json = "{\"name\":\"Custom\",\"value\":100}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Act
            var deserialized = bytes.FromJsonBytes<TestObject>(options => { options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; return options; });

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("Custom", deserialized.Name);
            Assert.Equal(100, deserialized.Value);
        }
    }
}