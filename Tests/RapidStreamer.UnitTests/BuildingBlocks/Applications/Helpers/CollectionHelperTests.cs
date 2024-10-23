using RapidStreamer.BuildingBlocks.Application.Helpers;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Helpers
{
    public
#if !DEBUG
        sealed
#endif
        class CollectionHelperTests
    {
        [Fact]
        public void Filter_WithValidArrayAndCondition_ReturnsFilteredArray()
        {
            // Arrange
            var array = new[] { 1, 2, 3, 4, 5 };
            var condition = new Func<int, bool>(x => x % 2 == 0);

            // Act
            var result = array.Filter(condition);

            // Assert
            Assert.Equal(new[] { 2, 4 }, result.ToArray());
        }

        [Fact]
        public void Convert_WithValidArrayAndConverter_ReturnsConvertedArray()
        {
            // Arrange
            var array = new[] { 1, 2, 3, 4, 5 };
            var converter = new Func<int, string>(x => x.ToString());

            // Act
            var result = array.Convert(converter);

            // Assert
            Assert.Equal(new[] { "1", "2", "3", "4", "5" }, result);
        }
    }
}