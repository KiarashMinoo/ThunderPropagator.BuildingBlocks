?using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;

public  interface IBatteryMetricsClient : IMetricsClient<BatteryMetrics>;

/// <summary>
/// Client for collecting battery metrics.
/// </summary>
internal sealed class BatteryMetricsClient : IBatteryMetricsClient
{
    private readonly IBatteryMetricsProvider _provider;

    internal BatteryMetricsClient(IBatteryMetricsProvider provider)
    {
        _provider = provider;
    }

    public BatteryMetricsClient() : this(CreatePlatformProvider())
    {
    }

    public async Task<BatteryMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _provider.GetBatteryMetricsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
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
    Task<BatteryMetrics> GetBatteryMetricsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Windows battery metrics provider using WMI queries.
/// </summary>
internal sealed class WindowsBatteryMetricsProvider : IBatteryMetricsProvider
{
    public async Task<BatteryMetrics> GetBatteryMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await GetWindowsBatteryInfoAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
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

    private static async Task<BatteryMetrics> GetWindowsBatteryInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use WMIC to query battery status (available on Windows without additional packages)
            var psi = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "path Win32_Battery get BatteryStatus,EstimatedChargeRemaining,EstimatedRunTime /format:list",
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

            // Ensure we can cancel the wait.
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(output) || !output.Contains("EstimatedChargeRemaining", StringComparison.OrdinalIgnoreCase))
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

            // Parse list output: key=value pairs
            int? batteryStatusCode = null;
            double? chargePercent = null;
            int? remainingMinutes = null;

            foreach (var line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "BatteryStatus":
                        if (int.TryParse(value, out var code))
                            batteryStatusCode = code;
                        break;
                    case "EstimatedChargeRemaining":
                        if (double.TryParse(value, out var charge))
                            chargePercent = charge;
                        break;
                    case "EstimatedRunTime":
                        if (int.TryParse(value, out var minutes))
                            remainingMinutes = minutes;
                        break;
                }
            }

            if (!batteryStatusCode.HasValue && !chargePercent.HasValue)
            {
                return new BatteryMetrics
                {
                    BatteryPresent = false,
                    Status = BatteryStatus.NotPresent,
                    OnACPower = true
                };
            }

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
                OnACPower = status is BatteryStatus.Charging or BatteryStatus.Full
            };
        }
        catch (OperationCanceledException)
        {
            throw;
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
    public Task<BatteryMetrics> GetBatteryMetricsAsync(CancellationToken cancellationToken)
    {
        // File IO is fast; keep it sync but respect cancellation.
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetBatteryMetricsCore(cancellationToken));
    }

    private static BatteryMetrics GetBatteryMetricsCore(CancellationToken cancellationToken)
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

            cancellationToken.ThrowIfCancellationRequested();

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

            var designCapacityMWh = MicroToMilli(energyFullDesign);
            var fullChargeCapacityMWh = MicroToMilli(energyFull);
            var chargeRateMw = MicroToMilli(powerNow);
            var voltageMv = MicroToMilli(voltageNow);

            // Adjust charge rate sign based on status
            if (chargeRateMw.HasValue && status == BatteryStatus.Discharging)
            {
                chargeRateMw = -chargeRateMw.Value;
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

            static long? MicroToMilli(long? micro) => micro / 1000;
        }
        catch (OperationCanceledException)
        {
            throw;
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
    public async Task<BatteryMetrics> GetBatteryMetricsAsync(CancellationToken cancellationToken)
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

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            var batteryPresent = output.Contains("InternalBattery", StringComparison.OrdinalIgnoreCase) ||
                                 output.Contains("Battery", StringComparison.OrdinalIgnoreCase);

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
            var onAcPower = output.Contains("'AC Power'", StringComparison.OrdinalIgnoreCase) ||
                            output.Contains("AC attached", StringComparison.OrdinalIgnoreCase);

            return new BatteryMetrics
            {
                BatteryPresent = true,
                Status = BatteryStatus.Unknown,
                OnACPower = onAcPower,
                ErrorMessage = "macOS battery metrics require IOKit or more sophisticated pmset parsing"
            };
        }
        catch (OperationCanceledException)
        {
            throw;
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
    public Task<BatteryMetrics> GetBatteryMetricsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new BatteryMetrics
        {
            BatteryPresent = false,
            Status = BatteryStatus.NotPresent,
            OnACPower = true,
            ErrorMessage = "Battery metrics not supported on this platform"
        });
    }
}