using ThunderPropagator.BuildingBlocks.Application.CorrelationId;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.CorrelationId
{
    public
#if !DEBUG
        sealed
#endif
        class CorrelationIdProviderTests
    {
        [Fact]
        public void GenerateCorrelationId_WithNonNullInput_ReturnsValidCorrelationId()
        {
            // Arrange
            var input = new TestClass();

            // Act
            var correlationId = input.GenerateCorrelationId();

            // Assert
            Assert.NotNull(correlationId);
            Assert.NotEmpty(correlationId);
        }

        [Fact]
        public void GenerateCorrelationId_WithDifferentInputs_ReturnsDifferentCorrelationIds()
        {
            // Arrange
            var input1 = new TestClass();
            var input2 = new TestClass();

            // Act
            var correlationId1 = input1.GenerateCorrelationId();
            var correlationId2 = input2.GenerateCorrelationId();

            // Assert
            Assert.NotEqual(correlationId1, correlationId2);
        }

        [Fact]
        public void GenerateCorrelationId_WithSameInput_ReturnsSameCorrelationId()
        {
            // Arrange
            var input = new TestClass();

            // Act
            var correlationId1 = input.GenerateCorrelationId();
            var correlationId2 = input.GenerateCorrelationId();

            // Assert
            Assert.Equal(correlationId1, correlationId2);
        }

        private class TestClass
        {
            // TryAdd properties or methods if needed for the test cases
        }
    }
}