using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;

/// <summary>
/// Client for collecting disk speed/performance metrics.
/// </summary>
internal sealed class DiskSpeedMetricsClient
{
    private readonly IDiskSpeedProvider _provider = CreatePlatformProvider();

    public DiskSpeedMetrics[] GetMetrics()
    {
        try
        {
            return _provider.GetDiskSpeedMetrics();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error collecting disk speed metrics: {ex.Message}");
            return Array.Empty<DiskSpeedMetrics>();
        }
    }

    private static IDiskSpeedProvider CreatePlatformProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsDiskSpeedProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxDiskSpeedProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsDiskSpeedProvider();

        return new UnsupportedDiskSpeedProvider();
    }
}

/// <summary>
/// Platform-specific disk speed provider interface.
/// </summary>
internal interface IDiskSpeedProvider
{
    DiskSpeedMetrics[] GetDiskSpeedMetrics();
}

/// <summary>
/// Windows disk speed provider using Performance Counters.
/// Note: PerformanceCounter is only available on Windows and requires System.Diagnostics.PerformanceCounter package.
/// </summary>
internal sealed class WindowsDiskSpeedProvider : IDiskSpeedProvider
{
    public DiskSpeedMetrics[] GetDiskSpeedMetrics()
    {
        var metrics = new List<DiskSpeedMetrics>();

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });

            foreach (var drive in drives)
            {
                try
                {
                    // Windows disk speed metrics require System.Diagnostics.PerformanceCounter package
                    // which is Windows-only. This is a placeholder implementation.
                    metrics.Add(new DiskSpeedMetrics
                    {
                        DriveId = drive.Name,
                        PerformanceCountersAvailable = false,
                        ErrorMessage = "Windows disk speed metrics require System.Diagnostics.PerformanceCounter package (Windows-only)"
                    });
                }
                catch (Exception ex)
                {
                    metrics.Add(new DiskSpeedMetrics
                    {
                        DriveId = drive.Name,
                        PerformanceCountersAvailable = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows disk speed provider error: {ex.Message}");
        }

        return metrics.ToArray();
    }
}

/// <summary>
/// Linux disk speed provider using /proc/diskstats.
/// </summary>
internal sealed class LinuxDiskSpeedProvider : IDiskSpeedProvider
{
    public DiskSpeedMetrics[] GetDiskSpeedMetrics()
    {
        var metrics = new List<DiskSpeedMetrics>();

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });

            foreach (var drive in drives)
            {
                try
                {
                    // Linux disk speed metrics require parsing /proc/diskstats
                    // This is a placeholder for the actual implementation
                    metrics.Add(new DiskSpeedMetrics
                    {
                        DriveId = drive.Name,
                        PerformanceCountersAvailable = false,
                        ErrorMessage = "Linux disk speed metrics require /proc/diskstats parsing"
                    });
                }
                catch (Exception ex)
                {
                    metrics.Add(new DiskSpeedMetrics
                    {
                        DriveId = drive.Name,
                        PerformanceCountersAvailable = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Linux disk speed provider error: {ex.Message}");
        }

        return metrics.ToArray();
    }
}

/// <summary>
/// macOS disk speed provider using iostat.
/// </summary>
internal sealed class MacOsDiskSpeedProvider : IDiskSpeedProvider
{
    public DiskSpeedMetrics[] GetDiskSpeedMetrics()
    {
        var metrics = new List<DiskSpeedMetrics>();

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });

            foreach (var drive in drives)
            {
                try
                {
                    metrics.Add(new DiskSpeedMetrics
                    {
                        DriveId = drive.Name,
                        PerformanceCountersAvailable = false,
                        ErrorMessage = "macOS disk speed metrics require iostat parsing"
                    });
                }
                catch (Exception ex)
                {
                    metrics.Add(new DiskSpeedMetrics
                    {
                        DriveId = drive.Name,
                        PerformanceCountersAvailable = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"macOS disk speed provider error: {ex.Message}");
        }

        return metrics.ToArray();
    }
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedDiskSpeedProvider : IDiskSpeedProvider
{
    public DiskSpeedMetrics[] GetDiskSpeedMetrics()
    {
        return Array.Empty<DiskSpeedMetrics>();
    }
}