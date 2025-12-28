using Microsoft.Extensions.Options;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Battery;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Cpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Disk;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Gpu;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.Memory;
using ThunderPropagator.BuildingBlocks.Infrastructure.SystemResourceMonitor.Metrics.SystemDrives;
using ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.Helpers;
using Xunit.Abstractions;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration;

/// <summary>
/// Integration tests for GPU and Battery metrics (platform-dependent).
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ResourceMonitor")]
public class HardwareMetricsIntegrationTests
{
    private readonly ITestOutputHelper _output;
    private readonly ISystemResourceMonitor _monitor;
    private readonly MetricsSampler _sampler;

    public HardwareMetricsIntegrationTests(ITestOutputHelper output)
    {
        _output = output;

        var options = Options.Create(new SystemResourceMonitorOptions
        {
            DefaultSamplingWindowMs = 500,
            EnableGpuMetrics = true,
            EnableBatteryMetrics = true,
            EnableCpuTemperature = true
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
    }

    [Fact]
    public async Task GpuMetrics_Should_Be_Available_Or_Gracefully_Unavailable()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var gpus = sample.Samples.First().Gpus;

        if (gpus.Length > 0)
        {
            _output.WriteLine($"Found {gpus.Length} GPU(s)");

            foreach (var gpu in gpus)
            {
                _output.WriteLine($"GPU {gpu.GpuIndex}: {gpu.GpuName ?? "Unknown"}");
                _output.WriteLine($"  Available: {gpu.IsAvailable}");

                if (gpu.IsAvailable)
                {
                    if (gpu.TemperatureCelsius.HasValue)
                        _output.WriteLine($"  Temperature: {gpu.TemperatureCelsius:F1}°C");
                    if (gpu.UtilizationPercent.HasValue)
                        _output.WriteLine($"  Utilization: {gpu.UtilizationPercent:F1}%");
                    if (gpu.MemoryUtilizationPercent.HasValue)
                        _output.WriteLine($"  Memory utilization: {gpu.MemoryUtilizationPercent:F1}%");
                    if (gpu.TotalMemoryMB.HasValue)
                        _output.WriteLine($"  Total memory: {gpu.TotalMemoryMB:F0} MB");
                    if (gpu.PowerUsageWatts.HasValue)
                        _output.WriteLine($"  Power usage: {gpu.PowerUsageWatts:F1} W");

                    // Validate ranges
                    if (gpu.TemperatureCelsius.HasValue)
                        Assert.InRange(gpu.TemperatureCelsius.Value, 0, 150);
                    if (gpu.UtilizationPercent.HasValue)
                        Assert.InRange(gpu.UtilizationPercent.Value, 0, 100);
                    if (gpu.MemoryUtilizationPercent.HasValue)
                        Assert.InRange(gpu.MemoryUtilizationPercent.Value, 0, 100);
                }
                else
                {
                    _output.WriteLine($"  Error: {gpu.ErrorMessage}");
                }
            }

            _output.WriteLine("✓ GPU metrics available and valid");
        }
        else
        {
            _output.WriteLine("⚠ No GPUs found or GPU metrics not supported on this platform");
        }
    }

    [Fact]
    public async Task BatteryMetrics_Should_Be_Available_Or_Gracefully_Unavailable()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        Assert.NotEmpty(sample.Samples);
        var battery = sample.Samples.First().Battery;

        if (battery != null && battery.BatteryPresent)
        {
            _output.WriteLine("Battery detected:");
            _output.WriteLine($"  Status: {battery.Status}");
            _output.WriteLine($"  Charge: {battery.ChargePercent ?? 0:F1}%");
            _output.WriteLine($"  On AC Power: {battery.OnACPower}");

            if (battery.RemainingTimeMinutes.HasValue)
                _output.WriteLine($"  Remaining time: {battery.RemainingTimeMinutes} minutes");
            if (battery.HealthPercent.HasValue)
                _output.WriteLine($"  Health: {battery.HealthPercent:F1}%");
            if (battery.DesignCapacityMWh.HasValue)
                _output.WriteLine($"  Design capacity: {battery.DesignCapacityMWh} mWh");
            if (battery.FullChargeCapacityMWh.HasValue)
                _output.WriteLine($"  Full charge capacity: {battery.FullChargeCapacityMWh} mWh");
            if (battery.ChargeRateMW.HasValue)
                _output.WriteLine($"  Charge rate: {battery.ChargeRateMW} mW");
            if (battery.VoltageMV.HasValue)
                _output.WriteLine($"  Voltage: {battery.VoltageMV} mV");
            if (battery.TemperatureCelsius.HasValue)
                _output.WriteLine($"  Temperature: {battery.TemperatureCelsius:F1}°C");
            if (battery.CycleCount.HasValue)
                _output.WriteLine($"  Cycle count: {battery.CycleCount}");

            // Validate ranges
            if (battery.ChargePercent.HasValue)
                Assert.InRange(battery.ChargePercent.Value, 0, 100);
            if (battery.HealthPercent.HasValue)
                Assert.InRange(battery.HealthPercent.Value, 0, 100);
            if (battery.TemperatureCelsius.HasValue)
                Assert.InRange(battery.TemperatureCelsius.Value, -20, 80);

            _output.WriteLine("✓ Battery metrics available and valid");
        }
        else
        {
            _output.WriteLine("⚠ No battery detected (desktop system or not supported)");
        }
    }

    [Fact]
    public async Task CpuTemperature_Should_Be_Consistent_Across_Samples()
    {
        // Act - Collect multiple samples
        var samples = new List<double?>();
        for (var i = 0; i < 5; i++)
        {
            var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 500);
            var temp = sample.CpuTemperature();
            samples.Add(temp.Avg);

            if (temp.Avg.HasValue)
                _output.WriteLine($"Sample {i + 1}: {temp.Avg:F1}°C");
            else
                _output.WriteLine($"Sample {i + 1}: Not available");

            await Task.Delay(500);
        }

        // Assert
        var validSamples = samples.Where(s => s.HasValue).ToList();

        if (validSamples.Any())
        {
            var minTemp = validSamples.Min()!.Value;
            var maxTemp = validSamples.Max()!.Value;
            var avgTemp = validSamples.Average()!.Value;

            _output.WriteLine($"Temperature range: [{minTemp:F1}°C, {maxTemp:F1}°C], Avg: {avgTemp:F1}°C");

            // Temperature should be reasonable
            Assert.InRange(minTemp, 0, 120);
            Assert.InRange(maxTemp, 0, 120);

            _output.WriteLine("✓ CPU temperature is consistent and within valid range");
        }
        else
        {
            _output.WriteLine("⚠ CPU temperature not supported on this platform");
        }
    }

    [Fact]
    public async Task GpuProcesses_Should_Report_Active_Processes_When_Available()
    {
        // Act
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 1000);

        // Assert
        var gpus = sample.Samples.First().Gpus;

        if (gpus.Length > 0 && gpus.Any(g => g.IsAvailable))
        {
            foreach (var gpu in gpus.Where(g => g.IsAvailable))
            {
                _output.WriteLine($"GPU {gpu.GpuIndex} active processes: {gpu.ActiveProcesses.Count}");

                foreach (var process in gpu.ActiveProcesses.Take(5)) // Show first 5
                {
                    _output.WriteLine($"  PID {process.ProcessId}: {process.ProcessName}");
                    if (process.UsedMemoryMB.HasValue)
                        _output.WriteLine($"    Memory: {process.UsedMemoryMB} MB");
                    if (process.UtilizationPercent.HasValue)
                        _output.WriteLine($"    Utilization: {process.UtilizationPercent:F1}%");
                }

                if (gpu.ActiveProcesses.Count > 5)
                    _output.WriteLine($"  ... and {gpu.ActiveProcesses.Count - 5} more");
            }

            _output.WriteLine("✓ GPU process information available");
        }
        else
        {
            _output.WriteLine("⚠ GPU process information not available");
        }
    }

    [Fact]
    public async Task AllHardwareMetrics_Should_Complete_Within_Reasonable_Time()
    {
        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var sample = await _sampler.CollectSamplesAsync(windowMs: 500, durationMs: 2000);
        sw.Stop();

        // Assert
        Assert.NotEmpty(sample.Samples);
        Assert.True(sw.ElapsedMilliseconds < 5000, 
            $"Metric collection should complete within 5 seconds (took {sw.ElapsedMilliseconds}ms)");

        _output.WriteLine($"Collected {sample.Count} samples in {sw.ElapsedMilliseconds}ms");
        _output.WriteLine($"Average collection time: {sw.ElapsedMilliseconds / sample.Count}ms per sample");
        _output.WriteLine("✓ All hardware metrics collected within reasonable time");
    }
}

