using System.Diagnostics;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

public interface ICpuMetricsClient : IMetricsClient<CpuMetrics>
{
    Task<CpuMetrics> GetMetricsAsync(long window, bool all = false, CancellationToken cancellationToken = default);
}

internal sealed class CpuMetricsClient : ICpuMetricsClient
{
    private readonly long _window;
    private readonly bool _all;

    public CpuMetricsClient()
    {
    }

    public CpuMetricsClient(long window, bool all = false) : this()
    {
        _window = window;
        _all = all;
    }

    public Task<CpuMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return GetMetricsAsync(_window, _all, cancellationToken);
    }

    public async Task<CpuMetrics> GetMetricsAsync(long window, bool all = false, CancellationToken cancellationToken = default)
    {
        var samplingWindowMs = window <= 0 ? 1 : window;

        using var currentProcess = Process.GetCurrentProcess();

        var processorCount = Environment.ProcessorCount;

        // Enumerate processes once, keep them alive for the duration of the sampling window,
        // then dispose them in a finally.
        Process[]? processes;
        long processesCount;

        try
        {
            processes = Process.GetProcesses();
            processesCount = processes.LongLength;
        }
        catch
        {
            processes = null;
            processesCount = 1;
        }

        long currentProcessThreads;
        try
        {
            currentProcessThreads = currentProcess.Threads.Count;
        }
        catch
        {
            currentProcessThreads = 0;
        }

        long totalThreads = 0;
        if (processes is not null)
        {
            foreach (var p in processes)
            {
                try
                {
                    totalThreads += p.Threads.Count;
                }
                catch
                {
                    // ignore
                }
            }
        }
        else
        {
            totalThreads = currentProcessThreads;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startCpuMs = GetTotalCpuMilliseconds(all, currentProcess, processes);
            var sw = Stopwatch.StartNew();

            // True async wait that supports cancellation.
            await Task.Delay(TimeSpan.FromMilliseconds(samplingWindowMs), cancellationToken).ConfigureAwait(false);

            sw.Stop();
            var endCpuMs = GetTotalCpuMilliseconds(all, currentProcess, processes);

            var elapsedMs = Math.Max(1.0, sw.Elapsed.TotalMilliseconds);
            var cpuUsedMs = Math.Max(0.0, endCpuMs - startCpuMs);

            // Normalize: 100% == one full core.
            var usage = cpuUsedMs / (processorCount * elapsedMs) * 100.0;
            if (double.IsNaN(usage) || double.IsInfinity(usage))
                usage = 0;
            else
                usage = Math.Clamp(usage, 0.0, 100.0);

            return new CpuMetrics(
                ProcessorCount: processorCount,
                Usage: usage,
                Threads: currentProcessThreads,
                Processes: processesCount,
                TotalThreads: totalThreads);
        }
        finally
        {
            if (processes is not null)
            {
                foreach (var p in processes)
                {
                    try
                    {
                        p.Dispose();
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }
    }

    private static double GetTotalCpuMilliseconds(bool all, Process currentProcess, Process[]? processes)
    {
        if (!all)
        {
            try
            {
                return currentProcess.TotalProcessorTime.TotalMilliseconds;
            }
            catch
            {
                return 0;
            }
        }

        if (processes is null)
        {
            try
            {
                return currentProcess.TotalProcessorTime.TotalMilliseconds;
            }
            catch
            {
                return 0;
            }
        }

        double sum = 0;
        foreach (var p in processes)
        {
            try
            {
                sum += p.TotalProcessorTime.TotalMilliseconds;
            }
            catch
            {
                // ignore
            }
        }

        return sum;
    }
}