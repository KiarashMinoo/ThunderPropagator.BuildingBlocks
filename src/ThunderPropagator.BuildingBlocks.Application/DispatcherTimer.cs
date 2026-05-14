using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.BuildingBlocks.Application
{
    public static class DispatcherTimer
    {
        public static IDisposable Run<TState>(Func<TState?, bool> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task? backgroundTask = null;

            backgroundTask = Task.Run(Start, CancellationToken.None);

            return DisposableObject.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
                // Observe task for faults
                if (backgroundTask?.IsFaulted == true)
                {
                    _ = backgroundTask.Exception; // Observe the exception
                }
            });

            async Task Start()
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(interval, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    try
                    {
                        if (!action(state)) break;
                    }
                    catch
                    {
                        // Exception in action - decide whether to continue or break
                        // For now, we rethrow to surface the error
                        throw;
                    }
                }
            }
        }

        public static IDisposable Run(Func<bool> action, TimeSpan interval, CancellationToken cancellationToken = default)
            => Run(_ => action(), interval, cancellationToken);

        public static IDisposable Run<TState>(Func<TState?, CancellationToken, Task<bool>> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task? backgroundTask = null;

            backgroundTask = Task.Run(Start, CancellationToken.None);

            return DisposableObject.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
                // Observe task for faults
                if (backgroundTask?.IsFaulted == true)
                {
                    _ = backgroundTask.Exception; // Observe the exception
                }
            });

            async Task Start()
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(interval, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    try
                    {
                        if (!await action(state, cts.Token)) break;
                    }
                    catch
                    {
                        // Exception in action - decide whether to continue or break
                        // For now, we rethrow to surface the error
                        throw;
                    }
                }
            }
        }

        public static IDisposable Run(Func<CancellationToken, Task<bool>> action, TimeSpan interval, CancellationToken cancellationToken = default)
            => Run((_, token) => action(token), interval, cancellationToken);

        public static IDisposable RunOnce<TState>(Action<TState?> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(interval, cts.Token);
                    action(state);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation is expected
                }
            }, cts.Token);

            return DisposableObject.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
                if (task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled)
                    task.Dispose();
            });
        }

        public static IDisposable RunOnce(Action action, TimeSpan interval, CancellationToken cancellationToken = default)
            => RunOnce(_ => action(), interval, cancellationToken);

        public static IDisposable RunOnce<TState>(Func<TState?, CancellationToken, Task> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(interval, cts.Token);
                    await action(state, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation is expected
                }
            }, cts.Token);

            return DisposableObject.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
                if (task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled)
                    task.Dispose();
            });
        }

        public static IDisposable RunOnce(Func<CancellationToken, Task> action, TimeSpan interval, CancellationToken cancellationToken = default)
            => RunOnce((_, token) => action(token), interval, cancellationToken);
    }
}