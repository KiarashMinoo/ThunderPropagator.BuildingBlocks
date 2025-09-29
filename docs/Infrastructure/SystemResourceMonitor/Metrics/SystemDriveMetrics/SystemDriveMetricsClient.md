# SystemDriveMetricsClient

## Overview

The `SystemDriveMetricsClient` class is responsible for collecting storage metrics from all available system drives. This client provides comprehensive drive information including capacity, free space, and operational status across all mounted storage devices.

## Purpose

- **Drive Enumeration**: Collect metrics from all system drives and storage devices
- **Storage Monitoring**: Provide real-time storage utilization data
- **Cross-Platform Support**: Work consistently across different operating systems
- **Capacity Planning**: Support storage capacity planning and management

## Class Declaration

```csharp
public class SystemDriveMetricsClient
{
    public SystemDriveMetrics[] GetMetrics()
}
```

## Methods

### GetMetrics()

Collects comprehensive storage metrics from all available and ready system drives.

#### Returns
- **SystemDriveMetrics[]**: Array of drive metrics for all accessible storage devices

#### Implementation
- Uses `DriveInfo.GetDrives()` to enumerate all system drives
- Filters for ready drives (`drive.IsReady`)
- Collects capacity, free space, and identification information
- Returns standardized metrics for each accessible drive

## Usage Examples

### Basic Drive Collection
```csharp
public class DriveCollectionService
{
    private readonly SystemDriveMetricsClient _client;

    public DriveCollectionService()
    {
        _client = new SystemDriveMetricsClient();
    }

    public void DisplayAllDrives()
    {
        try
        {
            var drives = _client.GetMetrics();
            
            Console.WriteLine($"Found {drives.Length} accessible drives:");
            Console.WriteLine();
            
            foreach (var drive in drives.OrderBy(d => d.Letter))
            {
                var totalGB = drive.Total / (1024.0 * 1024.0 * 1024.0);
                var freeGB = drive.Free / (1024.0 * 1024.0 * 1024.0);
                var usedGB = drive.Used / (1024.0 * 1024.0 * 1024.0);
                
                Console.WriteLine($"Drive: {drive.Letter}");
                Console.WriteLine($"  Total Capacity: {totalGB:F2} GB");
                Console.WriteLine($"  Free Space: {freeGB:F2} GB");
                Console.WriteLine($"  Used Space: {usedGB:F2} GB");
                Console.WriteLine($"  Usage: {drive.UsagePercentage:F2}%");
                Console.WriteLine($"  Ready: {drive.IsReady}");
                Console.WriteLine();
            }
            
            // Summary statistics
            var totalCapacity = drives.Sum(d => d.Total) / (1024.0 * 1024.0 * 1024.0);
            var totalFree = drives.Sum(d => d.Free) / (1024.0 * 1024.0 * 1024.0);
            var overallUsage = ((totalCapacity - totalFree) / totalCapacity) * 100;
            
            Console.WriteLine("System Storage Summary:");
            Console.WriteLine($"  Total Storage: {totalCapacity:F2} GB");
            Console.WriteLine($"  Total Free: {totalFree:F2} GB");
            Console.WriteLine($"  Overall Usage: {overallUsage:F2}%");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collecting drive metrics: {ex.Message}");
        }
    }
}
```

### Advanced Drive Analysis
```csharp
public class AdvancedDriveAnalyzer
{
    private readonly SystemDriveMetricsClient _client = new();

    public async Task<DriveSystemAnalysis> AnalyzeDriveSystem()
    {
        var drives = _client.GetMetrics();
        
        var analysis = new DriveSystemAnalysis
        {
            CollectionTime = DateTime.UtcNow,
            TotalDrives = drives.Length,
            
            // Basic statistics
            DriveStatistics = CalculateDriveStatistics(drives),
            
            // Categorize drives
            DriveCategories = CategorizeDrives(drives),
            
            // Storage distribution
            StorageDistribution = AnalyzeStorageDistribution(drives),
            
            // Risk assessment
            RiskAssessment = AssessStorageRisks(drives),
            
            // Performance indicators
            PerformanceIndicators = AnalyzePerformanceIndicators(drives),
            
            // Recommendations
            SystemRecommendations = GenerateSystemRecommendations(drives)
        };
        
        return analysis;
    }

    private DriveStatistics CalculateDriveStatistics(SystemDriveMetrics[] drives)
    {
        if (!drives.Any())
        {
            return new DriveStatistics { HasDrives = false };
        }
        
        var usagePercentages = drives.Select(d => d.UsagePercentage).ToArray();
        var capacitiesGB = drives.Select(d => d.Total / (1024.0 * 1024.0 * 1024.0)).ToArray();
        var freeSpaceGB = drives.Select(d => d.Free / (1024.0 * 1024.0 * 1024.0)).ToArray();
        
        return new DriveStatistics
        {
            HasDrives = true,
            TotalDrives = drives.Length,
            
            // Usage statistics
            AverageUsagePercent = usagePercentages.Average(),
            MinUsagePercent = usagePercentages.Min(),
            MaxUsagePercent = usagePercentages.Max(),
            UsageStandardDeviation = CalculateStandardDeviation(usagePercentages),
            
            // Capacity statistics
            TotalCapacityGB = capacitiesGB.Sum(),
            AverageCapacityGB = capacitiesGB.Average(),
            LargestDriveGB = capacitiesGB.Max(),
            SmallestDriveGB = capacitiesGB.Min(),
            
            // Free space statistics
            TotalFreeSpaceGB = freeSpaceGB.Sum(),
            AverageFreeSpaceGB = freeSpaceGB.Average(),
            MostFreeSpaceGB = freeSpaceGB.Max(),
            LeastFreeSpaceGB = freeSpaceGB.Min(),
            
            // Derived metrics
            OverallUsagePercent = ((capacitiesGB.Sum() - freeSpaceGB.Sum()) / capacitiesGB.Sum()) * 100,
            StorageEfficiency = CalculateStorageEfficiency(drives),
            CapacityDistribution = CalculateCapacityDistribution(capacitiesGB)
        };
    }

    private DriveCategories CategorizeDrives(SystemDriveMetrics[] drives)
    {
        return new DriveCategories
        {
            // By usage level
            CriticalDrives = drives.Where(d => d.UsagePercentage > 95).ToArray(),
            HighUsageDrives = drives.Where(d => d.UsagePercentage > 85 && d.UsagePercentage <= 95).ToArray(),
            ModerateUsageDrives = drives.Where(d => d.UsagePercentage > 70 && d.UsagePercentage <= 85).ToArray(),
            LowUsageDrives = drives.Where(d => d.UsagePercentage <= 70).ToArray(),
            
            // By capacity
            LargeDrives = drives.Where(d => d.Total > 1024L * 1024 * 1024 * 1024).ToArray(), // > 1TB
            MediumDrives = drives.Where(d => d.Total > 100L * 1024 * 1024 * 1024 && d.Total <= 1024L * 1024 * 1024 * 1024).ToArray(), // 100GB - 1TB
            SmallDrives = drives.Where(d => d.Total <= 100L * 1024 * 1024 * 1024).ToArray(), // <= 100GB
            
            // By free space
            LowFreeSpace = drives.Where(d => d.Free < 5L * 1024 * 1024 * 1024).ToArray(), // < 5GB
            ModeratelyFull = drives.Where(d => d.Free >= 5L * 1024 * 1024 * 1024 && d.Free < 20L * 1024 * 1024 * 1024).ToArray(), // 5-20GB
            AmpleFreeSpace = drives.Where(d => d.Free >= 20L * 1024 * 1024 * 1024).ToArray() // >= 20GB
        };
    }

    private StorageDistribution AnalyzeStorageDistribution(SystemDriveMetrics[] drives)
    {
        var totalCapacity = drives.Sum(d => d.Total);
        
        return new StorageDistribution
        {
            DriveDistribution = drives.Select(d => new DriveDistributionInfo
            {
                DriveLetter = d.Letter,
                CapacityPercent = (d.Total / totalCapacity) * 100,
                UsagePercent = d.UsagePercentage,
                CapacityGB = d.Total / (1024.0 * 1024.0 * 1024.0),
                FreeSpaceGB = d.Free / (1024.0 * 1024.0 * 1024.0),
                Role = ClassifyDriveRole(d, drives)
            }).OrderByDescending(d => d.CapacityPercent).ToArray(),
            
            CapacityConcentration = CalculateCapacityConcentration(drives),
            UsageBalance = CalculateUsageBalance(drives),
            StorageFragmentation = CalculateStorageFragmentation(drives)
        };
    }

    private RiskAssessment AssessStorageRisks(SystemDriveMetrics[] drives)
    {
        var risks = new List<StorageRisk>();
        var riskLevel = StorageRiskLevel.Low;
        
        // Check for critical space conditions
        var criticalDrives = drives.Where(d => d.UsagePercentage > 95).ToArray();
        if (criticalDrives.Any())
        {
            riskLevel = StorageRiskLevel.Critical;
            risks.Add(new StorageRisk
            {
                Type = "Critical Space Usage",
                Level = StorageRiskLevel.Critical,
                Description = $"{criticalDrives.Length} drive(s) above 95% capacity",
                AffectedDrives = criticalDrives.Select(d => d.Letter).ToArray(),
                ImpactAssessment = "System stability at risk, immediate action required",
                Mitigation = "Free up space immediately, consider emergency cleanup"
            });
        }
        
        // Check for low free space
        var lowFreeSpace = drives.Where(d => d.Free < 1024L * 1024 * 1024).ToArray(); // < 1GB
        if (lowFreeSpace.Any())
        {
            riskLevel = Math.Max(riskLevel, StorageRiskLevel.High);
            risks.Add(new StorageRisk
            {
                Type = "Low Free Space",
                Level = StorageRiskLevel.High,
                Description = $"{lowFreeSpace.Length} drive(s) with less than 1GB free",
                AffectedDrives = lowFreeSpace.Select(d => d.Letter).ToArray(),
                ImpactAssessment = "Performance degradation likely, operations may fail",
                Mitigation = "Clean up temporary files, move data to other drives"
            });
        }
        
        // Check for unbalanced storage
        var usageVariance = CalculateUsageVariance(drives);
        if (usageVariance > 30 && drives.Length > 1)
        {
            riskLevel = Math.Max(riskLevel, StorageRiskLevel.Medium);
            risks.Add(new StorageRisk
            {
                Type = "Unbalanced Storage Usage",
                Level = StorageRiskLevel.Medium,
                Description = $"High variance in drive usage ({usageVariance:F1}%)",
                AffectedDrives = drives.Select(d => d.Letter).ToArray(),
                ImpactAssessment = "Inefficient storage utilization, potential hotspots",
                Mitigation = "Redistribute data across drives for better balance"
            });
        }
        
        // Check for single points of failure
        if (drives.Length == 1)
        {
            riskLevel = Math.Max(riskLevel, StorageRiskLevel.Medium);
            risks.Add(new StorageRisk
            {
                Type = "Single Storage Device",
                Level = StorageRiskLevel.Medium,
                Description = "System relies on single storage device",
                AffectedDrives = drives.Select(d => d.Letter).ToArray(),
                ImpactAssessment = "No redundancy, total data loss risk if drive fails",
                Mitigation = "Consider adding additional storage for redundancy"
            });
        }
        
        return new RiskAssessment
        {
            OverallRiskLevel = riskLevel,
            IdentifiedRisks = risks,
            RiskScore = CalculateRiskScore(drives),
            RecommendedActions = GenerateRiskMitigationActions(risks)
        };
    }

    private PerformanceIndicators AnalyzePerformanceIndicators(SystemDriveMetrics[] drives)
    {
        return new PerformanceIndicators
        {
            StorageEfficiency = CalculateStorageEfficiency(drives),
            CapacityUtilization = CalculateCapacityUtilization(drives),
            SpaceDistributionIndex = CalculateSpaceDistributionIndex(drives),
            PerformanceRisk = AssessPerformanceRisk(drives),
            OptimizationPotential = CalculateOptimizationPotential(drives)
        };
    }

    private List<string> GenerateSystemRecommendations(SystemDriveMetrics[] drives)
    {
        var recommendations = new List<string>();
        
        // Critical recommendations
        var criticalDrives = drives.Where(d => d.UsagePercentage > 95).ToArray();
        if (criticalDrives.Any())
        {
            recommendations.Add($"URGENT: Clean up {criticalDrives.Length} drive(s) with critical space usage");
            recommendations.Add("Implement emergency data archival procedures");
        }
        
        // Capacity planning
        var overallUsage = drives.Any() ? ((drives.Sum(d => d.Total) - drives.Sum(d => d.Free)) / drives.Sum(d => d.Total)) * 100 : 0;
        if (overallUsage > 80)
        {
            recommendations.Add("Consider expanding total storage capacity");
            recommendations.Add("Evaluate data retention policies");
        }
        
        // Storage optimization
        var usageVariance = CalculateUsageVariance(drives);
        if (usageVariance > 25 && drives.Length > 1)
        {
            recommendations.Add("Rebalance data distribution across drives");
            recommendations.Add("Implement automated storage tiering");
        }
        
        // Redundancy
        if (drives.Length == 1)
        {
            recommendations.Add("Add additional storage devices for redundancy");
            recommendations.Add("Implement backup strategy for data protection");
        }
        
        // Maintenance
        recommendations.Add("Schedule regular storage cleanup maintenance");
        recommendations.Add("Monitor storage trends for proactive capacity planning");
        
        return recommendations;
    }

    // Helper calculation methods
    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Length - 1));
    }

    private double CalculateStorageEfficiency(SystemDriveMetrics[] drives)
    {
        if (!drives.Any()) return 0;
        
        var totalCapacity = drives.Sum(d => d.Total);
        var totalUsed = drives.Sum(d => d.Used);
        var avgUsage = drives.Average(d => d.UsagePercentage);
        
        // Efficiency considers both utilization and balance
        var utilizationFactor = (totalUsed / totalCapacity) * 100;
        var balanceFactor = 100 - CalculateUsageVariance(drives);
        
        return (utilizationFactor * 0.7 + balanceFactor * 0.3);
    }

    private Dictionary<string, double> CalculateCapacityDistribution(double[] capacities)
    {
        var total = capacities.Sum();
        return new Dictionary<string, double>
        {
            ["largest_drive_percent"] = (capacities.Max() / total) * 100,
            ["smallest_drive_percent"] = (capacities.Min() / total) * 100,
            ["capacity_concentration"] = CalculateConcentrationIndex(capacities)
        };
    }

    private double CalculateConcentrationIndex(double[] values)
    {
        // Herfindahl-Hirschman Index for capacity concentration
        var total = values.Sum();
        return values.Sum(v => Math.Pow(v / total, 2)) * 10000;
    }

    private string ClassifyDriveRole(SystemDriveMetrics drive, SystemDriveMetrics[] allDrives)
    {
        var totalCapacity = allDrives.Sum(d => d.Total);
        var driveCapacityPercent = (drive.Total / totalCapacity) * 100;
        
        return (driveCapacityPercent, drive.Letter.ToUpper()) switch
        {
            (> 50, _) => "Primary Storage",
            (_, "C:\\") => "System Drive",
            (> 20, _) => "Secondary Storage",
            _ => "Additional Storage"
        };
    }

    private double CalculateCapacityConcentration(SystemDriveMetrics[] drives)
    {
        var capacities = drives.Select(d => d.Total).ToArray();
        return CalculateConcentrationIndex(capacities);
    }

    private double CalculateUsageBalance(SystemDriveMetrics[] drives)
    {
        if (drives.Length <= 1) return 100;
        
        var usageVariance = CalculateUsageVariance(drives);
        return Math.Max(0, 100 - usageVariance);
    }

    private double CalculateStorageFragmentation(SystemDriveMetrics[] drives)
    {
        // Simple fragmentation indicator based on drive count and size distribution
        if (drives.Length <= 1) return 0;
        
        var capacities = drives.Select(d => d.Total).ToArray();
        var sizeVariance = CalculateStandardDeviation(capacities);
        var meanCapacity = capacities.Average();
        
        return (sizeVariance / meanCapacity) * 100;
    }

    private double CalculateUsageVariance(SystemDriveMetrics[] drives)
    {
        if (drives.Length <= 1) return 0;
        
        var usagePercentages = drives.Select(d => d.UsagePercentage).ToArray();
        return CalculateStandardDeviation(usagePercentages);
    }

    private int CalculateRiskScore(SystemDriveMetrics[] drives)
    {
        var score = 0;
        
        // High usage penalty
        score += drives.Count(d => d.UsagePercentage > 95) * 30;
        score += drives.Count(d => d.UsagePercentage > 85) * 15;
        
        // Low free space penalty
        score += drives.Count(d => d.Free < 1024L * 1024 * 1024) * 25; // < 1GB
        score += drives.Count(d => d.Free < 5L * 1024 * 1024 * 1024) * 10; // < 5GB
        
        // Single drive penalty
        if (drives.Length == 1) score += 20;
        
        return Math.Min(100, score);
    }

    private List<string> GenerateRiskMitigationActions(List<StorageRisk> risks)
    {
        var actions = new List<string>();
        
        foreach (var risk in risks.OrderByDescending(r => r.Level))
        {
            actions.Add($"Address {risk.Type}: {risk.Mitigation}");
        }
        
        if (!actions.Any())
        {
            actions.Add("Continue regular monitoring and maintenance");
        }
        
        return actions;
    }

    private double CalculateCapacityUtilization(SystemDriveMetrics[] drives)
    {
        if (!drives.Any()) return 0;
        
        var totalCapacity = drives.Sum(d => d.Total);
        var totalUsed = drives.Sum(d => d.Used);
        
        return (totalUsed / totalCapacity) * 100;
    }

    private double CalculateSpaceDistributionIndex(SystemDriveMetrics[] drives)
    {
        if (drives.Length <= 1) return 100;
        
        var freeSpacePercentages = drives.Select(d => (d.Free / d.Total) * 100).ToArray();
        var variance = CalculateStandardDeviation(freeSpacePercentages);
        
        return Math.Max(0, 100 - variance);
    }

    private string AssessPerformanceRisk(SystemDriveMetrics[] drives)
    {
        var criticalCount = drives.Count(d => d.UsagePercentage > 95);
        var lowFreeCount = drives.Count(d => d.Free < 1024L * 1024 * 1024);
        
        return (criticalCount, lowFreeCount) switch
        {
            (> 0, _) => "High - Critical space usage detected",
            (0, > 0) => "Medium - Low free space may impact performance",
            _ => "Low - Adequate space for normal operations"
        };
    }

    private double CalculateOptimizationPotential(SystemDriveMetrics[] drives)
    {
        if (drives.Length <= 1) return 0;
        
        var usageVariance = CalculateUsageVariance(drives);
        var balancePotential = Math.Min(50, usageVariance);
        
        var overallUsage = CalculateCapacityUtilization(drives);
        var utilizationPotential = overallUsage < 70 ? (70 - overallUsage) : 0;
        
        return balancePotential + utilizationPotential;
    }
}

// Supporting data structures
public class DriveSystemAnalysis
{
    public DateTime CollectionTime { get; set; }
    public int TotalDrives { get; set; }
    public DriveStatistics DriveStatistics { get; set; } = new();
    public DriveCategories DriveCategories { get; set; } = new();
    public StorageDistribution StorageDistribution { get; set; } = new();
    public RiskAssessment RiskAssessment { get; set; } = new();
    public PerformanceIndicators PerformanceIndicators { get; set; } = new();
    public List<string> SystemRecommendations { get; set; } = new();
}

public class DriveStatistics
{
    public bool HasDrives { get; set; }
    public int TotalDrives { get; set; }
    
    public double AverageUsagePercent { get; set; }
    public double MinUsagePercent { get; set; }
    public double MaxUsagePercent { get; set; }
    public double UsageStandardDeviation { get; set; }
    
    public double TotalCapacityGB { get; set; }
    public double AverageCapacityGB { get; set; }
    public double LargestDriveGB { get; set; }
    public double SmallestDriveGB { get; set; }
    
    public double TotalFreeSpaceGB { get; set; }
    public double AverageFreeSpaceGB { get; set; }
    public double MostFreeSpaceGB { get; set; }
    public double LeastFreeSpaceGB { get; set; }
    
    public double OverallUsagePercent { get; set; }
    public double StorageEfficiency { get; set; }
    public Dictionary<string, double> CapacityDistribution { get; set; } = new();
}

public class DriveCategories
{
    public SystemDriveMetrics[] CriticalDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] HighUsageDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] ModerateUsageDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] LowUsageDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    
    public SystemDriveMetrics[] LargeDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] MediumDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] SmallDrives { get; set; } = Array.Empty<SystemDriveMetrics>();
    
    public SystemDriveMetrics[] LowFreeSpace { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] ModeratelyFull { get; set; } = Array.Empty<SystemDriveMetrics>();
    public SystemDriveMetrics[] AmpleFreeSpace { get; set; } = Array.Empty<SystemDriveMetrics>();
}

public class StorageDistribution
{
    public DriveDistributionInfo[] DriveDistribution { get; set; } = Array.Empty<DriveDistributionInfo>();
    public double CapacityConcentration { get; set; }
    public double UsageBalance { get; set; }
    public double StorageFragmentation { get; set; }
}

public class DriveDistributionInfo
{
    public string DriveLetter { get; set; } = "";
    public double CapacityPercent { get; set; }
    public double UsagePercent { get; set; }
    public double CapacityGB { get; set; }
    public double FreeSpaceGB { get; set; }
    public string Role { get; set; } = "";
}

public class RiskAssessment
{
    public StorageRiskLevel OverallRiskLevel { get; set; }
    public List<StorageRisk> IdentifiedRisks { get; set; } = new();
    public int RiskScore { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
}

public class StorageRisk
{
    public string Type { get; set; } = "";
    public StorageRiskLevel Level { get; set; }
    public string Description { get; set; } = "";
    public string[] AffectedDrives { get; set; } = Array.Empty<string>();
    public string ImpactAssessment { get; set; } = "";
    public string Mitigation { get; set; } = "";
}

public class PerformanceIndicators
{
    public double StorageEfficiency { get; set; }
    public double CapacityUtilization { get; set; }
    public double SpaceDistributionIndex { get; set; }
    public string PerformanceRisk { get; set; } = "";
    public double OptimizationPotential { get; set; }
}

public enum StorageRiskLevel { Low, Medium, High, Critical }
```

### Continuous Drive Monitoring
```csharp
public class ContinuousDriveMonitor : IDisposable
{
    private readonly SystemDriveMetricsClient _client;
    private readonly Timer _timer;
    private readonly ConcurrentDictionary<string, List<(DateTime Time, SystemDriveMetrics Metrics)>> _driveHistory;
    private bool _disposed;

    public ContinuousDriveMonitor(int intervalMinutes = 10)
    {
        _client = new SystemDriveMetricsClient();
        _driveHistory = new ConcurrentDictionary<string, List<(DateTime, SystemDriveMetrics)>>();
        
        _timer = new Timer(CollectDriveMetrics, null, TimeSpan.Zero, TimeSpan.FromMinutes(intervalMinutes));
    }

    public event Action<SystemDriveMetrics[]>? DriveMetricsCollected;
    public event Action<DriveAlert>? DriveAlertRaised;

    private async void CollectDriveMetrics(object? state)
    {
        if (_disposed) return;

        try
        {
            var timestamp = DateTime.UtcNow;
            var drives = await Task.Run(() => _client.GetMetrics());
            
            // Update history for each drive
            foreach (var drive in drives)
            {
                _driveHistory.AddOrUpdate(
                    drive.Letter,
                    new List<(DateTime, SystemDriveMetrics)> { (timestamp, drive) },
                    (key, existing) =>
                    {
                        existing.Add((timestamp, drive));
                        
                        // Keep last 24 hours (144 entries at 10-minute intervals)
                        if (existing.Count > 144)
                        {
                            existing.RemoveAt(0);
                        }
                        
                        return existing;
                    });
            }
            
            // Raise events
            DriveMetricsCollected?.Invoke(drives);
            
            // Check for alerts
            var alerts = CheckForDriveAlerts(drives);
            foreach (var alert in alerts)
            {
                DriveAlertRaised?.Invoke(alert);
            }
        }
        catch (Exception ex)
        {
            // Log error but continue monitoring
            Console.WriteLine($"Error collecting drive metrics: {ex.Message}");
        }
    }

    public SystemDriveMetrics[] GetLatestMetrics()
    {
        return _client.GetMetrics();
    }

    public DriveTrendData GetDriveTrends(string driveLetter, TimeSpan? period = null)
    {
        var targetPeriod = period ?? TimeSpan.FromHours(6);
        var cutoff = DateTime.UtcNow - targetPeriod;
        
        if (!_driveHistory.TryGetValue(driveLetter, out var history))
        {
            return new DriveTrendData { Status = "No data available for drive" };
        }
        
        var relevantData = history
            .Where(h => h.Time >= cutoff)
            .OrderBy(h => h.Time)
            .ToArray();
        
        if (!relevantData.Any())
        {
            return new DriveTrendData { Status = "No data in specified period" };
        }
        
        var usageValues = relevantData.Select(d => d.Metrics.UsagePercentage).ToArray();
        var freeSpaceValues = relevantData.Select(d => d.Metrics.Free / (1024.0 * 1024.0 * 1024.0)).ToArray();
        
        return new DriveTrendData
        {
            DriveLetter = driveLetter,
            Period = targetPeriod,
            DataPoints = relevantData.Length,
            StartTime = relevantData.First().Time,
            EndTime = relevantData.Last().Time,
            
            UsageTrend = new TrendInfo
            {
                Current = usageValues.Last(),
                Average = usageValues.Average(),
                Minimum = usageValues.Min(),
                Maximum = usageValues.Max(),
                Trend = CalculateTrend(usageValues),
                Slope = CalculateSlope(usageValues)
            },
            
            FreeSpaceTrend = new TrendInfo
            {
                Current = freeSpaceValues.Last(),
                Average = freeSpaceValues.Average(),
                Minimum = freeSpaceValues.Min(),
                Maximum = freeSpaceValues.Max(),
                Trend = CalculateTrend(freeSpaceValues),
                Slope = CalculateSlope(freeSpaceValues)
            },
            
            Status = "Data available"
        };
    }

    private List<DriveAlert> CheckForDriveAlerts(SystemDriveMetrics[] drives)
    {
        var alerts = new List<DriveAlert>();
        
        foreach (var drive in drives)
        {
            var freeGB = drive.Free / (1024.0 * 1024.0 * 1024.0);
            
            // Critical space alerts
            if (drive.UsagePercentage > 95)
            {
                alerts.Add(new DriveAlert
                {
                    DriveLetter = drive.Letter,
                    AlertType = "Critical Space Usage",
                    Level = AlertLevel.Critical,
                    Message = $"Drive {drive.Letter} is {drive.UsagePercentage:F1}% full",
                    Timestamp = DateTime.UtcNow,
                    Metrics = drive
                });
            }
            else if (drive.UsagePercentage > 90)
            {
                alerts.Add(new DriveAlert
                {
                    DriveLetter = drive.Letter,
                    AlertType = "High Space Usage",
                    Level = AlertLevel.Warning,
                    Message = $"Drive {drive.Letter} is {drive.UsagePercentage:F1}% full",
                    Timestamp = DateTime.UtcNow,
                    Metrics = drive
                });
            }
            
            // Low free space alerts
            if (freeGB < 1)
            {
                alerts.Add(new DriveAlert
                {
                    DriveLetter = drive.Letter,
                    AlertType = "Critical Free Space",
                    Level = AlertLevel.Critical,
                    Message = $"Drive {drive.Letter} has only {freeGB:F1}GB free",
                    Timestamp = DateTime.UtcNow,
                    Metrics = drive
                });
            }
            else if (freeGB < 5)
            {
                alerts.Add(new DriveAlert
                {
                    DriveLetter = drive.Letter,
                    AlertType = "Low Free Space",
                    Level = AlertLevel.Warning,
                    Message = $"Drive {drive.Letter} has {freeGB:F1}GB free",
                    Timestamp = DateTime.UtcNow,
                    Metrics = drive
                });
            }
            
            // Trend-based alerts
            CheckTrendAlerts(drive, alerts);
        }
        
        return alerts;
    }

    private void CheckTrendAlerts(SystemDriveMetrics drive, List<DriveAlert> alerts)
    {
        if (!_driveHistory.TryGetValue(drive.Letter, out var history) || history.Count < 6)
            return;
        
        var recentHistory = history.TakeLast(6).ToArray(); // Last hour
        var usageValues = recentHistory.Select(h => h.Metrics.UsagePercentage).ToArray();
        
        // Check for rapid usage increase
        var usageIncrease = usageValues.Last() - usageValues.First();
        if (usageIncrease > 5) // More than 5% increase in last hour
        {
            alerts.Add(new DriveAlert
            {
                DriveLetter = drive.Letter,
                AlertType = "Rapid Usage Increase",
                Level = AlertLevel.Warning,
                Message = $"Drive {drive.Letter} usage increased by {usageIncrease:F1}% in last hour",
                Timestamp = DateTime.UtcNow,
                Metrics = drive
            });
        }
    }

    private string CalculateTrend(double[] values)
    {
        if (values.Length < 3) return "Stable";
        
        var slope = CalculateSlope(values);
        
        return slope switch
        {
            > 0.1 => "Rising",
            < -0.1 => "Falling",
            _ => "Stable"
        };
    }

    private double CalculateSlope(double[] values)
    {
        if (values.Length < 2) return 0;
        
        var n = values.Length;
        var sumX = n * (n - 1) / 2.0;
        var sumY = values.Sum();
        var sumXY = values.Select((y, i) => i * y).Sum();
        var sumX2 = Enumerable.Range(0, n).Sum(i => i * i);
        
        return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _timer?.Dispose();
    }
}

public class DriveTrendData
{
    public string DriveLetter { get; set; } = "";
    public TimeSpan Period { get; set; }
    public int DataPoints { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TrendInfo UsageTrend { get; set; } = new();
    public TrendInfo FreeSpaceTrend { get; set; } = new();
    public string Status { get; set; } = "";
}

public class TrendInfo
{
    public double Current { get; set; }
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public string Trend { get; set; } = "";
    public double Slope { get; set; }
}

public class DriveAlert
{
    public string DriveLetter { get; set; } = "";
    public string AlertType { get; set; } = "";
    public AlertLevel Level { get; set; }
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public SystemDriveMetrics Metrics { get; set; } = default!;
}

public enum AlertLevel { Info, Warning, Critical }
```

## Implementation Analysis

### Core Implementation
```csharp
public SystemDriveMetrics[] GetMetrics() => DriveInfo.GetDrives()
    .Where(drive => drive.IsReady)
    .Select(drive => new SystemDriveMetrics(drive.Name, drive.TotalSize, drive.TotalFreeSpace, drive.IsReady))
    .ToArray();
```

**Key Characteristics:**
- **Simple Implementation**: Leverages .NET's built-in `DriveInfo` class
- **Cross-Platform**: Works consistently across Windows, Linux, and macOS
- **Filtering**: Automatically filters for ready drives to avoid exceptions
- **Real-time**: Provides current drive state information

### Platform Behavior

#### Windows
- **Drive Letters**: Returns drives as "C:\", "D:\", etc.
- **Drive Types**: Includes fixed, removable, network, and CD-ROM drives
- **Ready State**: Accurately reflects drive accessibility
- **Performance**: Fast enumeration and property access

#### Unix/Linux
- **Mount Points**: Returns mount paths like "/", "/home", "/var"
- **File Systems**: Includes all mounted file systems
- **Ready State**: Indicates successful mount status
- **Permissions**: May require appropriate permissions for some mount points

#### macOS
- **Similar to Linux**: Uses Unix-style mount paths
- **System Drives**: Includes system and user drives
- **External Media**: Properly handles external drives and media

## Performance Considerations

### Collection Speed
- **Fast Enumeration**: `DriveInfo.GetDrives()` is optimized system call
- **Minimal Overhead**: Direct system API access
- **Caching**: No built-in caching - implement at higher level if needed
- **Frequency**: Suitable for frequent monitoring (every 5-15 minutes)

### Resource Usage
- **Low Memory**: Minimal memory footprint
- **No I/O Overhead**: Uses system metadata, not disk I/O
- **Thread Safe**: Safe for concurrent access
- **Scalability**: Performance independent of drive size or content

### Error Handling
- **Ready State Filter**: Eliminates most common errors
- **Exception Safety**: `IsReady` check prevents access exceptions
- **Robustness**: Handles drives that become unavailable during enumeration
- **Graceful Degradation**: Continues processing even if individual drives fail

## Best Practices

### Integration Guidelines
1. **Error Handling**: Always check `IsReady` before using drive data
2. **Filtering**: Consider additional filtering based on drive type if needed
3. **Caching**: Implement caching for high-frequency monitoring scenarios
4. **Alerting**: Combine with alerting systems for proactive monitoring
5. **Trend Analysis**: Collect historical data for capacity planning

### Monitoring Strategy
1. **Collection Frequency**: Monitor every 5-15 minutes for most scenarios
2. **Threshold Management**: Set both percentage and absolute space thresholds
3. **Multi-Drive Analysis**: Consider system-wide storage pressure
4. **Platform Awareness**: Account for platform-specific drive naming conventions

### Operational Considerations
1. **Drive Types**: Consider filtering by drive type for specific use cases
2. **Network Drives**: Be aware that network drives may have different performance characteristics
3. **Removable Media**: Handle removable drives that may not be present
4. **Permissions**: Ensure appropriate permissions for accessing drive information

## Related Components

- **[SystemDriveMetrics](SystemDriveMetrics.md)** - Drive metrics data model
- **[SystemResourceMonitorMetrics](../SystemResourceMonitorMetrics.md)** - Aggregate metrics container
- **[MemoryMetricsClient](../MemoryMetricsClient.md)** - Memory metrics collection client
- **[ISystemResourceMonitor](../ISystemResourceMonitor.md)** - Main monitoring interface
- **[System Resource Monitor Overview](../README.md)** - Complete documentation

## Security and Privacy

### Data Sensitivity
1. **System Information**: Exposes drive configuration and usage patterns
2. **No Personal Data**: Drive metrics contain no personal or sensitive information
3. **Network Safety**: Safe for transmission and logging
4. **Audit Compliance**: Consider logging requirements for compliance scenarios

### Access Control
1. **System Permissions**: Uses current process permissions
2. **Drive Access**: May require permissions for network or system drives
3. **Security Context**: Inherits security context from calling application
4. **Least Privilege**: Requires only read access to drive information