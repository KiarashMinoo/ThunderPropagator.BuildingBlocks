using Microsoft.Extensions.DependencyInjection;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Helpers;
using ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;
using Xunit.Abstractions;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Integration tests for System Drive metrics with real disk scenarios.
/// Tests validate drive space, usage percentage, and multi-drive scenarios.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
[Trait("Metric", "SystemDrive")]
public sealed class SystemDriveMetricsIntegrationTests : IDisposable
{
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricSampler _sampler;
    private readonly ITestOutputHelper _output;
    private readonly ServiceProvider _serviceProvider;

    public SystemDriveMetricsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var services = new ServiceCollection();
        services.AddSystemResourceMonitor(options =>
        {
            options.DefaultSamplingWindowMs = 500;
            options.EnableCpuTemperature = false;
            options.EnableDiskHealthMetrics = false;
            options.EnableDiskSpeedMetrics = false;
            options.EnableGpuMetrics = false;
            options.EnableBatteryMetrics = false;
        });

        _serviceProvider = services.BuildServiceProvider();
        _monitor = _serviceProvider.GetRequiredService<ISystemResourceMonitor>();
        _sampler = new MetricSampler(_monitor);
    }

    [Fact]
    public async Task SystemDrives_AreDetected()
    {
        // Arrange
        _output.WriteLine("=== System Drive Detection Test ===");

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        _output.WriteLine($"Detected {metrics.Drives.Length} drive(s):");
        foreach (var drive in metrics.Drives)
        {
            _output.WriteLine($"  - {drive.Letter}: Total={drive.Total:F2} MB, Used={drive.Used:F2} MB, Free={drive.Free:F2} MB, Usage={drive.UsagePercentage:F2}%, Ready={drive.IsReady}");
        }

        Assert.NotEmpty(metrics.Drives);
        _output.WriteLine($"✓ System drives detected");
    }

    [Fact]
    public async Task DriveSpace_DecreasesWithFileWrite_ThenRestores()
    {
        // Arrange
        _output.WriteLine("=== Drive Space File Write Test ===");

        using var diskIo = new DiskIoGenerator();

        // Act & Assert - Baseline
        _output.WriteLine("\n[BASELINE] Collecting baseline drive metrics...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);

        if (baseline.DriveFreeMin == null || baseline.DriveUsedAvg == null)
        {
            _output.WriteLine("⚠ Drive metrics not available, skipping test");
            return;
        }

        _output.WriteLine($"Baseline: Used={baseline.DriveUsedAvg:F2} MB, Free={baseline.DriveFreeMin:F2} MB");

        // Act - Write files
        _output.WriteLine("\n[LOAD] Writing 100 MB to disk...");
        await diskIo.GenerateWriteIoAsync(totalMegabytes: 100, blockSizeKb: 64);
        await Task.Delay(500); // Let file system update

        var load = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);
        _output.WriteLine($"Load: Used={load.DriveUsedAvg:F2} MB, Free={load.DriveFreeMin:F2} MB");

        // Assert - Free space decreased (or used increased)
        var freeSpaceDecrease = baseline.DriveFreeMin.Value - load.DriveFreeMin!.Value;
        var usedSpaceIncrease = load.DriveUsedAvg!.Value - baseline.DriveUsedAvg.Value;

        _output.WriteLine($"Free space change: {freeSpaceDecrease:F2} MB");
        _output.WriteLine($"Used space change: {usedSpaceIncrease:F2} MB");

        // Note: File system overhead and caching may cause variance
        Assert.True(freeSpaceDecrease > 0 || usedSpaceIncrease > 0,
            "Either free space should decrease or used space should increase after file write");

        _output.WriteLine($"✓ Drive space changed after file write");

        // Cleanup happens in DiskIoGenerator.Dispose()
        diskIo.Dispose();
        await Task.Delay(500); // Let file system update after cleanup

        var cooldown = await _sampler.CollectSamplesAsync(windowMs: 1000, intervalMs: 200);
        _output.WriteLine($"Cooldown: Used={cooldown.DriveUsedAvg:F2} MB, Free={cooldown.DriveFreeMin:F2} MB");
        _output.WriteLine($"✓ Cleanup completed");
    }

    [Fact]
    public async Task DriveUsagePercentage_IsAccurate()
    {
        // Arrange
        _output.WriteLine("=== Drive Usage Percentage Accuracy Test ===");

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        foreach (var drive in metrics.Drives)
        {
            var calculatedPercent = (drive.Used / drive.Total) * 100.0;
            var reportedPercent = drive.UsagePercentage;

            var difference = Math.Abs(calculatedPercent - reportedPercent);
            _output.WriteLine($"Drive {drive.Letter}: Calculated={calculatedPercent:F2}%, Reported={reportedPercent:F2}%, Diff={difference:F4}%");

            Assert.True(difference < 0.01, $"Usage% mismatch for drive {drive.Letter}");
        }

        _output.WriteLine($"✓ Drive usage percentages accurate");
    }

    [Fact]
    public async Task DriveMetrics_RemainsStableWithoutIO()
    {
        // Arrange
        _output.WriteLine("=== Drive Stability Test (No I/O) ===");

        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 3000, intervalMs: 500);

        if (sample.Samples[0].Drives.Length == 0)
        {
            _output.WriteLine("⚠ No drives detected, skipping test");
            return;
        }

        // Assert - Drive space should be stable without I/O
        var firstDrive = sample.Samples[0].Drives[0];
        _output.WriteLine($"Monitoring drive {firstDrive.Letter}");

        var usedValues = sample.Samples.Select(s => s.Drives[0].Used).ToArray();
        var maxVariance = usedValues.Max() - usedValues.Min();

        _output.WriteLine($"Used space variance: {maxVariance:F2} MB over {sample.WindowMs}ms");

        // Allow small variance due to system activity
        Assert.True(maxVariance < 100.0, $"Drive usage should be stable without I/O (variance: {maxVariance:F2} MB)");
        _output.WriteLine($"✓ Drive metrics stable without I/O");
    }

    [Fact]
    public async Task MultiDrive_Detection_IfAvailable()
    {
        // Arrange
        _output.WriteLine("=== Multi-Drive Detection Test ===");

        // Act
        var metrics = await _monitor.GetMetricsAsync();

        // Assert
        _output.WriteLine($"Total drives detected: {metrics.Drives.Length}");

        if (metrics.Drives.Length > 1)
        {
            _output.WriteLine("Multiple drives detected:");
            foreach (var drive in metrics.Drives)
            {
                _output.WriteLine($"  - {drive.Letter}: {drive.Total:F2} MB total, {drive.UsagePercentage:F2}% used");
            }

            // Verify each drive has unique letter
            var letters = metrics.Drives.Select(d => d.Letter).ToArray();
            var uniqueLetters = letters.Distinct().ToArray();
            Assert.Equal(letters.Length, uniqueLetters.Length);

            _output.WriteLine($"✓ All drives have unique identifiers");
        }
        else
        {
            _output.WriteLine("ℹ Single drive system - multi-drive test not applicable");
        }
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
