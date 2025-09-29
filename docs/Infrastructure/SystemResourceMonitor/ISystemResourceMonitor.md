# ISystemResourceMonitor

## Overview

The `ISystemResourceMonitor` interface provides a unified contract for system resource monitoring, enabling applications to collect comprehensive system performance metrics including CPU, memory, and disk usage. The interface is designed for dependency injection and supports configurable monitoring windows and scope control.

## Purpose

- **System Resource Aggregation**: Unified interface for collecting all system metrics
- **Dependency Injection**: Clean abstraction for service registration and testing
- **Configurable Monitoring**: Flexible monitoring windows and scope control
- **Performance Tracking**: Comprehensive system performance analysis

## Interface Declaration

```csharp
public interface ISystemResourceMonitor
{
    SystemResourceMonitorMetrics GetMetrics(long window, bool all = false);
}
```

## Implementation

```csharp
internal sealed class SystemResourceMonitorImpl(
    CpuMetricsClient cpuMetricsClient,
    MemoryMetricsClient memoryMetricsClient,
    SystemDriveMetricsClient systemDriveMetricsClient)
    : ISystemResourceMonitor
{
    public SystemResourceMonitorMetrics GetMetrics(long window, bool all = false)
    {
        var cpuMetrics = cpuMetricsClient.GetMetrics(window, all);
        var memoryMetrics = memoryMetricsClient.GetMetrics();
        var systemDriveMetrics = systemDriveMetricsClient.GetMetrics();

        return new SystemResourceMonitorMetrics(cpuMetrics, memoryMetrics, systemDriveMetrics);
    }
}
```

## Method Details

### GetMetrics

Collects comprehensive system resource metrics including CPU, memory, and disk information.

#### Parameters
- **window** (`long`): Monitoring window duration in milliseconds for CPU measurements
- **all** (`bool`): When `true`, monitors all system processes; when `false`, monitors only current process (default: `false`)

#### Returns
`SystemResourceMonitorMetrics` containing aggregated system resource information:
- **CPU Metrics**: Processor usage, thread counts, process information
- **Memory Metrics**: System memory usage, available memory, utilization percentages
- **Disk Metrics**: Drive information, storage capacity, and usage statistics

## Usage Examples

### Basic System Monitoring
```csharp
public class SystemMonitoringService
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly ILogger<SystemMonitoringService> _logger;

    public SystemMonitoringService(ISystemResourceMonitor monitor, ILogger<SystemMonitoringService> logger)
    {
        _monitor = monitor;
        _logger = logger;
    }

    public async Task MonitorSystemResources()
    {
        // Monitor current process with 1-second window
        var metrics = _monitor.GetMetrics(window: 1000, all: false);
        
        _logger.LogInformation("System Resources - CPU: {CpuUsage:F2}%, Memory: {MemoryUsage:F2}%, Drives: {DriveCount}",
            metrics.Cpu.Usage, metrics.Memory.UsagePercentage, metrics.Drives.Length);
    }
}
```

### Comprehensive System Analysis
```csharp
public class SystemPerformanceAnalyzer
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemPerformanceAnalyzer(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public SystemPerformanceReport GenerateReport()
    {
        // Collect metrics for all processes with 5-second measurement window
        var metrics = _monitor.GetMetrics(window: 5000, all: true);

        return new SystemPerformanceReport
        {
            Timestamp = DateTime.UtcNow,
            
            // CPU Analysis
            CpuUsagePercentage = metrics.Cpu.Usage,
            ProcessorCount = metrics.Cpu.ProcessorCount,
            ActiveProcesses = metrics.Cpu.Processes,
            TotalThreads = metrics.Cpu.TotalThreads,
            
            // Memory Analysis
            TotalMemoryMB = metrics.Memory.Total,
            FreeMemoryMB = metrics.Memory.Free,
            UsedMemoryMB = metrics.Memory.Used,
            MemoryUsagePercentage = metrics.Memory.UsagePercentage,
            
            // Storage Analysis
            Drives = metrics.Drives.Select(d => new DriveInfo
            {
                Name = d.Name,
                TotalSizeGB = d.TotalSize / 1024.0,
                FreeSpaceGB = d.FreeSpace / 1024.0,
                UsagePercentage = d.UsagePercentage
            }).ToArray(),
            
            // Performance Classification
            PerformanceLevel = ClassifyPerformance(metrics)
        };
    }

    private PerformanceLevel ClassifyPerformance(SystemResourceMonitorMetrics metrics)
    {
        // Classify overall system performance
        if (metrics.Cpu.Usage > 90 || metrics.Memory.UsagePercentage > 95)
            return PerformanceLevel.Critical;
        
        if (metrics.Cpu.Usage > 75 || metrics.Memory.UsagePercentage > 85)
            return PerformanceLevel.High;
        
        if (metrics.Cpu.Usage > 50 || metrics.Memory.UsagePercentage > 70)
            return PerformanceLevel.Moderate;
        
        return PerformanceLevel.Low;
    }
}

public enum PerformanceLevel { Low, Moderate, High, Critical }
```

### Background Monitoring Service
```csharp
public class SystemResourceBackgroundMonitor : BackgroundService
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly ILogger<SystemResourceBackgroundMonitor> _logger;
    private readonly IMetricsPublisher _metricsPublisher;
    private readonly SystemMonitoringOptions _options;

    public SystemResourceBackgroundMonitor(
        ISystemResourceMonitor monitor,
        ILogger<SystemResourceBackgroundMonitor> logger,
        IMetricsPublisher metricsPublisher,
        IOptions<SystemMonitoringOptions> options)
    {
        _monitor = monitor;
        _logger = logger;
        _metricsPublisher = metricsPublisher;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var metrics = _monitor.GetMetrics(
                    window: _options.MeasurementWindowMs,
                    all: _options.MonitorAllProcesses);

                // Log metrics
                _logger.LogInformation("System Performance: CPU {CpuUsage:F1}%, Memory {MemoryUsage:F1}%, {DriveCount} drives",
                    metrics.Cpu.Usage, metrics.Memory.UsagePercentage, metrics.Drives.Length);

                // Publish to monitoring system
                await _metricsPublisher.PublishAsync(new
                {
                    cpu_usage_percent = metrics.Cpu.Usage,
                    memory_usage_percent = metrics.Memory.UsagePercentage,
                    memory_total_mb = metrics.Memory.Total,
                    memory_free_mb = metrics.Memory.Free,
                    processor_count = metrics.Cpu.ProcessorCount,
                    process_count = metrics.Cpu.Processes,
                    total_threads = metrics.Cpu.TotalThreads,
                    drive_metrics = metrics.Drives.Select(d => new
                    {
                        name = d.Name,
                        total_gb = d.TotalSize / 1024.0,
                        free_gb = d.FreeSpace / 1024.0,
                        usage_percent = d.UsagePercentage
                    })
                });

                // Check thresholds and alert if necessary
                await CheckThresholdsAndAlert(metrics);

                await Task.Delay(TimeSpan.FromSeconds(_options.CollectionIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System resource monitoring failed");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task CheckThresholdsAndAlert(SystemResourceMonitorMetrics metrics)
    {
        var alerts = new List<string>();

        if (metrics.Cpu.Usage > _options.CpuThreshold.Critical)
            alerts.Add($"Critical CPU usage: {metrics.Cpu.Usage:F1}%");
        else if (metrics.Cpu.Usage > _options.CpuThreshold.Warning)
            alerts.Add($"High CPU usage: {metrics.Cpu.Usage:F1}%");

        if (metrics.Memory.UsagePercentage > _options.MemoryThreshold.Critical)
            alerts.Add($"Critical memory usage: {metrics.Memory.UsagePercentage:F1}%");
        else if (metrics.Memory.UsagePercentage > _options.MemoryThreshold.Warning)
            alerts.Add($"High memory usage: {metrics.Memory.UsagePercentage:F1}%");

        foreach (var drive in metrics.Drives)
        {
            if (drive.UsagePercentage > _options.DiskThreshold.Critical)
                alerts.Add($"Critical disk usage on {drive.Name}: {drive.UsagePercentage:F1}%");
            else if (drive.UsagePercentage > _options.DiskThreshold.Warning)
                alerts.Add($"High disk usage on {drive.Name}: {drive.UsagePercentage:F1}%");
        }

        if (alerts.Any())
        {
            var alertMessage = string.Join("; ", alerts);
            _logger.LogWarning("System resource alerts: {Alerts}", alertMessage);
            await _metricsPublisher.PublishAlertAsync(alertMessage);
        }
    }
}

public class SystemMonitoringOptions
{
    public int MeasurementWindowMs { get; set; } = 1000;
    public bool MonitorAllProcesses { get; set; } = false;
    public int CollectionIntervalSeconds { get; set; } = 60;
    public ThresholdOptions CpuThreshold { get; set; } = new() { Warning = 75, Critical = 90 };
    public ThresholdOptions MemoryThreshold { get; set; } = new() { Warning = 80, Critical = 95 };
    public ThresholdOptions DiskThreshold { get; set; } = new() { Warning = 85, Critical = 95 };
}

public class ThresholdOptions
{
    public double Warning { get; set; }
    public double Critical { get; set; }
}
```

### Health Check Integration
```csharp
public class SystemResourceHealthCheck : IHealthCheck
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly SystemMonitoringOptions _options;

    public SystemResourceHealthCheck(ISystemResourceMonitor monitor, IOptions<SystemMonitoringOptions> options)
    {
        _monitor = monitor;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            
            var healthData = new Dictionary<string, object>
            {
                ["cpu_usage_percent"] = metrics.Cpu.Usage,
                ["memory_usage_percent"] = metrics.Memory.UsagePercentage,
                ["memory_total_mb"] = metrics.Memory.Total,
                ["memory_free_mb"] = metrics.Memory.Free,
                ["processor_count"] = metrics.Cpu.ProcessorCount,
                ["process_count"] = metrics.Cpu.Processes,
                ["drive_count"] = metrics.Drives.Length
            };

            var issues = new List<string>();
            var status = HealthStatus.Healthy;

            // Check CPU thresholds
            if (metrics.Cpu.Usage > _options.CpuThreshold.Critical)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical CPU usage: {metrics.Cpu.Usage:F1}%");
            }
            else if (metrics.Cpu.Usage > _options.CpuThreshold.Warning)
            {
                status = HealthStatus.Degraded;
                issues.Add($"High CPU usage: {metrics.Cpu.Usage:F1}%");
            }

            // Check memory thresholds
            if (metrics.Memory.UsagePercentage > _options.MemoryThreshold.Critical)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical memory usage: {metrics.Memory.UsagePercentage:F1}%");
            }
            else if (metrics.Memory.UsagePercentage > _options.MemoryThreshold.Warning)
            {
                if (status == HealthStatus.Healthy) status = HealthStatus.Degraded;
                issues.Add($"High memory usage: {metrics.Memory.UsagePercentage:F1}%");
            }

            // Check disk thresholds
            foreach (var drive in metrics.Drives)
            {
                if (drive.UsagePercentage > _options.DiskThreshold.Critical)
                {
                    status = HealthStatus.Unhealthy;
                    issues.Add($"Critical disk usage on {drive.Name}: {drive.UsagePercentage:F1}%");
                }
                else if (drive.UsagePercentage > _options.DiskThreshold.Warning)
                {
                    if (status == HealthStatus.Healthy) status = HealthStatus.Degraded;
                    issues.Add($"High disk usage on {drive.Name}: {drive.UsagePercentage:F1}%");
                }
            }

            var description = issues.Any() ? string.Join("; ", issues) : "System resources within normal limits";
            
            return new HealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("System resource monitoring failed", ex);
        }
    }
}
```

### ASP.NET Core Controller Integration
```csharp
[ApiController]
[Route("api/[controller]")]
public class SystemResourceController : ControllerBase
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemResourceController(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    [HttpGet("current")]
    public ActionResult<SystemResourceMonitorMetrics> GetCurrentMetrics([FromQuery] long window = 1000, [FromQuery] bool all = false)
    {
        try
        {
            var metrics = _monitor.GetMetrics(window, all);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to collect system metrics: {ex.Message}");
        }
    }

    [HttpGet("summary")]
    public ActionResult<object> GetSystemSummary()
    {
        try
        {
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            
            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                cpu = new
                {
                    usage_percent = Math.Round(metrics.Cpu.Usage, 2),
                    processor_count = metrics.Cpu.ProcessorCount,
                    processes = metrics.Cpu.Processes,
                    threads = metrics.Cpu.Threads,
                    total_threads = metrics.Cpu.TotalThreads
                },
                memory = new
                {
                    total_mb = Math.Round(metrics.Memory.Total, 0),
                    free_mb = Math.Round(metrics.Memory.Free, 0),
                    used_mb = Math.Round(metrics.Memory.Used, 0),
                    usage_percent = Math.Round(metrics.Memory.UsagePercentage, 2)
                },
                drives = metrics.Drives.Select(d => new
                {
                    name = d.Name,
                    total_gb = Math.Round(d.TotalSize / 1024.0, 2),
                    free_gb = Math.Round(d.FreeSpace / 1024.0, 2),
                    used_gb = Math.Round((d.TotalSize - d.FreeSpace) / 1024.0, 2),
                    usage_percent = Math.Round(d.UsagePercentage, 2)
                }).ToArray()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to generate system summary: {ex.Message}");
        }
    }

    [HttpGet("prometheus")]
    public async Task<IActionResult> GetPrometheusMetrics()
    {
        try
        {
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            var prometheusMetrics = SystemMetricsFormatter.ToPrometheus(metrics);
            
            return Content(prometheusMetrics, "text/plain");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to generate Prometheus metrics: {ex.Message}");
        }
    }
}
```

## Architecture Benefits

### Dependency Injection
- **Testable Design**: Interface allows for easy mocking and unit testing
- **Service Composition**: Aggregates multiple metric clients through dependency injection
- **Lifetime Management**: Proper service lifetime management through DI container

### Performance Optimization
- **Efficient Aggregation**: Single method call collects all system metrics
- **Configurable Scope**: Choose between current process or system-wide monitoring
- **Flexible Windows**: Adjustable measurement windows for accuracy vs. performance trade-offs

### Extensibility
- **Additional Metrics**: Easy to extend with new metric types
- **Custom Implementations**: Interface allows for alternative implementations
- **Integration Ready**: Designed for integration with monitoring and alerting systems

## Related Components

- **[SystemResourceMonitorExtensions](SystemResourceMonitorExtensions.md)** - Dependency injection setup
- **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** - Aggregated metrics model
- **[CPU Metrics](Metrics/Cpu/CpuMetrics.md)** - CPU performance metrics
- **[Memory Metrics](Metrics/Memory/MemoryMetrics.md)** - Memory utilization metrics
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Best Practices

### Monitoring Configuration
1. **Appropriate Windows**: Use longer windows (2-5 seconds) for accurate CPU measurements
2. **Scope Selection**: Monitor current process for application metrics, all processes for system analysis
3. **Collection Frequency**: Balance monitoring frequency with performance impact
4. **Threshold Configuration**: Set appropriate warning and critical thresholds

### Performance Considerations
1. **Measurement Overhead**: CPU measurement requires a sampling window
2. **Resource Usage**: System-wide monitoring (`all=true`) is more resource-intensive
3. **Caching Strategy**: Consider caching metrics for high-frequency access
4. **Error Handling**: Implement graceful degradation for monitoring failures

### Integration Patterns
1. **Health Checks**: Use for application health monitoring
2. **Background Services**: Continuous monitoring with configurable intervals
3. **Metrics Export**: Export to monitoring systems like Prometheus
4. **Alerting**: Integrate with alerting systems for threshold violations