namespace ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;

/// <summary>
/// Represents CPU temperature metrics.
/// </summary>
public record CpuTemperatureMetrics : IMetrics
{
    /// <summary>
    /// Overall CPU package temperature in Celsius. Null if not supported.
    /// </summary>
    public double? PackageTemperatureCelsius { get; init; }

    /// <summary>
    /// Per-core temperatures in Celsius. Empty if not supported.
    /// </summary>
    public Dictionary<int, double> CoreTemperatures { get; init; } = new();

    /// <summary>
    /// Maximum temperature across all cores. Null if not supported.
    /// </summary>
    public double? MaxTemperatureCelsius { get; init; }

    /// <summary>
    /// Average temperature across all cores. Null if not supported.
    /// </summary>
    public double? AverageTemperatureCelsius { get; init; }

    /// <summary>
    /// Whether temperature sensors are available.
    /// </summary>
    public bool TemperatureSensorsAvailable { get; init; }

    /// <summary>
    /// Error message if metrics collection failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}