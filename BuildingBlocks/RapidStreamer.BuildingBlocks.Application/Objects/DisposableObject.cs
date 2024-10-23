namespace RapidStreamer.BuildingBlocks.Application.Objects
{
    public abstract class DisposableObject : EquatableObject,
        IDisposable,
        IAsyncDisposable
    {
        private sealed class EmptyDisposable : DisposableObject
        {
            public static readonly EmptyDisposable Instance = new();

            private EmptyDisposable()
            {
            }
        }

        internal sealed class AnonymousDisposable : DisposableObject
        {
            private volatile Action? _disposeAction;

            protected override bool Disposed => base.Disposed && _disposeAction == null;

            public AnonymousDisposable(Action disposeAction) => _disposeAction = disposeAction;

            protected override void DisposeManagedResources() => Interlocked.Exchange(ref _disposeAction, null)?.Invoke();
        }

        internal sealed class AnonymousDisposable<TState> : DisposableObject
        {
            private TState _state;
            private volatile Action<TState>? _disposeAction;

            protected override bool Disposed => base.Disposed && _disposeAction == null;

            public AnonymousDisposable(TState state, Action<TState> disposeAction)
            {
                _state = state;
                _disposeAction = disposeAction;
            }

            protected override void DisposeManagedResources()
            {
                Interlocked.Exchange(ref _disposeAction, null)?.Invoke(_state);
                _state = default!;
            }
        }

        protected virtual bool Disposing { get; private set; }
        protected virtual bool Disposed { get; private set; }

        ~DisposableObject()
        {
            Dispose(false);
        }

        #region "IDisposable"

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     dispose managed state(managed objects)
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            // TODO: dispose managed state(managed objects)    
        }

        /// <summary>
        ///     free unmanaged resources(unmanaged objects)
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
            // TODO: free unmanaged resources(unmanaged objects)
        }

        /// <summary>
        ///     set large fields to null
        /// </summary>
        protected virtual void SetLargeFieldsAsNull()
        {
            // TODO: set large fields to null
        }

        private void Dispose(bool disposing)
        {
            Disposing = disposing;

            if (!Disposed)
            {
                if (Disposing)
                {
                    DisposeManagedResources();
                }

                ReleaseUnmanagedResources();
                SetLargeFieldsAsNull();

                Disposed = true;
            }
        }

        #endregion

        #region "IAsyncDisposable"

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     dispose managed state(managed objects) async
        /// </summary>
        protected virtual ValueTask DisposeManagedResourcesAsync()
        {
            // TODO: dispose managed state(managed objects)
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     free unmanaged resources(unmanaged objects) async
        /// </summary>
        protected virtual ValueTask ReleaseUnmanagedResourcesAsync()
        {
            // TODO: free unmanaged resources(unmanaged objects)
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     set large fields to null async
        /// </summary>
        protected virtual ValueTask SetLargeFieldsAsNullAsync()
        {
            // TODO: set large fields to null
            return ValueTask.CompletedTask;
        }

        private async ValueTask DisposeAsync(bool disposing)
        {
            Disposing = disposing;

            if (!Disposed)
            {
                if (Disposing)
                {
                    await DisposeManagedResourcesAsync();
                }

                await ReleaseUnmanagedResourcesAsync();
                await SetLargeFieldsAsNullAsync();

                Disposed = true;
            }
        }

        #endregion

        public static IDisposable Empty => EmptyDisposable.Instance;

        public static IDisposable Create(Action disposeAction) => new AnonymousDisposable(disposeAction ?? throw new ArgumentNullException(nameof(disposeAction)));

        public static IDisposable Create<TState>(TState state, Action<TState> disposeAction)
            => new AnonymousDisposable<TState>(state, disposeAction ?? throw new ArgumentNullException(nameof(disposeAction)));
    }
}