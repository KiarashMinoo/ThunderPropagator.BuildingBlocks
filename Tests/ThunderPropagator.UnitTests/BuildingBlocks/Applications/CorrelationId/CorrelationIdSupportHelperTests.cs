using ThunderPropagator.BuildingBlocks.Application.CorrelationId;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.CorrelationId
{
    public
#if !DEBUG
        sealed
#endif
        class CorrelationIdSupportHelperTests
    {
        [Fact]
        public void GenerateCorrelationId_WithNonNullInput_SetsCorrelationId()
        {
            // Arrange
            var input = new TestClass();

            // Act
            var result = CorrelationIdSupportHelper.GenerateCorrelationId(input);

            // Assert
            Assert.NotNull(result.CorrelationId);
            Assert.NotEmpty(result.CorrelationId);
        }

        [Fact]
        public void SetCorrelationId_WithValidCorrelationId_SetsCorrelationId()
        {
            // Arrange
            var input = new TestClass();
            var correlationId = "test-correlation-id";

            // Act
            var result = input.SetCorrelationId(correlationId);

            // Assert
            Assert.Equal(correlationId, result.CorrelationId);
        }

        [Fact]
        public void GenerateCorrelationId_WithNonNullInput_ReturnsSameInstance()
        {
            // Arrange
            var input = new TestClass();

            // Act
            var result = CorrelationIdSupportHelper.GenerateCorrelationId(input);

            // Assert
            Assert.Same(input, result);
        }

        [Fact]
        public void SetCorrelationId_WithValidCorrelationId_ReturnsSameInstance()
        {
            // Arrange
            var input = new TestClass();
            var correlationId = "test-correlation-id";

            // Act
            var result = input.SetCorrelationId(correlationId);

            // Assert
            Assert.Same(input, result);
        }

        private class TestClass : ICorrelationIdSupport
        {
            public string CorrelationId { get; set; } = null!;
        }
    }
}