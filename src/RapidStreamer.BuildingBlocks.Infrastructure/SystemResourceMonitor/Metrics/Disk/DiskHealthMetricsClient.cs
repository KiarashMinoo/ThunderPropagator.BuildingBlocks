using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;

/// <summary>
/// Client for collecting disk health metrics.
/// </summary>
internal sealed class DiskHealthMetricsClient
{
    private readonly IDiskHealthProvider _provider = CreatePlatformProvider();

    public DiskHealthMetrics[] GetMetrics()
    {
        try
        {
            return _provider.GetDiskHealthMetrics();
        }
        catch (Exception ex)
        {
            // Return empty array on error, individual drives may still report their errors
            Debug.WriteLine($"Error collecting disk health metrics: {ex.Message}");
            return Array.Empty<DiskHealthMetrics>();
        }
    }

    private static IDiskHealthProvider CreatePlatformProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsDiskHealthProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxDiskHealthProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsDiskHealthProvider();

        return new UnsupportedDiskHealthProvider();
    }
}

/// <summary>
/// Platform-specific disk health provider interface.
/// </summary>
internal interface IDiskHealthProvider
{
    DiskHealthMetrics[] GetDiskHealthMetrics();
}

/// <summary>
/// Windows disk health provider using WMI.
/// </summary>
internal sealed class WindowsDiskHealthProvider : IDiskHealthProvider
{
    public DiskHealthMetrics[] GetDiskHealthMetrics()
    {
        var metrics = new List<DiskHealthMetrics>();

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });

            foreach (var drive in drives)
            {
                try
                {
                    // On Windows, we can use WMI to get SMART data
                    // This is a simplified implementation - full SMART requires WMI queries
                    var healthMetric = new DiskHealthMetrics
                    {
                        DriveId = drive.Name,
                        Status = DiskHealthStatus.Unknown,
                        SmartAvailable = false,
                        WearLevelPercent = null,
                        TemperatureCelsius = null,
                        ReallocatedSectorsCount = null,
                        PowerOnHours = null,
                        ErrorMessage = "SMART data collection requires WMI implementation"
                    };

                    metrics.Add(healthMetric);
                }
                catch (Exception ex)
                {
                    metrics.Add(new DiskHealthMetrics
                    {
                        DriveId = drive.Name,
                        Status = DiskHealthStatus.Unknown,
                        SmartAvailable = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Windows disk health provider error: {ex.Message}");
        }

        return metrics.ToArray();
    }
}

/// <summary>
/// Linux disk health provider using smartctl.
/// </summary>
internal sealed class LinuxDiskHealthProvider : IDiskHealthProvider
{
    public DiskHealthMetrics[] GetDiskHealthMetrics()
    {
        var metrics = new List<DiskHealthMetrics>();

        try
        {
            // Try to detect disk devices
            var devices = GetLinuxDiskDevices();

            foreach (var device in devices)
            {
                try
                {
                    var healthMetric = new DiskHealthMetrics
                    {
                        DriveId = device,
                        Status = DiskHealthStatus.Unknown,
                        SmartAvailable = false,
                        ErrorMessage = "SMART data collection requires smartctl installation and permissions"
                    };

                    metrics.Add(healthMetric);
                }
                catch (Exception ex)
                {
                    metrics.Add(new DiskHealthMetrics
                    {
                        DriveId = device,
                        Status = DiskHealthStatus.Unknown,
                        SmartAvailable = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Linux disk health provider error: {ex.Message}");
        }

        return metrics.ToArray();
    }

    private static string[] GetLinuxDiskDevices()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d is { IsReady: true, DriveType: DriveType.Fixed })
                .Select(d => d.Name)
                .ToArray();

            return drives.Length > 0 ? drives : ["/dev/sda"];
        }
        catch
        {
            return ["/dev/sda"];
        }
    }
}

/// <summary>
/// macOS disk health provider using diskutil.
/// </summary>
internal sealed class MacOsDiskHealthProvider : IDiskHealthProvider
{
    public DiskHealthMetrics[] GetDiskHealthMetrics()
    {
        var metrics = new List<DiskHealthMetrics>();

        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d is { IsReady: true, DriveType: DriveType.Fixed });

            foreach (var drive in drives)
            {
                try
                {
                    var healthMetric = new DiskHealthMetrics
                    {
                        DriveId = drive.Name,
                        Status = DiskHealthStatus.Unknown,
                        SmartAvailable = false,
                        ErrorMessage = "SMART data collection requires diskutil smartdata implementation"
                    };

                    metrics.Add(healthMetric);
                }
                catch (Exception ex)
                {
                    metrics.Add(new DiskHealthMetrics
                    {
                        DriveId = drive.Name,
                        Status = DiskHealthStatus.Unknown,
                        SmartAvailable = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"macOS disk health provider error: {ex.Message}");
        }

        return metrics.ToArray();
    }
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedDiskHealthProvider : IDiskHealthProvider
{
    public DiskHealthMetrics[] GetDiskHealthMetrics()
    {
        return Array.Empty<DiskHealthMetrics>();
    }
}