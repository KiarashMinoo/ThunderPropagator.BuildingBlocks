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
        // /proc/meminfo is the kernel memory interface — named fields are stable
        // across all Linux distros and procps versions, no column layout assumptions needed.
        const string procMeminfo = "/proc/meminfo";
        if (!File.Exists(procMeminfo))
            return new MemoryMetrics(0, 0);

        var content = await File.ReadAllTextAsync(procMeminfo, cancellationToken).ConfigureAwait(false);
        long totalKb = 0, availableKb = 0, freeKb = 0;

        foreach (var line in content.Split('\n'))
        {
            var sep = line.IndexOf(':');
            if (sep < 0) continue;

            var key = line[..sep].Trim();
            var valueStr = line[(sep + 1)..].Trim();
            // Each value is "<number> kB"; strip the unit before parsing.
            var spaceIdx = valueStr.IndexOf(' ');
            var numStr = spaceIdx > 0 ? valueStr[..spaceIdx] : valueStr;

            if (key == "MemTotal" && long.TryParse(numStr, out var total)) totalKb = total;
            else if (key == "MemAvailable" && long.TryParse(numStr, out var avail)) availableKb = avail;
            else if (key == "MemFree" && long.TryParse(numStr, out var free)) freeKb = free;
        }

        // MemAvailable accounts for reclaimable buffers/cache; fall back to MemFree on older kernels.
        var effectiveFreeKb = availableKb > 0 ? availableKb : freeKb;
        return new MemoryMetrics(
            Math.Round(totalKb / 1024.0, 0),
            Math.Round(effectiveFreeKb / 1024.0, 0)
        );
    }
}