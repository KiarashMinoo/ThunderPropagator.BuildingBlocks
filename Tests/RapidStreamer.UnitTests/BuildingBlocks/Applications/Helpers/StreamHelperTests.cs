using RapidStreamer.BuildingBlocks.Application.Helpers;
using System.Text;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Helpers
{
    public
#if !DEBUG
        sealed
#endif
        class StreamHelperTests
    {
        [Fact]
        public void ToByteArray_WithMemoryStream_ReturnsByteArray()
        {
            // Arrange
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
            using var memoryStream = new MemoryStream(expectedBytes);

            // Act
            var result = memoryStream.ToByteArray();

            // Assert
            Assert.Equal(expectedBytes, result);
        }

        [Fact]
        public void ToByteArray_WithNonMemoryStream_ReturnsByteArray()
        {
            // Arrange
            var expectedBytes = new byte[] { 6, 7, 8, 9, 10 };
            using var stream = new MemoryStream(expectedBytes);
            using var nonMemoryStream = new BufferedStream(stream);

            // Act
            var result = nonMemoryStream.ToByteArray();

            // Assert
            Assert.Equal(expectedBytes, result);
        }

        [Fact]
        public void ToStream_WithString_ReturnsMemoryStream()
        {
            // Arrange
            var inputString = "Hello, World!";

            // Act
            var resultStream = inputString.ToStream();

            // Assert
            Assert.IsType<MemoryStream>(resultStream);
            var resultBytes = ((MemoryStream)resultStream).ToArray();
            var expectedBytes = Encoding.UTF8.GetBytes(inputString);
            Assert.Equal(expectedBytes, resultBytes);
        }
    }
}