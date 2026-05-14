using ThunderPropagator.BuildingBlocks.Application;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications
{
    public
#if !DEBUG
        sealed
#endif
        class DispatcherTimerTests
    {
        [Fact]
        public async Task Run_ShouldInvokeCallback_MultipleTimes()
        {
            var count = 0;
            using var timer = DispatcherTimer.Run(() => { count++; return true; }, TimeSpan.FromMilliseconds(20));

            await Task.Delay(120);

            Assert.True(count >= 3);
        }

        [Fact]
        public async Task Run_ShouldStop_WhenDisposed()
        {
            var count = 0;
            var timer = DispatcherTimer.Run(() => { count++; return true; }, TimeSpan.FromMilliseconds(20));

            await Task.Delay(60);
            timer.Dispose();
            var countAfterDispose = count;

            await Task.Delay(60);

            Assert.True(count - countAfterDispose <= 1);
        }

        [Fact]
        public async Task Run_ShouldStop_WhenCallbackReturnsFalse()
        {
            var count = 0;
            using var timer = DispatcherTimer.Run(() => { count++; return count < 2; }, TimeSpan.FromMilliseconds(20));

            await Task.Delay(200);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task Run_ShouldStop_WhenCancellationRequested()
        {
            using var cts = new CancellationTokenSource();
            var count = 0;
            using var timer = DispatcherTimer.Run<int>(_ => { count++; return true; }, TimeSpan.FromMilliseconds(20), 0, cts.Token);

            await Task.Delay(60);
            cts.Cancel();
            var countAfterCancel = count;

            await Task.Delay(60);

            Assert.True(count - countAfterCancel <= 1);
        }

        [Fact]
        public async Task Run_WithState_ShouldPassStateToCallback()
        {
            var received = new List<int>();
            using var timer = DispatcherTimer.Run<int>(state => { received.Add(state); return received.Count < 3; }, TimeSpan.FromMilliseconds(20), 42);

            await Task.Delay(200);

            Assert.All(received, v => Assert.Equal(42, v));
            Assert.Equal(3, received.Count);
        }

        [Fact]
        public async Task RunAsync_ShouldInvokeCallback_MultipleTimes()
        {
            var count = 0;
            using var timer = DispatcherTimer.Run(async ct =>
            {
                await Task.Yield();
                count++;
                return true;
            }, TimeSpan.FromMilliseconds(20));

            await Task.Delay(120);

            Assert.True(count >= 3);
        }

        [Fact]
        public async Task RunAsync_ShouldStop_WhenCallbackReturnsFalse()
        {
            var count = 0;
            using var timer = DispatcherTimer.Run(async ct =>
            {
                await Task.Yield();
                count++;
                return count < 2;
            }, TimeSpan.FromMilliseconds(20));

            await Task.Delay(200);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task RunAsync_ShouldStop_WhenCancellationRequested()
        {
            using var cts = new CancellationTokenSource();
            var count = 0;
            using var timer = DispatcherTimer.Run<int>(async (_, ct) =>
            {
                await Task.Yield();
                count++;
                return true;
            }, TimeSpan.FromMilliseconds(20), 0, cts.Token);

            await Task.Delay(60);
            cts.Cancel();
            var countAfterCancel = count;

            await Task.Delay(60);

            Assert.True(count - countAfterCancel <= 1);
        }

        [Fact]
        public async Task RunAsync_WithState_ShouldPassStateToCallback()
        {
            var received = new List<int>();
            using var timer = DispatcherTimer.Run<int>(async (state, ct) =>
            {
                await Task.Yield();
                received.Add(state);
                return received.Count < 3;
            }, TimeSpan.FromMilliseconds(20), 99);

            await Task.Delay(200);

            Assert.All(received, v => Assert.Equal(99, v));
            Assert.Equal(3, received.Count);
        }
    }
}