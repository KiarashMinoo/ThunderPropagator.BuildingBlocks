using Microsoft.Extensions.Options;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.Helpers;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;
using Xunit.Abstractions;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Comprehensive integration tests that validate the complete system resource monitor
/// under combined load scenarios.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
[Trait("Category", "Comprehensive")]
public class ComprehensiveSystemMonitorTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricsSampler _sampler;
    private readonly CpuLoadGenerator _cpuLoader;
    private readonly MemoryLoadGenerator _memoryLoader;
    private readonly DiskIoGenerator _diskIoLoader;
    private readonly ProcessLoadGenerator _processLoader;

    public ComprehensiveSystemMonitorTests(ITestOutputHelper output)
    {
        _output = output;

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            DefaultSamplingWindowMs = 500,
            CollectAllProcesses = true,
            EnableCpuTemperature = true,
            EnableDiskHealthMetrics = true,
            EnableDiskSpeedMetrics = true,
            EnableGpuMetrics = true,
            EnableBatteryMetrics = true
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
        _cpuLoader = new CpuLoadGenerator();
        _memoryLoader = new MemoryLoadGenerator();
        _diskIoLoader = new DiskIoGenerator();
        _processLoader = new ProcessLoadGenerator();
    }

    [Fact]
    public async Task AllMetrics_Should_Be_Collected_Successfully_Under_No_Load()
    {
        // Act
        _output.WriteLine("Collecting all metrics under idle conditions...");
        var sample = await _sampler.CollectSamplesAsync(windowMs: 1000, durationMs: 3000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var metrics = sample.Samples.First();

        _output.WriteLine("\n=== CPU Metrics ===");
        _output.WriteLine($"Usage: {metrics.Cpu.Usage:F2}%");
        _output.WriteLine($"Processor Count: {metrics.Cpu.ProcessorCount}");
        _output.WriteLine($"Threads: {metrics.Cpu.Threads}");
        _output.WriteLine($"Processes: {metrics.Cpu.Processes}");

        _output.WriteLine("\n=== Memory Metrics ===");
        _output.WriteLine($"Total: {metrics.Memory.Total / (1024.0 * 1024 * 1024):F2} GB");
        _output.WriteLine($"Used: {metrics.Memory.Used / (1024.0 * 1024 * 1024):F2} GB ({metrics.Memory.UsagePercentage:F2}%)");
        _output.WriteLine($"Free: {metrics.Memory.Free / (1024.0 * 1024 * 1024):F2} GB");

        _output.WriteLine("\n=== Drive Metrics ===");
        foreach (var drive in metrics.Drives)
        {
            _output.WriteLine($"{drive.Letter}: {drive.Free / (1024.0 * 1024 * 1024):F2} GB free of {drive.Total / (1024.0 * 1024 * 1024):F2} GB");
        }

        _output.WriteLine("\n=== Optional Metrics ===");
        _output.WriteLine($"CPU Temperature: {(metrics.CpuTemperature?.TemperatureSensorsAvailable == true ? "Available" : "Not available")}");
        _output.WriteLine($"Disk Health: {metrics.DiskHealth.Length} disk(s)");
        _output.WriteLine($"Disk Speed: {metrics.DiskSpeed.Length} disk(s)");
        _output.WriteLine($"GPUs: {metrics.Gpus.Length} GPU(s)");
        _output.WriteLine($"Battery: {(metrics.Battery?.BatteryPresent == true ? "Present" : "Not present")}");

        // Basic validations
        Assert.True(metrics.Cpu.Usage is >= 0 and <= 100);
        Assert.True(metrics.Memory.Total > 0);
        Assert.True(metrics.Memory.Free >= 0);
        Assert.NotEmpty(metrics.Drives);

        _output.WriteLine("\n✓ All metrics collected successfully");
    }

    [Fact]
    public async Task CombinedLoad_Should_Affect_Multiple_Metrics_Simultaneously()
    {
        // Baseline
        _output.WriteLine("Collecting baseline metrics...");
        var baseline = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        
        var baselineCpu = baseline.CpuUsage();
        var baselineMemory = baseline.MemoryUsed();
        var baselineThreads = baseline.ThreadCount();

        _output.WriteLine($"Baseline - CPU: {baselineCpu.Avg:F2}%, Memory: {baselineMemory.Avg / (1024 * 1024):F2} MB, Threads: {baselineThreads.Avg:F1}");

        // Act - Apply combined load
        _output.WriteLine("\nApplying combined load (CPU + Memory + Disk + Threads)...");
        
        _cpuLoader.Start(threadCount: Environment.ProcessorCount / 2, targetUtilizationPercent: 60);
        _memoryLoader.AllocateAndRetain(50);
        _diskIoLoader.Start(blockSizeKb: 32, operationsPerSecond: 50);
        _processLoader.CreateThreads(10);

        await Task.Delay(1500); // Warm-up

        var load = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 3000);
        
        var loadCpu = load.CpuUsage();
        var loadMemory = load.MemoryUsed();
        var loadThreads = load.ThreadCount();

        _output.WriteLine($"Load - CPU: {loadCpu.Avg:F2}%, Memory: {loadMemory.Avg / (1024 * 1024):F2} MB, Threads: {loadThreads.Avg:F1}");

        // Stop load
        await _cpuLoader.StopAsync();
        await _diskIoLoader.StopAsync();

        // Cooldown
        _output.WriteLine("\nCooling down...");
        await Task.Delay(2000);
        
        var cooldown = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        var cooldownCpu = cooldown.CpuUsage();

        _output.WriteLine($"Cooldown - CPU: {cooldownCpu.Avg:F2}%");

        // Assert
        Assert.True(loadCpu.Avg > baselineCpu.Avg, "CPU usage should increase under load");
        Assert.True(loadMemory.Avg > baselineMemory.Avg, "Memory usage should increase under load");
        Assert.True(loadThreads.Avg > baselineThreads.Avg, "Thread count should increase under load");
        Assert.True(cooldownCpu.Avg < loadCpu.Avg, "CPU should cool down after load");

        _output.WriteLine("\n✓ Combined load affected multiple metrics as expected");
    }

    [Fact]
    public async Task MetricCollection_Should_Be_Stable_Over_Extended_Period()
    {
        // Act - Collect metrics over 10 seconds
        _output.WriteLine("Collecting metrics over 10 seconds...");
        var samples = new List<SystemResourceMonitorMetrics>();

        for (var i = 0; i < 10; i++)
        {
            var metrics = await _monitor.GetMetricsAsync(500, null, CancellationToken.None);
            samples.Add(metrics);
            _output.WriteLine($"Sample {i + 1}/10: CPU={metrics.Cpu.Usage:F2}%, Memory={metrics.Memory.UsagePercentage:F2}%");
            await Task.Delay(1000);
        }

        // Assert - All samples should be valid
        Assert.Equal(10, samples.Count);
        Assert.All(samples, s =>
        {
            Assert.True(s.Cpu.Usage is >= 0 and <= 100);
            Assert.True(s.Memory.Total > 0);
            Assert.True(s.Memory.Free >= 0);
        });

        _output.WriteLine("\n✓ Metric collection is stable over extended period");
    }

    [Fact]
    public async Task MetricCollection_Should_Handle_Rapid_Successive_Calls()
    {
        // Act - Collect metrics rapidly
        _output.WriteLine("Collecting metrics rapidly (10 times in quick succession)...");
        var tasks = new List<Task<SystemResourceMonitorMetrics>>();

        for (var i = 0; i < 10; i++)
        {
            tasks.Add(_monitor.GetMetricsAsync(100, null, CancellationToken.None));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All calls should succeed
        Assert.Equal(10, results.Length);
        Assert.All(results, r =>
        {
            Assert.NotNull(r);
            Assert.NotNull(r.Cpu);
            Assert.NotNull(r.Memory);
        });

        _output.WriteLine($"Collected {results.Length} samples rapidly");
        _output.WriteLine("✓ Rapid successive calls handled successfully");
    }

    [Fact]
    public async Task DisabledMetrics_Should_Return_Empty_Or_Null()
    {
        // Arrange - Create monitor with disabled metrics
        var options = Options.Create(new SystemResourceMonitorOptions
        {
            EnableCpuTemperature = false,
            EnableDiskHealthMetrics = false,
            EnableDiskSpeedMetrics = false,
            EnableGpuMetrics = false,
            EnableBatteryMetrics = false
        });

        var disabledMonitor = new SystemResourceMonitorImpl(
            new CpuMetricsClient(),
            new CpuTemperatureMetricsClient(),
            new MemoryMetricsClient(),
            new SystemDriveMetricsClient(),
            new DiskHealthMetricsClient(),
            new DiskSpeedMetricsClient(),
            new GpuMetricsClient(),
            new BatteryMetricsClient(),
            options);

        // Act
        var metrics = await disabledMonitor.GetMetricsAsync(500, null, CancellationToken.None);

        // Assert
        Assert.Null(metrics.CpuTemperature);
        Assert.Empty(metrics.DiskHealth);
        Assert.Empty(metrics.DiskSpeed);
        Assert.Empty(metrics.Gpus);
        Assert.Null(metrics.Battery);

        // Core metrics should still be present
        Assert.NotNull(metrics.Cpu);
        Assert.NotNull(metrics.Memory);
        Assert.NotEmpty(metrics.Drives);

        _output.WriteLine("✓ Disabled metrics return empty/null as expected");
    }

    [Fact]
    public async Task MetricCollection_Should_Be_Thread_Safe()
    {
        // Act - Collect metrics from multiple threads simultaneously
        _output.WriteLine("Collecting metrics from 5 threads simultaneously...");
        var tasks = new List<Task>();

        for (var i = 0; i < 5; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(async () =>
            {
                for (var j = 0; j < 3; j++)
                {
                    var metrics = await _monitor.GetMetricsAsync(500, null, CancellationToken.None);
                    _output.WriteLine($"Thread {threadId}, Sample {j + 1}: CPU={metrics.Cpu.Usage:F2}%");
                    await Task.Delay(100);
                }
            }));
        }

        await Task.WhenAll(tasks);

        _output.WriteLine("✓ Metric collection is thread-safe");
    }

    [Fact]
    public async Task LongRunningLoad_Should_Show_Sustained_Impact()
    {
        // Act - Apply sustained CPU load
        _output.WriteLine("Applying sustained CPU load for 10 seconds...");
        _cpuLoader.Start(threadCount: 2, targetUtilizationPercent: 70);

        var samples = new List<double>();
        for (var i = 0; i < 10; i++)
        {
            var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);
            var cpuAvg = sample.CpuUsage().Avg;
            samples.Add(cpuAvg);
            _output.WriteLine($"Second {i + 1}: CPU={cpuAvg:F2}%");
        }

        await _cpuLoader.StopAsync();

        // Assert - Most samples should show elevated CPU usage
        var elevatedSamples = samples.Count(s => s > 30);
        Assert.True(elevatedSamples >= 7, $"Expected at least 7/10 samples with elevated CPU (got {elevatedSamples})");

        _output.WriteLine($"\n{elevatedSamples}/10 samples showed elevated CPU usage");
        _output.WriteLine("✓ Long-running load shows sustained impact");
    }

    public void Dispose()
    {
        _cpuLoader?.Dispose();
        _memoryLoader?.Dispose();
        _diskIoLoader?.Dispose();
        _processLoader?.Dispose();
    }
}

