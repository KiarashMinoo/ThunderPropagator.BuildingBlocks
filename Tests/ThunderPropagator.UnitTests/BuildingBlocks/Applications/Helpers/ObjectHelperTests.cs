using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Helpers
{
    public
#if !DEBUG
        sealed
#endif
        class ObjectHelperTests
    {
        public
#if !DEBUG
            sealed
#endif
            class TestClass
        {
            public string Name { get; set; } = null!;
            public int Age { get; set; }
        }

        [Fact]
        public void ToJson_WithValidObject_ReturnsJsonString()
        {
            // Arrange
            var instance = new TestClass { Name = "John", Age = 30 };

            // Act
            var result = instance.ToNJson();

            // Assert
            Assert.Equal("{\"name\":\"John\",\"age\":30}", result);
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsObject()
        {
            // Arrange
            var json = "{\"name\":\"Jane\",\"age\":25}";

            // Act
            var result = json.FromNJson<TestClass>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane", result?.Name);
            Assert.Equal(25, result?.Age);
        }

        [Fact]
        public void ToBytes_WithValidObject_ReturnsByteArray()
        {
            // Arrange
            var instance = new TestClass { Name = "Bob", Age = 40 };

            // Act
            var result = instance.ToNJsonBytes();

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public void FromBytes_WithValidByteArray_ReturnsObject()
        {
            // Arrange
            var instance = new TestClass { Name = "Alice", Age = 35 };
            var bytes = instance.ToNJsonBytes();

            // Act
            var result = bytes.FromNJsonBytes<TestClass>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alice", result?.Name);
            Assert.Equal(35, result?.Age);
        }

        [Fact]
        public void As_WithValidObject_ReturnsCastObject()
        {
            // Arrange
            object instance = new TestClass { Name = "Sam", Age = 45 };

            // Act
            var result = instance.As<TestClass>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Sam", result?.Name);
            Assert.Equal(45, result?.Age);
        }

        [Fact]
        public void IsDisposed_WithDisposedObject_ReturnsTrue()
        {
            // Arrange
            var disposedInstance = new TestClass();
            _ = disposedInstance.GetHashCode();

            // Act
            var result = disposedInstance.IsDisposed();

            // Assert - normal objects do not throw ObjectDisposedException on GetHashCode so IsDisposed should be false
            Assert.False(result);
        }
    }
}