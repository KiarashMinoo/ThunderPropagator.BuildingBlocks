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
            return new MemoryMetrics(0, 0);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        long freeKb = 0, totalKb = 0;
        foreach (var line in output.Split('\n'))
        {
            var trimmed = line.Trim();
            var sep = trimmed.IndexOf('=');
            if (sep < 0) continue;

            var key = trimmed[..sep].Trim();
            var valueStr = trimmed[(sep + 1)..].Trim();

            if (key.Equals("FreePhysicalMemory", StringComparison.OrdinalIgnoreCase) && long.TryParse(valueStr, out var free))
                freeKb = free;
            else if (key.Equals("TotalVisibleMemorySize", StringComparison.OrdinalIgnoreCase) && long.TryParse(valueStr, out var total))
                totalKb = total;
        }

        return new MemoryMetrics(
            Math.Round(totalKb / 1024.0, 0),
            Math.Round(freeKb / 1024.0, 0)
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

        var memLine = output.Split('\n').FirstOrDefault(l => l.StartsWith("Mem:", StringComparison.OrdinalIgnoreCase));
        if (memLine == null)
            return new MemoryMetrics(0, 0);

        var memory = memLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (memory.Length < 4 || !double.TryParse(memory[1], out var total) || !double.TryParse(memory[3], out var free))
            return new MemoryMetrics(0, 0);

        return new MemoryMetrics(total, free);
    }
}