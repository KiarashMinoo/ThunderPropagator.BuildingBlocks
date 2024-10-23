using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Objects
{
    public
#if !DEBUG
        sealed
#endif
        class CompressedObjectTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithByteArray()
        {
            // Arrange
            byte[] data = [1, 2, 3, 4];

            // Act
            var compressedObject = new CompressedObject(data);

            // Assert
            Assert.Equal(data.Length, compressedObject.Length);
        }

        [Fact]
        public void ToString_ShouldReturnBase64String()
        {
            // Arrange
            byte[] data = [1, 2, 3, 4];
            var compressedObject = new CompressedObject(data);

            // Act
            string result = compressedObject.ToString();

            // Assert
            string expectedBase64String = Convert.ToBase64String(data);
            Assert.Equal(expectedBase64String, result);
        }

        [Fact]
        public void ImplicitConversion_FromBase64String_ShouldCreateCompressedObject()
        {
            // Arrange
            string base64String = Convert.ToBase64String(new byte[] { 5, 6, 7, 8 });

            // Act
            CompressedObject compressedObject = base64String;

            // Assert
            Assert.Equal(4, compressedObject.Length);
        }

        [Fact]
        public void ImplicitConversion_ToBase64String_ShouldConvertCompressedObject()
        {
            // Arrange
            byte[] data = { 9, 10, 11, 12 };
            var compressedObject = new CompressedObject(data);

            // Act
            string result = compressedObject;

            // Assert
            string expectedBase64String = Convert.ToBase64String(data);
            Assert.Equal(expectedBase64String, result);
        }

        [Fact]
        public void ImplicitConversion_FromByteArray_ShouldCreateCompressedObject()
        {
            // Arrange
            byte[] data = { 13, 14, 15, 16 };

            // Act
            CompressedObject compressedObject = data;

            // Assert
            Assert.Equal(data.Length, compressedObject.Length);
        }

        [Fact]
        public void ImplicitConversion_ToByteArray_ShouldReturnOriginalByteArray()
        {
            // Arrange
            byte[] data = { 17, 18, 19, 20 };
            var compressedObject = new CompressedObject(data);

            // Act
            byte[] result = compressedObject;

            // Assert
            Assert.Equal(data, result);
        }
    }
}