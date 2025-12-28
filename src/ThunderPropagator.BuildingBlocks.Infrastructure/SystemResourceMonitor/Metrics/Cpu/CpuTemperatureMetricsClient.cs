using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

public interface ICpuTemperatureMetricsClient : IMetricsClient<CpuTemperatureMetrics>;

/// <summary>
/// Client for collecting CPU temperature metrics.
/// </summary>
internal sealed class CpuTemperatureMetricsClient : ICpuTemperatureMetricsClient
{
    private readonly ICpuTemperatureProvider _provider;

    internal CpuTemperatureMetricsClient(ICpuTemperatureProvider provider)
    {
        _provider = provider;
    }

    public CpuTemperatureMetricsClient() : this(CreatePlatformProvider())
    {
    }

    public async Task<CpuTemperatureMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _provider.GetCpuTemperatureMetricsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
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

    internal static ICpuTemperatureProvider CreatePlatformProvider()
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
    Task<CpuTemperatureMetrics> GetCpuTemperatureMetricsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Windows CPU temperature provider.
/// Uses WMI queries to MSAcpi_ThermalZoneTemperature for actual implementation.
/// </summary>
internal sealed class WindowsCpuTemperatureProvider : ICpuTemperatureProvider
{
    public async Task<CpuTemperatureMetrics> GetCpuTemperatureMetricsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await GetWindowsCpuTemperatureAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
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

    private static async Task<CpuTemperatureMetrics> GetWindowsCpuTemperatureAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try WMI first
            var wmiResult = await TryWmiTemperatureAsync(cancellationToken);
            if (wmiResult != null)
            {
                return wmiResult;
            }

            // If WMI fails, try alternative methods
            // Note: On Windows, thermal monitoring often requires:
            // 1. Hardware support (thermal sensors)
            // 2. Proper drivers
            // 3. Administrative privileges for some sensors

            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = "CPU temperature sensors not available. This may be due to: " +
                               "1) Hardware without thermal sensors, " +
                               "2) Missing drivers, " +
                               "3) Virtualized environment, " +
                               "4) Insufficient permissions"
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = $"Windows CPU temperature collection error: {ex.Message}"
            };
        }
    }

    private static async Task<CpuTemperatureMetrics?> TryWmiTemperatureAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use WMI to query thermal zone temperatures
            var psi = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "path MSAcpi_ThermalZoneTemperature get CurrentTemperature /format:list",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return null;
            }

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            // Parse WMI output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var temperatures = new List<double>();

            foreach (var line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2 && parts[0].Trim() == "CurrentTemperature")
                {
                    if (int.TryParse(parts[1].Trim(), out var tempKelvin))
                    {
                        // WMI returns temperature in Kelvin * 10 (tenths of Kelvin)
                        var tempCelsius = (tempKelvin / 10.0) - 273.15;
                        temperatures.Add(tempCelsius);
                    }
                }
            }

            if (temperatures.Count == 0)
            {
                return null;
            }

            // Create core temperatures dictionary (simplified - all sensors treated as cores)
            var coreTemperatures = new Dictionary<int, double>();
            for (int i = 0; i < temperatures.Count; i++)
            {
                coreTemperatures[i] = temperatures[i];
            }

            var maxTemp = temperatures.Max();
            var avgTemp = temperatures.Average();

            return new CpuTemperatureMetrics
            {
                PackageTemperatureCelsius = maxTemp,
                CoreTemperatures = coreTemperatures,
                MaxTemperatureCelsius = maxTemp,
                AverageTemperatureCelsius = avgTemp,
                TemperatureSensorsAvailable = true
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Linux CPU temperature provider using sysfs.
/// </summary>
internal sealed class LinuxCpuTemperatureProvider : ICpuTemperatureProvider
{
    public async Task<CpuTemperatureMetrics> GetCpuTemperatureMetricsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var thermalZones = Directory.GetDirectories("/sys/class/thermal", "thermal_zone*");

            if (thermalZones.Length == 0)
            {
                return new CpuTemperatureMetrics
                {
                    TemperatureSensorsAvailable = false,
                    ErrorMessage = "No thermal zones found in /sys/class/thermal."
                };
            }

            var coreTemperatures = new Dictionary<int, double>();
            var temperatures = new List<double>();

            foreach (var (zonePath, index) in thermalZones.Select((path, i) => (path, i)))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var type = await File.ReadAllTextAsync(Path.Combine(zonePath, "type"), cancellationToken).ConfigureAwait(false);

                // We are interested in CPU temperatures, so we look for 'x86_pkg_temp' or similar identifiers.
                if (!type.Contains("pkg") && !type.Contains("core")) continue;

                var tempFile = Path.Combine(zonePath, "temp");
                if (!File.Exists(tempFile)) continue;

                var tempStr = await File.ReadAllTextAsync(tempFile, cancellationToken).ConfigureAwait(false);
                if (int.TryParse(tempStr.Trim(), out var tempMilliC))
                {
                    var tempC = tempMilliC / 1000.0;
                    coreTemperatures[index] = tempC;
                    temperatures.Add(tempC);
                }
            }

            if (temperatures.Count == 0)
            {
                return new CpuTemperatureMetrics
                {
                    TemperatureSensorsAvailable = false,
                    ErrorMessage = "Could not read any CPU temperature values from thermal zones."
                };
            }

            return new CpuTemperatureMetrics
            {
                PackageTemperatureCelsius = temperatures.Max(),
                CoreTemperatures = coreTemperatures,
                MaxTemperatureCelsius = temperatures.Max(),
                AverageTemperatureCelsius = temperatures.Average(),
                TemperatureSensorsAvailable = true
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = $"An error occurred while collecting CPU temperature on Linux: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// macOS CPU temperature provider using IOKit.
/// </summary>
internal sealed partial class MacOsCpuTemperatureProvider : ICpuTemperatureProvider
{
    public async Task<CpuTemperatureMetrics> GetCpuTemperatureMetricsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // On macOS, we can use the `smc` command-line tool to get sensor readings.
            var result = await TryGetTemperatureFromSmcAsync(cancellationToken);
            return result;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics
            {
                TemperatureSensorsAvailable = false,
                ErrorMessage = $"An error occurred while collecting CPU temperature on macOS: {ex.Message}"
            };
        }
    }

    private static async Task<CpuTemperatureMetrics> TryGetTemperatureFromSmcAsync(CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "smc",
            Arguments = "-k TC0P -r", // TC0P is a common key for CPU temperature on Intel Macs
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
            {
                return new CpuTemperatureMetrics { TemperatureSensorsAvailable = false, ErrorMessage = "Failed to start 'smc' process." };
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return new CpuTemperatureMetrics { TemperatureSensorsAvailable = false, ErrorMessage = "'smc' command failed or returned no output. It might not be installed or accessible." };
            }

            // Example output: "  TC0P  [sp78]  45.125"
            var match = SmcOutputRegex().Match(output);
            if (match.Success && double.TryParse(match.Value, out var tempC))
            {
                return new CpuTemperatureMetrics
                {
                    PackageTemperatureCelsius = tempC,
                    CoreTemperatures = new Dictionary<int, double> { { 0, tempC } }, // SMC often gives a single package temperature
                    MaxTemperatureCelsius = tempC,
                    AverageTemperatureCelsius = tempC,
                    TemperatureSensorsAvailable = true
                };
            }

            return new CpuTemperatureMetrics { TemperatureSensorsAvailable = false, ErrorMessage = "Failed to parse temperature from 'smc' output." };
        }
        catch (Exception ex)
        {
            return new CpuTemperatureMetrics { TemperatureSensorsAvailable = false, ErrorMessage = $"Failed to execute 'smc' command. Ensure it is in the system's PATH. Error: {ex.Message}" };
        }
    }

    [GeneratedRegex(@"\d+\.\d+")]
    private static partial Regex SmcOutputRegex();
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedCpuTemperatureProvider : ICpuTemperatureProvider
{
    public Task<CpuTemperatureMetrics> GetCpuTemperatureMetricsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new CpuTemperatureMetrics
        {
            TemperatureSensorsAvailable = false,
            ErrorMessage = "CPU temperature metrics not supported on this platform"
        });
    }
}