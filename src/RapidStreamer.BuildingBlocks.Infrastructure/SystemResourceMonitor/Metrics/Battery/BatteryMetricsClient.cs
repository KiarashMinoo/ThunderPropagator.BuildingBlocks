using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;

/// <summary>
/// Client for collecting battery metrics.
/// </summary>
internal sealed class BatteryMetricsClient
{
    private readonly IBatteryMetricsProvider _provider = CreatePlatformProvider();

    public BatteryMetrics GetMetrics()
    {
        try
        {
            return _provider.GetBatteryMetrics();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error collecting battery metrics: {ex.Message}");
            return new BatteryMetrics
            {
                BatteryPresent = false,
                Status = BatteryStatus.Unknown,
                OnACPower = true,
                ErrorMessage = ex.Message
            };
        }
    }

    private static IBatteryMetricsProvider CreatePlatformProvider()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsBatteryMetricsProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxBatteryMetricsProvider();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOsBatteryMetricsProvider();

        return new UnsupportedBatteryMetricsProvider();
    }
}

/// <summary>
/// Platform-specific battery metrics provider interface.
/// </summary>
internal interface IBatteryMetricsProvider
{
    BatteryMetrics GetBatteryMetrics();
}

/// <summary>
/// Windows battery metrics provider using WMI queries.
/// </summary>
internal sealed class WindowsBatteryMetricsProvider : IBatteryMetricsProvider
{
    public BatteryMetrics GetBatteryMetrics()
    {
        try
        {
            // Use WMIC to query battery status
            var batteryInfo = GetWindowsBatteryInfo();

            return batteryInfo;
        }
        catch (Exception ex)
        {
            return new BatteryMetrics
            {
                BatteryPresent = false,
                Status = BatteryStatus.Unknown,
                OnACPower = true,
                ErrorMessage = ex.Message
            };
        }
    }

    private static BatteryMetrics GetWindowsBatteryInfo()
    {
        try
        {
            // Use WMIC to query battery status (available on Windows without additional packages)
            var psi = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "path Win32_Battery get BatteryStatus,EstimatedChargeRemaining,EstimatedRunTime /format:csv",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true,
                    ErrorMessage = "Could not start wmic process"
                };
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrWhiteSpace(output) || !output.Contains("EstimatedChargeRemaining"))
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

            // Parse CSV output
            var dataLine = lines[^1].Trim();
            var parts = dataLine.Split(',');

            if (parts.Length < 3)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = true,
                    Status = BatteryStatus.Unknown,
                    OnACPower = true,
                    ErrorMessage = "Unable to parse battery data"
                };
            }

            var batteryStatusCode = parts.Length > 1 && int.TryParse(parts[1], out var code) ? code : 0;
            var chargePercent = parts.Length > 2 && double.TryParse(parts[2], out var charge) ? (double?)charge : null;
            var remainingMinutes = parts.Length > 3 && int.TryParse(parts[3], out var minutes) ? (int?)minutes : null;

            var status = batteryStatusCode switch
            {
                1 => BatteryStatus.Discharging,
                2 => BatteryStatus.Charging,
                3 => BatteryStatus.Full,
                _ => BatteryStatus.Unknown
            };

            return new BatteryMetrics
            {
                BatteryPresent = true,
                ChargePercent = chargePercent,
                Status = status,
                RemainingTimeMinutes = remainingMinutes,
                OnACPower = status == BatteryStatus.Charging || status == BatteryStatus.Full
            };
        }
        catch (Exception ex)
        {
            return new BatteryMetrics
            {
                BatteryPresent = false,
                Status = BatteryStatus.Unknown,
                OnACPower = true,
                ErrorMessage = $"Windows battery detection error: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Linux battery metrics provider using /sys/class/power_supply.
/// </summary>
internal sealed class LinuxBatteryMetricsProvider : IBatteryMetricsProvider
{
    public BatteryMetrics GetBatteryMetrics()
    {
        try
        {
            var powerSupplyPath = "/sys/class/power_supply";

            if (!Directory.Exists(powerSupplyPath))
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true,
                    ErrorMessage = "/sys/class/power_supply not found"
                };
            }

            // Find battery directory (usually BAT0, BAT1, etc.)
            var batteryDirs = Directory.GetDirectories(powerSupplyPath, "BAT*");

            if (batteryDirs.Length == 0)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

            var batteryPath = batteryDirs[0]; // Use first battery

            var chargePercent = TryReadDouble(Path.Combine(batteryPath, "capacity"));
            var statusStr = TryReadString(Path.Combine(batteryPath, "status"));
            var status = MapLinuxBatteryStatus(statusStr);

            var energyNow = TryReadLong(Path.Combine(batteryPath, "energy_now"));
            var energyFull = TryReadLong(Path.Combine(batteryPath, "energy_full"));
            var energyFullDesign = TryReadLong(Path.Combine(batteryPath, "energy_full_design"));
            var powerNow = TryReadLong(Path.Combine(batteryPath, "power_now"));
            var voltageNow = TryReadLong(Path.Combine(batteryPath, "voltage_now"));
            var cycleCount = TryReadInt(Path.Combine(batteryPath, "cycle_count"));

            // Calculate health percent if we have the data
            double? healthPercent = null;
            if (energyFull.HasValue && energyFullDesign is > 0)
            {
                healthPercent = (double)energyFull.Value / energyFullDesign.Value * 100;
            }

            // Calculate remaining time
            int? remainingTimeMinutes = null;
            if (energyNow.HasValue && powerNow is > 0)
            {
                var hoursRemaining = (double)energyNow.Value / powerNow.Value;
                remainingTimeMinutes = (int)(hoursRemaining * 60);
            }

            // Check AC power
            var acDirs = Directory.GetDirectories(powerSupplyPath, "AC*")
                .Concat(Directory.GetDirectories(powerSupplyPath, "ADP*")).ToArray();
            var onAcPower = false;
            if (acDirs.Length > 0)
            {
                var onlineStr = TryReadString(Path.Combine(acDirs[0], "online"));
                onAcPower = onlineStr == "1";
            }

            // Convert microWh to milliWh for consistency
            var designCapacityMWh = energyFullDesign / 1000;
            var fullChargeCapacityMWh = energyFull / 1000;
            var chargeRateMw = powerNow / 1000;
            var voltageMv = voltageNow / 1000;

            // Adjust charge rate sign based on status
            if (chargeRateMw.HasValue && status == BatteryStatus.Discharging)
            {
                chargeRateMw = -chargeRateMw;
            }

            return new BatteryMetrics
            {
                BatteryPresent = true,
                ChargePercent = chargePercent,
                Status = status,
                RemainingTimeMinutes = remainingTimeMinutes,
                HealthPercent = healthPercent,
                DesignCapacityMWh = designCapacityMWh,
                FullChargeCapacityMWh = fullChargeCapacityMWh,
                ChargeRateMW = chargeRateMw,
                VoltageMV = voltageMv,
                TemperatureCelsius = null, // Not commonly available in sysfs
                CycleCount = cycleCount,
                OnACPower = onAcPower
            };
        }
        catch (Exception ex)
        {
            return new BatteryMetrics
            {
                BatteryPresent = false,
                Status = BatteryStatus.Unknown,
                OnACPower = true,
                ErrorMessage = ex.Message
            };
        }
    }

    private static BatteryStatus MapLinuxBatteryStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "charging" => BatteryStatus.Charging,
            "discharging" => BatteryStatus.Discharging,
            "full" => BatteryStatus.Full,
            "not charging" => BatteryStatus.NotCharging,
            _ => BatteryStatus.Unknown
        };
    }

    private static string? TryReadString(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
                return File.ReadAllText(filePath).Trim();
        }
        catch
        {
            // Ignore file read errors - method returns null to indicate failure
        }

        return null;
    }

    private static double? TryReadDouble(string filePath)
    {
        var str = TryReadString(filePath);
        if (str != null && double.TryParse(str, out var value))
            return value;
        return null;
    }

    private static long? TryReadLong(string filePath)
    {
        var str = TryReadString(filePath);
        if (str != null && long.TryParse(str, out var value))
            return value;
        return null;
    }

    private static int? TryReadInt(string filePath)
    {
        var str = TryReadString(filePath);
        if (str != null && int.TryParse(str, out var value))
            return value;
        return null;
    }
}

/// <summary>
/// macOS battery metrics provider using IOKit.
/// </summary>
internal sealed class MacOsBatteryMetricsProvider : IBatteryMetricsProvider
{
    public BatteryMetrics GetBatteryMetrics()
    {
        try
        {
            // Try using pmset command which is more accessible than IOKit
            var psi = new ProcessStartInfo
            {
                FileName = "pmset",
                Arguments = "-g batt",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.Unknown,
                    OnACPower = true,
                    ErrorMessage = "Failed to start pmset"
                };
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse pmset output
            // Example: "Now drawing from 'Battery Power'"
            // Example: "InternalBattery-0 (id=12345)	85%; discharging; 3:45 remaining present: true"

            var batteryPresent = output.Contains("InternalBattery") || output.Contains("Battery");

            if (!batteryPresent)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

            // This is a simplified parser - production code would be more robust
            var onAcPower = output.Contains("'AC Power'") || output.Contains("AC attached");

            return new BatteryMetrics
            {
                BatteryPresent = true,
                Status = BatteryStatus.Unknown,
                OnACPower = onAcPower,
                ErrorMessage = "macOS battery metrics require IOKit or more sophisticated pmset parsing"
            };
        }
        catch (Exception ex)
        {
            return new BatteryMetrics
            {
                BatteryPresent = false,
                Status = BatteryStatus.Unknown,
                OnACPower = true,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// Unsupported platform provider.
/// </summary>
internal sealed class UnsupportedBatteryMetricsProvider : IBatteryMetricsProvider
{
    public BatteryMetrics GetBatteryMetrics()
    {
        return new BatteryMetrics
        {
            BatteryPresent = false,
            Status = BatteryStatus.NotPresent,
            OnACPower = true,
            ErrorMessage = "Battery metrics not supported on this platform"
        };
    }
}