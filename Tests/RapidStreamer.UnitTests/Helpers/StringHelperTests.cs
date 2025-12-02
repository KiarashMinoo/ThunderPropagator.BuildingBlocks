using Xunit;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.UnitTests.Helpers
{
    public class StringHelperTests
    {
        [Fact]
        public void ToByteArray_ShouldConvertStringToBytes()
        {
            // Arrange
            var data = "Test data";

            // Act
            var bytes = data.ToByteArray();

            // Assert
            Assert.NotNull(bytes);
            Assert.True(bytes.Length > 0);
        }

        [Fact]
        public void ToByteReadOnlyMemory_ShouldConvertStringToReadOnlyMemory()
        {
            // Arrange
            var data = "Test data";

            // Act
            var memory = data.ToByteReadOnlyMemory();

            // Assert
            Assert.True(memory.Length > 0);
        }

        [Fact]
        public void FromByteArray_ShouldConvertBytesToString()
        {
            // Arrange
            var data = "Test data";
            var bytes = data.ToByteArray();

            // Act
            var result = bytes.FromByteArray();

            // Assert
            Assert.Equal(data, result);
        }

        [Fact]
        public void ToBase64_ShouldConvertStringToBase64()
        {
            // Arrange
            var data = "Test data";

            // Act
            var base64 = data.ToBase64();

            // Assert
            Assert.NotNull(base64);
            Assert.True(base64.Length > 0);
        }

        [Fact]
        public void FromBase64_ShouldConvertBase64ToString()
        {
            // Arrange
            var data = "Test data";
            var base64 = data.ToBase64();

            // Act
            var result = base64.FromBase64();

            // Assert
            Assert.Equal(data, result);
        }

        [Fact]
        public void DecompressString_ShouldDecompressCompressedObject()
        {
            // Arrange
            var data = "Test data";
            var compressed = data.ToNJsonBytes().Compress();

            // Act
            var result = compressed.DecompressString();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}