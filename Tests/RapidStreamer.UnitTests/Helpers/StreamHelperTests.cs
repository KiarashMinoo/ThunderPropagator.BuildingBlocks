using Xunit;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.BuildingBlocks.Application.Objects;
using System.Text;

namespace RapidStreamer.UnitTests.Helpers
{
    public class StreamHelperTests
    {
        [Fact]
        public void ToByteArray_ShouldConvertStreamToBytes()
        {
            // Arrange
            var data = "Test data";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            // Act
            var bytes = stream.ToByteArray();

            // Assert
            Assert.NotNull(bytes);
            Assert.Equal(data, Encoding.UTF8.GetString(bytes));
        }

        [Fact]
        public void ToStream_ShouldConvertStringToStream()
        {
            // Arrange
            var data = "Test data";

            // Act
            using var stream = data.ToStream();
            using var reader = new StreamReader(stream);

            // Assert
            var result = reader.ReadToEnd();
            Assert.Equal(data, result);
        }

        [Fact]
        public void DecompressStream_ShouldDecompressCompressedObject()
        {
            // Arrange
            var data = "Test data";
            var compressed = data.ToNJsonBytes().Compress();

            // Act
            using var decompressedStream = compressed.DecompressStream();

            // Assert
            Assert.NotNull(decompressedStream);
            Assert.True(decompressedStream.Length > 0);
        }
    }
}