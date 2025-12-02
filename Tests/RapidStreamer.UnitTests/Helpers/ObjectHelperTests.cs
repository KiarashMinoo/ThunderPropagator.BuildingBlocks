using Xunit;
using RapidStreamer.BuildingBlocks.Application.Helpers;
using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.UnitTests.Helpers
{
    public class ObjectHelperTests
    {
        private class TestObject
        {
            public string Name { get; set; } = "Test";
            public int Value { get; set; } = 42;
        }

        [Fact]
        public void EquatableEqual_ShouldReturnTrueForEqualObjects()
        {
            // Arrange
            var obj1 = new TestObject();
            var obj2 = new TestObject();

            // Act
            var result = obj1.EquatableEqual(obj2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EquatableHashCode_ShouldReturnSameHashForEqualObjects()
        {
            // Arrange
            var obj1 = new TestObject();
            var obj2 = new TestObject();

            // Act
            var hash1 = obj1.EquatableHashCode();
            var hash2 = obj2.EquatableHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void As_ShouldCastToType()
        {
            // Arrange
            object obj = new TestObject();

            // Act
            var result = obj.As<TestObject>();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestObject>(result);
        }

        [Fact]
        public void Clone_ShouldCreateDeepCopy()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var cloned = obj.Clone();

            // Assert
            Assert.NotNull(cloned);
            Assert.NotSame(obj, cloned);
            Assert.Equal(obj.Name, ((TestObject)cloned).Name);
            Assert.Equal(obj.Value, ((TestObject)cloned).Value);
        }

        [Fact]
        public void IsDisposed_ShouldReturnFalseForNonDisposedObject()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var result = obj.IsDisposed();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Compress_ShouldCompressObject()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var compressed = obj.Compress();

            // Assert
            Assert.NotNull(compressed);
            Assert.True(compressed.Length > 0);
        }

        [Fact]
        public void Decompress_ShouldDecompressObject()
        {
            // Arrange
            var obj = new TestObject();
            var compressed = obj.Compress();

            // Act
            var decompressed = compressed.Decompress<TestObject>();

            // Assert
            Assert.NotNull(decompressed);
            Assert.Equal(obj.Name, decompressed.Name);
            Assert.Equal(obj.Value, decompressed.Value);
        }

        [Fact]
        public void ToSafeString_ShouldReturnStringRepresentation()
        {
            // Arrange
            var obj = new TestObject();

            // Act
            var result = obj.ToSafeString();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}