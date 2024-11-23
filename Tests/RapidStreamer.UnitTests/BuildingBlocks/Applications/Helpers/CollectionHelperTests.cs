using RapidStreamer.BuildingBlocks.Application.Helpers;
using Xunit.Abstractions;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications.Helpers
{
    public
#if !DEBUG
        sealed
#endif
        class CollectionHelperTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CollectionHelperTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

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

        [Theory]
        [InlineData(1, 100, 1)]
        [InlineData(10, 100, 1)]
        [InlineData(100, 100, 1)]
        [InlineData(200, 100, 2)]
        [InlineData(200, 50, 4)]
        public void Splice_Must_Work_Properly(int count, int size, int splittedCount)
        {
            //Arrange
            var array = Enumerable.Range(0, count).Select((_, index) => index).ToArray();

            //Act
            int spliceCount = 0;
            bool gotException = false;
            try
            {
                spliceCount = array.Splice(size).Count();
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine(e.ToString());
                gotException = true;
            }

            //Assert
            Assert.False(gotException);
            Assert.Equal(splittedCount, spliceCount);
        }
    }
}