using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications
{
    public
#if !DEBUG
        sealed
#endif
        class InconvertibleExceptionTests
    {
        [Fact]
        public void TestInconvertibleException()
        {
            // Arrange
            var sourceType = typeof(int);
            var destinationType = typeof(string);

            // Act and Assert
            var exception = Assert.Throws<InconvertibleException>(() =>
            {
                InconvertibleException.ThrowIfInconvertible(() => ConversionFails(sourceType, destinationType),
                    $"Conversion from {sourceType} to {destinationType} is expected to fail.");
            });

            // You can further assert the exception message or check other details if needed
            Assert.Contains(sourceType.ToString(), exception.Message);
            Assert.Contains(destinationType.ToString(), exception.Message);
        }

        // Simulate a condition where conversion fails
        private bool ConversionFails(Type sourceType, Type destinationType)
        {
            // You may implement your logic here that causes the conversion failure
            return false; // For this example, always return false
        }
    }
}