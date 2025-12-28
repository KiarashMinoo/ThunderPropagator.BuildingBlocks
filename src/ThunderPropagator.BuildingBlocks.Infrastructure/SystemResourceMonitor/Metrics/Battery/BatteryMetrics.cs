namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;

/// <summary>
/// Represents battery status and health metrics.
/// </summary>
public record BatteryMetrics : IMetrics
{
    /// <summary>
    /// Whether a battery is present in the system.
    /// </summary>
    public bool BatteryPresent { get; init; }

    /// <summary>
    /// Battery charge level percentage (0-100). Null if no battery or not supported.
    /// </summary>
    public double? ChargePercent { get; init; }

    /// <summary>
    /// Battery status (Charging, Discharging, Full, etc.).
    /// </summary>
    public BatteryStatus Status { get; init; }

    /// <summary>
    /// Estimated remaining time in minutes. Null if not available or not supported.
    /// </summary>
    public int? RemainingTimeMinutes { get; init; }

    /// <summary>
    /// Battery health percentage (0-100, where 100 is new battery). Null if not supported.
    /// </summary>
    public double? HealthPercent { get; init; }

    /// <summary>
    /// Battery design capacity in mWh. Null if not supported.
    /// </summary>
    public long? DesignCapacityMWh { get; init; }

    /// <summary>
    /// Battery full charge capacity in mWh. Null if not supported.
    /// </summary>
    public long? FullChargeCapacityMWh { get; init; }

    /// <summary>
    /// Battery charge rate in mW (positive when charging, negative when discharging). Null if not supported.
    /// </summary>
    public long? ChargeRateMW { get; init; }

    /// <summary>
    /// Battery voltage in mV. Null if not supported.
    /// </summary>
    public long? VoltageMV { get; init; }

    /// <summary>
    /// Battery temperature in Celsius. Null if not supported.
    /// </summary>
    public double? TemperatureCelsius { get; init; }

    /// <summary>
    /// Number of charge cycles. Null if not supported.
    /// </summary>
    public int? CycleCount { get; init; }

    /// <summary>
    /// Whether the system is on AC power.
    /// </summary>
    public bool OnACPower { get; init; }

    /// <summary>
    /// Error message if metrics collection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

public enum BatteryStatus
{
    Unknown = 0,
    Charging = 1,
    Discharging = 2,
    Full = 3,
    NotCharging = 4,
    NotPresent = 5
}