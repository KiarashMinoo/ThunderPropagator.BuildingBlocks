# MemoryMetrics

## Overview

The `MemoryMetrics` record represents comprehensive system memory utilization metrics including total memory, free memory, and calculated usage statistics. This immutable data model provides essential memory performance indicators for system monitoring, resource planning, and performance analysis.

## Purpose

- **Memory Monitoring**: Track system memory utilization and availability
- **Resource Planning**: Monitor memory consumption for capacity planning
- **Performance Analysis**: Identify memory pressure and optimization opportunities
- **System Health**: Assess memory-related system health indicators

## Record Declaration

```csharp
public record MemoryMetrics(double Total, double Free)
{
    public double Used => Total - Free;
    public double UsagePercentage
    {
        get
        {
            var usage = .0;
            if (Total > 0)
                usage = 100.0 - ((1.0 * Free / Total) * 100);
            return usage;
        }
    }
}
```

## Properties

### Total
- **Type**: `double`
- **Description**: Total system memory in megabytes (MB)
- **Source**: Physical memory capacity from operating system
- **Usage**: Baseline for memory calculations and capacity planning

### Free
- **Type**: `double`
- **Description**: Available (free) system memory in megabytes (MB)
- **Source**: Currently unallocated physical memory
- **Usage**: Indicates immediately available memory for new processes

### Used (Calculated)
- **Type**: `double`
- **Description**: Currently allocated memory in megabytes (MB)
- **Calculation**: `Total - Free`
- **Usage**: Shows active memory consumption

### UsagePercentage (Calculated)
- **Type**: `double`
- **Description**: Memory utilization as a percentage (0-100)
- **Calculation**: `100.0 - ((1.0 * Free / Total) * 100)`
- **Usage**: Primary metric for memory monitoring and alerting

## Usage Examples

### Basic Memory Monitoring
```csharp
public class MemoryMonitoringService
{
    private readonly ISystemResourceMonitor _monitor;

    public MemoryMonitoringService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public void DisplayMemoryStatus()
    {
        var metrics = _monitor.GetMetrics(window: 1000, all: false);
        var memory = metrics.Memory;
        
        Console.WriteLine("System Memory Status:");
        Console.WriteLine($"  Total Memory: {memory.Total:F0} MB ({memory.Total / 1024:F1} GB)");
        Console.WriteLine($"  Used Memory: {memory.Used:F0} MB ({memory.Used / 1024:F1} GB)");
        Console.WriteLine($"  Free Memory: {memory.Free:F0} MB ({memory.Free / 1024:F1} GB)");
        Console.WriteLine($"  Usage: {memory.UsagePercentage:F2}%");
        Console.WriteLine($"  Status: {GetMemoryStatus(memory.UsagePercentage)}");
        Console.WriteLine($"  Available for Apps: {memory.Free:F0} MB");
        
        // Memory pressure indicators
        if (memory.UsagePercentage > 90)
            Console.WriteLine("  ⚠️  WARNING: Critical memory usage!");
        else if (memory.UsagePercentage > 80)
            Console.WriteLine("  ⚠️  CAUTION: High memory usage");
        else if (memory.UsagePercentage < 50)
            Console.WriteLine("  ✅ Memory usage is healthy");
    }

    private string GetMemoryStatus(double usage) => usage switch
    {
        > 95 => "Critical",
        > 85 => "High",
        > 70 => "Moderate",
        > 50 => "Normal",
        _ => "Low"
    };
}
```

### Memory Analysis and Recommendations
```csharp
public class MemoryAnalyzer
{
    public MemoryAnalysisResult AnalyzeMemory(MemoryMetrics memory)
    {
        return new MemoryAnalysisResult
        {
            // Basic metrics
            TotalMemoryGB = memory.Total / 1024.0,
            UsedMemoryGB = memory.Used / 1024.0,
            FreeMemoryGB = memory.Free / 1024.0,
            UsagePercentage = memory.UsagePercentage,
            
            // Analysis
            MemoryClassification = ClassifyMemoryUsage(memory.UsagePercentage),
            MemoryPressure = CalculateMemoryPressure(memory),
            AvailabilityStatus = AssessAvailability(memory),
            
            // Capacity planning
            EstimatedCapacityRemaining = EstimateRemainingCapacity(memory),
            RecommendedThresholds = GetRecommendedThresholds(memory.Total),
            
            // Recommendations
            ActionRecommendations = GenerateRecommendations(memory),
            OptimizationSuggestions = GetOptimizationSuggestions(memory)
        };
    }

    private MemoryUsageLevel ClassifyMemoryUsage(double usage) => usage switch
    {
        > 95 => MemoryUsageLevel.Critical,
        > 85 => MemoryUsageLevel.High,
        > 70 => MemoryUsageLevel.Moderate,
        > 50 => MemoryUsageLevel.Normal,
        _ => MemoryUsageLevel.Low
    };

    private MemoryPressureInfo CalculateMemoryPressure(MemoryMetrics memory)
    {
        var pressure = memory.UsagePercentage / 100.0;
        
        return new MemoryPressureInfo
        {
            PressureLevel = pressure,
            PressureDescription = pressure switch
            {
                > 0.95 => "Severe pressure - immediate action required",
                > 0.85 => "High pressure - monitor closely",
                > 0.70 => "Moderate pressure - plan optimization",
                > 0.50 => "Normal pressure - no immediate concern",
                _ => "Low pressure - plenty of available memory"
            },
            RiskLevel = pressure switch
            {
                > 0.95 => "Critical",
                > 0.85 => "High",
                > 0.70 => "Medium",
                _ => "Low"
            },
            TimeToExhaustion = EstimateTimeToExhaustion(memory)
        };
    }

    private string AssessAvailability(MemoryMetrics memory)
    {
        var availableGB = memory.Free / 1024.0;
        
        return availableGB switch
        {
            > 4.0 => "Excellent - Sufficient memory for demanding applications",
            > 2.0 => "Good - Adequate memory for normal operations",
            > 1.0 => "Limited - May constrain resource-intensive operations",
            > 0.5 => "Low - Risk of performance degradation",
            _ => "Critical - Immediate memory shortage risk"
        };
    }

    private double EstimateRemainingCapacity(MemoryMetrics memory)
    {
        // Estimate usable remaining capacity (reserve 10% for system stability)
        var reserveAmount = memory.Total * 0.10;
        var availableForUse = memory.Free - reserveAmount;
        return Math.Max(0, availableForUse);
    }

    private MemoryThresholds GetRecommendedThresholds(double totalMemory)
    {
        // Adjust thresholds based on total memory capacity
        var totalGB = totalMemory / 1024.0;
        
        return totalGB switch
        {
            >= 32 => new MemoryThresholds { Warning = 80, Critical = 90, Reserve = 5 },
            >= 16 => new MemoryThresholds { Warning = 75, Critical = 85, Reserve = 8 },
            >= 8 => new MemoryThresholds { Warning = 70, Critical = 80, Reserve = 10 },
            _ => new MemoryThresholds { Warning = 65, Critical = 75, Reserve = 15 }
        };
    }

    private List<string> GenerateRecommendations(MemoryMetrics memory)
    {
        var recommendations = new List<string>();
        
        if (memory.UsagePercentage > 95)
        {
            recommendations.Add("CRITICAL: Close unnecessary applications immediately");
            recommendations.Add("Consider restarting the system to free up memory");
            recommendations.Add("Investigate memory leaks in running applications");
        }
        else if (memory.UsagePercentage > 85)
        {
            recommendations.Add("Close unused applications and browser tabs");
            recommendations.Add("Consider adding more RAM to the system");
            recommendations.Add("Monitor for memory-intensive processes");
        }
        else if (memory.UsagePercentage > 70)
        {
            recommendations.Add("Monitor memory usage trends");
            recommendations.Add("Optimize memory-intensive applications");
            recommendations.Add("Plan for future memory requirements");
        }
        else if (memory.Free / 1024.0 < 1.0)
        {
            recommendations.Add("Low absolute free memory despite good percentage");
            recommendations.Add("Consider memory upgrade for better performance");
        }
        
        if (recommendations.Count == 0)
            recommendations.Add("Memory usage is healthy - no immediate action required");
        
        return recommendations;
    }

    private List<string> GetOptimizationSuggestions(MemoryMetrics memory)
    {
        var suggestions = new List<string>();
        
        var totalGB = memory.Total / 1024.0;
        var freeGB = memory.Free / 1024.0;
        
        // System-specific suggestions
        if (totalGB < 8)
            suggestions.Add("Consider upgrading to at least 8GB RAM for modern applications");
        else if (totalGB >= 8 && memory.UsagePercentage > 80)
            suggestions.Add("Consider upgrading to 16GB RAM for better multitasking");
        
        // Usage-specific suggestions
        if (memory.UsagePercentage > 80)
        {
            suggestions.Add("Enable virtual memory/page file if not already configured");
            suggestions.Add("Use Task Manager to identify memory-intensive processes");
            suggestions.Add("Consider using lightweight alternatives for heavy applications");
        }
        
        // Performance suggestions
        if (freeGB < 2 && totalGB >= 8)
        {
            suggestions.Add("Check for memory leaks in long-running applications");
            suggestions.Add("Restart applications periodically to free up memory");
            suggestions.Add("Configure application memory limits where possible");
        }
        
        // Monitoring suggestions
        suggestions.Add("Set up memory monitoring alerts at 80% usage");
        suggestions.Add("Track memory usage trends over time");
        suggestions.Add("Monitor memory usage during peak application loads");
        
        return suggestions;
    }

    private string EstimateTimeToExhaustion(MemoryMetrics memory)
    {
        if (memory.UsagePercentage < 70)
            return "No immediate risk";
        
        // This would require historical data for accurate prediction
        // For now, provide general guidance based on current state
        return memory.UsagePercentage switch
        {
            > 95 => "Immediate risk - minutes",
            > 90 => "High risk - hours",
            > 85 => "Moderate risk - hours to days",
            > 80 => "Low risk - days to weeks",
            _ => "No significant risk"
        };
    }
}

public enum MemoryUsageLevel { Low, Normal, Moderate, High, Critical }

public class MemoryAnalysisResult
{
    public double TotalMemoryGB { get; set; }
    public double UsedMemoryGB { get; set; }
    public double FreeMemoryGB { get; set; }
    public double UsagePercentage { get; set; }
    
    public MemoryUsageLevel MemoryClassification { get; set; }
    public MemoryPressureInfo MemoryPressure { get; set; } = new();
    public string AvailabilityStatus { get; set; } = "";
    
    public double EstimatedCapacityRemaining { get; set; }
    public MemoryThresholds RecommendedThresholds { get; set; } = new();
    
    public List<string> ActionRecommendations { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
}

public class MemoryPressureInfo
{
    public double PressureLevel { get; set; }
    public string PressureDescription { get; set; } = "";
    public string RiskLevel { get; set; } = "";
    public string TimeToExhaustion { get; set; } = "";
}

public class MemoryThresholds
{
    public double Warning { get; set; }
    public double Critical { get; set; }
    public double Reserve { get; set; }
}
```

### Real-time Memory Monitoring
```csharp
public class MemoryMonitoringDashboard
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly List<(DateTime Time, MemoryMetrics Memory)> _history = new();
    private readonly object _lock = new();

    public MemoryMonitoringDashboard(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public async Task StartMonitoring(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting Memory Monitoring Dashboard...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var timestamp = DateTime.UtcNow;
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            var memory = metrics.Memory;
            
            lock (_lock)
            {
                _history.Add((timestamp, memory));
                if (_history.Count > 120) // Keep last 2 hours at 1-minute intervals
                {
                    _history.RemoveAt(0);
                }
            }

            DisplayMemoryDashboard(memory, timestamp);
            CheckMemoryAlerts(memory);
            
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
        }
    }

    private void DisplayMemoryDashboard(MemoryMetrics memory, DateTime timestamp)
    {
        Console.Clear();
        Console.WriteLine("=== Memory Monitoring Dashboard ===");
        Console.WriteLine($"Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();
        
        // Current status
        Console.WriteLine("Current Memory Status:");
        Console.WriteLine($"  Total:  {memory.Total / 1024:F1} GB");
        Console.WriteLine($"  Used:   {memory.Used / 1024:F1} GB");
        Console.WriteLine($"  Free:   {memory.Free / 1024:F1} GB");
        Console.WriteLine($"  Usage:  {GenerateUsageBar(memory.UsagePercentage)} {memory.UsagePercentage:F1}%");
        Console.WriteLine();
        
        // Memory classification
        var status = GetMemoryStatusDisplay(memory.UsagePercentage);
        Console.WriteLine($"Status: {status}");
        Console.WriteLine();
        
        // Historical trend
        DisplayMemoryTrend();
        
        // System recommendations
        DisplayMemoryRecommendations(memory);
    }

    private string GenerateUsageBar(double usage)
    {
        var barLength = 30;
        var filled = (int)(usage / 100.0 * barLength);
        var bar = new string('█', filled) + new string('░', barLength - filled);
        
        var color = usage switch
        {
            > 90 => "🔴",
            > 80 => "🟡",
            > 60 => "🟢",
            _ => "⚪"
        };
        
        return $"{color} [{bar}]";
    }

    private string GetMemoryStatusDisplay(double usage) => usage switch
    {
        > 95 => "🔴 CRITICAL - Memory exhaustion imminent",
        > 85 => "🟡 HIGH - Memory pressure detected",
        > 70 => "🟡 MODERATE - Monitor memory usage",
        > 50 => "🟢 NORMAL - Memory usage healthy",
        _ => "⚪ LOW - Plenty of memory available"
    };

    private void DisplayMemoryTrend()
    {
        lock (_lock)
        {
            if (_history.Count < 2) return;
            
            Console.WriteLine("Memory Trend (Last Hour):");
            var recentData = _history.TakeLast(60).ToArray(); // Last hour
            
            if (recentData.Length >= 2)
            {
                var usageValues = recentData.Select(h => h.Memory.UsagePercentage).ToArray();
                var freeValues = recentData.Select(h => h.Memory.Free).ToArray();
                
                Console.WriteLine($"  Average Usage: {usageValues.Average():F1}%");
                Console.WriteLine($"  Min Usage: {usageValues.Min():F1}%");
                Console.WriteLine($"  Max Usage: {usageValues.Max():F1}%");
                Console.WriteLine($"  Usage Trend: {CalculateTrend(usageValues)}");
                Console.WriteLine($"  Free Memory Trend: {CalculateFreeTrend(freeValues)}");
                Console.WriteLine($"  Stability: {CalculateStability(usageValues)}");
            }
            Console.WriteLine();
        }
    }

    private void DisplayMemoryRecommendations(MemoryMetrics memory)
    {
        Console.WriteLine("Recommendations:");
        
        if (memory.UsagePercentage > 90)
        {
            Console.WriteLine("  🚨 URGENT: Close unnecessary applications");
            Console.WriteLine("  🚨 Consider restarting memory-intensive programs");
            Console.WriteLine("  🚨 Check for memory leaks");
        }
        else if (memory.UsagePercentage > 80)
        {
            Console.WriteLine("  ⚠️  Close unused browser tabs and applications");
            Console.WriteLine("  ⚠️  Monitor memory usage closely");
            Console.WriteLine("  ⚠️  Consider memory optimization");
        }
        else if (memory.UsagePercentage > 70)
        {
            Console.WriteLine("  ℹ️  Monitor memory trends");
            Console.WriteLine("  ℹ️  Plan for peak usage scenarios");
        }
        else
        {
            Console.WriteLine("  ✅ Memory usage is healthy");
            Console.WriteLine("  ℹ️  Continue regular monitoring");
        }
        
        // Capacity recommendations
        var totalGB = memory.Total / 1024.0;
        if (totalGB < 8 && memory.UsagePercentage > 70)
        {
            Console.WriteLine("  💡 Consider upgrading to 8GB+ RAM");
        }
        else if (totalGB >= 8 && memory.UsagePercentage > 85)
        {
            Console.WriteLine("  💡 Consider upgrading to 16GB+ RAM");
        }
    }

    private void CheckMemoryAlerts(MemoryMetrics memory)
    {
        var alerts = new List<string>();
        
        if (memory.UsagePercentage > 95)
            alerts.Add("🚨 CRITICAL ALERT: Memory usage above 95%");
        else if (memory.UsagePercentage > 90)
            alerts.Add("⚠️ WARNING: Memory usage above 90%");
        
        if (memory.Free < 512) // Less than 512MB free
            alerts.Add("⚠️ WARNING: Less than 512MB free memory");
        
        if (memory.Free < 256) // Less than 256MB free
            alerts.Add("🚨 CRITICAL: Less than 256MB free memory");
        
        // Check for memory trend alerts
        CheckMemoryTrendAlerts(alerts);
        
        if (alerts.Any())
        {
            Console.WriteLine();
            Console.WriteLine("MEMORY ALERTS:");
            foreach (var alert in alerts)
            {
                Console.WriteLine($"  {alert}");
            }
        }
    }

    private void CheckMemoryTrendAlerts(List<string> alerts)
    {
        lock (_lock)
        {
            if (_history.Count < 10) return;
            
            var recent = _history.TakeLast(10).ToArray();
            var usageValues = recent.Select(h => h.Memory.UsagePercentage).ToArray();
            
            // Check for rapid memory consumption
            var usageIncrease = usageValues.Last() - usageValues.First();
            if (usageIncrease > 20)
            {
                alerts.Add($"⚠️ TREND ALERT: Memory usage increased by {usageIncrease:F1}% in last 10 minutes");
            }
            
            // Check for sustained high usage
            if (usageValues.All(u => u > 85))
            {
                alerts.Add("⚠️ TREND ALERT: Sustained high memory usage (>85%) for 10+ minutes");
            }
            
            // Check for memory leak pattern
            var trend = CalculateTrend(usageValues);
            if (trend == "Rising" && usageValues.Average() > 75)
            {
                alerts.Add("⚠️ TREND ALERT: Possible memory leak detected - usage consistently rising");
            }
        }
    }

    private string CalculateTrend(double[] values)
    {
        if (values.Length < 3) return "Stable";
        
        var firstThird = values.Take(values.Length / 3).Average();
        var lastThird = values.TakeLast(values.Length / 3).Average();
        
        var change = (lastThird - firstThird) / firstThird * 100;
        
        return change switch
        {
            > 5 => "Rising",
            < -5 => "Falling", 
            _ => "Stable"
        };
    }

    private string CalculateFreeTrend(double[] freeValues)
    {
        var trend = CalculateTrend(freeValues);
        return trend switch
        {
            "Rising" => "Increasing (Good)",
            "Falling" => "Decreasing (Concern)",
            _ => "Stable"
        };
    }

    private string CalculateStability(double[] values)
    {
        if (values.Length < 2) return "Unknown";
        
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Length;
        var stdDev = Math.Sqrt(variance);
        
        return stdDev switch
        {
            < 2 => "Very Stable",
            < 5 => "Stable",
            < 10 => "Moderate",
            _ => "Volatile"
        };
    }

    public MemoryTrendAnalysis GetTrendAnalysis(TimeSpan? period = null)
    {
        var targetPeriod = period ?? TimeSpan.FromHours(1);
        var cutoff = DateTime.UtcNow - targetPeriod;
        
        lock (_lock)
        {
            var relevantData = _history
                .Where(h => h.Time >= cutoff)
                .OrderBy(h => h.Time)
                .ToArray();
            
            if (!relevantData.Any())
                return new MemoryTrendAnalysis { Status = "No data available" };
            
            var usageValues = relevantData.Select(d => d.Memory.UsagePercentage).ToArray();
            var freeValues = relevantData.Select(d => d.Memory.Free).ToArray();
            
            return new MemoryTrendAnalysis
            {
                Period = targetPeriod,
                DataPoints = relevantData.Length,
                StartTime = relevantData.First().Time,
                EndTime = relevantData.Last().Time,
                
                UsageAnalysis = new MemoryTrendInfo
                {
                    Current = usageValues.Last(),
                    Average = usageValues.Average(),
                    Minimum = usageValues.Min(),
                    Maximum = usageValues.Max(),
                    Trend = CalculateTrend(usageValues),
                    Volatility = CalculateStability(usageValues)
                },
                
                FreeMemoryAnalysis = new MemoryTrendInfo
                {
                    Current = freeValues.Last(),
                    Average = freeValues.Average(),
                    Minimum = freeValues.Min(),
                    Maximum = freeValues.Max(),
                    Trend = CalculateTrend(freeValues),
                    Volatility = CalculateStability(freeValues)
                },
                
                PerformanceSummary = GeneratePerformanceSummary(relevantData),
                Status = "Data available"
            };
        }
    }

    private string GeneratePerformanceSummary(IEnumerable<(DateTime Time, MemoryMetrics Memory)> data)
    {
        var memories = data.Select(d => d.Memory).ToArray();
        var avgUsage = memories.Average(m => m.UsagePercentage);
        var maxUsage = memories.Max(m => m.UsagePercentage);
        var minFree = memories.Min(m => m.Free);
        
        return (avgUsage, maxUsage, minFree) switch
        {
            (> 90, _, _) => "Critical memory pressure period",
            (> 80, _, _) => "High memory utilization period",
            (_, > 95, _) => "Memory stress events detected",
            (_, _, < 256) => "Low memory availability detected",
            (< 50, _, _) => "Healthy memory utilization",
            _ => "Normal memory performance"
        };
    }
}

public class MemoryTrendAnalysis
{
    public TimeSpan Period { get; set; }
    public int DataPoints { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public MemoryTrendInfo UsageAnalysis { get; set; } = new();
    public MemoryTrendInfo FreeMemoryAnalysis { get; set; } = new();
    public string PerformanceSummary { get; set; } = "";
    public string Status { get; set; } = "";
}

public class MemoryTrendInfo
{
    public double Current { get; set; }
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public string Trend { get; set; } = "";
    public string Volatility { get; set; } = "";
}
```

### Health Check Integration
```csharp
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly MemoryHealthThresholds _thresholds;

    public MemoryHealthCheck(ISystemResourceMonitor monitor, IOptions<MemoryHealthThresholds> thresholds)
    {
        _monitor = monitor;
        _thresholds = thresholds.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            var memory = metrics.Memory;
            
            var healthData = new Dictionary<string, object>
            {
                ["total_memory_mb"] = memory.Total,
                ["free_memory_mb"] = memory.Free,
                ["used_memory_mb"] = memory.Used,
                ["usage_percentage"] = memory.UsagePercentage,
                ["total_memory_gb"] = Math.Round(memory.Total / 1024.0, 2),
                ["free_memory_gb"] = Math.Round(memory.Free / 1024.0, 2),
                ["used_memory_gb"] = Math.Round(memory.Used / 1024.0, 2)
            };

            var status = DetermineHealthStatus(memory);
            var description = GenerateHealthDescription(memory, status);
            
            return new HealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to collect memory metrics", ex);
        }
    }

    private HealthStatus DetermineHealthStatus(MemoryMetrics memory)
    {
        if (memory.UsagePercentage > _thresholds.Critical || memory.Free < _thresholds.MinimumFreeMB)
            return HealthStatus.Unhealthy;
        
        if (memory.UsagePercentage > _thresholds.Warning || memory.Free < _thresholds.LowFreeMB)
            return HealthStatus.Degraded;
        
        return HealthStatus.Healthy;
    }

    private string GenerateHealthDescription(MemoryMetrics memory, HealthStatus status)
    {
        var freeGB = memory.Free / 1024.0;
        
        return status switch
        {
            HealthStatus.Unhealthy => $"Critical memory usage: {memory.UsagePercentage:F1}% used, {freeGB:F1}GB free",
            HealthStatus.Degraded => $"High memory usage: {memory.UsagePercentage:F1}% used, {freeGB:F1}GB free",
            _ => $"Memory usage normal: {memory.UsagePercentage:F1}% used, {freeGB:F1}GB free"
        };
    }
}

public class MemoryHealthThresholds
{
    public double Warning { get; set; } = 80.0;
    public double Critical { get; set; } = 90.0;
    public double LowFreeMB { get; set; } = 1024.0; // 1GB
    public double MinimumFreeMB { get; set; } = 512.0; // 512MB
}
```

### Metrics Export
```csharp
public class MemoryMetricsExporter
{
    public string ExportPrometheusMetrics(MemoryMetrics memory)
    {
        var sb = new StringBuilder();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        sb.AppendLine("# HELP memory_total_mb Total system memory in megabytes");
        sb.AppendLine("# TYPE memory_total_mb gauge");
        sb.AppendLine($"memory_total_mb {memory.Total:F0} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP memory_free_mb Free system memory in megabytes");
        sb.AppendLine("# TYPE memory_free_mb gauge");
        sb.AppendLine($"memory_free_mb {memory.Free:F0} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP memory_used_mb Used system memory in megabytes");
        sb.AppendLine("# TYPE memory_used_mb gauge");
        sb.AppendLine($"memory_used_mb {memory.Used:F0} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP memory_usage_percent Memory usage percentage");
        sb.AppendLine("# TYPE memory_usage_percent gauge");
        sb.AppendLine($"memory_usage_percent {memory.UsagePercentage:F2} {timestamp}");
        sb.AppendLine();
        
        // Additional calculated metrics
        sb.AppendLine("# HELP memory_total_gb Total system memory in gigabytes");
        sb.AppendLine("# TYPE memory_total_gb gauge");
        sb.AppendLine($"memory_total_gb {memory.Total / 1024.0:F2} {timestamp}");
        sb.AppendLine();
        
        sb.AppendLine("# HELP memory_free_gb Free system memory in gigabytes");
        sb.AppendLine("# TYPE memory_free_gb gauge");
        sb.AppendLine($"memory_free_gb {memory.Free / 1024.0:F2} {timestamp}");
        
        return sb.ToString();
    }

    public object ExportJsonMetrics(MemoryMetrics memory)
    {
        return new
        {
            timestamp = DateTime.UtcNow,
            memory = new
            {
                total_mb = Math.Round(memory.Total, 0),
                free_mb = Math.Round(memory.Free, 0),
                used_mb = Math.Round(memory.Used, 0),
                usage_percent = Math.Round(memory.UsagePercentage, 2),
                total_gb = Math.Round(memory.Total / 1024.0, 2),
                free_gb = Math.Round(memory.Free / 1024.0, 2),
                used_gb = Math.Round(memory.Used / 1024.0, 2),
                status = ClassifyMemoryStatus(memory.UsagePercentage),
                pressure_level = CalculatePressureLevel(memory.UsagePercentage),
                availability = AssessAvailability(memory.Free)
            }
        };
    }

    private string ClassifyMemoryStatus(double usage) => usage switch
    {
        > 95 => "critical",
        > 85 => "high",
        > 70 => "moderate",
        > 50 => "normal",
        _ => "low"
    };

    private string CalculatePressureLevel(double usage) => usage switch
    {
        > 95 => "severe",
        > 85 => "high", 
        > 70 => "moderate",
        > 50 => "low",
        _ => "minimal"
    };

    private string AssessAvailability(double freeMB)
    {
        var freeGB = freeMB / 1024.0;
        return freeGB switch
        {
            > 4.0 => "excellent",
            > 2.0 => "good",
            > 1.0 => "adequate",
            > 0.5 => "limited",
            _ => "critical"
        };
    }
}
```

## Properties Deep Dive

### Total Memory
- **System Capacity**: Represents total physical RAM installed
- **Planning Baseline**: Used for capacity planning and upgrade decisions
- **Percentage Calculations**: Denominator for usage percentage calculations
- **Static Value**: Generally remains constant unless hardware changes

### Free Memory
- **Available Resources**: Memory immediately available for allocation
- **Dynamic Value**: Changes constantly based on system activity
- **Performance Indicator**: Low free memory indicates resource pressure
- **Cache Considerations**: May include memory used for file system cache on some systems

### Calculated Properties
- **Used Memory**: Simple calculation providing absolute memory consumption
- **Usage Percentage**: Primary monitoring metric for alerting and analysis
- **Precision**: Calculations maintain precision for accurate monitoring

## Performance Considerations

### Measurement Accuracy
1. **Cross-Platform Consistency**: Different calculation methods between Windows and Unix systems
2. **Cache Memory**: Some systems include cache in free memory calculations
3. **Buffer Memory**: Operating system buffers may affect reported values
4. **Timing Sensitivity**: Memory values can change rapidly during system activity

### Monitoring Best Practices
1. **Threshold Setting**: Set thresholds based on total memory capacity
2. **Trend Analysis**: Focus on trends rather than individual readings
3. **Context Awareness**: Consider system workload when interpreting values
4. **Alert Tuning**: Tune alerts to avoid notification fatigue

## Related Components

- **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** - Aggregate metrics container
- **[MemoryMetricsClient](MemoryMetricsClient.md)** - Memory metrics collection client
- **[CpuMetrics](CpuMetrics.md)** - CPU performance metrics
- **[ISystemResourceMonitor](ISystemResourceMonitor.md)** - Main monitoring interface
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Best Practices

### Monitoring Strategy
1. **Baseline Establishment**: Establish normal memory usage patterns for your system
2. **Threshold Configuration**: Set appropriate warning (80%) and critical (90%) thresholds
3. **Trend Monitoring**: Track memory usage trends over time
4. **Capacity Planning**: Use historical data for memory upgrade planning

### Alert Configuration
1. **Usage Thresholds**: Alert on sustained high usage (>80% for >10 minutes)
2. **Absolute Thresholds**: Alert when free memory drops below absolute minimums
3. **Trend Alerts**: Alert on rapid memory consumption increases
4. **Rate Limiting**: Implement alert cooldown periods to prevent spam

### Integration Guidelines
1. **Health Checks**: Include memory metrics in application health assessments
2. **Performance Testing**: Monitor memory during load testing
3. **Capacity Management**: Use for infrastructure capacity planning
4. **Application Optimization**: Guide application memory optimization efforts