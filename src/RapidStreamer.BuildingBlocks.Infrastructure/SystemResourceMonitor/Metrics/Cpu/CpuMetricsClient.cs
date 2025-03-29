using System.Diagnostics;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

internal class CpuMetricsClient
{
    public CpuMetrics GetMetrics(long window, bool all = false)
    {
        var resetEvent = new ManualResetEvent(false);

        long processorCount = Environment.ProcessorCount;
        double usage = 0;
        long threadsCount = 0;
        long processesCount = 0;
        long totalThreadsCount = 0;
        Task.Run(async () =>
        {
            var process = Process.GetCurrentProcess();
            var processes = Process.GetProcesses();
            threadsCount = process.Threads.Count;
            processesCount = processes.Length;
            totalThreadsCount = processes.Sum(p =>
            {
                try
                {
                    return p.Threads.Count;
                }
                catch
                {
                    return 0;
                }
            });

            // Start watching CPU
            var startTime = DateTime.UtcNow;
            var startCpuUsage = CpuUsage();
            var stopWatch = Stopwatch.StartNew();

            // Measure something else, such as .Net Core Middleware
            await Task.Delay(TimeSpan.FromMilliseconds(window));

            // Stop watching to measure
            stopWatch.Stop();
            var endTime = DateTime.UtcNow;
            var endCpuUsage = CpuUsage();

            var cpuUsedMs = endCpuUsage - startCpuUsage;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            usage = cpuUsedMs / (processorCount * totalMsPassed);

            resetEvent.Set();

            return;

            double CpuUsage() => all
                ? processes.Sum(p =>
                {
                    try
                    {
                        return p.TotalProcessorTime.TotalMilliseconds;
                    }
                    catch
                    {
                        return 0;
                    }
                })
                : process.TotalProcessorTime.TotalMilliseconds;
        });

        resetEvent.WaitOne();

        return new CpuMetrics(processesCount, usage * 100, threadsCount, processesCount, totalThreadsCount);
    }
}