# System Resource Monitoring - Hardware Health & Performance Metrics

## Overview

The RapidStreamer Building Blocks System Resource Monitoring module provides comprehensive hardware health and performance metrics collection across multiple platforms (Windows, Linux, macOS). The module supports real-time monitoring of CPU, memory, disk, GPU, and battery metrics with configurable sampling intervals and platform-specific providers.

## Features

### Supported Metrics

#### 1. **CPU Metrics**
- **Usage**: CPU utilization percentage (current process or all processes)
- **Threads**: Thread count for current/all processes
- **Process Count**: Number of running processes
- **Temperature**: Per-core and package temperature (platform-dependent)

#### 2. **Memory Metrics**
- **Total Memory**: Total physical memory in MB
- **Free Memory**: Available physical memory in MB
- **Used Memory**: Calculated used memory
- **Usage Percentage**: Memory utilization percentage

#### 3. **Disk Metrics**

##### **Space Metrics**
- **Drive Letter/Name**: Drive identifier
- **Total Space**: Total disk capacity
- **Free Space**: Available disk space
- **Used Space**: Calculated used space
- **Usage Percentage**: Disk utilization percentage

##### **Health Metrics (SMART)**
- **Health Status**: Healthy, Warning, Critical, Unknown, Not Supported
- **Wear Level**: SSD wear level percentage (0-100)
- **Temperature**: Disk temperature in Celsius
- **Reallocated Sectors**: Count of reallocated sectors
- **Power-On Hours**: Cumulative drive power-on time

##### **Speed/Performance Metrics**
- **Read/Write Throughput**: MB/s for read and write operations
- **IOPS**: Input/Output Operations Per Second (read/write)
- **Latency**: Average read/write latency in milliseconds
- **Queue Depth**: Number of pending I/O operations
- **Active Time**: Disk active time percentage

#### 4. **GPU Metrics**
- **Temperature**: GPU temperature in Celsius
- **Utilization**: GPU utilization percentage (0-100)
- **Memory Utilization**: GPU memory usage percentage
- **Total/Used Memory**: GPU memory in MB
- **Power Usage**: GPU power consumption in watts
- **Fan Speed**: Fan speed percentage (0-100)
- **Active Processes**: List of processes using GPU resources
  - Process ID and name
  - GPU memory used by process
  - GPU utilization by process

#### 5. **Battery Metrics** (Only if battery present)
- **Battery Presence**: Whether a battery exists in the system
- **Charge Level**: Battery charge percentage (0-100)
- **Status**: Charging, Discharging, Full, Not Charging
- **Remaining Time**: Estimated remaining time in minutes
- **Health**: Battery health percentage (100 = new battery)
- **Design/Full Charge Capacity**: Battery capacities in mWh
- **Charge Rate**: Charging/discharging rate in mW
- **Voltage**: Battery voltage in mV
- **Temperature**: Battery temperature in Celsius
- **Cycle Count**: Number of charge cycles
- **AC Power Status**: Whether system is on AC power

## Platform Support

### Windows
- ✅ CPU Usage, Threads, Processes
- ⚠️ CPU Temperature (requires WMI or hardware monitoring library)
- ✅ Memory Metrics
- ✅ Disk Space
- ⚠️ Disk Health (requires WMI SMART implementation)
- ⚠️ Disk Speed (requires System.Diagnostics.PerformanceCounter package)
- ⚠️ GPU Metrics (requires NVML/AMD Display Library)
- ✅ Battery Metrics (via WMIC)

### Linux
- ✅ CPU Usage, Threads, Processes
- ✅ CPU Temperature (via `/sys/class/thermal`)
- ✅ Memory Metrics (via `free` command)
- ✅ Disk Space
- ⚠️ Disk Health (requires `smartctl`)
- ⚠️ Disk Speed (requires `/proc/diskstats` parsing)
- ⚠️ GPU Metrics (requires `nvidia-smi` or `rocm-smi`)
- ✅ Battery Metrics (via `/sys/class/power_supply`)

### macOS
- ✅ CPU Usage, Threads, Processes
- ⚠️ CPU Temperature (requires IOKit or `smc` tool)
- ✅ Memory Metrics
- ✅ Disk Space
- ⚠️ Disk Health (requires `diskutil smartdata`)
- ⚠️ Disk Speed (requires `iostat` parsing)
- ⚠️ GPU Metrics (requires Metal framework)
- ⚠️ Battery Metrics (requires IOKit or enhanced `pmset` parsing)

**Legend:**
- ✅ Fully implemented
- ⚠️ Placeholder/partial implementation - requires additional platform-specific implementation

## Installation

Add the package reference to your project:

```xml
<ItemGroup>
    <PackageReference Include="RapidStreamer.BuildingBlocks.Modules" Version="1.0.x" />
</ItemGroup>
```

## Usage

### Basic Setup

```csharp
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

// In your Startup.cs or Program.cs
services.AddSystemResourceMonitor();
```

### Configuration

Configure monitoring options:

```csharp
services.AddSystemResourceMonitor(options =>
{
    // Enable/disable specific metric groups
    options.EnableCpuMetrics = true;
    options.EnableCpuTemperature = true;
    options.EnableMemoryMetrics = true;
    options.EnableDiskSpaceMetrics = true;
    options.EnableDiskHealthMetrics = true;
    options.EnableDiskSpeedMetrics = true;
    options.EnableGpuMetrics = true;
    options.EnableBatteryMetrics = true;

    // Sampling configuration
    options.DefaultSamplingWindowMs = 500; // CPU sampling window
    options.CollectAllProcesses = false;    // false = current process only

    // GPU configuration
    options.MaxGpuProcesses = 10;  // Max GPU processes to track per GPU

    // Cache configuration
    options.HardwareMetricsCacheDurationSeconds = 60;
});
```

### Collecting Metrics

```csharp
using RapidStreamer.BuildingBlocks.Infrastructure.SystemResourceMonitor;

public class MonitoringService
{
    private readonly ISystemResourceMonitor _monitor;

    public MonitoringService(ISystemResourceMonitor monitor)
    {
        _monitor = monitor;
    }

    public void CollectMetrics()
    {
        // Get all metrics with default settings
        var metrics = _monitor.GetMetrics();

        // Get metrics with custom sampling window and all processes
        var detailedMetrics = _monitor.GetMetrics(window: 1000, all: true);

        // Access specific metrics
        Console.WriteLine($"CPU Usage: {metrics.Cpu.Usage}%");
        Console.WriteLine($"Memory Usage: {metrics.Memory.UsagePercentage}%");

        // CPU Temperature (may be null if not supported)
        if (metrics.CpuTemperature?.TemperatureSensorsAvailable == true)
        {
            Console.WriteLine($"CPU Temp: {metrics.CpuTemperature.PackageTemperatureCelsius}°C");
        }

        // Disk metrics
        foreach (var drive in metrics.Drives)
        {
            Console.WriteLine($"Drive {drive.Letter}: {drive.UsagePercentage}% used");
        }

        // Disk health
        foreach (var diskHealth in metrics.DiskHealth)
        {
            if (diskHealth.SmartAvailable)
            {
                Console.WriteLine($"Disk {diskHealth.DriveId} Health: {diskHealth.Status}");
                if (diskHealth.TemperatureCelsius.HasValue)
                    Console.WriteLine($"  Temperature: {diskHealth.TemperatureCelsius}°C");
            }
        }

        // Disk speed
        foreach (var diskSpeed in metrics.DiskSpeed)
        {
            if (diskSpeed.PerformanceCountersAvailable)
            {
                Console.WriteLine($"Disk {diskSpeed.DriveId}:");
                Console.WriteLine($"  Read: {diskSpeed.ReadThroughputMBps} MB/s");
                Console.WriteLine($"  Write: {diskSpeed.WriteThroughputMBps} MB/s");
            }
        }

        // GPU metrics
        foreach (var gpu in metrics.Gpus)
        {
            if (gpu.IsAvailable)
            {
                Console.WriteLine($"GPU {gpu.GpuIndex} ({gpu.GpuName}):");
                Console.WriteLine($"  Utilization: {gpu.UtilizationPercent}%");
                Console.WriteLine($"  Temperature: {gpu.TemperatureCelsius}°C");
                Console.WriteLine($"  Active Processes: {gpu.ActiveProcesses.Count}");
            }
        }

        // Battery metrics (only if battery present)
        if (metrics.Battery?.BatteryPresent == true)
        {
            Console.WriteLine($"Battery: {metrics.Battery.ChargePercent}%");
            Console.WriteLine($"Status: {metrics.Battery.Status}");
            if (metrics.Battery.RemainingTimeMinutes.HasValue)
                Console.WriteLine($"Remaining: {metrics.Battery.RemainingTimeMinutes} minutes");
            if (metrics.Battery.HealthPercent.HasValue)
                Console.WriteLine($"Health: {metrics.Battery.HealthPercent}%");
        }
    }
}
```

### Error Handling

Metrics that are not supported on the current platform will:
- Return `null` for optional metrics (e.g., `CpuTemperature`, `Battery`)
- Return empty arrays for collection metrics (e.g., `DiskHealth`, `Gpus`)
- Include `ErrorMessage` property explaining why the metric is unavailable
- Not throw exceptions - errors are logged internally

Example:

```csharp
var metrics = _monitor.GetMetrics();

// Check if CPU temperature is available
if (metrics.CpuTemperature != null)
{
    if (metrics.CpuTemperature.TemperatureSensorsAvailable)
    {
        // Use temperature data
        var temp = metrics.CpuTemperature.PackageTemperatureCelsius;
    }
    else
    {
        // Log why it's not available
        Console.WriteLine(metrics.CpuTemperature.ErrorMessage);
    }
}

// Check individual disk health
foreach (var diskHealth in metrics.DiskHealth)
{
    if (diskHealth.SmartAvailable)
    {
        // Use SMART data
    }
    else if (!string.IsNullOrEmpty(diskHealth.ErrorMessage))
    {
        Console.WriteLine($"Disk {diskHealth.DriveId}: {diskHealth.ErrorMessage}");
    }
}
```

## Performance Considerations

1. **CPU Metrics**: Uses async sampling with configurable window (default 500ms). Collecting all processes is more expensive than current process only.

2. **Disk Health/Speed**: Can be resource-intensive. Consider:
   - Caching results (use `HardwareMetricsCacheDurationSeconds`)
   - Disabling if not needed
   - Collecting on a background schedule rather than every request

3. **GPU Metrics**: Querying GPU status can have overhead, especially when enumerating processes. Limit with `MaxGpuProcesses` option.

4. **Battery Metrics**: Relatively lightweight, but disabled automatically if no battery present.

## Extending Platform Support

To enhance platform-specific implementations:

### Windows SMART Data
Implement WMI queries in `WindowsDiskHealthProvider`:
```csharp
// Query Win32_DiskDrive and MSStorageDriver_ATAPISmartData
```

### Linux Disk Speed
Parse `/proc/diskstats` in `LinuxDiskSpeedProvider`:
```csharp
// Read /proc/diskstats and calculate deltas
```

### GPU Metrics
Integrate vendor SDKs:
- **NVIDIA**: NVML (nvidia-ml library)
- **AMD**: AMD Display Library
- **Intel**: Intel GPU metrics API

## Troubleshooting

### Metric returns null or empty
- Check platform support matrix above
- Review `ErrorMessage` property for details
- Ensure required OS permissions (e.g., SMART data may require admin/root)
- Verify required tools are installed (e.g., `smartctl`, `nvidia-smi`)

### High CPU usage
- Increase `DefaultSamplingWindowMs` (reduces sampling frequency)
- Set `CollectAllProcesses = false`
- Disable unused metric groups

### Missing disk health data
- **Windows**: Requires WMI implementation
- **Linux**: Install `smartmontools` package
- **macOS**: Use `diskutil smartdata` (may require permissions)

## API Reference

### `ISystemResourceMonitor`

```csharp
public interface ISystemResourceMonitor
{
    /// <summary>
    /// Gets all configured system resource metrics.
    /// </summary>
    /// <param name="window">Sampling window in milliseconds for CPU usage. Null uses default.</param>
    /// <param name="all">Whether to collect for all processes. Null uses default.</param>
    /// <returns>Comprehensive system resource metrics.</returns>
    SystemResourceMonitorMetrics GetMetrics(long? window = null, bool? all = null);
}
```

### `SystemResourceMonitorMetrics`

```csharp
public record SystemResourceMonitorMetrics
{
    public CpuMetrics Cpu { get; init; }
    public CpuTemperatureMetrics? CpuTemperature { get; init; }
    public MemoryMetrics Memory { get; init; }
    public SystemDriveMetrics[] Drives { get; init; }
    public DiskHealthMetrics[] DiskHealth { get; init; }
    public DiskSpeedMetrics[] DiskSpeed { get; init; }
    public GpuMetrics[] Gpus { get; init; }
    public BatteryMetrics? Battery { get; init; }
}
```

For detailed property documentation, see the XML comments in the source code.

## Contributing

To contribute platform-specific implementations:

1. Implement the appropriate provider interface (e.g., `IDiskHealthProvider`)
2. Add platform-specific logic in the provider class
3. Update platform support matrix in this README
4. Add unit tests for the new functionality
5. Update examples with platform-specific notes

## License

This module is part of the RapidStreamer Building Blocks and follows the same license as the parent project.

