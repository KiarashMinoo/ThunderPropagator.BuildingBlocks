# CpuMetricsClient

## Overview

The `CpuMetricsClient` class is the internal implementation responsible for collecting CPU performance metrics from the operating system. This client provides accurate CPU usage measurements using process timing information and system-wide statistics, supporting both application-specific and system-wide monitoring modes.

## Purpose

- **CPU Usage Measurement**: Accurate CPU utilization calculation using processor time sampling
- **Process Monitoring**: Collection of process and thread statistics
- **Flexible Scope**: Support for both application-specific and system-wide monitoring
- **Performance Tracking**: Real-time CPU performance data collection

## Class Declaration

```csharp
internal class CpuMetricsClient
{
    public CpuMetrics GetMetrics(long window, bool all = false)
}
```

## Methods

### GetMetrics(long window, bool all = false)

Collects comprehensive CPU metrics over a specified measurement window.

#### Parameters
- **window** (`long`): Measurement window duration in milliseconds (default: 1000ms)
- **all** (`bool`): When `true`, measures system-wide CPU usage; when `false`, measures current process only

#### Returns
- **CpuMetrics**: Complete CPU performance metrics including usage, counts, and system information

#### Implementation Details

The method performs CPU measurement using a two-phase approach:

1. **Initial Sampling**: Records baseline CPU time and process information
2. **Measurement Window**: Waits for specified duration while system operates
3. **Final Sampling**: Records end-state CPU time and calculates usage
4. **Calculation**: Computes CPU usage percentage based on processor time differences

## Usage Examples

### Basic CPU Monitoring
```csharp
public class CpuMonitoringService
{
    private readonly CpuMetricsClient _client;

    public CpuMonitoringService()
    {
        _client = new CpuMetricsClient();
    }

    public void DisplayCurrentCpuStatus()
    {
        // Monitor current application only with 1-second window
        var appMetrics = _client.GetMetrics(window: 1000, all: false);
        
        Console.WriteLine("Application CPU Metrics:");
        Console.WriteLine($"  Application CPU Usage: {appMetrics.Usage:F2}%");
        Console.WriteLine($"  Application Threads: {appMetrics.Threads}");
        Console.WriteLine($"  Processors Available: {appMetrics.ProcessorCount}");
        
        // Monitor entire system with 2-second window
        var systemMetrics = _client.GetMetrics(window: 2000, all: true);
        
        Console.WriteLine("\nSystem-Wide CPU Metrics:");
        Console.WriteLine($"  System CPU Usage: {systemMetrics.Usage:F2}%");
        Console.WriteLine($"  Total Processes: {systemMetrics.Processes}");
        Console.WriteLine($"  Total Threads: {systemMetrics.TotalThreads}");
        Console.WriteLine($"  Threads per Core: {systemMetrics.TotalThreads / (double)systemMetrics.ProcessorCount:F1}");
    }
}
```

### Performance Comparison Analysis
```csharp
public class CpuPerformanceComparator
{
    private readonly CpuMetricsClient _client = new();

    public async Task<CpuComparisonResult> CompareMeasurementWindows()
    {
        var results = new List<(int Window, CpuMetrics Metrics)>();
        
        // Test different measurement windows
        var windows = new[] { 500, 1000, 2000, 5000 };
        
        foreach (var window in windows)
        {
            Console.WriteLine($"Measuring CPU with {window}ms window...");
            
            var metrics = _client.GetMetrics(window, all: false);
            results.Add((window, metrics));
            
            // Wait between measurements to ensure independence
            await Task.Delay(1000);
        }
        
        return new CpuComparisonResult
        {
            Measurements = results.ToArray(),
            Analysis = AnalyzeMeasurements(results),
            Recommendation = GetWindowRecommendation(results)
        };
    }

    public async Task<CpuScopeComparison> CompareMonitoringScopes()
    {
        const int window = 2000;
        
        Console.WriteLine("Comparing application vs system-wide monitoring...");
        
        // Measure application-specific CPU usage
        var appMetrics = _client.GetMetrics(window, all: false);
        
        // Small delay to ensure measurements are independent
        await Task.Delay(100);
        
        // Measure system-wide CPU usage
        var systemMetrics = _client.GetMetrics(window, all: true);
        
        return new CpuScopeComparison
        {
            ApplicationMetrics = appMetrics,
            SystemMetrics = systemMetrics,
            Analysis = new ScopeAnalysis
            {
                ApplicationCpuPercentage = (appMetrics.Usage / systemMetrics.Usage) * 100,
                ApplicationThreadPercentage = (appMetrics.Threads / (double)systemMetrics.TotalThreads) * 100,
                SystemEfficiency = systemMetrics.Usage / systemMetrics.ProcessorCount,
                ApplicationEfficiency = appMetrics.Usage / appMetrics.ProcessorCount,
                ResourceIntensity = ClassifyResourceIntensity(appMetrics, systemMetrics)
            }
        };
    }

    private MeasurementAnalysis AnalyzeMeasurements(List<(int Window, CpuMetrics Metrics)> results)
    {
        var usageValues = results.Select(r => r.Metrics.Usage).ToArray();
        
        return new MeasurementAnalysis
        {
            AverageUsage = usageValues.Average(),
            MinUsage = usageValues.Min(),
            MaxUsage = usageValues.Max(),
            StandardDeviation = CalculateStandardDeviation(usageValues),
            Stability = DetermineStability(usageValues),
            ConsistentThreadCount = results.All(r => r.Metrics.Threads == results.First().Metrics.Threads),
            MostStableWindow = results.OrderBy(r => Math.Abs(r.Metrics.Usage - usageValues.Average())).First().Window
        };
    }

    private string GetWindowRecommendation(List<(int Window, CpuMetrics Metrics)> results)
    {
        var analysis = AnalyzeMeasurements(results);
        
        return analysis.StandardDeviation switch
        {
            < 2.0 => $"System is stable - {analysis.MostStableWindow}ms window recommended for accuracy",
            < 5.0 => "Moderate variability - 2000ms window recommended for balanced accuracy and responsiveness",
            < 10.0 => "High variability - 5000ms window recommended for stable readings",
            _ => "Very high variability - consider longer measurement windows or system analysis"
        };
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        
        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Length - 1));
    }

    private string DetermineStability(double[] values) => CalculateStandardDeviation(values) switch
    {
        < 2.0 => "Very Stable",
        < 5.0 => "Stable",
        < 10.0 => "Moderate",
        < 20.0 => "Variable",
        _ => "Highly Variable"
    };

    private string ClassifyResourceIntensity(CpuMetrics app, CpuMetrics system)
    {
        var appPercentage = (app.Usage / system.Usage) * 100;
        
        return appPercentage switch
        {
            > 80 => "Very High - Application dominates system CPU",
            > 60 => "High - Application uses significant system CPU",
            > 40 => "Moderate - Balanced CPU usage",
            > 20 => "Low - Minimal system CPU impact",
            _ => "Very Low - Negligible CPU usage"
        };
    }
}

public class CpuComparisonResult
{
    public (int Window, CpuMetrics Metrics)[] Measurements { get; set; } = Array.Empty<(int, CpuMetrics)>();
    public MeasurementAnalysis Analysis { get; set; } = new();
    public string Recommendation { get; set; } = "";
}

public class CpuScopeComparison
{
    public CpuMetrics ApplicationMetrics { get; set; } = default!;
    public CpuMetrics SystemMetrics { get; set; } = default!;
    public ScopeAnalysis Analysis { get; set; } = new();
}

public class MeasurementAnalysis
{
    public double AverageUsage { get; set; }
    public double MinUsage { get; set; }
    public double MaxUsage { get; set; }
    public double StandardDeviation { get; set; }
    public string Stability { get; set; } = "";
    public bool ConsistentThreadCount { get; set; }
    public int MostStableWindow { get; set; }
}

public class ScopeAnalysis
{
    public double ApplicationCpuPercentage { get; set; }
    public double ApplicationThreadPercentage { get; set; }
    public double SystemEfficiency { get; set; }
    public double ApplicationEfficiency { get; set; }
    public string ResourceIntensity { get; set; } = "";
}
```

### Continuous Monitoring Implementation
```csharp
public class ContinuousCpuMonitor : IDisposable
{
    private readonly CpuMetricsClient _client;
    private readonly Timer _timer;
    private readonly ConcurrentQueue<(DateTime Timestamp, CpuMetrics Metrics)> _history;
    private readonly object _lock = new();
    private bool _disposed;

    public ContinuousCpuMonitor(int intervalSeconds = 30, bool systemWide = false)
    {
        _client = new CpuMetricsClient();
        _history = new ConcurrentQueue<(DateTime, CpuMetrics)>();
        
        SystemWide = systemWide;
        MeasurementWindow = 2000; // 2-second measurement window
        
        _timer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
    }

    public bool SystemWide { get; }
    public int MeasurementWindow { get; set; }
    public event Action<CpuMetrics>? MetricsCollected;
    public event Action<CpuAlert>? AlertRaised;

    private async void CollectMetrics(object? state)
    {
        if (_disposed) return;

        try
        {
            var timestamp = DateTime.UtcNow;
            
            // Collect metrics in background thread to avoid blocking timer
            var metrics = await Task.Run(() => _client.GetMetrics(MeasurementWindow, SystemWide));
            
            // Store in history (keep last 120 readings = 1 hour at 30-second intervals)
            _history.Enqueue((timestamp, metrics));
            while (_history.Count > 120)
            {
                _history.TryDequeue(out _);
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
            Console.WriteLine($"Error collecting CPU metrics: {ex.Message}");
        }
    }

    public CpuMetrics GetLatestMetrics()
    {
        return _history.LastOrDefault().Metrics ?? GetCurrentMetrics();
    }

    public CpuMetrics GetCurrentMetrics()
    {
        return _client.GetMetrics(MeasurementWindow, SystemWide);
    }

    public CpuTrendData GetTrendData(TimeSpan? period = null)
    {
        var targetPeriod = period ?? TimeSpan.FromMinutes(30);
        var cutoff = DateTime.UtcNow - targetPeriod;
        
        var relevantData = _history
            .Where(h => h.Timestamp >= cutoff)
            .OrderBy(h => h.Timestamp)
            .ToArray();
        
        if (!relevantData.Any())
            return new CpuTrendData { Status = "No data available" };
        
        var usageValues = relevantData.Select(d => d.Metrics.Usage).ToArray();
        var threadValues = relevantData.Select(d => d.Metrics.TotalThreads).ToArray();
        
        return new CpuTrendData
        {
            Period = targetPeriod,
            DataPoints = relevantData.Length,
            StartTime = relevantData.First().Timestamp,
            EndTime = relevantData.Last().Timestamp,
            
            UsageTrend = new TrendInfo
            {
                Current = usageValues.Last(),
                Average = usageValues.Average(),
                Minimum = usageValues.Min(),
                Maximum = usageValues.Max(),
                Trend = CalculateTrend(usageValues),
                StandardDeviation = CalculateStandardDeviation(usageValues)
            },
            
            ThreadTrend = new TrendInfo
            {
                Current = threadValues.Last(),
                Average = threadValues.Average(),
                Minimum = threadValues.Min(),
                Maximum = threadValues.Max(),
                Trend = CalculateTrend(threadValues.Select(t => (double)t).ToArray()),
                StandardDeviation = CalculateStandardDeviation(threadValues.Select(t => (double)t).ToArray())
            },
            
            PerformanceSummary = GeneratePerformanceSummary(relevantData),
            Status = "Data available"
        };
    }

    private List<CpuAlert> CheckForAlerts(CpuMetrics metrics)
    {
        var alerts = new List<CpuAlert>();
        
        // High CPU usage alert
        if (metrics.Usage > 90)
        {
            alerts.Add(new CpuAlert
            {
                Level = AlertLevel.Critical,
                Type = "high_cpu_usage",
                Message = $"Critical CPU usage: {metrics.Usage:F1}%",
                Timestamp = DateTime.UtcNow,
                Metrics = metrics
            });
        }
        else if (metrics.Usage > 75)
        {
            alerts.Add(new CpuAlert
            {
                Level = AlertLevel.Warning,
                Type = "elevated_cpu_usage",
                Message = $"Elevated CPU usage: {metrics.Usage:F1}%",
                Timestamp = DateTime.UtcNow,
                Metrics = metrics
            });
        }
        
        // High thread count alert
        var threadsPerCore = metrics.TotalThreads / (double)metrics.ProcessorCount;
        if (threadsPerCore > 100)
        {
            alerts.Add(new CpuAlert
            {
                Level = AlertLevel.Warning,
                Type = "high_thread_density",
                Message = $"High thread density: {threadsPerCore:F1} threads per core",
                Timestamp = DateTime.UtcNow,
                Metrics = metrics
            });
        }
        
        // Sustained high usage check (requires history)
        CheckSustainedHighUsage(alerts);
        
        return alerts;
    }

    private void CheckSustainedHighUsage(List<CpuAlert> alerts)
    {
        var recentMetrics = _history
            .Where(h => h.Timestamp >= DateTime.UtcNow.AddMinutes(-5))
            .Select(h => h.Metrics)
            .ToArray();
        
        if (recentMetrics.Length >= 6 && recentMetrics.All(m => m.Usage > 80))
        {
            alerts.Add(new CpuAlert
            {
                Level = AlertLevel.Critical,
                Type = "sustained_high_usage",
                Message = $"Sustained high CPU usage over 5 minutes (average: {recentMetrics.Average(m => m.Usage):F1}%)",
                Timestamp = DateTime.UtcNow,
                Metrics = recentMetrics.Last()
            });
        }
    }

    private string CalculateTrend(double[] values)
    {
        if (values.Length < 3) return "Insufficient data";
        
        var midpoint = values.Length / 2;
        var firstHalf = values.Take(midpoint).Average();
        var secondHalf = values.Skip(midpoint).Average();
        
        var change = (secondHalf - firstHalf) / firstHalf * 100;
        
        return change switch
        {
            > 10 => "Rising",
            < -10 => "Falling",
            _ => "Stable"
        };
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        
        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Length - 1));
    }

    private string GeneratePerformanceSummary(IEnumerable<(DateTime Timestamp, CpuMetrics Metrics)> data)
    {
        var metrics = data.Select(d => d.Metrics).ToArray();
        var avgUsage = metrics.Average(m => m.Usage);
        var maxUsage = metrics.Max(m => m.Usage);
        var stability = CalculateStandardDeviation(metrics.Select(m => m.Usage).ToArray());
        
        return (avgUsage, maxUsage, stability) switch
        {
            (> 80, _, _) => "High resource utilization period",
            (_, > 95, _) => "Performance stress detected",
            (_, _, > 15) => "Highly variable performance",
            (_, _, > 10) => "Moderate performance variability", 
            (< 30, _, < 5) => "Stable low utilization",
            _ => "Normal performance characteristics"
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _timer?.Dispose();
    }
}

public class CpuTrendData
{
    public TimeSpan Period { get; set; }
    public int DataPoints { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TrendInfo UsageTrend { get; set; } = new();
    public TrendInfo ThreadTrend { get; set; } = new();
    public string PerformanceSummary { get; set; } = "";
    public string Status { get; set; } = "";
}

public class TrendInfo
{
    public double Current { get; set; }
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public string Trend { get; set; } = "";
    public double StandardDeviation { get; set; }
}

public class CpuAlert
{
    public AlertLevel Level { get; set; }
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public CpuMetrics Metrics { get; set; } = default!;
}

public enum AlertLevel { Info, Warning, Critical }
```

### Advanced CPU Analysis
```csharp
public class AdvancedCpuAnalyzer
{
    private readonly CpuMetricsClient _client = new();

    public async Task<CpuLoadAnalysis> AnalyzeSystemLoad(int durationMinutes = 5)
    {
        var measurements = new List<(DateTime Time, CpuMetrics App, CpuMetrics System)>();
        var interval = TimeSpan.FromSeconds(10);
        var endTime = DateTime.UtcNow.AddMinutes(durationMinutes);
        
        Console.WriteLine($"Starting {durationMinutes}-minute CPU load analysis...");
        
        while (DateTime.UtcNow < endTime)
        {
            var timestamp = DateTime.UtcNow;
            
            // Collect both application and system metrics
            var appMetrics = _client.GetMetrics(2000, all: false);
            await Task.Delay(100); // Small gap between measurements
            var systemMetrics = _client.GetMetrics(2000, all: true);
            
            measurements.Add((timestamp, appMetrics, systemMetrics));
            
            Console.WriteLine($"[{timestamp:HH:mm:ss}] App: {appMetrics.Usage:F1}% | System: {systemMetrics.Usage:F1}% | Threads: {systemMetrics.TotalThreads}");
            
            await Task.Delay(interval);
        }
        
        return AnalyzeMeasurements(measurements);
    }

    private CpuLoadAnalysis AnalyzeMeasurements(List<(DateTime Time, CpuMetrics App, CpuMetrics System)> measurements)
    {
        var appUsage = measurements.Select(m => m.App.Usage).ToArray();
        var systemUsage = measurements.Select(m => m.System.Usage).ToArray();
        var threadCounts = measurements.Select(m => m.System.TotalThreads).ToArray();
        
        return new CpuLoadAnalysis
        {
            Duration = measurements.Last().Time - measurements.First().Time,
            MeasurementCount = measurements.Count,
            
            ApplicationAnalysis = new LoadMetrics
            {
                Average = appUsage.Average(),
                Minimum = appUsage.Min(),
                Maximum = appUsage.Max(),
                StandardDeviation = CalculateStandardDeviation(appUsage),
                PercentileP95 = CalculatePercentile(appUsage, 95),
                PercentileP99 = CalculatePercentile(appUsage, 99),
                SustainedHighPeriods = CountSustainedPeriods(appUsage, 75, 3),
                SpikesAboveThreshold = appUsage.Count(u => u > 90)
            },
            
            SystemAnalysis = new LoadMetrics
            {
                Average = systemUsage.Average(),
                Minimum = systemUsage.Min(),
                Maximum = systemUsage.Max(),
                StandardDeviation = CalculateStandardDeviation(systemUsage),
                PercentileP95 = CalculatePercentile(systemUsage, 95),
                PercentileP99 = CalculatePercentile(systemUsage, 99),
                SustainedHighPeriods = CountSustainedPeriods(systemUsage, 80, 3),
                SpikesAboveThreshold = systemUsage.Count(u => u > 95)
            },
            
            ResourceCorrelation = new CorrelationAnalysis
            {
                AppSystemCorrelation = CalculateCorrelation(appUsage, systemUsage),
                AppContributionPercent = (appUsage.Average() / systemUsage.Average()) * 100,
                ConcurrentHighUsagePeriods = CountConcurrentHighUsage(measurements),
                ThreadGrowthRate = CalculateThreadGrowthRate(threadCounts),
                SystemEfficiency = systemUsage.Average() / measurements.First().System.ProcessorCount
            },
            
            PerformanceClassification = ClassifyPerformance(appUsage, systemUsage),
            Recommendations = GenerateRecommendations(appUsage, systemUsage, threadCounts)
        };
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        var mean = values.Average();
        return Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / (values.Length - 1));
    }

    private double CalculatePercentile(double[] values, int percentile)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        var index = (percentile / 100.0) * (sorted.Length - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        
        if (lower == upper) return sorted[lower];
        
        var weight = index - lower;
        return sorted[lower] * (1 - weight) + sorted[upper] * weight;
    }

    private int CountSustainedPeriods(double[] values, double threshold, int consecutiveCount)
    {
        var count = 0;
        var consecutive = 0;
        
        foreach (var value in values)
        {
            if (value > threshold)
            {
                consecutive++;
                if (consecutive >= consecutiveCount)
                {
                    count++;
                    consecutive = 0; // Reset to avoid double counting
                }
            }
            else
            {
                consecutive = 0;
            }
        }
        
        return count;
    }

    private double CalculateCorrelation(double[] x, double[] y)
    {
        if (x.Length != y.Length || x.Length == 0) return 0;
        
        var meanX = x.Average();
        var meanY = y.Average();
        
        var numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        var denominator = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)) * y.Sum(yi => Math.Pow(yi - meanY, 2)));
        
        return denominator == 0 ? 0 : numerator / denominator;
    }

    private int CountConcurrentHighUsage(List<(DateTime Time, CpuMetrics App, CpuMetrics System)> measurements)
    {
        return measurements.Count(m => m.App.Usage > 70 && m.System.Usage > 80);
    }

    private double CalculateThreadGrowthRate(long[] threadCounts)
    {
        if (threadCounts.Length < 2) return 0;
        
        var initial = threadCounts.First();
        var final = threadCounts.Last();
        
        return ((double)(final - initial) / initial) * 100;
    }

    private string ClassifyPerformance(double[] appUsage, double[] systemUsage)
    {
        var avgApp = appUsage.Average();
        var avgSystem = systemUsage.Average();
        var appVolatility = CalculateStandardDeviation(appUsage);
        var systemVolatility = CalculateStandardDeviation(systemUsage);
        
        return (avgApp, avgSystem, appVolatility, systemVolatility) switch
        {
            (> 80, > 85, _, _) => "High Resource Stress",
            (_, > 90, _, _) => "System Overload",
            (> 70, _, > 20, _) => "Application Performance Issues",
            (_, _, _, > 25) => "System Instability",
            (< 30, < 40, < 10, < 10) => "Excellent Performance",
            (< 50, < 60, < 15, < 15) => "Good Performance",
            _ => "Moderate Performance"
        };
    }

    private List<string> GenerateRecommendations(double[] appUsage, double[] systemUsage, long[] threadCounts)
    {
        var recommendations = new List<string>();
        
        var avgApp = appUsage.Average();
        var avgSystem = systemUsage.Average();
        var maxApp = appUsage.Max();
        var maxSystem = systemUsage.Max();
        var appVolatility = CalculateStandardDeviation(appUsage);
        var threadGrowth = CalculateThreadGrowthRate(threadCounts);
        
        if (maxSystem > 95)
            recommendations.Add("Critical: System CPU reached dangerous levels - investigate immediately");
        
        if (avgSystem > 80)
            recommendations.Add("High system CPU usage - consider load balancing or scaling");
        
        if (avgApp > 70)
            recommendations.Add("Application CPU usage is high - profile for optimization opportunities");
        
        if (appVolatility > 20)
            recommendations.Add("Application CPU usage is highly variable - investigate performance spikes");
        
        if (threadGrowth > 50)
            recommendations.Add($"Thread count increased by {threadGrowth:F1}% - check for thread leaks");
        
        if (threadGrowth < -20)
            recommendations.Add("Thread count decreased significantly - verify thread pool health");
        
        var appContribution = (avgApp / avgSystem) * 100;
        if (appContribution > 80)
            recommendations.Add("Application dominates system CPU - consider optimization or dedicated resources");
        else if (appContribution < 10)
            recommendations.Add("Application uses minimal CPU - other processes may be causing system load");
        
        if (recommendations.Count == 0)
            recommendations.Add("CPU performance appears normal - continue monitoring");
        
        return recommendations;
    }
}

public class CpuLoadAnalysis
{
    public TimeSpan Duration { get; set; }
    public int MeasurementCount { get; set; }
    public LoadMetrics ApplicationAnalysis { get; set; } = new();
    public LoadMetrics SystemAnalysis { get; set; } = new();
    public CorrelationAnalysis ResourceCorrelation { get; set; } = new();
    public string PerformanceClassification { get; set; } = "";
    public List<string> Recommendations { get; set; } = new();
}

public class LoadMetrics
{
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double StandardDeviation { get; set; }
    public double PercentileP95 { get; set; }
    public double PercentileP99 { get; set; }
    public int SustainedHighPeriods { get; set; }
    public int SpikesAboveThreshold { get; set; }
}

public class CorrelationAnalysis
{
    public double AppSystemCorrelation { get; set; }
    public double AppContributionPercent { get; set; }
    public int ConcurrentHighUsagePeriods { get; set; }
    public double ThreadGrowthRate { get; set; }
    public double SystemEfficiency { get; set; }
}
```

## Implementation Details

### CPU Usage Calculation

The CPU usage calculation uses a time-based sampling approach:

1. **Initial Measurement**: Records `TotalProcessorTime` at start of measurement window
2. **Wait Period**: System operates normally during measurement window
3. **Final Measurement**: Records `TotalProcessorTime` at end of measurement window
4. **Calculation**: `Usage = (EndTime - StartTime) / (ProcessorCount * WindowDuration) * 100`

### Thread-Safe Operation

The implementation uses `ManualResetEvent` to coordinate between the measurement task and the calling thread, ensuring accurate timing and thread safety.

### Error Handling

Process enumeration includes exception handling for processes that may terminate or become inaccessible during measurement, ensuring robust operation in dynamic system environments.

### Scope Selection

- **Application Mode** (`all = false`): Measures current process CPU usage only
- **System Mode** (`all = true`): Measures aggregate CPU usage across all processes

## Performance Considerations

### Measurement Windows
- **Short Windows** (< 1000ms): May be less accurate due to sampling variability
- **Optimal Windows** (1000-2000ms): Good balance of accuracy and responsiveness  
- **Long Windows** (> 5000ms): More accurate but less responsive to changes

### System Impact
- **Measurement Overhead**: Minimal - uses existing system counters
- **Process Enumeration**: Can be expensive on systems with many processes
- **Frequency**: Consider measurement frequency to balance monitoring needs with overhead

### Accuracy Factors
- **System Load**: High system load may affect measurement timing
- **Process Lifecycle**: Processes starting/stopping during measurement may affect accuracy
- **Clock Resolution**: System clock resolution affects timing accuracy

## Configuration Options

### Window Selection Guidelines
```csharp
public static class CpuMeasurementWindows
{
    public const int Quick = 500;          // Quick response, lower accuracy
    public const int Standard = 1000;      // Standard balanced measurement
    public const int Accurate = 2000;      // Higher accuracy measurement
    public const int Stable = 5000;        // Very stable, slower response
    
    public static int GetRecommendedWindow(SystemType systemType) => systemType switch
    {
        SystemType.Development => Standard,
        SystemType.Testing => Accurate,
        SystemType.Production => Accurate,
        SystemType.HighLoad => Stable,
        _ => Standard
    };
}

public enum SystemType { Development, Testing, Production, HighLoad }
```

### Best Practices

1. **Measurement Frequency**: Don't measure more frequently than every 5-10 seconds
2. **Window Selection**: Use 2000ms windows for production monitoring
3. **Scope Selection**: Use system-wide monitoring for infrastructure oversight
4. **Error Handling**: Always handle potential exceptions in process enumeration
5. **Resource Management**: Monitor the monitoring overhead itself

## Related Components

- **[CpuMetrics](CpuMetrics.md)** - CPU metrics data model
- **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** - Aggregate metrics container
- **[MemoryMetricsClient](MemoryMetricsClient.md)** - Memory metrics collection
- **[ISystemResourceMonitor](ISystemResourceMonitor.md)** - Main monitoring interface
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Integration Guidelines

1. **Dependency Injection**: Register as internal service, not exposed publicly
2. **Caching**: Implement caching to reduce measurement frequency
3. **Background Collection**: Use background services for continuous monitoring
4. **Alert Integration**: Combine with alerting systems for proactive monitoring
5. **Metrics Export**: Export data to monitoring platforms like Prometheus