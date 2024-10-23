using RapidStreamer.BuildingBlocks.Application;

namespace RapidStreamer.UnitTests.BuildingBlocks.Applications
{
    public
#if !DEBUG
        sealed
#endif
        class ConcurrentStringBuilderConcurrencyTests
    {
        [Fact]
        public void ConcurrentAppend_CorrectResult()
        {
            // Arrange
            var concurrentStringBuilder = new ConcurrentStringBuilder();

            // Act
            Parallel.For(0, 100, i => { concurrentStringBuilder.Append(i.ToString()); });

            // Assert
            Assert.Equal("0123456789" + string.Concat(Enumerable.Range(10, 90)), concurrentStringBuilder.ToString());
        }

        [Fact]
        public void ConcurrentAppendWithConcurrentRead_CorrectResult()
        {
            // Arrange
            var concurrentStringBuilder = new ConcurrentStringBuilder();

            // Act
            Parallel.Invoke(
                () => { Parallel.For(0, 100, i => { concurrentStringBuilder.Append(i.ToString()); }); },
                () =>
                {
                    // Wait for the first thread to finish appending
                    Thread.Sleep(500);

                    // Concurrently read the StringBuilder
                    var result = concurrentStringBuilder.ToString();

                    // Assert (Ensure the read is correct)
                    Assert.Equal("0123456789" + string.Concat(Enumerable.Range(10, 90)), result);
                });

            // Additional Assert (Ensure the append is correct)
            Assert.Equal("0123456789" + string.Concat(Enumerable.Range(10, 90)), concurrentStringBuilder.ToString());
        }

        [Fact]
        public void ConcurrentAppendWithConcurrentRemove_CorrectResult()
        {
            // Arrange
            var concurrentStringBuilder = new ConcurrentStringBuilder();

            // Act
            Parallel.Invoke(
                () => { Parallel.For(0, 100, i => { concurrentStringBuilder.Append(i.ToString()); }); },
                () =>
                {
                    // Wait for the first thread to finish appending
                    Thread.Sleep(500);

                    // Concurrently remove a portion of the StringBuilder
                    concurrentStringBuilder.Remove(10, 90);
                });

            // Assert (Ensure the result is correct)
            Assert.Equal("0123456789", concurrentStringBuilder.ToString());
        }
    }
}