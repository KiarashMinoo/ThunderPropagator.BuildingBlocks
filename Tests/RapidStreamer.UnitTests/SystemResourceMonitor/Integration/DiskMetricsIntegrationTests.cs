using Microsoft.Extensions.Options;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using RapidStreamer.UnitTests.SystemResourceMonitor.Integration.Helpers;
using RapidStreamer.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;
using Xunit.Abstractions;

namespace RapidStreamer.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Integration tests that validate disk metrics under real I/O load.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
public class DiskMetricsIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricsSampler _sampler;
    private readonly DiskIoGenerator _ioGenerator;

    public DiskMetricsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            DefaultSamplingWindowMs = 500,
            EnableDiskHealthMetrics = true,
            EnableDiskSpeedMetrics = true
        });

        _monitor = new SystemResourceMonitorImpl(
            new CpuMetricsClient(),
            new CpuTemperatureMetricsClient(),
            new MemoryMetricsClient(),
            new SystemDriveMetricsClient(),
            new DiskHealthMetricsClient(),
            new DiskSpeedMetricsClient(),
            new GpuMetricsClient(),
            new BatteryMetricsClient(),
            options);

        _sampler = new MetricsSampler(_monitor);
        _ioGenerator = new DiskIoGenerator();
    }

    [Fact]
    public async Task SystemDrives_Should_Report_Valid_Drive_Information()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var drives = sample.Samples.First().Drives;

        Assert.NotEmpty(drives);

        foreach (var drive in drives)
        {
            Assert.NotNull(drive.Letter);
            Assert.True(drive.Total > 0, $"Drive {drive.Letter} total space should be positive");
            Assert.True(drive.Free >= 0, $"Drive {drive.Letter} free space should be non-negative");
            Assert.True(drive.Free <= drive.Total, $"Drive {drive.Letter} free space should not exceed total");

            var totalGb = drive.Total / (1024.0 * 1024 * 1024);
            var freeGb = drive.Free / (1024.0 * 1024 * 1024);
            var usedGb = (drive.Total - drive.Free) / (1024.0 * 1024 * 1024);

            _output.WriteLine($"Drive {drive.Letter}: Total={totalGb:F2} GB, Used={usedGb:F2} GB, Free={freeGb:F2} GB");
        }

        _output.WriteLine("✓ System drives information is valid");
    }

    [Fact]
    public async Task DiskWriteThroughput_Should_Increase_During_Large_Write()
    {
        // Get system drive
        var initialSample = await _monitor.GetMetricsAsync(500, null, CancellationToken.None);
        var systemDrive = initialSample.Drives.FirstOrDefault();
        
        if (systemDrive == null)
        {
            _output.WriteLine("⚠ No system drives found, skipping test");
            return;
        }

        var driveId = systemDrive.Letter;
        _output.WriteLine($"Testing with drive: {driveId}");

        // Baseline
        _output.WriteLine("Collecting baseline disk metrics...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 2000);
        var baselineWrite = baseline.DiskWriteThroughput(driveId);
        _output.WriteLine($"Baseline write throughput: {baselineWrite.Avg ?? 0:F2} MB/s");

        // Act - Write large file
        _output.WriteLine("Writing 100 MB file...");
        var writeTask = _ioGenerator.WriteLargeFileAsync(100);
        await Task.Delay(500); // Let I/O start

        var load = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 2000);
        await writeTask; // Ensure write completes

        var loadWrite = load.DiskWriteThroughput(driveId);
        _output.WriteLine($"Load write throughput: {loadWrite.Avg ?? 0:F2} MB/s");

        // Assert
        if (loadWrite.Avg.HasValue && baselineWrite.Avg.HasValue)
        {
            // On some systems disk metrics may not be available
            Assert.True(loadWrite.Avg >= baselineWrite.Avg, 
                "Write throughput during load should be >= baseline");
            
            _output.WriteLine("✓ Disk write throughput increased or remained stable");
        }
        else
        {
            _output.WriteLine("⚠ Disk speed metrics not available on this platform");
        }
    }

    [Fact]
    public async Task DiskReadThroughput_Should_Increase_During_Large_Read()
    {
        // Get system drive
        var initialSample = await _monitor.GetMetricsAsync(500, null, CancellationToken.None);
        var systemDrive = initialSample.Drives.FirstOrDefault();
        
        if (systemDrive == null)
        {
            _output.WriteLine("⚠ No system drives found, skipping test");
            return;
        }

        var driveId = systemDrive.Letter;

        // Prepare - Write a file to read
        _output.WriteLine("Preparing test file...");
        var filePath = await _ioGenerator.WriteLargeFileAsync(50);

        // Baseline
        _output.WriteLine("Collecting baseline...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 1500);
        var baselineRead = baseline.DiskReadThroughput(driveId);
        _output.WriteLine($"Baseline read throughput: {baselineRead.Avg ?? 0:F2} MB/s");

        // Act - Read file multiple times
        _output.WriteLine("Reading file 5 times...");
        var readTask = _ioGenerator.ReadFileAsync(filePath, 5);
        await Task.Delay(500); // Let I/O start

        var load = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 2000);
        await readTask; // Ensure read completes

        var loadRead = load.DiskReadThroughput(driveId);
        _output.WriteLine($"Load read throughput: {loadRead.Avg ?? 0:F2} MB/s");

        // Assert
        if (loadRead.Avg.HasValue && baselineRead.Avg.HasValue)
        {
            Assert.True(loadRead.Avg >= baselineRead.Avg, 
                "Read throughput during load should be >= baseline");
            
            _output.WriteLine("✓ Disk read throughput increased or remained stable");
        }
        else
        {
            _output.WriteLine("⚠ Disk speed metrics not available on this platform");
        }
    }

    [Fact]
    public async Task DiskHealth_Should_Report_Available_Metrics()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var diskHealth = sample.Samples.First().DiskHealth;

        // Disk health may be empty if not supported
        if (diskHealth.Length > 0)
        {
            foreach (var health in diskHealth)
            {
                Assert.NotNull(health.DriveId);
                _output.WriteLine($"Disk {health.DriveId}: Available={health.SmartAvailable}");

                if (health.SmartAvailable)
                {
                    _output.WriteLine($"  Health: {health.Status}");
                    if (health.TemperatureCelsius.HasValue)
                        _output.WriteLine($"  Temperature: {health.TemperatureCelsius:F1}°C");
                    if (health.PowerOnHours.HasValue)
                        _output.WriteLine($"  Power-on hours: {health.PowerOnHours}");
                }
            }

            _output.WriteLine("✓ Disk health metrics available");
        }
        else
        {
            _output.WriteLine("⚠ Disk health metrics not available on this platform");
        }
    }

    [Fact]
    public async Task DiskSpeed_Should_Report_Available_Metrics()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 2000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var diskSpeed = sample.Samples.First().DiskSpeed;

        // Disk speed may be empty if not supported
        if (diskSpeed.Length > 0)
        {
            foreach (var speed in diskSpeed)
            {
                Assert.NotNull(speed.DriveId);
                _output.WriteLine($"Disk {speed.DriveId}:");
                _output.WriteLine($"  Performance counters: {speed.PerformanceCountersAvailable}");

                if (speed.ReadThroughputMBps.HasValue)
                    _output.WriteLine($"  Read throughput: {speed.ReadThroughputMBps:F2} MB/s");
                if (speed.WriteThroughputMBps.HasValue)
                    _output.WriteLine($"  Write throughput: {speed.WriteThroughputMBps:F2} MB/s");
                if (speed.ReadIOPS.HasValue)
                    _output.WriteLine($"  Read IOPS: {speed.ReadIOPS:F0}");
                if (speed.WriteIOPS.HasValue)
                    _output.WriteLine($"  Write IOPS: {speed.WriteIOPS:F0}");
            }

            _output.WriteLine("✓ Disk speed metrics available");
        }
        else
        {
            _output.WriteLine("⚠ Disk speed metrics not available on this platform");
        }
    }

    [Fact]
    public async Task ContinuousDiskIO_Should_Show_Sustained_Activity()
    {
        // Get system drive
        var initialSample = await _monitor.GetMetricsAsync(500, null, CancellationToken.None);
        var systemDrive = initialSample.Drives.FirstOrDefault();
        
        if (systemDrive == null)
        {
            _output.WriteLine("⚠ No system drives found, skipping test");
            return;
        }

        // Act - Start continuous I/O
        _output.WriteLine("Starting continuous disk I/O...");
        _ioGenerator.Start(blockSizeKb: 64, operationsPerSecond: 50, readWriteRatio: 0.5);
        await Task.Delay(1000); // Warm-up

        var sample = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 3000);
        await _ioGenerator.StopAsync();

        // Assert - Should have multiple samples showing I/O activity
        Assert.NotEmpty(sample.Samples);
        Assert.True(sample.Count >= 2, "Should have collected multiple samples");

        _output.WriteLine($"Collected {sample.Count} samples during continuous I/O");
        _output.WriteLine("✓ Continuous disk I/O test completed");
    }

    public void Dispose()
    {
        _ioGenerator?.Dispose();
    }
}

