using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

/// <summary>
/// Client for collecting CPU temperature metrics.
/// </summary>
internal sealed class CpuTemperatureMetricsClient
{
    private readonly ICpuTemperatureProvider _provider = CreatePlatformProvider();

    public CpuTemperatureMetrics GetMetrics()
    {
        try
        {
            return _provider.GetCpuTemperatureMetrics();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error collecting CPU temperature metrics: {ex.Message}");
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static ICpuTemperatureProvider CreatePlatformProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsCpuTemperatureProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxCpuTemperatureProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsCpuTemperatureProvider();

        return new UnsupportedCpuTemperatureProvider();
    }
}

/// <summary>
/// Platform-specific CPU temperature provider interface.
/// </summary>
internal interface ICpuTemperatureProvider
{
    CpuTemperatureMetrics GetCpuTemperatureMetrics();
}

/// <summary>
/// Windows CPU temperature provider.
/// Requires WMI or OpenHardwareMonitor library for actual implementation.
/// </summary>
internal sealed class WindowsCpuTemperatureProvider : ICpuTemperatureProvider
{
    public CpuTemperatureMetrics GetCpuTemperatureMetrics()
    {
        try
        {
            // Windows CPU temperature requires WMI queries to MSAcpi_ThermalZoneTemperature
            // or third-party libraries like OpenHardwareMonitor/LibreHardwareMonitor
            // This is a placeholder for the actual implementation

            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = "CPU temperature on Windows requires WMI or hardware monitoring library"
            };
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// Linux CPU temperature provider using sysfs.
/// </summary>
internal sealed class LinuxCpuTemperatureProvider : ICpuTemperatureProvider
{
    public CpuTemperatureMetrics GetCpuTemperatureMetrics()
    {
        try
        {
            // Try to read from /sys/class/thermal/thermal_zone*/temp
            var thermalZones = Directory.GetDirectories("/sys/class/thermal", "thermal_zone*");

            if (thermalZones.Length == 0)
            {
                return new CpuTemperatureMetrics
                {
                    TemperatureSensorsAvailable = false,
                    ErrorMessage = "No thermal zones found"
                };
            }

            var coreTemperatures = new Dictionary<int, double>();
            double? maxTemp = null;
            var totalTemp = 0.0;
            var validReadings = 0;

            for (int i = 0; i < thermalZones.Length; i++)
            {
                try
                {
                    var tempFile = Path.Combine(thermalZones[i], "temp");
                    if (File.Exists(tempFile))
                    {
                        var tempStr = File.ReadAllText(tempFile).Trim();
                        if (int.TryParse(tempStr, out var tempMilliC))
                        {
                            var tempC = tempMilliC / 1000.0;
                            coreTemperatures[i] = tempC;
                            totalTemp += tempC;
                            validReadings++;

                            if (!maxTemp.HasValue || tempC > maxTemp.Value)
                                maxTemp = tempC;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading thermal zone {i}: {ex.Message}");
                }
            }

            if (validReadings == 0)
            {
                return new CpuTemperatureMetrics
                {
                    TemperatureSensorsAvailable = false,
                    ErrorMessage = "Could not read any temperature values"
                };
            }

            return new CpuTemperatureMetrics
            {
                PackageTemperatureCelsius = maxTemp,
                CoreTemperatures = coreTemperatures,
                MaxTemperatureCelsius = maxTemp,
                AverageTemperatureCelsius = totalTemp / validReadings,
                TemperatureSensorsAvailable = true
            };
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// macOS CPU temperature provider using IOKit.
/// </summary>
internal sealed class MacOsCpuTemperatureProvider : ICpuTemperatureProvider
{
    public CpuTemperatureMetrics GetCpuTemperatureMetrics()
    {
        try
        {
            // macOS CPU temperature requires IOKit framework calls
            // or using smc command line tool
            // This is a placeholder for the actual implementation

            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = "CPU temperature on macOS requires IOKit or smc tool"
            };
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedCpuTemperatureProvider : ICpuTemperatureProvider
{
    public CpuTemperatureMetrics GetCpuTemperatureMetrics()
    {
        return new CpuTemperatureMetrics
        {
            TemperatureSensorsAvailable = false,
            ErrorMessage = "CPU temperature not supported on this platform"
        };
    }
}