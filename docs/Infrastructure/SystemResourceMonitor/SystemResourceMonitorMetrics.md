# SystemResourceMonitorMetrics

## Overview

The `SystemResourceMonitorMetrics` record provides a comprehensive aggregate data model containing all system resource metrics including CPU, memory, and disk drive information. This immutable record serves as the primary data transfer object for complete system performance analysis.

## Purpose

- **Comprehensive System View**: Single object containing all system resource metrics
- **Immutable Data Model**: Thread-safe record type for consistent data representation
- **Performance Analysis**: Complete system state for performance monitoring and analysis
- **Integration Ready**: Structured data model for monitoring systems and APIs

## Record Declaration

```csharp
public record SystemResourceMonitorMetrics(CpuMetrics Cpu, MemoryMetrics Memory, SystemDriveMetrics[] Drives);
```

## Properties

### Cpu
- **Type**: `CpuMetrics`
- **Description**: Complete CPU performance metrics including usage, thread counts, and process information
- **Contents**: Processor count, usage percentage, thread counts, and process statistics

### Memory  
- **Type**: `MemoryMetrics`
- **Description**: System memory utilization metrics
- **Contents**: Total memory, free memory, used memory, and usage percentage

### Drives
- **Type**: `SystemDriveMetrics[]`
- **Description**: Array of disk drive metrics for all system drives
- **Contents**: Drive information including name, total size, free space, and usage percentage

## Usage Examples

### Basic Metrics Access
```csharp
public class SystemPerformanceService
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemPerformanceService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public void DisplaySystemStatus()
    {
        var metrics = _monitor.GetMetrics(window: 1000, all: false);
        
        // CPU Information
        Console.WriteLine($"CPU Usage: {metrics.Cpu.Usage:F2}%");
        Console.WriteLine($"Processors: {metrics.Cpu.ProcessorCount}");
        Console.WriteLine($"Processes: {metrics.Cpu.Processes}");
        Console.WriteLine($"Total Threads: {metrics.Cpu.TotalThreads}");
        
        // Memory Information
        Console.WriteLine($"Memory Usage: {metrics.Memory.UsagePercentage:F2}%");
        Console.WriteLine($"Total Memory: {metrics.Memory.Total:F0} MB");
        Console.WriteLine($"Free Memory: {metrics.Memory.Free:F0} MB");
        Console.WriteLine($"Used Memory: {metrics.Memory.Used:F0} MB");
        
        // Drive Information
        Console.WriteLine($"Drives: {metrics.Drives.Length}");
        foreach (var drive in metrics.Drives)
        {
            Console.WriteLine($"  {drive.Name}: {drive.UsagePercentage:F1}% used " +
                            $"({drive.FreeSpace / 1024.0:F1} GB free of {drive.TotalSize / 1024.0:F1} GB)");
        }
    }
}
```

### Performance Analysis
```csharp
public class SystemPerformanceAnalyzer
{
    public SystemPerformanceReport AnalyzeSystem(SystemResourceMonitorMetrics metrics)
    {
        return new SystemPerformanceReport
        {
            Timestamp = DateTime.UtcNow,
            
            // Overall system assessment
            OverallHealth = AssessOverallHealth(metrics),
            PerformanceScore = CalculatePerformanceScore(metrics),
            
            // CPU Analysis
            CpuAnalysis = new CpuAnalysis
            {
                UsageLevel = ClassifyCpuUsage(metrics.Cpu.Usage),
                ProcessorEfficiency = metrics.Cpu.Usage / metrics.Cpu.ProcessorCount,
                ThreadsPerProcess = metrics.Cpu.TotalThreads / (double)metrics.Cpu.Processes,
                IsHighThreadUsage = metrics.Cpu.TotalThreads > metrics.Cpu.ProcessorCount * 100
            },
            
            // Memory Analysis
            MemoryAnalysis = new MemoryAnalysis
            {
                UsageLevel = ClassifyMemoryUsage(metrics.Memory.UsagePercentage),
                AvailableMemoryGB = metrics.Memory.Free / 1024.0,
                MemoryPressure = metrics.Memory.UsagePercentage > 85,
                RecommendedAction = GetMemoryRecommendation(metrics.Memory)
            },
            
            // Storage Analysis
            StorageAnalysis = new StorageAnalysis
            {
                DriveCount = metrics.Drives.Length,
                TotalStorageGB = metrics.Drives.Sum(d => d.TotalSize) / 1024.0,
                FreeStorageGB = metrics.Drives.Sum(d => d.FreeSpace) / 1024.0,
                HighestUsageDrive = metrics.Drives.OrderByDescending(d => d.UsagePercentage).FirstOrDefault(),
                CriticalDrives = metrics.Drives.Where(d => d.UsagePercentage > 90).ToArray(),
                WarningDrives = metrics.Drives.Where(d => d.UsagePercentage > 80 && d.UsagePercentage <= 90).ToArray()
            }
        };
    }

    private SystemHealth AssessOverallHealth(SystemResourceMonitorMetrics metrics)
    {
        var cpuCritical = metrics.Cpu.Usage > 90;
        var memoryCritical = metrics.Memory.UsagePercentage > 95;
        var diskCritical = metrics.Drives.Any(d => d.UsagePercentage > 95);
        
        if (cpuCritical || memoryCritical || diskCritical)
            return SystemHealth.Critical;
        
        var cpuHigh = metrics.Cpu.Usage > 75;
        var memoryHigh = metrics.Memory.UsagePercentage > 85;
        var diskHigh = metrics.Drives.Any(d => d.UsagePercentage > 85);
        
        if (cpuHigh || memoryHigh || diskHigh)
            return SystemHealth.Warning;
        
        return SystemHealth.Healthy;
    }

    private int CalculatePerformanceScore(SystemResourceMonitorMetrics metrics)
    {
        // Calculate performance score (0-100)
        var cpuScore = Math.Max(0, 100 - metrics.Cpu.Usage);
        var memoryScore = Math.Max(0, 100 - metrics.Memory.UsagePercentage);
        var diskScore = metrics.Drives.Any() 
            ? Math.Max(0, 100 - metrics.Drives.Max(d => d.UsagePercentage))
            : 100;
        
        return (int)((cpuScore + memoryScore + diskScore) / 3);
    }
}

public enum SystemHealth { Healthy, Warning, Critical }
public enum UsageLevel { Low, Moderate, High, Critical }
```

### Monitoring Dashboard Data
```csharp
public class SystemDashboardService
{
    private readonly ISystemResourceMonitor _monitor;

    public SystemDashboardService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public async Task<object> GetDashboardData()
    {
        var metrics = _monitor.GetMetrics(window: 2000, all: false);
        
        return new
        {
            timestamp = DateTime.UtcNow,
            system = new
            {
                cpu = new
                {
                    usage_percent = Math.Round(metrics.Cpu.Usage, 2),
                    processor_count = metrics.Cpu.ProcessorCount,
                    processes = metrics.Cpu.Processes,
                    threads = metrics.Cpu.Threads,
                    total_threads = metrics.Cpu.TotalThreads,
                    threads_per_core = Math.Round(metrics.Cpu.TotalThreads / (double)metrics.Cpu.ProcessorCount, 1),
                    status = GetCpuStatus(metrics.Cpu.Usage)
                },
                memory = new
                {
                    usage_percent = Math.Round(metrics.Memory.UsagePercentage, 2),
                    total_gb = Math.Round(metrics.Memory.Total / 1024.0, 2),
                    free_gb = Math.Round(metrics.Memory.Free / 1024.0, 2),
                    used_gb = Math.Round(metrics.Memory.Used / 1024.0, 2),
                    status = GetMemoryStatus(metrics.Memory.UsagePercentage)
                },
                storage = new
                {
                    drive_count = metrics.Drives.Length,
                    total_gb = Math.Round(metrics.Drives.Sum(d => d.TotalSize) / 1024.0, 2),
                    free_gb = Math.Round(metrics.Drives.Sum(d => d.FreeSpace) / 1024.0, 2),
                    drives = metrics.Drives.Select(drive => new
                    {
                        name = drive.Name,
                        total_gb = Math.Round(drive.TotalSize / 1024.0, 2),
                        free_gb = Math.Round(drive.FreeSpace / 1024.0, 2),
                        used_gb = Math.Round((drive.TotalSize - drive.FreeSpace) / 1024.0, 2),
                        usage_percent = Math.Round(drive.UsagePercentage, 2),
                        status = GetDiskStatus(drive.UsagePercentage)
                    }).ToArray()
                }
            },
            performance_score = CalculateOverallScore(metrics),
            alerts = GenerateAlerts(metrics)
        };
    }

    private string GetCpuStatus(double usage) => usage switch
    {
        > 90 => "critical",
        > 75 => "warning",
        > 50 => "moderate",
        _ => "normal"
    };

    private string GetMemoryStatus(double usage) => usage switch
    {
        > 95 => "critical",
        > 85 => "warning",
        > 70 => "moderate",
        _ => "normal"
    };

    private string GetDiskStatus(double usage) => usage switch
    {
        > 95 => "critical",
        > 85 => "warning",
        > 75 => "moderate",
        _ => "normal"
    };
}
```

### Health Check Integration
```csharp
public class SystemResourceHealthCheck : IHealthCheck
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly SystemThresholds _thresholds;

    public SystemResourceHealthCheck(ISystemResourceMonitor monitor, IOptions<SystemThresholds> thresholds)
    {
        _monitor = monitor;
        _thresholds = thresholds.Value;
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
                ["drive_count"] = metrics.Drives.Length,
                ["processor_count"] = metrics.Cpu.ProcessorCount,
                ["total_memory_gb"] = Math.Round(metrics.Memory.Total / 1024.0, 2),
                ["free_memory_gb"] = Math.Round(metrics.Memory.Free / 1024.0, 2)
            };

            var issues = new List<string>();
            var status = HealthStatus.Healthy;

            // Check CPU thresholds
            if (metrics.Cpu.Usage > _thresholds.Cpu.Critical)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical CPU usage: {metrics.Cpu.Usage:F1}%");
            }
            else if (metrics.Cpu.Usage > _thresholds.Cpu.Warning)
            {
                status = HealthStatus.Degraded;
                issues.Add($"High CPU usage: {metrics.Cpu.Usage:F1}%");
            }

            // Check memory thresholds
            if (metrics.Memory.UsagePercentage > _thresholds.Memory.Critical)
            {
                status = HealthStatus.Unhealthy;
                issues.Add($"Critical memory usage: {metrics.Memory.UsagePercentage:F1}%");
            }
            else if (metrics.Memory.UsagePercentage > _thresholds.Memory.Warning)
            {
                if (status == HealthStatus.Healthy) status = HealthStatus.Degraded;
                issues.Add($"High memory usage: {metrics.Memory.UsagePercentage:F1}%");
            }

            // Check disk thresholds
            foreach (var drive in metrics.Drives)
            {
                if (drive.UsagePercentage > _thresholds.Disk.Critical)
                {
                    status = HealthStatus.Unhealthy;
                    issues.Add($"Critical disk usage on {drive.Name}: {drive.UsagePercentage:F1}%");
                }
                else if (drive.UsagePercentage > _thresholds.Disk.Warning)
                {
                    if (status == HealthStatus.Healthy) status = HealthStatus.Degraded;
                    issues.Add($"High disk usage on {drive.Name}: {drive.UsagePercentage:F1}%");
                }
            }

            var description = issues.Any() ? string.Join("; ", issues) : "All system resources within normal limits";
            
            return new HealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to collect system resource metrics", ex);
        }
    }
}
```

### Prometheus Metrics Export
```csharp
public class SystemResourcePrometheusExporter
{
    public string ExportMetrics(SystemResourceMonitorMetrics metrics)
    {
        var sb = new StringBuilder();
        
        // CPU Metrics
        sb.AppendLine("# HELP system_cpu_usage_percent CPU usage percentage");
        sb.AppendLine("# TYPE system_cpu_usage_percent gauge");
        sb.AppendLine($"system_cpu_usage_percent {metrics.Cpu.Usage:F2}");
        
        sb.AppendLine("# HELP system_cpu_processor_count Number of processors");
        sb.AppendLine("# TYPE system_cpu_processor_count gauge");
        sb.AppendLine($"system_cpu_processor_count {metrics.Cpu.ProcessorCount}");
        
        sb.AppendLine("# HELP system_cpu_processes Total number of processes");
        sb.AppendLine("# TYPE system_cpu_processes gauge");
        sb.AppendLine($"system_cpu_processes {metrics.Cpu.Processes}");
        
        sb.AppendLine("# HELP system_cpu_threads Current process thread count");
        sb.AppendLine("# TYPE system_cpu_threads gauge");
        sb.AppendLine($"system_cpu_threads {metrics.Cpu.Threads}");
        
        sb.AppendLine("# HELP system_cpu_total_threads Total system thread count");
        sb.AppendLine("# TYPE system_cpu_total_threads gauge");
        sb.AppendLine($"system_cpu_total_threads {metrics.Cpu.TotalThreads}");
        
        // Memory Metrics
        sb.AppendLine("# HELP system_memory_usage_percent Memory usage percentage");
        sb.AppendLine("# TYPE system_memory_usage_percent gauge");
        sb.AppendLine($"system_memory_usage_percent {metrics.Memory.UsagePercentage:F2}");
        
        sb.AppendLine("# HELP system_memory_total_mb Total system memory in MB");
        sb.AppendLine("# TYPE system_memory_total_mb gauge");
        sb.AppendLine($"system_memory_total_mb {metrics.Memory.Total:F0}");
        
        sb.AppendLine("# HELP system_memory_free_mb Free system memory in MB");
        sb.AppendLine("# TYPE system_memory_free_mb gauge");
        sb.AppendLine($"system_memory_free_mb {metrics.Memory.Free:F0}");
        
        sb.AppendLine("# HELP system_memory_used_mb Used system memory in MB");
        sb.AppendLine("# TYPE system_memory_used_mb gauge");
        sb.AppendLine($"system_memory_used_mb {metrics.Memory.Used:F0}");
        
        // Drive Metrics
        sb.AppendLine("# HELP system_disk_usage_percent Disk usage percentage");
        sb.AppendLine("# TYPE system_disk_usage_percent gauge");
        foreach (var drive in metrics.Drives)
        {
            sb.AppendLine($"system_disk_usage_percent{{drive=\"{drive.Name}\"}} {drive.UsagePercentage:F2}");
        }
        
        sb.AppendLine("# HELP system_disk_total_gb Total disk space in GB");
        sb.AppendLine("# TYPE system_disk_total_gb gauge");
        foreach (var drive in metrics.Drives)
        {
            sb.AppendLine($"system_disk_total_gb{{drive=\"{drive.Name}\"}} {drive.TotalSize / 1024.0:F2}");
        }
        
        sb.AppendLine("# HELP system_disk_free_gb Free disk space in GB");
        sb.AppendLine("# TYPE system_disk_free_gb gauge");
        foreach (var drive in metrics.Drives)
        {
            sb.AppendLine($"system_disk_free_gb{{drive=\"{drive.Name}\"}} {drive.FreeSpace / 1024.0:F2}");
        }
        
        return sb.ToString();
    }
}
```

### JSON Serialization
```csharp
// Custom JSON converter for clean serialization
public class SystemResourceMetricsJsonConverter : JsonConverter<SystemResourceMonitorMetrics>
{
    public override void Write(Utf8JsonWriter writer, SystemResourceMonitorMetrics value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WritePropertyName("timestamp");
        writer.WriteStringValue(DateTime.UtcNow);
        
        // CPU section
        writer.WritePropertyName("cpu");
        writer.WriteStartObject();
        writer.WritePropertyName("usage_percent");
        writer.WriteNumberValue(Math.Round(value.Cpu.Usage, 2));
        writer.WritePropertyName("processor_count");
        writer.WriteNumberValue(value.Cpu.ProcessorCount);
        writer.WritePropertyName("processes");
        writer.WriteNumberValue(value.Cpu.Processes);
        writer.WritePropertyName("threads");
        writer.WriteNumberValue(value.Cpu.Threads);
        writer.WritePropertyName("total_threads");
        writer.WriteNumberValue(value.Cpu.TotalThreads);
        writer.WriteEndObject();
        
        // Memory section
        writer.WritePropertyName("memory");
        writer.WriteStartObject();
        writer.WritePropertyName("usage_percent");
        writer.WriteNumberValue(Math.Round(value.Memory.UsagePercentage, 2));
        writer.WritePropertyName("total_mb");
        writer.WriteNumberValue(Math.Round(value.Memory.Total, 0));
        writer.WritePropertyName("free_mb");
        writer.WriteNumberValue(Math.Round(value.Memory.Free, 0));
        writer.WritePropertyName("used_mb");
        writer.WriteNumberValue(Math.Round(value.Memory.Used, 0));
        writer.WriteEndObject();
        
        // Drives section
        writer.WritePropertyName("drives");
        writer.WriteStartArray();
        foreach (var drive in value.Drives)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteStringValue(drive.Name);
            writer.WritePropertyName("total_gb");
            writer.WriteNumberValue(Math.Round(drive.TotalSize / 1024.0, 2));
            writer.WritePropertyName("free_gb");
            writer.WriteNumberValue(Math.Round(drive.FreeSpace / 1024.0, 2));
            writer.WritePropertyName("usage_percent");
            writer.WriteNumberValue(Math.Round(drive.UsagePercentage, 2));
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        
        writer.WriteEndObject();
    }

    public override SystemResourceMonitorMetrics Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization not supported for SystemResourceMonitorMetrics");
    }
}
```

## Record Benefits

### Immutability
- **Thread-Safe**: Immutable records are inherently thread-safe for reading
- **Consistent State**: No risk of partial updates or state corruption
- **Cacheable**: Safe to cache and share across multiple consumers

### Value Semantics
- **Equality**: Automatic value-based equality comparison
- **Hashing**: Consistent hash codes for dictionary keys
- **Comparison**: Structural equality for testing and validation

### Performance
- **Memory Efficient**: Value types with minimal object overhead
- **Copy Semantics**: Efficient copying for snapshot scenarios
- **Garbage Collection**: Reduced GC pressure compared to mutable classes

## Integration Patterns

### Background Monitoring
```csharp
public class SystemMetricsCollector : BackgroundService
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly List<SystemResourceMonitorMetrics> _history = new();
    private readonly object _lock = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var metrics = _monitor.GetMetrics(window: 2000, all: false);
            
            lock (_lock)
            {
                _history.Add(metrics);
                if (_history.Count > 100) // Keep last 100 readings
                {
                    _history.RemoveAt(0);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    public SystemResourceMonitorMetrics[] GetHistory()
    {
        lock (_lock)
        {
            return _history.ToArray();
        }
    }
}
```

### Caching Strategy
```csharp
public class CachedSystemResourceMonitor : ISystemResourceMonitor
{
    private readonly ISystemResourceMonitor _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromSeconds(5);

    public SystemResourceMonitorMetrics GetMetrics(long window, bool all = false)
    {
        var cacheKey = $"system_metrics_{window}_{all}";
        
        if (_cache.TryGetValue(cacheKey, out SystemResourceMonitorMetrics cached))
        {
            return cached;
        }

        var metrics = _inner.GetMetrics(window, all);
        _cache.Set(cacheKey, metrics, _cacheTimeout);
        
        return metrics;
    }
}
```

## Related Components

- **[ISystemResourceMonitor](ISystemResourceMonitor.md)** - Main monitoring interface
- **[SystemResourceMonitorExtensions](SystemResourceMonitorExtensions.md)** - Dependency injection setup
- **[CPU Metrics](Metrics/Cpu/CpuMetrics.md)** - CPU performance metrics
- **[Memory Metrics](Metrics/Memory/MemoryMetrics.md)** - Memory utilization metrics
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Best Practices

### Data Usage
1. **Snapshot Semantics**: Treat as point-in-time snapshots of system state
2. **Appropriate Caching**: Cache for short periods to reduce monitoring overhead
3. **Trend Analysis**: Collect multiple snapshots for trend analysis
4. **Alert Thresholds**: Use consistent thresholds across all monitoring scenarios

### Performance Considerations
1. **Collection Frequency**: Balance monitoring needs with system overhead
2. **Measurement Windows**: Use appropriate CPU measurement windows
3. **Scope Selection**: Choose between current process and system-wide monitoring
4. **Resource Management**: Monitor the monitoring overhead itself

### Integration Guidelines
1. **Structured Logging**: Include metrics in structured log entries
2. **Health Checks**: Use for operational health monitoring
3. **Metrics Export**: Export to monitoring platforms like Prometheus
4. **Dashboard Integration**: Design for real-time dashboard display