using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.BuildingBlocks.Application
{
    public static class DispatcherTimer
    {
        public static IDisposable Run<TState>(Func<TState?, bool> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var _ = Task.Run(Start, CancellationToken.None);

            return DisposableObject.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
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

                    if (!action(state)) break;
                }
            }
        }

        public static IDisposable Run(Func<bool> action, TimeSpan interval, CancellationToken cancellationToken = default)
            => Run(_ => action(), interval, cancellationToken);

        public static IDisposable Run<TState>(Func<TState?, CancellationToken, Task<bool>> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var _ = Task.Run(Start, CancellationToken.None);

            return DisposableObject.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
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

                    if (!await action(state, cts.Token)) break;
                }
            }
        }

        public static IDisposable Run(Func<CancellationToken, Task<bool>> action, TimeSpan interval, CancellationToken cancellationToken = default)
            => Run((_, token) => action(token), interval, cancellationToken);

        public static IDisposable RunOnce<TState>(Action<TState?> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(interval, cancellationToken);
                action(state);
            }, cancellationToken);

            return DisposableObject.Create(() =>
            {
                if (task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled)
                    task.Dispose();
            });
        }

        public static IDisposable RunOnce(Action action, TimeSpan interval, CancellationToken cancellationToken = default)
            => RunOnce(_ => action(), interval, cancellationToken);

        public static IDisposable RunOnce<TState>(Func<TState?, CancellationToken, Task> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(interval, cancellationToken);
                await action(state, cancellationToken);
            }, cancellationToken);

            return DisposableObject.Create(() =>
            {
                if (task.Status is TaskStatus.RanToCompletion or TaskStatus.Faulted or TaskStatus.Canceled)
                    task.Dispose();
            });
        }

        public static IDisposable RunOnce(Func<CancellationToken, Task> action, TimeSpan interval, CancellationToken cancellationToken = default)
            => RunOnce((_, token) => action(token), interval, cancellationToken);
    }
}