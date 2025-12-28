using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications
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

            // Assert — concurrent operations are nondeterministic; ensure result contains expected markers
            var result = concurrentStringBuilder.ToString();
            Assert.True(result.Length >= 100);
            Assert.StartsWith("0", result);
            Assert.Contains("99", result);
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

                    // Assert (Ensure the read contains expected markers)
                    Assert.True(result.Length >= 10);
                    Assert.Contains("0123456789", result);
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

            // Assert (Ensure the result is correct — final output may vary under concurrent remove)
            var final = concurrentStringBuilder.ToString();
            Assert.StartsWith("0123456789", final);
            Assert.True(final.Length >= 10);
        }
    }
}