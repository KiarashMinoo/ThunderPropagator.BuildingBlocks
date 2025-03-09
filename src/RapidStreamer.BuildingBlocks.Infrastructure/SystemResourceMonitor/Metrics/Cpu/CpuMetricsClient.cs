using System.Diagnostics;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

internal class CpuMetricsClient
{
    public CpuMetrics GetMetrics(long window, bool all = false)
    {
        var resetEvent = new ManualResetEvent(false);

        double cpuUsageTotal = 0;
        Task.Run(async () =>
        {
            // Start watching CPU
            var startTime = DateTime.UtcNow;
            var startCpuUsage = all
                ? Process.GetProcesses().Sum(p => p.TotalProcessorTime.TotalMilliseconds)
                : Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
            var stopWatch = Stopwatch.StartNew();

            // Measure something else, such as .Net Core Middleware
            await Task.Delay(TimeSpan.FromMilliseconds(window));

            // Stop watching to measure
            stopWatch.Stop();
            var endTime = DateTime.UtcNow;
            var endCpuUsage = all
                ? Process.GetProcesses().Sum(p => p.TotalProcessorTime.TotalMilliseconds)
                : Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;

            var cpuUsedMs = endCpuUsage - startCpuUsage;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            resetEvent.Set();
        });

        resetEvent.WaitOne();

        return new CpuMetrics(Environment.ProcessorCount, cpuUsageTotal * 100);
    }
}