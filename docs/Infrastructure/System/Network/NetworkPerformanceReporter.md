# NetworkPerformanceReporter

## Overview

The `NetworkPerformanceReporter` class provides real-time network performance monitoring capabilities using Event Tracing for Windows (ETW). This component captures TCP and UDP traffic statistics for specific processes, enabling detailed network performance analysis and monitoring in Windows environments.

## Purpose

- **Real-Time Monitoring**: Live network traffic monitoring using ETW
- **Process-Specific Tracking**: Monitor network activity for specific processes
- **Protocol Separation**: Distinct tracking of TCP and UDP protocols
- **Performance Analysis**: Calculate bytes per second rates for network traffic

## Class Declaration

```csharp
public sealed class NetworkPerformanceReporter : DisposableObject
{
    // Factory method for creating instances
    public static NetworkPerformanceReporter Create(
        int processId, 
        string sessionName, 
        bool enableUdp = false, 
        CancellationToken cancellationToken = default);

    // Main data collection method
    public NetworkPerformanceData GetNetworkPerformanceData();
}
```

## Key Features

### ETW Integration
- **Kernel Provider**: Uses ETW kernel network provider for low-level network events
- **Real-Time Events**: Processes network events as they occur
- **Minimal Overhead**: Efficient event handling with minimal performance impact
- **Administrative Privileges**: Requires elevated privileges for ETW session access

### Process Isolation
- **Process-Specific Monitoring**: Tracks only the specified process ID
- **Event Filtering**: Filters network events by process ID for accurate attribution
- **Multi-Process Support**: Multiple reporters can monitor different processes simultaneously

### Protocol Support
- **TCP Monitoring**: Always enabled for TCP send/receive events
- **UDP Monitoring**: Optional UDP send/receive event tracking
- **Bidirectional Tracking**: Separate counters for sent and received data

## Factory Method

### Create
Creates and initializes a new NetworkPerformanceReporter instance.

```csharp
public static NetworkPerformanceReporter Create(
    int processId,           // Process ID to monitor
    string sessionName,      // ETW session name (must be unique)
    bool enableUdp = false,  // Enable UDP monitoring (optional)
    CancellationToken cancellationToken = default)
```

#### Parameters
- **processId**: The Windows process ID to monitor
- **sessionName**: Unique name for the ETW session
- **enableUdp**: Whether to enable UDP traffic monitoring (default: false)
- **cancellationToken**: Cancellation token for stopping the ETW session

#### Returns
Configured and initialized `NetworkPerformanceReporter` instance.

## Usage Examples

### Basic TCP Monitoring
```csharp
// Monitor current process TCP traffic
var processId = Environment.ProcessId;
var sessionName = $"NetworkMonitor_{processId}_{Guid.NewGuid():N}";

using var reporter = NetworkPerformanceReporter.Create(
    processId: processId,
    sessionName: sessionName,
    enableUdp: false);

// Collect performance data every 30 seconds
while (true)
{
    await Task.Delay(TimeSpan.FromSeconds(30));
    
    var data = reporter.GetNetworkPerformanceData();
    Console.WriteLine($"TCP Traffic - Sent: {data.TcpBytesSent:N0} B/s, Received: {data.TcpBytesReceived:N0} B/s");
}
```

### TCP and UDP Monitoring
```csharp
using var cancellationTokenSource = new CancellationTokenSource();
var processId = Environment.ProcessId;
var sessionName = $"FullNetworkMonitor_{processId}";

using var reporter = NetworkPerformanceReporter.Create(
    processId: processId,
    sessionName: sessionName,
    enableUdp: true,  // Enable UDP monitoring
    cancellationToken: cancellationTokenSource.Token);

var data = reporter.GetNetworkPerformanceData();
Console.WriteLine($"Total Traffic: {data.BytesTotal:N0} bytes/sec");
Console.WriteLine($"TCP: {data.TcpBytesTotal:N0} B/s, UDP: {data.UdpBytesTotal:N0} B/s");
```

### Background Service Integration
```csharp
public class NetworkMonitoringService : BackgroundService
{
    private readonly ILogger<NetworkMonitoringService> _logger;
    private readonly IConfiguration _configuration;
    private NetworkPerformanceReporter? _reporter;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var processId = Environment.ProcessId;
            var sessionName = $"NetworkMonitor_{Environment.MachineName}_{processId}";
            
            _reporter = NetworkPerformanceReporter.Create(
                processId: processId,
                sessionName: sessionName,
                enableUdp: _configuration.GetValue<bool>("NetworkMonitoring:EnableUDP", false),
                cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var data = _reporter.GetNetworkPerformanceData();
                
                _logger.LogInformation("Network Performance: {TotalBytes:N0} B/s (TCP: {TcpBytes:N0}, UDP: {UdpBytes:N0})",
                    data.BytesTotal, data.TcpBytesTotal, data.UdpBytesTotal);

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Administrator"))
        {
            _logger.LogError("Network monitoring requires administrator privileges");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Network monitoring failed");
        }
    }

    public override void Dispose()
    {
        _reporter?.Dispose();
        base.Dispose();
    }
}
```

### Multi-Process Monitoring
```csharp
public class MultiProcessNetworkMonitor : IDisposable
{
    private readonly Dictionary<int, NetworkPerformanceReporter> _reporters = new();
    private readonly Timer _collectionTimer;

    public MultiProcessNetworkMonitor(IEnumerable<int> processIds)
    {
        foreach (var processId in processIds)
        {
            var sessionName = $"NetworkMonitor_{processId}_{Guid.NewGuid():N}";
            var reporter = NetworkPerformanceReporter.Create(processId, sessionName, enableUdp: true);
            _reporters[processId] = reporter;
        }

        _collectionTimer = new Timer(CollectNetworkData, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void CollectNetworkData(object? state)
    {
        foreach (var (processId, reporter) in _reporters)
        {
            try
            {
                var data = reporter.GetNetworkPerformanceData();
                Console.WriteLine($"Process {processId}: {data.BytesTotal:N0} bytes/sec total");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to collect data for process {processId}: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _collectionTimer?.Dispose();
        foreach (var reporter in _reporters.Values)
        {
            reporter.Dispose();
        }
        _reporters.Clear();
    }
}
```

## Implementation Details

### ETW Session Management
```csharp
// ETW session lifecycle management
using (_etwSession = new TraceEventSession(_sessionName))
{
    // Enable network trace events
    _etwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);
    
    // Register event handlers
    _etwSession.Source.Kernel.TcpIpRecv += HandleTcpReceive;
    _etwSession.Source.Kernel.TcpIpSend += HandleTcpSend;
    
    if (_enableUdp)
    {
        _etwSession.Source.Kernel.UdpIpRecv += HandleUdpReceive;
        _etwSession.Source.Kernel.UdpIpSend += HandleUdpSend;
    }
    
    // Process events until cancellation
    _etwSession.Source.Process();
}
```

### Thread Safety
```csharp
// Thread-safe counter management
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

private void HandleTcpReceive(TcpIpTraceData data)
{
    if (data.ProcessID == _processId)
    {
        lock (_lock)
        {
            _counters.TcpReceived += data.size;
        }
    }
}
```

### Rate Calculation
```csharp
public NetworkPerformanceData GetNetworkPerformanceData()
{
    var timeDifferenceInSeconds = (DateTime.UtcNow - _etwStartTime).TotalSeconds;

    NetworkPerformanceData networkData;
    
    lock (_lock)
    {
        // Convert cumulative bytes to bytes per second
        networkData = new NetworkPerformanceData
        {
            TcpBytesReceived = Convert.ToInt64(_counters.TcpReceived / timeDifferenceInSeconds),
            TcpBytesSent = Convert.ToInt64(_counters.TcpSent / timeDifferenceInSeconds),
            UdpBytesReceived = Convert.ToInt64(_counters.UdpReceived / timeDifferenceInSeconds),
            UdpBytesSent = Convert.ToInt64(_counters.UdpSent / timeDifferenceInSeconds)
        };
    }

    // Reset counters for next measurement
    ResetCounters();
    
    return networkData;
}
```

## Configuration and Setup

### Administrative Privileges
```csharp
// Check for required privileges
if (!(TraceEventSession.IsElevated() ?? false))
{
    throw new InvalidOperationException(
        "To turn on ETW events you need to be Administrator, please run from an Admin process.");
}
```

### Session Name Management
```csharp
// Generate unique session names to avoid conflicts
public static string GenerateSessionName(int processId)
{
    return $"NetworkPerf_{Environment.MachineName}_{processId}_{Guid.NewGuid():N}";
}

// Clean up existing sessions if needed
public static void CleanupExistingSessions(string sessionPrefix)
{
    // Implementation to clean up ETW sessions with matching prefix
}
```

### Process Discovery
```csharp
public static class ProcessHelper
{
    public static int GetCurrentProcessId() => Environment.ProcessId;
    
    public static IEnumerable<int> GetChildProcessIds(int parentProcessId)
    {
        return Process.GetProcesses()
            .Where(p => GetParentProcessId(p.Id) == parentProcessId)
            .Select(p => p.Id);
    }
    
    private static int GetParentProcessId(int processId)
    {
        // Implementation to get parent process ID
        // Using WMI or P/Invoke to Win32 APIs
        return 0;
    }
}
```

## Performance Considerations

### Memory Usage
- **Minimal Buffer Allocation**: Events processed immediately without large buffers
- **Counter Reset**: Counters reset after each data collection to prevent overflow
- **Efficient Locking**: Minimal lock contention with fast counter updates

### CPU Overhead
- **Event Filtering**: Only processes events for the target process ID
- **Selective Protocol Monitoring**: UDP monitoring optional to reduce overhead
- **Background Processing**: ETW events processed on dedicated thread

### Accuracy Considerations
- **Time-Based Calculations**: Accuracy depends on measurement intervals
- **Counter Overflow**: Long type provides sufficient range for most scenarios
- **Clock Synchronization**: Uses UTC time for consistent rate calculations

## Error Handling

### Common Exception Scenarios
```csharp
public class NetworkMonitoringException : Exception
{
    public NetworkMonitoringException(string message) : base(message) { }
    public NetworkMonitoringException(string message, Exception innerException) : base(message, innerException) { }
}

// Handle specific error conditions
try
{
    using var reporter = NetworkPerformanceReporter.Create(processId, sessionName, enableUdp: true);
    var data = reporter.GetNetworkPerformanceData();
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Administrator"))
{
    throw new NetworkMonitoringException("Administrator privileges required for network monitoring", ex);
}
catch (UnauthorizedAccessException ex)
{
    throw new NetworkMonitoringException("Access denied to ETW session", ex);
}
catch (Win32Exception ex)
{
    throw new NetworkMonitoringException($"Windows API error: {ex.Message}", ex);
}
```

### Graceful Degradation
```csharp
public class SafeNetworkMonitor
{
    private NetworkPerformanceReporter? _reporter;
    private readonly ILogger _logger;

    public NetworkPerformanceData? GetNetworkData()
    {
        try
        {
            return _reporter?.GetNetworkPerformanceData();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to collect network performance data");
            return null;
        }
    }

    public bool IsMonitoringAvailable()
    {
        try
        {
            return TraceEventSession.IsElevated() == true;
        }
        catch
        {
            return false;
        }
    }
}
```

## Integration Examples

### ASP.NET Core Metrics Endpoint
```csharp
[ApiController]
[Route("api/[controller]")]
public class NetworkMetricsController : ControllerBase
{
    private readonly NetworkPerformanceReporter _reporter;

    [HttpGet("current")]
    public ActionResult<NetworkPerformanceData> GetCurrentMetrics()
    {
        try
        {
            var data = _reporter.GetNetworkPerformanceData();
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to collect network metrics: {ex.Message}");
        }
    }

    [HttpGet("prometheus")]
    public async Task<IActionResult> GetPrometheusMetrics()
    {
        try
        {
            var data = _reporter.GetNetworkPerformanceData();
            var metrics = NetworkMetricsFormatter.ToPrometheus(data);
            return Content(metrics, "text/plain");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to generate metrics: {ex.Message}");
        }
    }
}
```

### Health Check Integration
```csharp
public class NetworkPerformanceHealthCheck : IHealthCheck
{
    private readonly NetworkPerformanceReporter _reporter;
    private readonly NetworkPerformanceOptions _options;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = _reporter.GetNetworkPerformanceData();
            
            var healthData = new Dictionary<string, object>
            {
                ["total_bytes_per_second"] = data.BytesTotal,
                ["tcp_bytes_per_second"] = data.TcpBytesTotal,
                ["udp_bytes_per_second"] = data.UdpBytesTotal
            };

            if (data.BytesTotal > _options.MaxBytesPerSecond)
            {
                return HealthCheckResult.Degraded(
                    $"High network traffic detected: {data.BytesTotal:N0} bytes/sec", 
                    data: healthData);
            }

            return HealthCheckResult.Healthy("Network performance normal", healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Network monitoring failed", ex);
        }
    }
}
```

## Related Components

- **[NetworkPerformanceData](NetworkPerformanceData.md)** - Network performance data model
- **[Network Performance Overview](README.md)** - Complete network monitoring documentation
- **[System Infrastructure](../README.md)** - System-level infrastructure components

## Best Practices

### Session Management
1. **Unique Session Names**: Use GUIDs or timestamps to ensure uniqueness
2. **Session Cleanup**: Properly dispose of ETW sessions to prevent resource leaks
3. **Error Recovery**: Handle session creation failures gracefully
4. **Privilege Checking**: Verify administrative privileges before attempting to create sessions

### Performance Optimization
1. **Selective Monitoring**: Enable UDP only when needed to reduce overhead
2. **Appropriate Intervals**: Balance monitoring frequency with performance impact
3. **Efficient Event Handling**: Minimize processing time in event handlers
4. **Counter Management**: Reset counters regularly to prevent overflow

### Security Considerations
1. **Privilege Requirements**: Clearly document administrative privilege requirements
2. **Access Control**: Restrict access to network monitoring capabilities
3. **Data Sensitivity**: Consider privacy implications of network traffic monitoring
4. **Audit Logging**: Log network monitoring activities for security auditing

## Troubleshooting

### Common Issues

#### Privilege Errors
```bash
# Run application as administrator
# Or use RunAs command
runas /user:Administrator "YourApplication.exe"
```

#### Session Name Conflicts
```csharp
// Always use unique session names
var sessionName = $"NetworkMonitor_{Environment.ProcessId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
```

#### Missing Events
- Verify process ID is correct and process is running
- Check that network activity is actually occurring
- Ensure ETW session is properly initialized

#### Performance Issues
- Reduce monitoring frequency if CPU usage is high
- Disable UDP monitoring if not needed
- Monitor ETW session resource usage

### Diagnostic Tools
```csharp
public static class NetworkMonitorDiagnostics
{
    public static void DiagnoseETWCapability()
    {
        Console.WriteLine($"Is Elevated: {TraceEventSession.IsElevated()}");
        Console.WriteLine($"OS Version: {Environment.OSVersion}");
        Console.WriteLine($"Process ID: {Environment.ProcessId}");
        Console.WriteLine($"Machine Name: {Environment.MachineName}");
    }

    public static void ListActiveETWSessions()
    {
        // Implementation to list active ETW sessions
        // Useful for debugging session conflicts
    }
}
```