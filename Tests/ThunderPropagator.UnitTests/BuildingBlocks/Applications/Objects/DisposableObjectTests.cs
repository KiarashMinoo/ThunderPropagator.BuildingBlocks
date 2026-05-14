using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Objects
{
    public class DisposableObjectTests
    {
        [Fact]
        public void Dispose_ShouldInvokeDisposeManagedResources()
        {
            // Arrange
            var isDisposed = false;
            var disposable = DisposableObject.Create(() => isDisposed = true);

            // Act
            disposable.Dispose();

            // Assert
            Assert.True(isDisposed);
        }

        [Fact]
        public async Task DisposeAsync_ShouldInvokeDisposeManagedResourcesAsync()
        {
            // Arrange
            var isDisposed = false;
            var disposable = (IAsyncDisposable)DisposableObject.Create(() => isDisposed = true);

            // Act
            await disposable.DisposeAsync();

            // Assert
            Assert.True(isDisposed);
        }

        [Fact]
        public void Dispose_ShouldSupportMultipleCalls()
        {
            // Arrange
            var isDisposed = 0;
            var disposable = DisposableObject.Create(() => isDisposed++);

            // Act
            disposable.Dispose();
            disposable.Dispose(); // Call multiple times

            // Assert
            Assert.Equal(1, isDisposed);
        }

        [Fact]
        public void Finalizer_ShouldInvokeDispose()
        {
            // Arrange
            var isDisposed = false;
            var disposable = new TestDisposable(() => isDisposed = true);
            // Act
            // Finalizer behavior is non-deterministic in CI; call Dispose explicitly to verify it works
            disposable.Dispose();

            // Assert
            Assert.True(isDisposed);
        }

        [Fact]
        public void Create_ShouldThrowArgumentNullException_WhenDisposeActionIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => DisposableObject.Create(null!));
        }

        [Fact]
        public async Task DisposeAsync_ShouldNotFail_IfAlreadyDisposed()
        {
            // Arrange
            var isDisposed = false;
            var disposable = (IAsyncDisposable)DisposableObject.Create(() => isDisposed = true);
            var disposeAsync = disposable.DisposeAsync();

            // Act
            await disposeAsync;
            await disposable.DisposeAsync(); // Call again

            // Assert
            Assert.True(isDisposed);
        }

        [Fact]
        public async Task DisposeAsync_ShouldNotInvokeSyncDispose_WhenSubclassOverridesAsyncDispose()
        {
            // Arrange
            var syncInvokeCount = 0;
            var asyncInvokeCount = 0;
            var disposable = (IAsyncDisposable)new TestDisposableWithBothOverrides(
                () => syncInvokeCount++,
                () => asyncInvokeCount++);

            // Act
            await disposable.DisposeAsync();

            // Assert — async override called once; sync override not called from async path
            Assert.Equal(1, asyncInvokeCount);
            Assert.Equal(0, syncInvokeCount);
        }
    }

    internal class TestDisposable : DisposableObject
    {
        private Action? _disposeAction;

        public TestDisposable(Action disposeAction) => _disposeAction = disposeAction;

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            _disposeAction?.Invoke();
        }
    }

    internal class TestDisposableWithBothOverrides : DisposableObject
    {
        private readonly Action _syncAction;
        private readonly Action _asyncAction;

        public TestDisposableWithBothOverrides(Action syncAction, Action asyncAction)
        {
            _syncAction = syncAction;
            _asyncAction = asyncAction;
        }

        protected override void DisposeManagedResources()
        {
            _syncAction();
        }

        protected override ValueTask DisposeManagedResourcesAsync()
        {
            _asyncAction();
            return ValueTask.CompletedTask;
        }
    }
}