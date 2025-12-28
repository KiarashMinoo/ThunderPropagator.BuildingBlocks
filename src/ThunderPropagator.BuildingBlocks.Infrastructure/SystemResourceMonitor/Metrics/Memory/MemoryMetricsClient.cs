using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;

internal interface IMemoryMetricsClient : IMetricsClient<MemoryMetrics>;

internal sealed class MemoryMetricsClient : IMemoryMetricsClient
{
    public async Task<MemoryMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        return IsUnix()
            ? await GetUnixMetricsAsync(cancellationToken).ConfigureAwait(false)
            : await GetWindowsMetricsAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool IsUnix() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private static async Task<MemoryMetrics> GetWindowsMetricsAsync(CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo
        {
            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(info);
        if (process == null)
        {
            return new MemoryMetrics(0, 0);
        }

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        var lines = output.Trim().Split("\n");
        var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

        return new MemoryMetrics(
            Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0),
            Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0)
        );
    }

    private static async Task<MemoryMetrics> GetUnixMetricsAsync(CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(info);
        if (process == null)
        {
            return new MemoryMetrics(0, 0);
        }

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        var lines = output.Split("\n");
        var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

        return new MemoryMetrics(
            double.Parse(memory[1]),
            double.Parse(memory[3])
        );
    }
}