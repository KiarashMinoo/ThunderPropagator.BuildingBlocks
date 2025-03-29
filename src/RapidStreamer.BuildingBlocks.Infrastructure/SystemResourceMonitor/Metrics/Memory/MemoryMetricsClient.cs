using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;

internal class MemoryMetricsClient
{
    public MemoryMetrics GetMetrics() => IsUnix() ? GetUnixMetrics() : GetWindowsMetrics();

    private static bool IsUnix() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private static MemoryMetrics GetWindowsMetrics()
    {
        var info = new ProcessStartInfo
        {
            FileName = "wmic",
            Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
            RedirectStandardOutput = true
        };

        string output;
        using (var process = Process.Start(info))
        {
            output = process!.StandardOutput.ReadToEnd();
        }

        var lines = output.Trim().Split("\n");
        var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
        var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

        return new MemoryMetrics(
            Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0),
            Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0)
        );
    }

    private static MemoryMetrics GetUnixMetrics()
    {
        var info = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput = true
        };

        string output;
        using (var process = Process.Start(info))
        {
            output = process!.StandardOutput.ReadToEnd();
        }

        var lines = output.Split("\n");
        var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

        return new MemoryMetrics(
            double.Parse(memory[1]),
            double.Parse(memory[3])
        );
    }
}