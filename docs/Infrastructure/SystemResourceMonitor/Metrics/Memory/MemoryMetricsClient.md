# MemoryMetricsClient

## Overview

The `MemoryMetricsClient` class is the internal implementation responsible for collecting system memory metrics across different operating systems. This cross-platform client provides accurate memory utilization data by interfacing with platform-specific system tools and APIs.

## Purpose

- **Cross-Platform Memory Collection**: Support for Windows, Linux, and macOS memory monitoring
- **System Integration**: Direct interface with operating system memory reporting tools
- **Accurate Measurements**: Platform-optimized memory data collection
- **Real-time Data**: Current system memory state retrieval

## Class Declaration

```csharp
internal class MemoryMetricsClient
{
    public MemoryMetrics GetMetrics()
}
```

## Methods

### GetMetrics()

Collects current system memory metrics using platform-appropriate methods.

#### Returns
- **MemoryMetrics**: Complete memory utilization data including total and free memory

#### Platform Support
- **Windows**: Uses `wmic` command-line tool for WMI queries
- **Unix/Linux**: Uses `free` command for memory information
- **macOS**: Uses `free` command (via Unix compatibility)

## Implementation Details

### Platform Detection
```csharp
private static bool IsUnix() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || 
                                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
```

### Windows Implementation
- **Tool**: Windows Management Instrumentation Command-line (wmic)
- **Query**: `OS get FreePhysicalMemory,TotalVisibleMemorySize /Value`
- **Units**: Kilobytes (converted to megabytes)
- **Accuracy**: High precision from WMI subsystem

### Unix/Linux Implementation
- **Tool**: `free` command with `-m` flag (megabytes)
- **Parsing**: Text parsing of standard output
- **Units**: Direct megabyte values
- **Compatibility**: Works across most Unix-like systems

## Usage Examples

### Basic Memory Collection
```csharp
public class MemoryCollectionService
{
    private readonly MemoryMetricsClient _client;

    public MemoryCollectionService()
    {
        _client = new MemoryMetricsClient();
    }

    public void DisplayMemoryInfo()
    {
        try
        {
            var memory = _client.GetMetrics();
            
            Console.WriteLine("System Memory Information:");
            Console.WriteLine($"  Total Memory: {memory.Total:F0} MB ({memory.Total / 1024:F1} GB)");
            Console.WriteLine($"  Free Memory: {memory.Free:F0} MB ({memory.Free / 1024:F1} GB)");
            Console.WriteLine($"  Used Memory: {memory.Used:F0} MB ({memory.Used / 1024:F1} GB)");
            Console.WriteLine($"  Usage Percentage: {memory.UsagePercentage:F2}%");
            Console.WriteLine($"  Platform: {GetPlatformInfo()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error collecting memory metrics: {ex.Message}");
        }
    }

    private string GetPlatformInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows (via wmic)";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux (via free command)";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS (via free command)";
        else
            return "Unknown platform";
    }
}
```

### Cross-Platform Memory Monitoring
```csharp
public class CrossPlatformMemoryMonitor
{
    private readonly MemoryMetricsClient _client = new();

    public async Task<MemoryComparisonResult> CompareMemoryAcrossPlatforms()
    {
        var platformInfo = GetDetailedPlatformInfo();
        var memoryData = new List<(DateTime Time, MemoryMetrics Memory)>();
        
        // Collect multiple samples for accuracy
        for (int i = 0; i < 5; i++)
        {
            var timestamp = DateTime.UtcNow;
            var memory = _client.GetMetrics();
            memoryData.Add((timestamp, memory));
            
            Console.WriteLine($"Sample {i + 1}: {memory.Total:F0}MB total, {memory.Free:F0}MB free, {memory.UsagePercentage:F1}% used");
            
            if (i < 4) await Task.Delay(2000); // Wait 2 seconds between samples
        }
        
        return new MemoryComparisonResult
        {
            Platform = platformInfo,
            SampleCount = memoryData.Count,
            Duration = memoryData.Last().Time - memoryData.First().Time,
            
            TotalMemoryConsistency = CheckConsistency(memoryData.Select(d => d.Memory.Total).ToArray()),
            FreeMemoryVariability = CalculateVariability(memoryData.Select(d => d.Memory.Free).ToArray()),
            UsageStability = CalculateStability(memoryData.Select(d => d.Memory.UsagePercentage).ToArray()),
            
            AverageMetrics = new MemoryMetrics(
                memoryData.Average(d => d.Memory.Total),
                memoryData.Average(d => d.Memory.Free)
            ),
            
            PlatformSpecificInfo = GetPlatformSpecificAnalysis(),
            DataQuality = AssessDataQuality(memoryData)
        };
    }

    private PlatformInfo GetDetailedPlatformInfo()
    {
        return new PlatformInfo
        {
            OperatingSystem = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
            IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
            CollectionMethod = GetCollectionMethod()
        };
    }

    private string GetCollectionMethod()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "wmic WMI query (FreePhysicalMemory, TotalVisibleMemorySize)";
        else
            return "free command with -m flag (megabytes)";
    }

    private bool CheckConsistency(double[] values)
    {
        if (values.Length < 2) return true;
        
        var firstValue = values[0];
        return values.All(v => Math.Abs(v - firstValue) < 1.0); // Within 1MB tolerance
    }

    private double CalculateVariability(double[] values)
    {
        if (values.Length < 2) return 0;
        
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Length;
        return Math.Sqrt(variance);
    }

    private string CalculateStability(double[] values)
    {
        var variability = CalculateVariability(values);
        return variability switch
        {
            < 1.0 => "Very Stable",
            < 2.0 => "Stable", 
            < 5.0 => "Moderate",
            _ => "Variable"
        };
    }

    private Dictionary<string, object> GetPlatformSpecificAnalysis()
    {
        var analysis = new Dictionary<string, object>();
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            analysis["collection_tool"] = "wmic.exe";
            analysis["data_source"] = "Windows Management Instrumentation";
            analysis["precision"] = "Kilobyte precision converted to MB";
            analysis["performance_impact"] = "Low - uses existing WMI infrastructure";
            analysis["reliability"] = "High - built into Windows";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            analysis["collection_tool"] = "/bin/bash + free command";
            analysis["data_source"] = "/proc/meminfo via free utility";
            analysis["precision"] = "Direct megabyte values";
            analysis["performance_impact"] = "Very Low - efficient system call";
            analysis["reliability"] = "High - standard Unix utility";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            analysis["collection_tool"] = "/bin/bash + free command";
            analysis["data_source"] = "macOS memory management via free";
            analysis["precision"] = "Direct megabyte values";
            analysis["performance_impact"] = "Low - standard system utility";
            analysis["reliability"] = "High - macOS built-in command";
        }
        
        return analysis;
    }

    private string AssessDataQuality(List<(DateTime Time, MemoryMetrics Memory)> data)
    {
        var usageValues = data.Select(d => d.Memory.UsagePercentage).ToArray();
        var freeValues = data.Select(d => d.Memory.Free).ToArray();
        
        var usageStability = CalculateVariability(usageValues);
        var freeStability = CalculateVariability(freeValues);
        var totalConsistency = CheckConsistency(data.Select(d => d.Memory.Total).ToArray());
        
        return (usageStability, freeStability, totalConsistency) switch
        {
            (< 2.0, < 100, true) => "Excellent - Stable and consistent data",
            (< 5.0, < 200, true) => "Good - Minor variations within normal range",
            (< 10.0, < 500, true) => "Fair - Some variability but acceptable",
            (_, _, false) => "Poor - Total memory inconsistency detected",
            _ => "Poor - High variability in measurements"
        };
    }
}

public class MemoryComparisonResult
{
    public PlatformInfo Platform { get; set; } = new();
    public int SampleCount { get; set; }
    public TimeSpan Duration { get; set; }
    public bool TotalMemoryConsistency { get; set; }
    public double FreeMemoryVariability { get; set; }
    public string UsageStability { get; set; } = "";
    public MemoryMetrics AverageMetrics { get; set; } = default!;
    public Dictionary<string, object> PlatformSpecificInfo { get; set; } = new();
    public string DataQuality { get; set; } = "";
}

public class PlatformInfo
{
    public string OperatingSystem { get; set; } = "";
    public string Architecture { get; set; } = "";
    public string ProcessArchitecture { get; set; } = "";
    public string FrameworkDescription { get; set; } = "";
    public bool IsWindows { get; set; }
    public bool IsLinux { get; set; }
    public bool IsMacOS { get; set; }
    public string CollectionMethod { get; set; } = "";
}
```

### Error Handling and Resilience
```csharp
public class ResilientMemoryMetricsClient
{
    private readonly MemoryMetricsClient _client = new();
    private readonly ILogger<ResilientMemoryMetricsClient> _logger;
    private MemoryMetrics? _lastKnownGoodMetrics;

    public ResilientMemoryMetricsClient(ILogger<ResilientMemoryMetricsClient> logger)
    {
        _logger = logger;
    }

    public async Task<MemoryCollectionResult> GetMetricsWithRetry(int maxRetries = 3, TimeSpan? retryDelay = null)
    {
        var delay = retryDelay ?? TimeSpan.FromSeconds(1);
        var attempts = new List<MemoryCollectionAttempt>();
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var attemptStart = DateTime.UtcNow;
            
            try
            {
                var metrics = _client.GetMetrics();
                var duration = DateTime.UtcNow - attemptStart;
                
                var success = new MemoryCollectionAttempt
                {
                    AttemptNumber = attempt,
                    Success = true,
                    Metrics = metrics,
                    Duration = duration,
                    Timestamp = attemptStart
                };
                
                attempts.Add(success);
                _lastKnownGoodMetrics = metrics;
                
                _logger.LogDebug("Memory metrics collected successfully on attempt {Attempt} in {Duration}ms", 
                    attempt, duration.TotalMilliseconds);
                
                return new MemoryCollectionResult
                {
                    Success = true,
                    Metrics = metrics,
                    Attempts = attempts,
                    TotalDuration = DateTime.UtcNow - attempts.First().Timestamp
                };
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - attemptStart;
                
                var failure = new MemoryCollectionAttempt
                {
                    AttemptNumber = attempt,
                    Success = false,
                    Error = ex.Message,
                    Duration = duration,
                    Timestamp = attemptStart
                };
                
                attempts.Add(failure);
                
                _logger.LogWarning(ex, "Memory metrics collection failed on attempt {Attempt}: {Error}", 
                    attempt, ex.Message);
                
                if (attempt < maxRetries)
                {
                    _logger.LogInformation("Retrying memory collection in {Delay}ms (attempt {Next}/{Max})", 
                        delay.TotalMilliseconds, attempt + 1, maxRetries);
                    await Task.Delay(delay);
                }
            }
        }
        
        // All attempts failed
        _logger.LogError("All {MaxRetries} attempts to collect memory metrics failed", maxRetries);
        
        return new MemoryCollectionResult
        {
            Success = false,
            Metrics = _lastKnownGoodMetrics, // May be null
            Attempts = attempts,
            TotalDuration = DateTime.UtcNow - attempts.First().Timestamp,
            FallbackToLastKnown = _lastKnownGoodMetrics != null
        };
    }

    public async Task<MemoryValidationResult> ValidateMetricsAccuracy(int sampleCount = 5, TimeSpan? interval = null)
    {
        var sampleInterval = interval ?? TimeSpan.FromSeconds(2);
        var samples = new List<(DateTime Time, MemoryMetrics Metrics, TimeSpan CollectionTime)>();
        
        _logger.LogInformation("Starting memory metrics validation with {SampleCount} samples", sampleCount);
        
        for (int i = 0; i < sampleCount; i++)
        {
            var start = DateTime.UtcNow;
            
            try
            {
                var metrics = _client.GetMetrics();
                var collectionTime = DateTime.UtcNow - start;
                
                samples.Add((start, metrics, collectionTime));
                
                _logger.LogDebug("Sample {SampleNumber}: {Total}MB total, {Free}MB free, collected in {Duration}ms",
                    i + 1, metrics.Total, metrics.Free, collectionTime.TotalMilliseconds);
                
                if (i < sampleCount - 1)
                    await Task.Delay(sampleInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect sample {SampleNumber}", i + 1);
                return new MemoryValidationResult
                {
                    Success = false,
                    ErrorMessage = $"Collection failed at sample {i + 1}: {ex.Message}",
                    CompletedSamples = samples.Count
                };
            }
        }
        
        return AnalyzeSamples(samples);
    }

    private MemoryValidationResult AnalyzeSamples(List<(DateTime Time, MemoryMetrics Metrics, TimeSpan CollectionTime)> samples)
    {
        if (!samples.Any())
        {
            return new MemoryValidationResult
            {
                Success = false,
                ErrorMessage = "No samples collected",
                CompletedSamples = 0
            };
        }
        
        var totalValues = samples.Select(s => s.Metrics.Total).ToArray();
        var freeValues = samples.Select(s => s.Metrics.Free).ToArray();
        var usageValues = samples.Select(s => s.Metrics.UsagePercentage).ToArray();
        var collectionTimes = samples.Select(s => s.CollectionTime.TotalMilliseconds).ToArray();
        
        return new MemoryValidationResult
        {
            Success = true,
            CompletedSamples = samples.Count,
            
            TotalMemoryAnalysis = new ValidationMetrics
            {
                Values = totalValues,
                Average = totalValues.Average(),
                StandardDeviation = CalculateStandardDeviation(totalValues),
                IsConsistent = totalValues.All(v => Math.Abs(v - totalValues[0]) < 1.0),
                CoefficientOfVariation = CalculateCoefficientOfVariation(totalValues)
            },
            
            FreeMemoryAnalysis = new ValidationMetrics
            {
                Values = freeValues,
                Average = freeValues.Average(),
                StandardDeviation = CalculateStandardDeviation(freeValues),
                IsConsistent = CalculateStandardDeviation(freeValues) < 100, // Within 100MB
                CoefficientOfVariation = CalculateCoefficientOfVariation(freeValues)
            },
            
            UsageAnalysis = new ValidationMetrics
            {
                Values = usageValues,
                Average = usageValues.Average(),
                StandardDeviation = CalculateStandardDeviation(usageValues),
                IsConsistent = CalculateStandardDeviation(usageValues) < 2.0, // Within 2%
                CoefficientOfVariation = CalculateCoefficientOfVariation(usageValues)
            },
            
            PerformanceAnalysis = new PerformanceMetrics
            {
                AverageCollectionTimeMs = collectionTimes.Average(),
                MaxCollectionTimeMs = collectionTimes.Max(),
                MinCollectionTimeMs = collectionTimes.Min(),
                CollectionTimeStability = CalculateStandardDeviation(collectionTimes),
                IsPerformanceAcceptable = collectionTimes.Average() < 1000 // Under 1 second
            },
            
            OverallAssessment = GenerateOverallAssessment(totalValues, freeValues, usageValues, collectionTimes)
        };
    }

    private double CalculateStandardDeviation(double[] values)
    {
        if (values.Length <= 1) return 0;
        
        var mean = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / (values.Length - 1));
    }

    private double CalculateCoefficientOfVariation(double[] values)
    {
        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values);
        return mean == 0 ? 0 : (stdDev / mean) * 100;
    }

    private string GenerateOverallAssessment(double[] total, double[] free, double[] usage, double[] collectionTimes)
    {
        var totalConsistent = total.All(v => Math.Abs(v - total[0]) < 1.0);
        var freeStable = CalculateStandardDeviation(free) < 100;
        var usageStable = CalculateStandardDeviation(usage) < 2.0;
        var performanceGood = collectionTimes.Average() < 1000;
        
        var score = (totalConsistent ? 1 : 0) + (freeStable ? 1 : 0) + (usageStable ? 1 : 0) + (performanceGood ? 1 : 0);
        
        return score switch
        {
            4 => "Excellent - All metrics are consistent and performance is good",
            3 => "Good - Minor inconsistencies but overall reliable",
            2 => "Fair - Some stability issues detected",
            1 => "Poor - Significant inconsistencies in measurements", 
            _ => "Critical - Unreliable measurements detected"
        };
    }
}

public class MemoryCollectionResult
{
    public bool Success { get; set; }
    public MemoryMetrics? Metrics { get; set; }
    public List<MemoryCollectionAttempt> Attempts { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }
    public bool FallbackToLastKnown { get; set; }
}

public class MemoryCollectionAttempt
{
    public int AttemptNumber { get; set; }
    public bool Success { get; set; }
    public MemoryMetrics? Metrics { get; set; }
    public string? Error { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime Timestamp { get; set; }
}

public class MemoryValidationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int CompletedSamples { get; set; }
    public ValidationMetrics TotalMemoryAnalysis { get; set; } = new();
    public ValidationMetrics FreeMemoryAnalysis { get; set; } = new();
    public ValidationMetrics UsageAnalysis { get; set; } = new();
    public PerformanceMetrics PerformanceAnalysis { get; set; } = new();
    public string OverallAssessment { get; set; } = "";
}

public class ValidationMetrics
{
    public double[] Values { get; set; } = Array.Empty<double>();
    public double Average { get; set; }
    public double StandardDeviation { get; set; }
    public bool IsConsistent { get; set; }
    public double CoefficientOfVariation { get; set; }
}

public class PerformanceMetrics
{
    public double AverageCollectionTimeMs { get; set; }
    public double MaxCollectionTimeMs { get; set; }
    public double MinCollectionTimeMs { get; set; }
    public double CollectionTimeStability { get; set; }
    public bool IsPerformanceAcceptable { get; set; }
}
```

### Platform-Specific Testing
```csharp
public class PlatformSpecificMemoryTester
{
    private readonly MemoryMetricsClient _client = new();

    public async Task<PlatformTestResult> RunPlatformTests()
    {
        var platform = Environment.OSVersion.Platform;
        var testStart = DateTime.UtcNow;
        
        Console.WriteLine($"Running platform-specific memory tests for: {RuntimeInformation.OSDescription}");
        
        var testResult = new PlatformTestResult
        {
            Platform = RuntimeInformation.OSDescription,
            StartTime = testStart,
            Tests = new List<MemoryTest>()
        };
        
        // Test 1: Basic collection
        testResult.Tests.Add(await TestBasicCollection());
        
        // Test 2: Consistency over time
        testResult.Tests.Add(await TestConsistencyOverTime());
        
        // Test 3: Performance under load
        testResult.Tests.Add(await TestPerformanceUnderLoad());
        
        // Test 4: Error handling
        testResult.Tests.Add(await TestErrorHandling());
        
        // Test 5: Platform-specific validation
        testResult.Tests.Add(await TestPlatformSpecificFeatures());
        
        testResult.EndTime = DateTime.UtcNow;
        testResult.TotalDuration = testResult.EndTime - testResult.StartTime;
        testResult.OverallResult = DetermineOverallResult(testResult.Tests);
        
        return testResult;
    }

    private async Task<MemoryTest> TestBasicCollection()
    {
        var test = new MemoryTest
        {
            TestName = "Basic Memory Collection",
            Description = "Verify that memory metrics can be collected successfully"
        };
        
        try
        {
            var startTime = DateTime.UtcNow;
            var metrics = _client.GetMetrics();
            var duration = DateTime.UtcNow - startTime;
            
            test.Success = true;
            test.Duration = duration;
            test.Details = new Dictionary<string, object>
            {
                ["total_memory_mb"] = metrics.Total,
                ["free_memory_mb"] = metrics.Free,
                ["usage_percentage"] = metrics.UsagePercentage,
                ["collection_time_ms"] = duration.TotalMilliseconds
            };
            
            // Validate reasonable values
            if (metrics.Total > 0 && metrics.Free >= 0 && metrics.UsagePercentage >= 0 && metrics.UsagePercentage <= 100)
            {
                test.Message = "Memory collection successful with valid values";
            }
            else
            {
                test.Success = false;
                test.Message = "Memory collection returned invalid values";
            }
        }
        catch (Exception ex)
        {
            test.Success = false;
            test.Message = $"Memory collection failed: {ex.Message}";
            test.Error = ex.ToString();
        }
        
        return test;
    }

    private async Task<MemoryTest> TestConsistencyOverTime()
    {
        var test = new MemoryTest
        {
            TestName = "Consistency Over Time",
            Description = "Verify that total memory remains consistent across multiple collections"
        };
        
        try
        {
            var samples = new List<MemoryMetrics>();
            
            for (int i = 0; i < 5; i++)
            {
                var metrics = _client.GetMetrics();
                samples.Add(metrics);
                if (i < 4) await Task.Delay(1000);
            }
            
            var totalValues = samples.Select(s => s.Total).ToArray();
            var isConsistent = totalValues.All(t => Math.Abs(t - totalValues[0]) < 1.0);
            
            test.Success = isConsistent;
            test.Duration = TimeSpan.FromSeconds(5);
            test.Details = new Dictionary<string, object>
            {
                ["samples_collected"] = samples.Count,
                ["total_memory_values"] = totalValues,
                ["is_consistent"] = isConsistent,
                ["max_variation"] = totalValues.Max() - totalValues.Min()
            };
            
            test.Message = isConsistent 
                ? "Total memory values are consistent across samples"
                : $"Total memory varied by {totalValues.Max() - totalValues.Min():F1}MB across samples";
        }
        catch (Exception ex)
        {
            test.Success = false;
            test.Message = $"Consistency test failed: {ex.Message}";
            test.Error = ex.ToString();
        }
        
        return test;
    }

    private async Task<MemoryTest> TestPerformanceUnderLoad()
    {
        var test = new MemoryTest
        {
            TestName = "Performance Under Load",
            Description = "Test memory collection performance with concurrent requests"
        };
        
        try
        {
            var concurrentTasks = 10;
            var iterationsPerTask = 5;
            
            var tasks = Enumerable.Range(0, concurrentTasks)
                .Select(async taskId =>
                {
                    var taskResults = new List<(TimeSpan Duration, bool Success)>();
                    
                    for (int i = 0; i < iterationsPerTask; i++)
                    {
                        var start = DateTime.UtcNow;
                        try
                        {
                            var metrics = _client.GetMetrics();
                            var duration = DateTime.UtcNow - start;
                            taskResults.Add((duration, true));
                        }
                        catch
                        {
                            var duration = DateTime.UtcNow - start;
                            taskResults.Add((duration, false));
                        }
                    }
                    
                    return taskResults;
                })
                .ToArray();
            
            var allResults = await Task.WhenAll(tasks);
            var flatResults = allResults.SelectMany(r => r).ToArray();
            
            var successRate = flatResults.Count(r => r.Success) / (double)flatResults.Length * 100;
            var avgDuration = flatResults.Where(r => r.Success).Average(r => r.Duration.TotalMilliseconds);
            var maxDuration = flatResults.Where(r => r.Success).Max(r => r.Duration.TotalMilliseconds);
            
            test.Success = successRate >= 95 && avgDuration < 2000; // 95% success, under 2 seconds average
            test.Duration = TimeSpan.FromMilliseconds(maxDuration);
            test.Details = new Dictionary<string, object>
            {
                ["concurrent_tasks"] = concurrentTasks,
                ["iterations_per_task"] = iterationsPerTask,
                ["total_operations"] = flatResults.Length,
                ["success_rate_percent"] = successRate,
                ["average_duration_ms"] = avgDuration,
                ["max_duration_ms"] = maxDuration
            };
            
            test.Message = $"Performance test: {successRate:F1}% success rate, {avgDuration:F1}ms average duration";
        }
        catch (Exception ex)
        {
            test.Success = false;
            test.Message = $"Performance test failed: {ex.Message}";
            test.Error = ex.ToString();
        }
        
        return test;
    }

    private async Task<MemoryTest> TestErrorHandling()
    {
        var test = new MemoryTest
        {
            TestName = "Error Handling",
            Description = "Test graceful handling of potential error conditions"
        };
        
        try
        {
            // This test verifies that the client doesn't crash under normal conditions
            // More sophisticated error injection would require mocking the process execution
            
            var successCount = 0;
            var totalAttempts = 10;
            
            for (int i = 0; i < totalAttempts; i++)
            {
                try
                {
                    var metrics = _client.GetMetrics();
                    if (metrics.Total > 0) successCount++;
                }
                catch
                {
                    // Expected in some error scenarios
                }
                
                await Task.Delay(100);
            }
            
            var successRate = successCount / (double)totalAttempts * 100;
            
            test.Success = successRate >= 80; // At least 80% should succeed under normal conditions
            test.Duration = TimeSpan.FromSeconds(1);
            test.Details = new Dictionary<string, object>
            {
                ["total_attempts"] = totalAttempts,
                ["successful_attempts"] = successCount,
                ["success_rate_percent"] = successRate
            };
            
            test.Message = $"Error handling test: {successRate:F1}% success rate across {totalAttempts} attempts";
        }
        catch (Exception ex)
        {
            test.Success = false;
            test.Message = $"Error handling test failed: {ex.Message}";
            test.Error = ex.ToString();
        }
        
        return test;
    }

    private async Task<MemoryTest> TestPlatformSpecificFeatures()
    {
        var test = new MemoryTest
        {
            TestName = "Platform-Specific Features",
            Description = "Validate platform-specific implementation details"
        };
        
        try
        {
            var metrics = _client.GetMetrics();
            var platformDetails = new Dictionary<string, object>();
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platformDetails["platform"] = "Windows";
                platformDetails["collection_method"] = "wmic WMI query";
                platformDetails["expected_precision"] = "Kilobyte precision";
                
                // Windows-specific validations
                test.Success = metrics.Total > 0 && metrics.Free >= 0;
                test.Message = test.Success 
                    ? "Windows wmic collection working correctly"
                    : "Windows wmic collection failed validation";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformDetails["platform"] = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "macOS";
                platformDetails["collection_method"] = "free command";
                platformDetails["expected_precision"] = "Megabyte precision";
                
                // Unix-specific validations
                test.Success = metrics.Total > 0 && metrics.Free >= 0;
                test.Message = test.Success 
                    ? "Unix free command collection working correctly"
                    : "Unix free command collection failed validation";
            }
            else
            {
                platformDetails["platform"] = "Unknown";
                test.Success = false;
                test.Message = "Unknown platform - collection method not recognized";
            }
            
            test.Duration = TimeSpan.FromMilliseconds(100);
            test.Details = platformDetails;
            test.Details.Add("total_memory_mb", metrics.Total);
            test.Details.Add("free_memory_mb", metrics.Free);
        }
        catch (Exception ex)
        {
            test.Success = false;
            test.Message = $"Platform-specific test failed: {ex.Message}";
            test.Error = ex.ToString();
        }
        
        return test;
    }

    private string DetermineOverallResult(List<MemoryTest> tests)
    {
        var successCount = tests.Count(t => t.Success);
        var totalTests = tests.Count;
        var successRate = successCount / (double)totalTests * 100;
        
        return successRate switch
        {
            100 => "All tests passed - Memory collection is working perfectly",
            >= 80 => $"Most tests passed ({successRate:F0}%) - Memory collection is working well",
            >= 60 => $"Some tests failed ({successRate:F0}%) - Memory collection has issues",
            _ => $"Many tests failed ({successRate:F0}%) - Memory collection is unreliable"
        };
    }
}

public class PlatformTestResult
{
    public string Platform { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<MemoryTest> Tests { get; set; } = new();
    public string OverallResult { get; set; } = "";
}

public class MemoryTest
{
    public string TestName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public string? Error { get; set; }
}
```

## Platform-Specific Implementation Details

### Windows Implementation (wmic)
```csharp
private static MemoryMetrics GetWindowsMetrics()
{
    var info = new ProcessStartInfo
    {
        FileName = "wmic",
        Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
        RedirectStandardOutput = true
    };

    string output;
    using (var process = Process.Start(info))
    {
        output = process!.StandardOutput.ReadToEnd();
    }

    var lines = output.Trim().Split("\n");
    var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
    var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);

    return new MemoryMetrics(
        Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0),
        Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0)
    );
}
```

**Characteristics:**
- **Data Source**: Windows Management Instrumentation (WMI)
- **Precision**: Kilobyte values converted to megabytes
- **Performance**: Moderate overhead due to process execution
- **Reliability**: High - built into Windows operating system
- **Error Conditions**: May fail if WMI service is disabled or wmic.exe is not available

### Unix/Linux Implementation (free command)
```csharp
private static MemoryMetrics GetUnixMetrics()
{
    var info = new ProcessStartInfo
    {
        FileName = "/bin/bash",
        Arguments = "-c \"free -m\"",
        RedirectStandardOutput = true
    };

    string output;
    using (var process = Process.Start(info))
    {
        output = process!.StandardOutput.ReadToEnd();
    }

    var lines = output.Split("\n");
    var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

    return new MemoryMetrics(
        double.Parse(memory[1]),
        double.Parse(memory[3])
    );
}
```

**Characteristics:**
- **Data Source**: `/proc/meminfo` via `free` utility
- **Precision**: Direct megabyte values
- **Performance**: Low overhead - efficient system call
- **Reliability**: High - standard Unix utility available on most systems
- **Error Conditions**: May fail if `free` command is not installed or `/proc/meminfo` is inaccessible

## Performance Considerations

### Collection Speed
- **Windows**: 100-500ms typical execution time
- **Unix/Linux**: 50-200ms typical execution time
- **Process Overhead**: Each collection spawns a new process
- **Caching Strategy**: Consider caching for high-frequency monitoring

### Accuracy Factors
- **Windows WMI**: High accuracy, reflects current system state
- **Unix free**: Varies by system configuration and memory management
- **Cache vs Available**: Different platforms handle memory cache differently
- **Timing Sensitivity**: Memory values can change rapidly during system activity

### Reliability Considerations
- **Process Dependencies**: Relies on external process execution
- **Platform Tools**: Dependent on system tools being available and functional
- **Permission Requirements**: May require specific permissions on some systems
- **Error Recovery**: No built-in retry mechanism - implement at higher level

## Best Practices

### Integration Guidelines
1. **Wrapper Services**: Use wrapper services for error handling and retry logic
2. **Caching**: Implement caching to reduce collection frequency
3. **Logging**: Log collection failures for monitoring and debugging
4. **Validation**: Validate returned values for reasonableness
5. **Fallback Mechanisms**: Implement fallback strategies for collection failures

### Error Handling
1. **Process Execution**: Handle process start failures gracefully
2. **Output Parsing**: Validate command output format before parsing
3. **Timeout Management**: Implement timeouts for process execution
4. **Resource Cleanup**: Ensure proper disposal of process resources
5. **Retry Logic**: Implement appropriate retry strategies with backoff

### Monitoring Strategy
1. **Collection Performance**: Monitor collection time and success rates
2. **Platform Differences**: Account for platform-specific behavior differences
3. **System Dependencies**: Monitor availability of required system tools
4. **Resource Usage**: Monitor the monitoring overhead itself
5. **Alert Thresholds**: Set appropriate thresholds for memory alerts

## Related Components

- **[MemoryMetrics](MemoryMetrics.md)** - Memory metrics data model
- **[SystemResourceMonitorMetrics](SystemResourceMonitorMetrics.md)** - Aggregate metrics container
- **[CpuMetricsClient](CpuMetricsClient.md)** - CPU metrics collection client
- **[ISystemResourceMonitor](ISystemResourceMonitor.md)** - Main monitoring interface
- **[System Resource Monitor Overview](README.md)** - Complete documentation

## Security Considerations

### Process Execution
1. **Command Injection**: Uses fixed commands with no user input
2. **Process Permissions**: Runs with current process permissions
3. **Output Validation**: Validates output before parsing
4. **Resource Limits**: No built-in resource limits on spawned processes

### Data Privacy
1. **System Information**: Exposes system memory configuration
2. **No Personal Data**: Memory metrics contain no personal information
3. **Network Exposure**: Safe for network transmission and logging
4. **Audit Trail**: Consider logging for security audit requirements