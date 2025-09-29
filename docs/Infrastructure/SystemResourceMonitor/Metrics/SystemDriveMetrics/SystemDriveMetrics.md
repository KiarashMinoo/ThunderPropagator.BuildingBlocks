# SystemDriveMetrics

## Overview

The `SystemDriveMetrics` record represents comprehensive disk drive utilization metrics for individual storage devices. This immutable data model provides essential storage performance indicators including capacity, usage, and availability status for system monitoring and capacity planning.

## Purpose

- **Storage Monitoring**: Track disk drive utilization and capacity across all system drives
- **Capacity Planning**: Monitor storage consumption for proactive capacity management
- **Performance Analysis**: Identify storage bottlenecks and optimization opportunities
- **System Health**: Assess storage-related system health indicators

## Record Declaration

```csharp
public record SystemDriveMetrics(string Letter, double Total, double Free, bool IsReady)
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

### Letter
- **Type**: `string`
- **Description**: Drive identifier (e.g., "C:\", "/dev/sda1")
- **Platform Differences**: Windows uses drive letters, Unix uses mount paths
- **Usage**: Unique identifier for drive in monitoring systems

### Total
- **Type**: `double`
- **Description**: Total storage capacity in bytes
- **Source**: File system reported total size
- **Usage**: Baseline for capacity calculations and planning

### Free
- **Type**: `double`
- **Description**: Available (free) storage space in bytes
- **Source**: Currently unallocated disk space
- **Usage**: Indicates immediately available storage capacity

### IsReady
- **Type**: `bool`
- **Description**: Indicates whether the drive is accessible and ready for operations
- **Usage**: Filter for operational drives, exclude removable media not present
- **Reliability**: Essential for avoiding errors when accessing drive information

### Used (Calculated)
- **Type**: `double`
- **Description**: Currently allocated storage space in bytes
- **Calculation**: `Total - Free`
- **Usage**: Shows active storage consumption

### UsagePercentage (Calculated)
- **Type**: `double`
- **Description**: Storage utilization as a percentage (0-100)
- **Calculation**: `100.0 - ((1.0 * Free / Total) * 100)`
- **Usage**: Primary metric for storage monitoring and alerting

## Usage Examples

### Basic Drive Monitoring
```csharp
public class DriveMonitoringService
{
    private readonly ISystemResourceMonitor _monitor;

    public DriveMonitoringService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public void DisplayDriveStatus()
    {
        var metrics = _monitor.GetMetrics(window: 1000, all: false);
        var drives = metrics.Drives;
        
        Console.WriteLine("System Drive Status:");
        Console.WriteLine($"  Total Drives: {drives.Length}");
        Console.WriteLine();
        
        foreach (var drive in drives.OrderBy(d => d.Letter))
        {
            Console.WriteLine($"Drive {drive.Letter}:");
            Console.WriteLine($"  Total Space: {FormatBytes(drive.Total)}");
            Console.WriteLine($"  Free Space: {FormatBytes(drive.Free)}");
            Console.WriteLine($"  Used Space: {FormatBytes(drive.Used)}");
            Console.WriteLine($"  Usage: {drive.UsagePercentage:F2}%");
            Console.WriteLine($"  Status: {GetDriveStatus(drive)}");
            Console.WriteLine($"  Ready: {(drive.IsReady ? "Yes" : "No")}");
            
            if (drive.UsagePercentage > 90)
                Console.WriteLine($"  ⚠️  WARNING: Drive {drive.Letter} is {drive.UsagePercentage:F1}% full!");
            else if (drive.UsagePercentage > 80)
                Console.WriteLine($"  ⚠️  CAUTION: Drive {drive.Letter} is {drive.UsagePercentage:F1}% full");
            
            Console.WriteLine();
        }
        
        // Overall storage summary
        var totalCapacity = drives.Sum(d => d.Total);
        var totalFree = drives.Sum(d => d.Free);
        var overallUsage = ((totalCapacity - totalFree) / totalCapacity) * 100;
        
        Console.WriteLine("Overall Storage Summary:");
        Console.WriteLine($"  Total Capacity: {FormatBytes(totalCapacity)}");
        Console.WriteLine($"  Total Free: {FormatBytes(totalFree)}");
        Console.WriteLine($"  Overall Usage: {overallUsage:F2}%");
    }

    private string FormatBytes(double bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
        int counter = 0;
        decimal number = (decimal)bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }

    private string GetDriveStatus(SystemDriveMetrics drive)
    {
        if (!drive.IsReady)
            return "Not Ready";
        
        return drive.UsagePercentage switch
        {
            > 95 => "Critical",
            > 85 => "High Usage",
            > 70 => "Moderate Usage",
            > 50 => "Normal",
            _ => "Low Usage"
        };
    }
}
```

### Drive Analysis and Recommendations
```csharp
public class DriveAnalyzer
{
    public DriveAnalysisResult AnalyzeDrives(SystemDriveMetrics[] drives)
    {
        var readyDrives = drives.Where(d => d.IsReady).ToArray();
        
        return new DriveAnalysisResult
        {
            TotalDrives = drives.Length,
            ReadyDrives = readyDrives.Length,
            NotReadyDrives = drives.Length - readyDrives.Length,
            
            TotalCapacityBytes = readyDrives.Sum(d => d.Total),
            TotalFreeBytes = readyDrives.Sum(d => d.Free),
            TotalUsedBytes = readyDrives.Sum(d => d.Used),
            OverallUsagePercentage = CalculateOverallUsage(readyDrives),
            
            DriveDetails = readyDrives.Select(AnalyzeDrive).ToArray(),
            
            CriticalDrives = readyDrives.Where(d => d.UsagePercentage > 95).ToArray(),
            WarningDrives = readyDrives.Where(d => d.UsagePercentage > 85 && d.UsagePercentage <= 95).ToArray(),
            HealthyDrives = readyDrives.Where(d => d.UsagePercentage <= 85).ToArray(),
            
            StorageRecommendations = GenerateStorageRecommendations(readyDrives),
            CapacityPredictions = PredictCapacityExhaustion(readyDrives),
            
            OverallStorageHealth = AssessOverallStorageHealth(readyDrives)
        };
    }

    private double CalculateOverallUsage(SystemDriveMetrics[] drives)
    {
        if (!drives.Any()) return 0;
        
        var totalCapacity = drives.Sum(d => d.Total);
        var totalFree = drives.Sum(d => d.Free);
        
        return totalCapacity > 0 ? ((totalCapacity - totalFree) / totalCapacity) * 100 : 0;
    }

    private DriveDetail AnalyzeDrive(SystemDriveMetrics drive)
    {
        return new DriveDetail
        {
            Letter = drive.Letter,
            CapacityGB = drive.Total / (1024.0 * 1024.0 * 1024.0),
            FreeSpaceGB = drive.Free / (1024.0 * 1024.0 * 1024.0),
            UsedSpaceGB = drive.Used / (1024.0 * 1024.0 * 1024.0),
            UsagePercentage = drive.UsagePercentage,
            
            UsageClassification = ClassifyDriveUsage(drive.UsagePercentage),
            CapacityClass = ClassifyDriveCapacity(drive.Total),
            RemainingCapacityClass = ClassifyRemainingCapacity(drive.Free),
            
            TimeToFullEstimate = EstimateTimeToFull(drive),
            RecommendedActions = GetDriveRecommendations(drive),
            PerformanceImpact = AssessPerformanceImpact(drive)
        };
    }

    private DriveUsageLevel ClassifyDriveUsage(double usage) => usage switch
    {
        > 95 => DriveUsageLevel.Critical,
        > 85 => DriveUsageLevel.High,
        > 70 => DriveUsageLevel.Moderate,
        > 50 => DriveUsageLevel.Normal,
        _ => DriveUsageLevel.Low
    };

    private DriveCapacityClass ClassifyDriveCapacity(double totalBytes)
    {
        var totalGB = totalBytes / (1024.0 * 1024.0 * 1024.0);
        
        return totalGB switch
        {
            > 2048 => DriveCapacityClass.VeryLarge, // > 2TB
            > 1024 => DriveCapacityClass.Large,     // > 1TB
            > 512 => DriveCapacityClass.Medium,     // > 512GB
            > 128 => DriveCapacityClass.Small,      // > 128GB
            _ => DriveCapacityClass.VerySmall       // <= 128GB
        };
    }

    private RemainingCapacityClass ClassifyRemainingCapacity(double freeBytes)
    {
        var freeGB = freeBytes / (1024.0 * 1024.0 * 1024.0);
        
        return freeGB switch
        {
            > 100 => RemainingCapacityClass.Abundant,
            > 50 => RemainingCapacityClass.Adequate,
            > 20 => RemainingCapacityClass.Limited,
            > 5 => RemainingCapacityClass.Low,
            _ => RemainingCapacityClass.Critical
        };
    }

    private string EstimateTimeToFull(SystemDriveMetrics drive)
    {
        // This would require historical data for accurate prediction
        // For now, provide general guidance based on current state
        return drive.UsagePercentage switch
        {
            > 95 => "Immediate risk - days to weeks",
            > 90 => "High risk - weeks to months",
            > 85 => "Moderate risk - months",
            > 80 => "Low risk - months to years",
            _ => "No immediate risk"
        };
    }

    private List<string> GetDriveRecommendations(SystemDriveMetrics drive)
    {
        var recommendations = new List<string>();
        var freeGB = drive.Free / (1024.0 * 1024.0 * 1024.0);
        
        if (drive.UsagePercentage > 95)
        {
            recommendations.Add("URGENT: Clean up unnecessary files immediately");
            recommendations.Add("Move large files to other drives or external storage");
            recommendations.Add("Uninstall unused applications");
            recommendations.Add("Empty recycle bin and temporary files");
        }
        else if (drive.UsagePercentage > 85)
        {
            recommendations.Add("Clean up temporary files and downloads");
            recommendations.Add("Consider moving large files to other locations");
            recommendations.Add("Run disk cleanup utilities");
            recommendations.Add("Monitor usage trends closely");
        }
        else if (drive.UsagePercentage > 70)
        {
            recommendations.Add("Regular cleanup maintenance recommended");
            recommendations.Add("Monitor for large file growth");
            recommendations.Add("Plan for future storage needs");
        }
        
        if (freeGB < 5)
        {
            recommendations.Add("Critical: Less than 5GB free space remaining");
            recommendations.Add("System performance may be impacted");
        }
        else if (freeGB < 20)
        {
            recommendations.Add("Warning: Low free space may impact performance");
        }
        
        return recommendations;
    }

    private string AssessPerformanceImpact(SystemDriveMetrics drive)
    {
        var freeGB = drive.Free / (1024.0 * 1024.0 * 1024.0);
        
        return (drive.UsagePercentage, freeGB) switch
        {
            (> 95, _) => "Severe - System performance significantly impacted",
            (> 90, _) => "High - Noticeable performance degradation likely",
            (> 85, _) => "Moderate - Some performance impact possible",
            (_, < 5) => "High - Low free space impacts system operations",
            (_, < 10) => "Moderate - Limited space for system operations",
            _ => "Minimal - Adequate space for normal operations"
        };
    }

    private List<string> GenerateStorageRecommendations(SystemDriveMetrics[] drives)
    {
        var recommendations = new List<string>();
        var criticalCount = drives.Count(d => d.UsagePercentage > 95);
        var warningCount = drives.Count(d => d.UsagePercentage > 85 && d.UsagePercentage <= 95);
        var totalCapacityGB = drives.Sum(d => d.Total) / (1024.0 * 1024.0 * 1024.0);
        var totalFreeGB = drives.Sum(d => d.Free) / (1024.0 * 1024.0 * 1024.0);
        
        if (criticalCount > 0)
        {
            recommendations.Add($"CRITICAL: {criticalCount} drive(s) require immediate attention");
            recommendations.Add("Implement emergency cleanup procedures");
            recommendations.Add("Consider immediate storage expansion");
        }
        
        if (warningCount > 0)
        {
            recommendations.Add($"WARNING: {warningCount} drive(s) approaching capacity limits");
            recommendations.Add("Schedule storage cleanup and optimization");
        }
        
        if (totalFreeGB < totalCapacityGB * 0.1) // Less than 10% free overall
        {
            recommendations.Add("Overall storage utilization is very high");
            recommendations.Add("Consider adding additional storage capacity");
        }
        
        if (totalCapacityGB < 500) // Less than 500GB total
        {
            recommendations.Add("Consider upgrading to higher capacity storage");
            recommendations.Add("Implement storage tiering strategy");
        }
        
        if (recommendations.Count == 0)
        {
            recommendations.Add("Storage utilization is healthy");
            recommendations.Add("Continue regular monitoring and maintenance");
        }
        
        return recommendations;
    }

    private Dictionary<string, string> PredictCapacityExhaustion(SystemDriveMetrics[] drives)
    {
        // This would require historical data for accurate predictions
        // For now, provide simple risk assessment
        
        var predictions = new Dictionary<string, string>();
        
        foreach (var drive in drives)
        {
            var risk = drive.UsagePercentage switch
            {
                > 95 => "Immediate risk (days)",
                > 90 => "High risk (weeks)",
                > 85 => "Moderate risk (months)",
                > 80 => "Low risk (6+ months)",
                _ => "No immediate risk"
            };
            
            predictions[drive.Letter] = risk;
        }
        
        return predictions;
    }

    private string AssessOverallStorageHealth(SystemDriveMetrics[] drives)
    {
        if (!drives.Any()) return "No drives available";
        
        var criticalCount = drives.Count(d => d.UsagePercentage > 95);
        var warningCount = drives.Count(d => d.UsagePercentage > 85);
        var averageUsage = drives.Average(d => d.UsagePercentage);
        
        return (criticalCount, warningCount, averageUsage) switch
        {
            (> 0, _, _) => "Critical - Immediate attention required",
            (0, > drives.Length / 2, _) => "Poor - Multiple drives approaching capacity",
            (0, > 0, _) => "Warning - Some drives require attention",
            (0, 0, > 70) => "Moderate - Higher than optimal usage",
            _ => "Healthy - Storage utilization within normal ranges"
        };
    }
}

public enum DriveUsageLevel { Low, Normal, Moderate, High, Critical }
public enum DriveCapacityClass { VerySmall, Small, Medium, Large, VeryLarge }
public enum RemainingCapacityClass { Critical, Low, Limited, Adequate, Abundant }

public class DriveAnalysisResult
{
    public int TotalDrives { get; set; }
    public int ReadyDrives { get; set; }
    public int NotReadyDrives { get; set; }
    
    public double TotalCapacityBytes { get; set; }
    public double TotalFreeBytes { get; set; }
    public double TotalUsedBytes { get; set; }
    public double OverallUsagePercentage { get; set; }
    
    public DriveDetail[] DriveDetails { get; set; } = Array.Empty<DriveDetail>();
    
    public SystemDriveMetrics[] CriticalDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] WarningDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] HealthyDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    
    public List<string> StorageRecommendations { get; set; } = new();
    public Dictionary<string, string> CapacityPredictions { get; set; } = new();
    public string OverallStorageHealth { get; set; } = "";
}

public class DriveDetail
{
    public string Letter { get; set; } = "";
    public double CapacityGB { get; set; }
    public double FreeSpaceGB { get; set; }
    public double UsedSpaceGB { get; set; }
    public double UsagePercentage { get; set; }
    
    public DriveUsageLevel UsageClassification { get; set; }
    public DriveCapacityClass CapacityClass { get; set; }
    public RemainingCapacityClass RemainingCapacityClass { get; set; }
    
    public string TimeToFullEstimate { get; set; } = "";
    public List<string> RecommendedActions { get; set; } = new();
    public string PerformanceImpact { get; set; } = "";
}
```

### Real-time Drive Monitoring
```csharp
public class DriveMonitoringDashboard
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly Dictionary<string, List<(DateTime Time, SystemDriveMetrics Metrics)>> _driveHistory = new();
    private readonly object _lock = new();

    public DriveMonitoringDashboard(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public async Task StartMonitoring(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting Drive Monitoring Dashboard...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var timestamp = DateTime.UtcNow;
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            var drives = metrics.Drives;
            
            UpdateDriveHistory(drives, timestamp);
            DisplayDriveDashboard(drives, timestamp);
            CheckDriveAlerts(drives);
            
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken); // Check every 5 minutes
        }
    }

    private void UpdateDriveHistory(SystemDriveMetrics[] drives, DateTime timestamp)
    {
        lock (_lock)
        {
            foreach (var drive in drives.Where(d => d.IsReady))
            {
                if (!_driveHistory.ContainsKey(drive.Letter))
                {
                    _driveHistory[drive.Letter] = new List<(DateTime, SystemDriveMetrics)>();
                }
                
                _driveHistory[drive.Letter].Add((timestamp, drive));
                
                // Keep last 24 hours (288 entries at 5-minute intervals)
                if (_driveHistory[drive.Letter].Count > 288)
                {
                    _driveHistory[drive.Letter].RemoveAt(0);
                }
            }
        }
    }

    private void DisplayDriveDashboard(SystemDriveMetrics[] drives, DateTime timestamp)
    {
        Console.Clear();
        Console.WriteLine("=== Drive Monitoring Dashboard ===");
        Console.WriteLine($"Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();
        
        // Current drive status
        Console.WriteLine("Current Drive Status:");
        foreach (var drive in drives.Where(d => d.IsReady).OrderBy(d => d.Letter))
        {
            var freeGB = drive.Free / (1024.0 * 1024.0 * 1024.0);
            var totalGB = drive.Total / (1024.0 * 1024.0 * 1024.0);
            
            Console.WriteLine($"  {drive.Letter}: {GenerateUsageBar(drive.UsagePercentage)} " +
                            $"{drive.UsagePercentage:F1}% ({freeGB:F1}GB free of {totalGB:F1}GB)");
        }
        Console.WriteLine();
        
        // Drive trends
        DisplayDriveTrends();
        
        // Storage summary
        DisplayStorageSummary(drives);
    }

    private string GenerateUsageBar(double usage)
    {
        var barLength = 20;
        var filled = (int)(usage / 100.0 * barLength);
        var bar = new string('█', filled) + new string('░', barLength - filled);
        
        var color = usage switch
        {
            > 95 => "🔴",
            > 85 => "🟡",
            > 70 => "🟢",
            _ => "⚪"
        };
        
        return $"{color} [{bar}]";
    }

    private void DisplayDriveTrends()
    {
        Console.WriteLine("24-Hour Trends:");
        
        lock (_lock)
        {
            foreach (var driveGroup in _driveHistory.OrderBy(kvp => kvp.Key))
            {
                var driveLetter = driveGroup.Key;
                var history = driveGroup.Value;
                
                if (history.Count >= 2)
                {
                    var recent = history.TakeLast(Math.Min(48, history.Count)).ToArray(); // Last 4 hours
                    var usageValues = recent.Select(h => h.Metrics.UsagePercentage).ToArray();
                    var freeValues = recent.Select(h => h.Metrics.Free / (1024.0 * 1024.0 * 1024.0)).ToArray();
                    
                    var usageTrend = CalculateTrend(usageValues);
                    var freeTrend = CalculateTrend(freeValues);
                    
                    Console.WriteLine($"  {driveLetter}: Usage {usageTrend}, Free Space {freeTrend}");
                }
            }
        }
        Console.WriteLine();
    }

    private void DisplayStorageSummary(SystemDriveMetrics[] drives)
    {
        var readyDrives = drives.Where(d => d.IsReady).ToArray();
        var totalCapacityGB = readyDrives.Sum(d => d.Total) / (1024.0 * 1024.0 * 1024.0);
        var totalFreeGB = readyDrives.Sum(d => d.Free) / (1024.0 * 1024.0 * 1024.0);
        var overallUsage = ((totalCapacityGB - totalFreeGB) / totalCapacityGB) * 100;
        
        Console.WriteLine("Storage Summary:");
        Console.WriteLine($"  Total Capacity: {totalCapacityGB:F1} GB");
        Console.WriteLine($"  Total Free: {totalFreeGB:F1} GB");
        Console.WriteLine($"  Overall Usage: {overallUsage:F1}%");
        Console.WriteLine($"  Ready Drives: {readyDrives.Length}");
        Console.WriteLine($"  Critical Drives: {readyDrives.Count(d => d.UsagePercentage > 95)}");
        Console.WriteLine($"  Warning Drives: {readyDrives.Count(d => d.UsagePercentage > 85 && d.UsagePercentage <= 95)}");
    }

    private void CheckDriveAlerts(SystemDriveMetrics[] drives)
    {
        var alerts = new List<string>();
        
        foreach (var drive in drives.Where(d => d.IsReady))
        {
            var freeGB = drive.Free / (1024.0 * 1024.0 * 1024.0);
            
            if (drive.UsagePercentage > 95)
                alerts.Add($"🚨 CRITICAL: Drive {drive.Letter} is {drive.UsagePercentage:F1}% full");
            else if (drive.UsagePercentage > 90)
                alerts.Add($"⚠️ WARNING: Drive {drive.Letter} is {drive.UsagePercentage:F1}% full");
            
            if (freeGB < 1)
                alerts.Add($"🚨 CRITICAL: Drive {drive.Letter} has less than 1GB free");
            else if (freeGB < 5)
                alerts.Add($"⚠️ WARNING: Drive {drive.Letter} has less than 5GB free");
        }
        
        if (alerts.Any())
        {
            Console.WriteLine();
            Console.WriteLine("DRIVE ALERTS:");
            foreach (var alert in alerts)
            {
                Console.WriteLine($"  {alert}");
            }
        }
    }

    private string CalculateTrend(double[] values)
    {
        if (values.Length < 3) return "Stable";
        
        var firstThird = values.Take(values.Length / 3).Average();
        var lastThird = values.TakeLast(values.Length / 3).Average();
        
        var change = Math.Abs(lastThird - firstThird);
        var direction = lastThird > firstThird ? "Rising" : "Falling";
        
        return change switch
        {
            > 2.0 => $"{direction} Rapidly",
            > 0.5 => direction,
            _ => "Stable"
        };
    }
}
```

### Health Check Integration
```csharp
public class DriveHealthCheck : IHealthCheck
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly DriveHealthThresholds _thresholds;

    public DriveHealthCheck(ISystemResourceMonitor monitor, IOptions<DriveHealthThresholds> thresholds)
    {
        _monitor = monitor;
        _thresholds = thresholds.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = _monitor.GetMetrics(window: 1000, all: false);
            var drives = metrics.Drives.Where(d => d.IsReady).ToArray();
            
            var healthData = new Dictionary<string, object>
            {
                ["total_drives"] = drives.Length,
                ["drives"] = drives.Select(d => new
                {
                    letter = d.Letter,
                    total_gb = Math.Round(d.Total / (1024.0 * 1024.0 * 1024.0), 2),
                    free_gb = Math.Round(d.Free / (1024.0 * 1024.0 * 1024.0), 2),
                    usage_percent = Math.Round(d.UsagePercentage, 2),
                    is_ready = d.IsReady
                }).ToArray()
            };

            var status = DetermineHealthStatus(drives);
            var description = GenerateHealthDescription(drives, status);
            
            return new HealthCheckResult(status, description, data: healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to collect drive metrics", ex);
        }
    }

    private HealthStatus DetermineHealthStatus(SystemDriveMetrics[] drives)
    {
        var criticalDrives = drives.Where(d => 
            d.UsagePercentage > _thresholds.Critical || 
            d.Free < _thresholds.MinimumFreeBytes).ToArray();
        
        var warningDrives = drives.Where(d => 
            d.UsagePercentage > _thresholds.Warning || 
            d.Free < _thresholds.LowFreeBytes).ToArray();
        
        if (criticalDrives.Any())
            return HealthStatus.Unhealthy;
        
        if (warningDrives.Any())
            return HealthStatus.Degraded;
        
        return HealthStatus.Healthy;
    }

    private string GenerateHealthDescription(SystemDriveMetrics[] drives, HealthStatus status)
    {
        var criticalCount = drives.Count(d => d.UsagePercentage > _thresholds.Critical);
        var warningCount = drives.Count(d => d.UsagePercentage > _thresholds.Warning);
        
        return status switch
        {
            HealthStatus.Unhealthy => $"Critical: {criticalCount} drive(s) require immediate attention",
            HealthStatus.Degraded => $"Warning: {warningCount} drive(s) approaching capacity limits",
            _ => $"All {drives.Length} drives operating within normal parameters"
        };
    }
}

public class DriveHealthThresholds
{
    public double Warning { get; set; } = 85.0;
    public double Critical { get; set; } = 95.0;
    public double LowFreeBytes { get; set; } = 5.0 * 1024 * 1024 * 1024; // 5GB
    public double MinimumFreeBytes { get; set; } = 1.0 * 1024 * 1024 * 1024; // 1GB
}
```

## Properties Deep Dive

### Drive Identification
- **Letter Property**: Platform-specific drive identification
- **Windows**: Drive letters like "C:\", "D:\"
- **Unix/Linux**: Mount paths like "/", "/home", "/var"
- **Consistency**: Provides unique identifier across monitoring cycles

### Storage Calculations
- **Byte Precision**: All values stored in bytes for maximum precision
- **Conversion Needs**: Convert to appropriate units (KB, MB, GB, TB) for display
- **Percentage Accuracy**: Usage percentage calculated with proper precision
- **Ready State**: Essential for filtering accessible drives

### Calculated Properties
- **Used Space**: Simple subtraction providing absolute consumption
- **Usage Percentage**: Primary monitoring metric for alerting
- **Precision Handling**: Maintains accuracy for small drives and large drives

## Performance Considerations

### Drive Access
1. **Ready State Check**: Always verify `IsReady` before accessing drive information
2. **Exception Handling**: Handle potential exceptions when accessing drive properties
3. **Removable Media**: Consider removable media that may not be present
4. **Network Drives**: May have different performance characteristics

### Monitoring Frequency
1. **Storage Changes**: Drive usage typically changes slowly compared to CPU/memory
2. **Appropriate Intervals**: Monitor every 5-15 minutes for most scenarios
3. **Real-time Needs**: More frequent monitoring during active file operations
4. **Performance Impact**: Drive enumeration has minimal system impact

## Related Components

- **[SystemDriveMetricsClient](SystemDriveMetricsClient.md)** - Drive metrics collection client
- **[SystemResourceMonitorMetrics](../SystemResourceMonitorMetrics.md)** - Aggregate metrics container
- **[MemoryMetrics](../MemoryMetrics.md)** - Memory performance metrics
- **[ISystemResourceMonitor](../ISystemResourceMonitor.md)** - Main monitoring interface
- **[System Resource Monitor Overview](../README.md)** - Complete documentation

## Best Practices

### Monitoring Strategy
1. **Threshold Setting**: Set appropriate warning (85%) and critical (95%) thresholds
2. **Absolute Minimums**: Also monitor absolute free space (e.g., < 5GB)
3. **Drive-Specific Thresholds**: Consider different thresholds for different drive types
4. **Trend Analysis**: Focus on usage trends rather than individual readings

### Alert Configuration
1. **Usage Alerts**: Alert on high usage percentages with appropriate thresholds
2. **Absolute Space**: Alert when free space drops below minimum operational requirements
3. **Multiple Drives**: Consider system-wide storage pressure, not just individual drives
4. **Rate Limiting**: Implement alert cooldown periods to prevent notification spam

### Integration Guidelines
1. **Health Checks**: Include drive metrics in application health assessments
2. **Capacity Planning**: Use for infrastructure capacity planning and procurement
3. **Automation**: Integrate with automated cleanup and archival processes
4. **Performance Monitoring**: Correlate storage usage with application performance