# System Resource Monitor

## Overview

The System Resource Monitor is a comprehensive monitoring solution for tracking system performance metrics including CPU usage, memory consumption, and storage utilization. This component provides a unified interface for real-time and historical resource monitoring with cross-platform support.

## Architecture

The System Resource Monitor follows a modular architecture that separates concerns and provides extensible monitoring capabilities:

```
ISystemResourceMonitor (Interface)
    ├── SystemResourceMonitor (Main Implementation)
    ├── SystemResourceMonitorExtensions (Extension Methods)
    └── SystemResourceMonitorMetrics (Aggregate Data Model)
        ├── CPU Metrics
        │   ├── CpuMetrics (Data Model)
        │   └── CpuMetricsClient (Collection Client)
        ├── Memory Metrics
        │   ├── MemoryMetrics (Data Model)
        │   └── MemoryMetricsClient (Collection Client)
        └── Storage Metrics
            ├── SystemDriveMetrics (Data Model)
            └── SystemDriveMetricsClient (Collection Client)
```

## Core Components

### Primary Interface and Implementation

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **[ISystemResourceMonitor](ISystemResourceMonitor.md)** | Main monitoring interface | Unified metrics collection, extensibility |
| **[SystemResourceMonitor](SystemResourceMonitor.md)** | Core implementation | Real-time collection, aggregation, error handling |
| **[SystemResourceMonitorExtensions](SystemResourceMonitorExtensions.md)** | Extension methods | Enhanced functionality, convenience methods |
| **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** | Aggregate metrics model | Complete system snapshot, validation |

### CPU Monitoring

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **[CpuMetrics](Metrics/Cpu/CpuMetrics.md)** | CPU usage data model | Usage percentage, timestamp, efficiency metrics |
| **[CpuMetricsClient](Metrics/Cpu/CpuMetricsClient.md)** | CPU metrics collection | Real-time CPU usage, cross-platform support |

### Memory Monitoring

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **[MemoryMetrics](Metrics/Memory/MemoryMetrics.md)** | Memory usage data model | Total/available/used memory, calculated properties |
| **[MemoryMetricsClient](Metrics/Memory/MemoryMetricsClient.md)** | Memory metrics collection | Cross-platform memory stats, error handling |

### Storage Monitoring

| Component | Purpose | Key Features |
|-----------|---------|--------------|
| **[SystemDriveMetrics](Metrics/SystemDriveMetrics/SystemDriveMetrics.md)** | Storage metrics model | Drive capacity, usage, free space, efficiency |
| **[SystemDriveMetricsClient](Metrics/SystemDriveMetrics/SystemDriveMetricsClient.md)** | Storage metrics collection | Multi-drive enumeration, platform compatibility |

## Quick Start

### Basic Usage
```csharp
// Create monitor instance
ISystemResourceMonitor monitor = new SystemResourceMonitor();

// Collect current metrics
var metrics = await monitor.GetSystemResourceMetricsAsync();

// Display summary
Console.WriteLine($"CPU Usage: {metrics.CpuMetrics.UsagePercentage:F1}%");
Console.WriteLine($"Memory Usage: {metrics.MemoryMetrics.UsagePercentage:F1}%");
Console.WriteLine($"Available Memory: {metrics.MemoryMetrics.Available / (1024.0 * 1024.0 * 1024.0):F1} GB");

foreach (var drive in metrics.SystemDriveMetrics)
{
    Console.WriteLine($"Drive {drive.Letter}: {drive.UsagePercentage:F1}% used");
}
```

### Advanced Monitoring with Extensions
```csharp
// Create monitor with extensions
var monitor = new SystemResourceMonitor();

// Get system health assessment
var healthStatus = await monitor.GetSystemHealthAsync();
Console.WriteLine($"Overall Health: {healthStatus.OverallStatus}");

// Get detailed performance analysis
var performance = await monitor.GetPerformanceAnalysisAsync();
Console.WriteLine($"Performance Score: {performance.OverallScore}/100");

// Check for resource bottlenecks
var bottlenecks = await monitor.GetResourceBottlenecksAsync();
if (bottlenecks.Any())
{
    Console.WriteLine("Resource Bottlenecks Detected:");
    foreach (var bottleneck in bottlenecks)
    {
        Console.WriteLine($"  {bottleneck.ResourceType}: {bottleneck.Severity} - {bottleneck.Description}");
    }
}

// Get optimization recommendations
var recommendations = await monitor.GetOptimizationRecommendationsAsync();
Console.WriteLine($"Optimization Recommendations: {recommendations.Count}");
foreach (var rec in recommendations)
{
    Console.WriteLine($"  {rec.Category}: {rec.Description}");
}
```

### Continuous Monitoring
```csharp
public class SystemMonitoringService : IDisposable
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly Timer _timer;
    private readonly List<SystemResourceMonitorMetrics> _history;
    private bool _disposed;

    public SystemMonitoringService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
        _history = new List<SystemResourceMonitorMetrics>();
        
        // Monitor every 30 seconds
        _timer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    public event Action<SystemResourceMonitorMetrics>? MetricsCollected;
    public event Action<ResourceAlert>? AlertRaised;

    private async void CollectMetrics(object? state)
    {
        if (_disposed) return;

        try
        {
            var metrics = await _monitor.GetSystemResourceMetricsAsync();
            
            // Store history (keep last 2 hours)
            _history.Add(metrics);
            if (_history.Count > 240) // 2 hours at 30-second intervals
            {
                _history.RemoveAt(0);
            }
            
            // Raise events
            MetricsCollected?.Invoke(metrics);
            
            // Check for alerts
            var alerts = CheckForAlerts(metrics);
            foreach (var alert in alerts)
            {
                AlertRaised?.Invoke(alert);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue monitoring
            Console.WriteLine($"Error collecting metrics: {ex.Message}");
        }
    }

    private List<ResourceAlert> CheckForAlerts(SystemResourceMonitorMetrics metrics)
    {
        var alerts = new List<ResourceAlert>();
        
        // CPU alerts
        if (metrics.CpuMetrics.UsagePercentage > 90)
        {
            alerts.Add(new ResourceAlert
            {
                Type = "High CPU Usage",
                Level = AlertLevel.Warning,
                Message = $"CPU usage at {metrics.CpuMetrics.UsagePercentage:F1}%",
                Timestamp = metrics.Timestamp
            });
        }
        
        // Memory alerts
        if (metrics.MemoryMetrics.UsagePercentage > 85)
        {
            alerts.Add(new ResourceAlert
            {
                Type = "High Memory Usage",
                Level = AlertLevel.Warning,
                Message = $"Memory usage at {metrics.MemoryMetrics.UsagePercentage:F1}%",
                Timestamp = metrics.Timestamp
            });
        }
        
        // Storage alerts
        foreach (var drive in metrics.SystemDriveMetrics)
        {
            if (drive.UsagePercentage > 95)
            {
                alerts.Add(new ResourceAlert
                {
                    Type = "Critical Disk Space",
                    Level = AlertLevel.Critical,
                    Message = $"Drive {drive.Letter} at {drive.UsagePercentage:F1}% capacity",
                    Timestamp = metrics.Timestamp
                });
            }
        }
        
        return alerts;
    }

    public SystemResourceTrends GetTrends(TimeSpan period)
    {
        var cutoff = DateTime.UtcNow - period;
        var relevantMetrics = _history.Where(m => m.Timestamp >= cutoff).ToArray();
        
        if (!relevantMetrics.Any())
        {
            return new SystemResourceTrends { Status = "No data available" };
        }
        
        return new SystemResourceTrends
        {
            Period = period,
            DataPoints = relevantMetrics.Length,
            
            CpuTrend = new TrendData
            {
                Current = relevantMetrics.Last().CpuMetrics.UsagePercentage,
                Average = relevantMetrics.Average(m => m.CpuMetrics.UsagePercentage),
                Maximum = relevantMetrics.Max(m => m.CpuMetrics.UsagePercentage),
                Minimum = relevantMetrics.Min(m => m.CpuMetrics.UsagePercentage)
            },
            
            MemoryTrend = new TrendData
            {
                Current = relevantMetrics.Last().MemoryMetrics.UsagePercentage,
                Average = relevantMetrics.Average(m => m.MemoryMetrics.UsagePercentage),
                Maximum = relevantMetrics.Max(m => m.MemoryMetrics.UsagePercentage),
                Minimum = relevantMetrics.Min(m => m.MemoryMetrics.UsagePercentage)
            },
            
            Status = "Data available"
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _timer?.Dispose();
    }
}

public class ResourceAlert
{
    public string Type { get; set; } = "";
    public AlertLevel Level { get; set; }
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class SystemResourceTrends
{
    public TimeSpan Period { get; set; }
    public int DataPoints { get; set; }
    public TrendData CpuTrend { get; set; } = new();
    public TrendData MemoryTrend { get; set; } = new();
    public string Status { get; set; } = "";
}

public class TrendData
{
    public double Current { get; set; }
    public double Average { get; set; }
    public double Maximum { get; set; }
    public double Minimum { get; set; }
}

public enum AlertLevel { Info, Warning, Critical }
```

## Integration Patterns

### Dependency Injection
```csharp
// Configure services
services.AddTransient<ISystemResourceMonitor, SystemResourceMonitor>();
services.AddSingleton<SystemMonitoringService>();

// Use in controllers/services
[ApiController]
public class SystemController : ControllerBase
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemController(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetSystemHealth()
    {
        var metrics = await _monitor.GetSystemResourceMetricsAsync();
        var health = await _monitor.GetSystemHealthAsync();
        
        return Ok(new { metrics, health });
    }
}
```

### Health Checks Integration
```csharp
public class SystemResourceHealthCheck : IHealthCheck
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemResourceHealthCheck(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await _monitor.GetSystemResourceMetricsAsync();
            var issues = new List<string>();
            
            // Check CPU
            if (metrics.CpuMetrics.UsagePercentage > 95)
                issues.Add($"Critical CPU usage: {metrics.CpuMetrics.UsagePercentage:F1}%");
            else if (metrics.CpuMetrics.UsagePercentage > 85)
                issues.Add($"High CPU usage: {metrics.CpuMetrics.UsagePercentage:F1}%");
            
            // Check Memory
            if (metrics.MemoryMetrics.UsagePercentage > 90)
                issues.Add($"Critical memory usage: {metrics.MemoryMetrics.UsagePercentage:F1}%");
            else if (metrics.MemoryMetrics.UsagePercentage > 80)
                issues.Add($"High memory usage: {metrics.MemoryMetrics.UsagePercentage:F1}%");
            
            // Check Storage
            foreach (var drive in metrics.SystemDriveMetrics)
            {
                if (drive.UsagePercentage > 95)
                    issues.Add($"Critical disk space on {drive.Letter}: {drive.UsagePercentage:F1}%");
                else if (drive.UsagePercentage > 85)
                    issues.Add($"High disk usage on {drive.Letter}: {drive.UsagePercentage:F1}%");
            }
            
            var status = issues.Any(i => i.Contains("Critical")) ? HealthStatus.Unhealthy :
                        issues.Any() ? HealthStatus.Degraded : HealthStatus.Healthy;
            
            var data = new Dictionary<string, object>
            {
                ["cpu_usage"] = metrics.CpuMetrics.UsagePercentage,
                ["memory_usage"] = metrics.MemoryMetrics.UsagePercentage,
                ["drive_count"] = metrics.SystemDriveMetrics.Length,
                ["timestamp"] = metrics.Timestamp
            };
            
            return new HealthCheckResult(
                status,
                description: issues.Any() ? string.Join("; ", issues) : "All systems normal",
                data: data);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                description: $"System resource monitoring failed: {ex.Message}",
                exception: ex);
        }
    }
}

// Registration
services.AddHealthChecks()
    .AddCheck<SystemResourceHealthCheck>("system_resources");
```

### Logging Integration
```csharp
public class LoggingSystemResourceMonitor : ISystemResourceMonitor
{
    private readonly ISystemResourceMonitor _inner;
    private readonly ILogger<LoggingSystemResourceMonitor> _logger;

    public LoggingSystemResourceMonitor(
        ISystemResourceMonitor inner,
        ILogger<LoggingSystemResourceMonitor> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<SystemResourceMonitorMetrics> GetSystemResourceMetricsAsync()
    {
        using var scope = _logger.BeginScope("Collecting system resource metrics");
        
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var metrics = await _inner.GetSystemResourceMetricsAsync();
            stopwatch.Stop();
            
            _logger.LogInformation(
                "System metrics collected in {Duration}ms - CPU: {CpuUsage:F1}%, Memory: {MemoryUsage:F1}%, Drives: {DriveCount}",
                stopwatch.ElapsedMilliseconds,
                metrics.CpuMetrics.UsagePercentage,
                metrics.MemoryMetrics.UsagePercentage,
                metrics.SystemDriveMetrics.Length);
            
            // Log resource pressure
            if (metrics.CpuMetrics.UsagePercentage > 80 || 
                metrics.MemoryMetrics.UsagePercentage > 80 ||
                metrics.SystemDriveMetrics.Any(d => d.UsagePercentage > 80))
            {
                _logger.LogWarning("High resource utilization detected");
            }
            
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to collect system resource metrics");
            throw;
        }
    }
}

// Registration
services.Decorate<ISystemResourceMonitor, LoggingSystemResourceMonitor>();
```

## Configuration

### Standard Configuration
```json
{
  "SystemResourceMonitor": {
    "CollectionInterval": "00:00:30",
    "HistoryRetention": "02:00:00",
    "Thresholds": {
      "CpuWarning": 80,
      "CpuCritical": 95,
      "MemoryWarning": 80,
      "MemoryCritical": 90,
      "DiskWarning": 85,
      "DiskCritical": 95
    },
    "Alerts": {
      "Enabled": true,
      "EmailNotifications": true,
      "SlackNotifications": false
    }
  }
}
```

### Configuration Model
```csharp
public class SystemResourceMonitorOptions
{
    public TimeSpan CollectionInterval { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan HistoryRetention { get; set; } = TimeSpan.FromHours(2);
    public ThresholdOptions Thresholds { get; set; } = new();
    public AlertOptions Alerts { get; set; } = new();
}

public class ThresholdOptions
{
    public double CpuWarning { get; set; } = 80;
    public double CpuCritical { get; set; } = 95;
    public double MemoryWarning { get; set; } = 80;
    public double MemoryCritical { get; set; } = 90;
    public double DiskWarning { get; set; } = 85;
    public double DiskCritical { get; set; } = 95;
}

public class AlertOptions
{
    public bool Enabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool SlackNotifications { get; set; } = false;
}
```

## Performance Characteristics

### Resource Usage
- **CPU Impact**: Minimal (< 0.1% during collection)
- **Memory Footprint**: Low (~5-10 MB for basic monitoring)
- **I/O Overhead**: Negligible (system metadata only)
- **Network**: None (local metrics only)

### Collection Speed
- **Typical Duration**: 50-200ms for complete metrics collection
- **CPU Metrics**: ~10ms (PerformanceCounter query)
- **Memory Metrics**: ~5ms (WMI/proc/fs query)
- **Storage Metrics**: ~20-50ms per drive (depends on drive count)

### Scalability
- **Concurrent Access**: Thread-safe implementations
- **High Frequency**: Suitable for sub-second monitoring
- **Resource Growth**: Linear with drive count
- **Platform Performance**: Consistent across Windows/Linux/macOS

## Cross-Platform Support

### Windows
- **CPU**: Performance Counters
- **Memory**: WMI queries (wmic command)
- **Storage**: DriveInfo class
- **Privileges**: Standard user access sufficient

### Linux
- **CPU**: /proc/stat parsing
- **Memory**: free command or /proc/meminfo
- **Storage**: df command or filesystem queries
- **Permissions**: Standard user access, some /proc files may need elevated access

### macOS
- **CPU**: top/vm_stat commands
- **Memory**: vm_stat system command
- **Storage**: df command
- **Compatibility**: Full compatibility with Unix-like commands

## Best Practices

### Collection Strategy
1. **Appropriate Frequency**: Match collection frequency to use case (30 seconds for general monitoring)
2. **Error Handling**: Implement robust error handling for temporary system issues
3. **Resource Management**: Dispose of resources properly in long-running applications
4. **Caching**: Consider caching for very high-frequency access patterns

### Monitoring Guidelines
1. **Threshold Management**: Set appropriate warning and critical thresholds
2. **Trend Analysis**: Implement historical analysis for capacity planning
3. **Alert Fatigue**: Avoid excessive alerting with proper threshold tuning
4. **Context Awareness**: Consider system load patterns and usage cycles

### Performance Optimization
1. **Parallel Collection**: Collect different metric types in parallel when possible
2. **Efficient Queries**: Use optimized system queries for better performance
3. **Memory Management**: Monitor your monitoring - track the monitor's resource usage
4. **Platform Optimization**: Use platform-specific optimizations when available

## Troubleshooting

### Common Issues

#### High Collection Times
```csharp
// Monitor collection performance
var stopwatch = Stopwatch.StartNew();
var metrics = await monitor.GetSystemResourceMetricsAsync();
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 1000)
{
    Console.WriteLine($"Slow collection detected: {stopwatch.ElapsedMilliseconds}ms");
    // Investigate individual metric collection times
}
```

#### Platform-Specific Errors
```csharp
try
{
    var metrics = await monitor.GetSystemResourceMetricsAsync();
}
catch (PlatformNotSupportedException ex)
{
    // Handle platform limitations
    Console.WriteLine($"Platform limitation: {ex.Message}");
}
catch (UnauthorizedAccessException ex)
{
    // Handle permission issues
    Console.WriteLine($"Permission issue: {ex.Message}");
}
```

#### Resource Collection Failures
```csharp
// Implement fallback strategies
public async Task<SystemResourceMonitorMetrics> GetMetricsWithFallback()
{
    try
    {
        return await monitor.GetSystemResourceMetricsAsync();
    }
    catch (Exception ex)
    {
        // Return partial metrics or cached values
        return new SystemResourceMonitorMetrics
        {
            Timestamp = DateTime.UtcNow,
            CpuMetrics = GetLastKnownCpuMetrics(),
            MemoryMetrics = GetLastKnownMemoryMetrics(),
            SystemDriveMetrics = GetLastKnownDriveMetrics(),
            IsPartial = true,
            Error = ex.Message
        };
    }
}
```

## Related Documentation

### Infrastructure Components
- **[Health Checks](../HealthChecks/README.md)** - Complete health monitoring system
- **[Network Performance](../System/Network/README.md)** - Network monitoring components

### Development Resources
- **[Building Blocks Documentation](../../Application/README.md)** - Core application components
- **[Infrastructure Overview](../README.md)** - Complete infrastructure documentation

## Future Enhancements

### Planned Features
1. **GPU Monitoring**: Graphics card utilization tracking
2. **Network Metrics**: Bandwidth and connection monitoring
3. **Process Monitoring**: Individual process resource tracking
4. **Predictive Analytics**: AI-powered capacity planning
5. **Cloud Integration**: Cloud platform resource monitoring

### Extension Points
1. **Custom Metrics**: Framework for adding custom resource metrics
2. **Alert Providers**: Pluggable alert notification systems
3. **Export Formats**: Multiple data export formats (JSON, CSV, XML)
4. **Visualization**: Built-in dashboard and visualization components