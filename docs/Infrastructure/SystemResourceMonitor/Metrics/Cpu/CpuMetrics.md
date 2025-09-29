# CpuMetrics

## Overview

The `CpuMetrics` record represents comprehensive CPU performance metrics including processor information, usage statistics, and process/thread counts. This immutable data model provides a complete snapshot of CPU utilization and system activity for performance monitoring and analysis.

## Purpose

- **Performance Monitoring**: Track CPU usage and system activity metrics
- **Process Analysis**: Monitor process and thread counts for resource management
- **Performance Baselines**: Establish performance baselines and detect anomalies
- **System Diagnostics**: Provide detailed CPU information for troubleshooting

## Record Declaration

```csharp
public record CpuMetrics(long ProcessorCount, double Usage, long Threads, long Processes, long TotalThreads);
```

## Properties

### ProcessorCount
- **Type**: `long`
- **Description**: The number of logical processors (cores) available in the system
- **Usage**: Determines the theoretical maximum parallel processing capability
- **Example**: On a 4-core system with hyperthreading: `ProcessorCount = 8`

### Usage
- **Type**: `double`
- **Description**: Current CPU utilization percentage across all processors
- **Range**: 0.0 to 100.0 (percentage)
- **Calculation**: Average CPU usage over the specified measurement window
- **Example**: `Usage = 45.67` represents 45.67% CPU utilization

### Threads
- **Type**: `long`
- **Description**: Number of threads in the current process
- **Scope**: Current application process only
- **Usage**: Monitor application thread utilization and potential thread leaks

### Processes
- **Type**: `long`
- **Description**: Total number of processes running on the system
- **Scope**: System-wide process count
- **Usage**: Indicator of overall system activity and resource contention

### TotalThreads
- **Type**: `long`
- **Description**: Total number of threads across all processes in the system
- **Scope**: System-wide thread count
- **Usage**: Comprehensive view of system threading activity

## Usage Examples

### Basic CPU Monitoring
```csharp
public class CpuMonitoringService
{
    private readonly ISystemResourceMonitor _monitor;

    public CpuMonitoringService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public void DisplayCpuStatus()
    {
        var metrics = _monitor.GetMetrics(window: 1000, all: false);
        var cpu = metrics.Cpu;
        
        Console.WriteLine($"CPU Status Report:");
        Console.WriteLine($"  Processors: {cpu.ProcessorCount}");
        Console.WriteLine($"  Current Usage: {cpu.Usage:F2}%");
        Console.WriteLine($"  Usage Level: {GetUsageLevel(cpu.Usage)}");
        Console.WriteLine($"  Application Threads: {cpu.Threads}");
        Console.WriteLine($"  System Processes: {cpu.Processes:N0}");
        Console.WriteLine($"  System Threads: {cpu.TotalThreads:N0}");
        Console.WriteLine($"  Threads per Core: {cpu.TotalThreads / (double)cpu.ProcessorCount:F1}");
        Console.WriteLine($"  Threads per Process: {cpu.TotalThreads / (double)cpu.Processes:F1}");
    }

    private string GetUsageLevel(double usage) => usage switch
    {
        > 90 => "Critical",
        > 75 => "High",
        > 50 => "Moderate",
        > 25 => "Low",
        _ => "Minimal"
    };
}
```

### Performance Analysis
```csharp
public class CpuPerformanceAnalyzer
{
    public CpuAnalysisResult AnalyzeCpuPerformance(CpuMetrics cpu)
    {
        return new CpuAnalysisResult
        {
            // Core metrics
            ProcessorCount = cpu.ProcessorCount,
            UsagePercentage = cpu.Usage,
            ApplicationThreads = cpu.Threads,
            SystemProcesses = cpu.Processes,
            SystemThreads = cpu.TotalThreads,
            
            // Calculated insights
            UsageClassification = ClassifyUsage(cpu.Usage),
            ProcessorEfficiency = cpu.Usage / cpu.ProcessorCount,
            ThreadsPerProcessor = cpu.TotalThreads / (double)cpu.ProcessorCount,
            ThreadsPerProcess = cpu.TotalThreads / (double)cpu.Processes,
            
            // Performance indicators
            IsHighCpuUsage = cpu.Usage > 80,
            IsHighThreadCount = cpu.TotalThreads > cpu.ProcessorCount * 100,
            IsHighProcessCount = cpu.Processes > 200,
            
            // Resource pressure indicators
            ThreadPressure = CalculateThreadPressure(cpu),
            ProcessPressure = CalculateProcessPressure(cpu),
            
            // Recommendations
            Recommendations = GenerateRecommendations(cpu)
        };
    }

    private CpuUsageLevel ClassifyUsage(double usage) => usage switch
    {
        > 95 => CpuUsageLevel.Critical,
        > 85 => CpuUsageLevel.High,
        > 65 => CpuUsageLevel.Moderate,
        > 35 => CpuUsageLevel.Low,
        _ => CpuUsageLevel.Minimal
    };

    private double CalculateThreadPressure(CpuMetrics cpu)
    {
        // Calculate thread pressure based on threads per core
        var threadsPerCore = cpu.TotalThreads / (double)cpu.ProcessorCount;
        return Math.Min(100, threadsPerCore / 50.0 * 100); // 50 threads per core = 100% pressure
    }

    private double CalculateProcessPressure(CpuMetrics cpu)
    {
        // Calculate process pressure based on total process count
        return Math.Min(100, cpu.Processes / 300.0 * 100); // 300 processes = 100% pressure
    }

    private List<string> GenerateRecommendations(CpuMetrics cpu)
    {
        var recommendations = new List<string>();
        
        if (cpu.Usage > 90)
            recommendations.Add("Critical CPU usage detected - investigate high-CPU processes");
        else if (cpu.Usage > 75)
            recommendations.Add("High CPU usage - monitor for sustained load");
        
        if (cpu.TotalThreads > cpu.ProcessorCount * 100)
            recommendations.Add("High thread count detected - check for thread leaks");
        
        if (cpu.Processes > 250)
            recommendations.Add("High process count - consider system cleanup");
        
        var threadsPerCore = cpu.TotalThreads / (double)cpu.ProcessorCount;
        if (threadsPerCore > 75)
            recommendations.Add($"High thread density ({threadsPerCore:F1} threads/core) - optimize threading");
        
        if (cpu.Threads > cpu.ProcessorCount * 4)
            recommendations.Add($"Application using many threads ({cpu.Threads}) - review threading strategy");
        
        return recommendations;
    }
}

public enum CpuUsageLevel { Minimal, Low, Moderate, High, Critical }

public class CpuAnalysisResult
{
    public long ProcessorCount { get; set; }
    public double UsagePercentage { get; set; }
    public long ApplicationThreads { get; set; }
    public long SystemProcesses { get; set; }
    public long SystemThreads { get; set; }
    
    public CpuUsageLevel UsageClassification { get; set; }
    public double ProcessorEfficiency { get; set; }
    public double ThreadsPerProcessor { get; set; }
    public double ThreadsPerProcess { get; set; }
    
    public bool IsHighCpuUsage { get; set; }
    public bool IsHighThreadCount { get; set; }
    public bool IsHighProcessCount { get; set; }
    
    public double ThreadPressure { get; set; }
    public double ProcessPressure { get; set; }
    
    public List<string> Recommendations { get; set; } = new();
}
```

### Real-time CPU Monitoring
```csharp
public class CpuMonitoringDashboard
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly List<CpuMetrics> _history = new();
    private readonly object _lock = new();

    public CpuMonitoringDashboard(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public async Task StartMonitoring(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var metrics = _monitor.GetMetrics(window: 2000, all: false);
            var cpu = metrics.Cpu;
            
            lock (_lock)
            {
                _history.Add(cpu);
                if (_history.Count > 60) // Keep last 60 readings (5 minutes at 5-second intervals)
                {
                    _history.RemoveAt(0);
                }
            }

            // Display current status
            DisplayRealTimeStatus(cpu);
            
            // Check for alerts
            CheckForAlerts(cpu);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    private void DisplayRealTimeStatus(CpuMetrics cpu)
    {
        Console.Clear();
        Console.WriteLine("=== CPU Monitoring Dashboard ===");
        Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();
        
        // Current status
        Console.WriteLine("Current CPU Status:");
        Console.WriteLine($"  Usage: {GenerateUsageBar(cpu.Usage)} {cpu.Usage:F1}%");
        Console.WriteLine($"  Processors: {cpu.ProcessorCount}");
        Console.WriteLine($"  App Threads: {cpu.Threads}");
        Console.WriteLine($"  Processes: {cpu.Processes:N0}");
        Console.WriteLine($"  Total Threads: {cpu.TotalThreads:N0}");
        Console.WriteLine();
        
        // Historical trend
        DisplayTrend();
        
        // Performance insights
        DisplayInsights(cpu);
    }

    private string GenerateUsageBar(double usage)
    {
        var barLength = 20;
        var filled = (int)(usage / 100.0 * barLength);
        var bar = new string('█', filled) + new string('░', barLength - filled);
        
        var color = usage switch
        {
            > 90 => "🔴",
            > 75 => "🟡",
            > 50 => "🟢",
            _ => "⚪"
        };
        
        return $"{color} [{bar}]";
    }

    private void DisplayTrend()
    {
        lock (_lock)
        {
            if (_history.Count < 2) return;
            
            Console.WriteLine("5-Minute Trend:");
            var latest = _history.TakeLast(12).ToArray(); // Last minute
            var usageValues = latest.Select(h => h.Usage).ToArray();
            
            Console.WriteLine($"  Average: {usageValues.Average():F1}%");
            Console.WriteLine($"  Min: {usageValues.Min():F1}%");
            Console.WriteLine($"  Max: {usageValues.Max():F1}%");
            Console.WriteLine($"  Std Dev: {CalculateStandardDeviation(usageValues):F1}%");
            Console.WriteLine();
        }
    }

    private void DisplayInsights(CpuMetrics cpu)
    {
        Console.WriteLine("Performance Insights:");
        
        var threadsPerCore = cpu.TotalThreads / (double)cpu.ProcessorCount;
        var threadsPerProcess = cpu.TotalThreads / (double)cpu.Processes;
        
        Console.WriteLine($"  Threads per Core: {threadsPerCore:F1}");
        Console.WriteLine($"  Threads per Process: {threadsPerProcess:F1}");
        Console.WriteLine($"  Processor Efficiency: {cpu.Usage / cpu.ProcessorCount:F1}% per core");
        
        // System load classification
        var loadClass = (cpu.Usage, cpu.Processes, threadsPerCore) switch
        {
            (> 90, _, _) => "🔴 CRITICAL - High CPU usage",
            (_, > 300, _) => "🟡 WARNING - High process count",
            (_, _, > 100) => "🟡 WARNING - High thread density",
            (> 75, _, _) => "🟡 MODERATE - Elevated CPU usage",
            _ => "🟢 NORMAL - System operating normally"
        };
        
        Console.WriteLine($"  Status: {loadClass}");
    }

    private void CheckForAlerts(CpuMetrics cpu)
    {
        var alerts = new List<string>();
        
        if (cpu.Usage > 95)
            alerts.Add("🚨 CRITICAL: CPU usage exceeds 95%");
        else if (cpu.Usage > 85)
            alerts.Add("⚠️ WARNING: High CPU usage detected");
        
        if (cpu.TotalThreads > cpu.ProcessorCount * 150)
            alerts.Add("⚠️ WARNING: Very high thread count");
        
        if (cpu.Processes > 400)
            alerts.Add("⚠️ WARNING: High process count");
        
        // Check for sustained high usage
        lock (_lock)
        {
            if (_history.Count >= 6)
            {
                var recent = _history.TakeLast(6).ToArray(); // Last 30 seconds
                if (recent.All(h => h.Usage > 80))
                    alerts.Add("🔥 ALERT: Sustained high CPU usage");
            }
        }
        
        if (alerts.Any())
        {
            Console.WriteLine();
            Console.WriteLine("ALERTS:");
            foreach (var alert in alerts)
            {
                Console.WriteLine($"  {alert}");
            }
        }
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        
        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Length - 1));
    }

    public CpuTrendAnalysis GetTrendAnalysis()
    {
        lock (_lock)
        {
            if (_history.Count < 2)
                return new CpuTrendAnalysis { Status = "Insufficient data" };
            
            var usageValues = _history.Select(h => h.Usage).ToArray();
            var threadCounts = _history.Select(h => h.TotalThreads).ToArray();
            var processCounts = _history.Select(h => h.Processes).ToArray();
            
            return new CpuTrendAnalysis
            {
                DataPoints = _history.Count,
                TimeSpan = TimeSpan.FromSeconds(_history.Count * 5),
                
                UsageStats = new TrendStats
                {
                    Current = usageValues.Last(),
                    Average = usageValues.Average(),
                    Minimum = usageValues.Min(),
                    Maximum = usageValues.Max(),
                    StandardDeviation = CalculateStandardDeviation(usageValues),
                    Trend = CalculateTrend(usageValues)
                },
                
                ThreadStats = new TrendStats
                {
                    Current = threadCounts.Last(),
                    Average = threadCounts.Average(),
                    Minimum = threadCounts.Min(),
                    Maximum = threadCounts.Max(),
                    StandardDeviation = CalculateStandardDeviation(threadCounts.Select(t => (double)t).ToArray()),
                    Trend = CalculateTrend(threadCounts.Select(t => (double)t).ToArray())
                },
                
                ProcessStats = new TrendStats
                {
                    Current = processCounts.Last(),
                    Average = processCounts.Average(),
                    Minimum = processCounts.Min(),
                    Maximum = processCounts.Max(),
                    StandardDeviation = CalculateStandardDeviation(processCounts.Select(p => (double)p).ToArray()),
                    Trend = CalculateTrend(processCounts.Select(p => (double)p).ToArray())
                },
                
                Status = DetermineTrendStatus(usageValues, threadCounts, processCounts)
            };
        }
    }

    private string CalculateTrend(double[] values)
    {
        if (values.Length < 3) return "Stable";
        
        var recentAvg = values.TakeLast(values.Length / 3).Average();
        var earlierAvg = values.Take(values.Length / 3).Average();
        
        var change = (recentAvg - earlierAvg) / earlierAvg * 100;
        
        return change switch
        {
            > 10 => "Rising",
            < -10 => "Falling",
            _ => "Stable"
        };
    }

    private string DetermineTrendStatus(double[] usage, long[] threads, long[] processes)
    {
        var avgUsage = usage.Average();
        var avgThreads = threads.Average();
        var avgProcesses = processes.Average();
        
        return (avgUsage, avgThreads, avgProcesses) switch
        {
            (> 85, _, _) => "Critical - High CPU usage",
            (_, > 10000, _) => "Warning - High thread count",
            (_, _, > 350) => "Warning - High process count",
            (> 70, _, _) => "Moderate - Elevated usage",
            _ => "Normal - System healthy"
        };
    }
}

public class CpuTrendAnalysis
{
    public int DataPoints { get; set; }
    public TimeSpan TimeSpan { get; set; }
    public TrendStats UsageStats { get; set; } = new();
    public TrendStats ThreadStats { get; set; } = new();
    public TrendStats ProcessStats { get; set; } = new();
    public string Status { get; set; } = "";
}

public class TrendStats
{
    public double Current { get; set; }
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double StandardDeviation { get; set; }
    public string Trend { get; set; } = "";
}
```

### Alert System Integration
```csharp
public class CpuAlertSystem
{
    private readonly ILogger<CpuAlertSystem> _logger;
    private readonly CpuThresholds _thresholds;
    private readonly Dictionary<string, DateTime> _lastAlerts = new();

    public CpuAlertSystem(ILogger<CpuAlertSystem> logger, IOptions<CpuThresholds> thresholds)
    {
        _logger = logger;
        _thresholds = thresholds.Value;
    }

    public List<CpuAlert> CheckAlerts(CpuMetrics cpu)
    {
        var alerts = new List<CpuAlert>();
        var now = DateTime.UtcNow;
        
        // CPU Usage alerts
        if (cpu.Usage > _thresholds.CriticalUsage)
        {
            alerts.Add(CreateAlert("cpu_usage_critical", AlertLevel.Critical,
                $"Critical CPU usage: {cpu.Usage:F1}%", cpu, now));
        }
        else if (cpu.Usage > _thresholds.WarningUsage)
        {
            alerts.Add(CreateAlert("cpu_usage_warning", AlertLevel.Warning,
                $"High CPU usage: {cpu.Usage:F1}%", cpu, now));
        }
        
        // Thread count alerts
        var threadsPerCore = cpu.TotalThreads / (double)cpu.ProcessorCount;
        if (threadsPerCore > _thresholds.CriticalThreadsPerCore)
        {
            alerts.Add(CreateAlert("thread_density_critical", AlertLevel.Critical,
                $"Critical thread density: {threadsPerCore:F1} threads per core", cpu, now));
        }
        else if (threadsPerCore > _thresholds.WarningThreadsPerCore)
        {
            alerts.Add(CreateAlert("thread_density_warning", AlertLevel.Warning,
                $"High thread density: {threadsPerCore:F1} threads per core", cpu, now));
        }
        
        // Process count alerts
        if (cpu.Processes > _thresholds.CriticalProcessCount)
        {
            alerts.Add(CreateAlert("process_count_critical", AlertLevel.Critical,
                $"Critical process count: {cpu.Processes}", cpu, now));
        }
        else if (cpu.Processes > _thresholds.WarningProcessCount)
        {
            alerts.Add(CreateAlert("process_count_warning", AlertLevel.Warning,
                $"High process count: {cpu.Processes}", cpu, now));
        }
        
        // Application thread alerts
        if (cpu.Threads > _thresholds.CriticalAppThreads)
        {
            alerts.Add(CreateAlert("app_threads_critical", AlertLevel.Critical,
                $"Critical application thread count: {cpu.Threads}", cpu, now));
        }
        
        // Filter for rate limiting
        return alerts.Where(alert => ShouldSendAlert(alert.Type, now)).ToList();
    }

    private CpuAlert CreateAlert(string type, AlertLevel level, string message, CpuMetrics cpu, DateTime timestamp)
    {
        return new CpuAlert
        {
            Type = type,
            Level = level,
            Message = message,
            Timestamp = timestamp,
            CpuMetrics = cpu,
            Details = new Dictionary<string, object>
            {
                ["processor_count"] = cpu.ProcessorCount,
                ["usage_percent"] = cpu.Usage,
                ["threads"] = cpu.Threads,
                ["processes"] = cpu.Processes,
                ["total_threads"] = cpu.TotalThreads,
                ["threads_per_core"] = cpu.TotalThreads / (double)cpu.ProcessorCount,
                ["threads_per_process"] = cpu.TotalThreads / (double)cpu.Processes
            }
        };
    }

    private bool ShouldSendAlert(string alertType, DateTime now)
    {
        var cooldownPeriod = alertType.Contains("critical") ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5);
        
        if (_lastAlerts.TryGetValue(alertType, out var lastAlert))
        {
            if (now - lastAlert < cooldownPeriod)
                return false;
        }
        
        _lastAlerts[alertType] = now;
        return true;
    }
}

public class CpuAlert
{
    public string Type { get; set; } = "";
    public AlertLevel Level { get; set; }
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public CpuMetrics CpuMetrics { get; set; } = default!;
    public Dictionary<string, object> Details { get; set; } = new();
}

public enum AlertLevel { Info, Warning, Critical }

public class CpuThresholds
{
    public double WarningUsage { get; set; } = 75.0;
    public double CriticalUsage { get; set; } = 90.0;
    public long WarningProcessCount { get; set; } = 250;
    public long CriticalProcessCount { get; set; } = 400;
    public double WarningThreadsPerCore { get; set; } = 50.0;
    public double CriticalThreadsPerCore { get; set; } = 100.0;
    public long CriticalAppThreads { get; set; } = 100;
}
```

### Metrics Export and Integration
```csharp
public class CpuMetricsExporter
{
    public string ExportPrometheusMetrics(CpuMetrics cpu)
    {
        var sb = new StringBuilder();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        sb.AppendLine("# HELP cpu_usage_percent Current CPU usage percentage");
        sb.AppendLine("# TYPE cpu_usage_percent gauge");
        sb.AppendLine($"cpu_usage_percent {cpu.Usage:F2} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP cpu_processor_count Number of logical processors");
        sb.AppendLine("# TYPE cpu_processor_count gauge");
        sb.AppendLine($"cpu_processor_count {cpu.ProcessorCount} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP cpu_application_threads Application thread count");
        sb.AppendLine("# TYPE cpu_application_threads gauge");
        sb.AppendLine($"cpu_application_threads {cpu.Threads} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP cpu_system_processes System process count");
        sb.AppendLine("# TYPE cpu_system_processes gauge");
        sb.AppendLine($"cpu_system_processes {cpu.Processes} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP cpu_system_threads Total system thread count");
        sb.AppendLine("# TYPE cpu_system_threads gauge");
        sb.AppendLine($"cpu_system_threads {cpu.TotalThreads} {timestamp}");
        sb.AppendLine();
        
        // Calculated metrics
        var threadsPerCore = cpu.TotalThreads / (double)cpu.ProcessorCount;
        sb.AppendLine("# HELP cpu_threads_per_core Average threads per processor core");
        sb.AppendLine("# TYPE cpu_threads_per_core gauge");
        sb.AppendLine($"cpu_threads_per_core {threadsPerCore:F2} {timestamp}");
        sb.AppendLine();
        
        var threadsPerProcess = cpu.TotalThreads / (double)cpu.Processes;
        sb.AppendLine("# HELP cpu_threads_per_process Average threads per process");
        sb.AppendLine("# TYPE cpu_threads_per_process gauge");
        sb.AppendLine($"cpu_threads_per_process {threadsPerProcess:F2} {timestamp}");
        
        return sb.ToString();
    }

    public object ExportJsonMetrics(CpuMetrics cpu)
    {
        return new
        {
            timestamp = DateTime.UtcNow,
            cpu = new
            {
                usage_percent = Math.Round(cpu.Usage, 2),
                processor_count = cpu.ProcessorCount,
                application_threads = cpu.Threads,
                system_processes = cpu.Processes,
                system_threads = cpu.TotalThreads,
                calculated = new
                {
                    threads_per_core = Math.Round(cpu.TotalThreads / (double)cpu.ProcessorCount, 2),
                    threads_per_process = Math.Round(cpu.TotalThreads / (double)cpu.Processes, 2),
                    processor_efficiency = Math.Round(cpu.Usage / cpu.ProcessorCount, 2),
                    usage_classification = ClassifyUsage(cpu.Usage),
                    thread_pressure = CalculateThreadPressure(cpu),
                    process_pressure = CalculateProcessPressure(cpu)
                }
            }
        };
    }

    private string ClassifyUsage(double usage) => usage switch
    {
        > 95 => "critical",
        > 85 => "high",
        > 65 => "moderate",
        > 35 => "low",
        _ => "minimal"
    };

    private string CalculateThreadPressure(CpuMetrics cpu)
    {
        var threadsPerCore = cpu.TotalThreads / (double)cpu.ProcessorCount;
        return threadsPerCore switch
        {
            > 100 => "critical",
            > 75 => "high",
            > 50 => "moderate",
            > 25 => "low",
            _ => "minimal"
        };
    }

    private string CalculateProcessPressure(CpuMetrics cpu) => cpu.Processes switch
    {
        > 400 => "critical",
        > 300 => "high",
        > 200 => "moderate",
        > 100 => "low",
        _ => "minimal"
    };
}
```

## Properties Deep Dive

### ProcessorCount
- **System Information**: Reflects the number of logical processors (including hyperthreading)
- **Scaling Factor**: Used to normalize CPU usage and calculate efficiency metrics
- **Thread Capacity**: Guideline for optimal thread pool sizing
- **Performance Baseline**: Essential for comparative performance analysis

### Usage Calculation
- **Measurement Window**: Calculated over the specified measurement window (default: 1000ms)
- **Average Value**: Represents average CPU utilization during the measurement period
- **All Cores**: Aggregated usage across all processor cores
- **Range Validation**: Values should be between 0.0 and 100.0

### Thread Metrics
- **Application Scope**: `Threads` represents current process threads only
- **System Scope**: `TotalThreads` includes all system processes
- **Resource Monitoring**: High thread counts may indicate resource leaks
- **Performance Impact**: Excessive threading can reduce system performance

### Process Monitoring
- **System Activity**: `Processes` indicates overall system activity level
- **Resource Contention**: High process counts may indicate resource contention
- **Baseline Establishment**: Normal process counts vary by system type and usage

## Performance Considerations

### Measurement Overhead
1. **Sampling Frequency**: Balance monitoring needs with system overhead
2. **Measurement Windows**: Longer windows provide more stable readings
3. **Scope Selection**: Choose between process-specific and system-wide monitoring
4. **Resource Usage**: Monitor the monitoring overhead itself

### Accuracy Factors
1. **Timing Windows**: CPU usage accuracy depends on measurement window length
2. **System Load**: High system load may affect measurement accuracy
3. **Process State**: Thread and process counts may fluctuate rapidly
4. **Measurement Context**: Consider measurement context (startup, steady-state, etc.)

## Related Components

- **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** - Aggregate metrics container
- **[CpuMetricsClient](CpuMetricsClient.md)** - CPU metrics collection client
- **[MemoryMetrics](MemoryMetrics.md)** - Memory performance metrics
- **[ISystemResourceMonitor](ISystemResourceMonitor.md)** - Main monitoring interface
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Best Practices

### Monitoring Strategy
1. **Baseline Establishment**: Establish normal CPU usage patterns for your application
2. **Threshold Configuration**: Set appropriate warning and critical thresholds
3. **Trend Analysis**: Monitor trends rather than individual readings
4. **Context Awareness**: Consider system context when interpreting metrics

### Alert Configuration
1. **Usage Thresholds**: Configure alerts for sustained high usage (>80% for >30 seconds)
2. **Thread Monitoring**: Alert on excessive thread counts (>4x processor count for applications)
3. **Process Monitoring**: Monitor for unusual process count changes
4. **Rate Limiting**: Implement alert rate limiting to prevent notification spam

### Integration Guidelines
1. **Structured Logging**: Include CPU metrics in structured log entries
2. **Health Checks**: Use CPU metrics for application health assessment
3. **Performance Testing**: Include CPU monitoring in performance test suites
4. **Capacity Planning**: Use historical CPU data for capacity planning decisions