using RapidStreamer.BuildingBlocks.Application.Helpers;
using System.Text;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Helpers
{
    public
#if !DEBUG
        sealed
#endif
        class StringHelperTests
    {
        [Fact]
        public void ToByteArray_WithString_ReturnsByteArray()
        {
            // Arrange
            var inputString = "Hello, World!";

            // Act
            var resultBytes = inputString.ToByteArray();

            // Assert
            var expectedBytes = Encoding.UTF8.GetBytes(inputString);
            Assert.Equal(expectedBytes, resultBytes);
        }
    }
}