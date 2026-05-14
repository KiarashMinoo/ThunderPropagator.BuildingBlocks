namespace ThunderPropagator.BuildingBlocks.Application.Objects
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

            protected override bool IsDisposed => base.IsDisposed && _disposeAction == null;

            public AnonymousDisposable(Action disposeAction) => _disposeAction = disposeAction;

            protected override void DisposeManagedResources() => Interlocked.Exchange(ref _disposeAction, null)?.Invoke();
        }

        internal sealed class AnonymousDisposable<TState> : DisposableObject
        {
            private TState _state;
            private volatile Action<TState>? _disposeAction;

            protected override bool IsDisposed => base.IsDisposed && _disposeAction == null;

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

        protected virtual bool IsDisposing { get; private set; }
        protected virtual bool IsDisposed { get; private set; }

        public event EventHandler? Disposing;
        public event EventHandler? Disposed;

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
            IsDisposing = disposing;

            if (!IsDisposed)
            {
                if (IsDisposing)
                {
                    OnDisposing();
                    DisposeManagedResources();
                }

                ReleaseUnmanagedResources();
                SetLargeFieldsAsNull();

                IsDisposed = true;
                OnDisposed();
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
            DisposeManagedResources();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     free unmanaged resources(unmanaged objects) async
        /// </summary>
        protected virtual ValueTask ReleaseUnmanagedResourcesAsync()
        {
            ReleaseUnmanagedResources();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        ///     set large fields to null async
        /// </summary>
        protected virtual ValueTask SetLargeFieldsAsNullAsync()
        {
            SetLargeFieldsAsNull();
            return ValueTask.CompletedTask;
        }

        private async ValueTask DisposeAsync(bool disposing)
        {
            IsDisposing = disposing;

            if (!IsDisposed)
            {
                if (IsDisposing)
                {
                    OnDisposing();
                    await DisposeManagedResourcesAsync();
                }

                await ReleaseUnmanagedResourcesAsync();
                await SetLargeFieldsAsNullAsync();

                IsDisposed = true;
                OnDisposed();
            }
        }

        #endregion

        protected virtual void OnDisposing()
        {
            Disposing?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisposed()
        {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public static IDisposable Empty => EmptyDisposable.Instance;

        public static IDisposable Create(Action disposeAction) => new AnonymousDisposable(disposeAction ?? throw new ArgumentNullException(nameof(disposeAction)));

        public static IDisposable Create<TState>(TState state, Action<TState> disposeAction)
            => new AnonymousDisposable<TState>(state, disposeAction ?? throw new ArgumentNullException(nameof(disposeAction)));
    }
}