# Network Performance Monitoring

## Overview

The Network Performance Monitoring module provides comprehensive real-time network traffic analysis capabilities for Windows applications. Using Event Tracing for Windows (ETW), this module delivers low-overhead, high-precision monitoring of TCP and UDP network traffic on a per-process basis.

## Purpose

- **Real-Time Network Monitoring**: Live network traffic tracking with minimal performance impact
- **Process-Specific Analysis**: Monitor network activity for specific processes or applications
- **Protocol-Level Insights**: Separate tracking of TCP and UDP traffic patterns
- **Performance Optimization**: Identify network bottlenecks and optimization opportunities

## Components

### Core Components

- **[NetworkPerformanceData](NetworkPerformanceData.md)** - Data model for network performance metrics
- **[NetworkPerformanceReporter](NetworkPerformanceReporter.md)** - ETW-based network performance monitoring implementation

## Architecture

### System Architecture
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Application   │───▶│  Network Perf    │───▶│  Performance    │
│   Processes     │    │    Reporter      │    │      Data       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Network I/O   │───▶│   ETW Events     │───▶│   Monitoring    │
│   Operations    │    │   Processing     │    │   Systems       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Data Flow
```
ETW Network Events → Process Filtering → Protocol Separation → Rate Calculation → Performance Data
        ↓                   ↓                    ↓                   ↓                ↓
   TCP/UDP I/O        Process ID         TCP vs UDP          Bytes/Second      Monitoring APIs
```

## Key Features

### Real-Time Monitoring
- **ETW Integration**: Leverages Windows Event Tracing for minimal overhead
- **Live Data Collection**: Real-time network event processing
- **Accurate Timing**: Precise rate calculations using high-resolution timestamps
- **Continuous Operation**: Background monitoring with configurable collection intervals

### Protocol Support
- **TCP Monitoring**: Comprehensive TCP send and receive event tracking
- **UDP Monitoring**: Optional UDP traffic monitoring for real-time applications
- **Bidirectional Analysis**: Separate tracking of inbound and outbound traffic
- **Combined Metrics**: Aggregate views across protocols and directions

### Process Isolation
- **Process-Specific Tracking**: Monitor individual processes or applications
- **Multi-Process Support**: Concurrent monitoring of multiple processes
- **Resource Attribution**: Accurate attribution of network usage to specific processes
- **Isolation Guarantees**: Process traffic isolation prevents cross-contamination

## Getting Started

### Basic Setup
```csharp
// Monitor current process network traffic
var processId = Environment.ProcessId;
var sessionName = $"NetworkMonitor_{processId}";

using var reporter = NetworkPerformanceReporter.Create(
    processId: processId,
    sessionName: sessionName);

// Collect metrics every 30 seconds
while (true)
{
    await Task.Delay(TimeSpan.FromSeconds(30));
    
    var data = reporter.GetNetworkPerformanceData();
    Console.WriteLine($"Network Traffic: {data.BytesTotal:N0} bytes/second");
}
```

### Advanced Configuration
```csharp
// Comprehensive monitoring with UDP support
using var cancellationTokenSource = new CancellationTokenSource();

var reporter = NetworkPerformanceReporter.Create(
    processId: Environment.ProcessId,
    sessionName: $"AdvancedNetworkMonitor_{Guid.NewGuid():N}",
    enableUdp: true,  // Enable UDP monitoring
    cancellationToken: cancellationTokenSource.Token);

var data = reporter.GetNetworkPerformanceData();

Console.WriteLine($"TCP Traffic: {data.TcpBytesTotal:N0} B/s");
Console.WriteLine($"UDP Traffic: {data.UdpBytesTotal:N0} B/s");
Console.WriteLine($"Total Traffic: {data.BytesTotal:N0} B/s");
```

### ASP.NET Core Integration
```csharp
// Startup.cs / Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register network monitoring as singleton
    services.AddSingleton<NetworkPerformanceReporter>(provider =>
    {
        var processId = Environment.ProcessId;
        var sessionName = $"WebApp_NetworkMonitor_{processId}";
        
        return NetworkPerformanceReporter.Create(
            processId: processId,
            sessionName: sessionName,
            enableUdp: false); // Web apps typically use TCP
    });

    // Register background monitoring service
    services.AddHostedService<NetworkMonitoringBackgroundService>();
    
    // Add health checks
    services.AddHealthChecks()
        .AddCheck<NetworkPerformanceHealthCheck>("network_performance");
}

// Configure monitoring endpoint
public void Configure(IApplicationBuilder app)
{
    app.UseHealthChecks("/health");
    
    // Custom metrics endpoint
    app.Map("/metrics/network", builder =>
    {
        builder.Run(async context =>
        {
            var reporter = context.RequestServices.GetRequiredService<NetworkPerformanceReporter>();
            var data = reporter.GetNetworkPerformanceData();
            
            await context.Response.WriteAsJsonAsync(data);
        });
    });
}
```

## Usage Patterns

### Background Monitoring Service
```csharp
public class NetworkMonitoringBackgroundService : BackgroundService
{
    private readonly NetworkPerformanceReporter _reporter;
    private readonly ILogger<NetworkMonitoringBackgroundService> _logger;
    private readonly IMetricsLogger _metricsLogger;

    public NetworkMonitoringBackgroundService(
        NetworkPerformanceReporter reporter,
        ILogger<NetworkMonitoringBackgroundService> logger,
        IMetricsLogger metricsLogger)
    {
        _reporter = reporter;
        _logger = logger;
        _metricsLogger = metricsLogger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var data = _reporter.GetNetworkPerformanceData();
                
                // Log metrics
                _logger.LogInformation("Network Performance - Total: {TotalBytes:N0} B/s, TCP: {TcpBytes:N0} B/s, UDP: {UdpBytes:N0} B/s",
                    data.BytesTotal, data.TcpBytesTotal, data.UdpBytesTotal);

                // Send to metrics system
                await _metricsLogger.GaugeAsync("network.tcp.bytes_received", data.TcpBytesReceived);
                await _metricsLogger.GaugeAsync("network.tcp.bytes_sent", data.TcpBytesSent);
                await _metricsLogger.GaugeAsync("network.udp.bytes_received", data.UdpBytesReceived);
                await _metricsLogger.GaugeAsync("network.udp.bytes_sent", data.UdpBytesSent);
                await _metricsLogger.GaugeAsync("network.total.bytes", data.BytesTotal);

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Network monitoring iteration failed");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
```

### Multi-Process Monitoring
```csharp
public class MultiProcessNetworkMonitor : IDisposable
{
    private readonly Dictionary<int, NetworkPerformanceReporter> _reporters = new();
    private readonly Dictionary<int, string> _processNames = new();
    private readonly Timer _collectionTimer;
    private readonly ILogger _logger;

    public MultiProcessNetworkMonitor(IEnumerable<int> processIds, ILogger logger)
    {
        _logger = logger;

        foreach (var processId in processIds)
        {
            try
            {
                var processName = Process.GetProcessById(processId).ProcessName;
                var sessionName = $"MultiNetworkMonitor_{processId}_{Guid.NewGuid():N}";
                
                var reporter = NetworkPerformanceReporter.Create(
                    processId: processId,
                    sessionName: sessionName,
                    enableUdp: true);
                
                _reporters[processId] = reporter;
                _processNames[processId] = processName;
                
                _logger.LogInformation("Started monitoring process {ProcessName} (PID: {ProcessId})", processName, processId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to start monitoring for process {ProcessId}", processId);
            }
        }

        _collectionTimer = new Timer(CollectAllNetworkData, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void CollectAllNetworkData(object? state)
    {
        foreach (var (processId, reporter) in _reporters.ToArray())
        {
            try
            {
                var data = reporter.GetNetworkPerformanceData();
                var processName = _processNames[processId];
                
                _logger.LogInformation("Process {ProcessName} (PID: {ProcessId}) - Network: {TotalBytes:N0} B/s (TCP: {TcpBytes:N0}, UDP: {UdpBytes:N0})",
                    processName, processId, data.BytesTotal, data.TcpBytesTotal, data.UdpBytesTotal);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to collect network data for process {ProcessId}", processId);
                
                // Remove failed reporters
                if (_reporters.Remove(processId))
                {
                    _processNames.Remove(processId);
                    reporter.Dispose();
                }
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
        _processNames.Clear();
    }
}
```

### Performance Analytics
```csharp
public class NetworkPerformanceAnalytics
{
    private readonly NetworkPerformanceReporter _reporter;
    private readonly List<NetworkPerformanceData> _history = new();
    private readonly object _lock = new();

    public NetworkPerformanceAnalytics(NetworkPerformanceReporter reporter)
    {
        _reporter = reporter;
    }

    public NetworkAnalysisResult AnalyzeCurrentPerformance()
    {
        var currentData = _reporter.GetNetworkPerformanceData();
        
        lock (_lock)
        {
            _history.Add(currentData);
            
            // Keep only last 100 measurements
            if (_history.Count > 100)
            {
                _history.RemoveAt(0);
            }
        }

        return new NetworkAnalysisResult
        {
            Current = currentData,
            AverageTraffic = CalculateAverage(),
            PeakTraffic = CalculatePeak(),
            TrendAnalysis = AnalyzeTrend(),
            ProtocolDistribution = AnalyzeProtocolDistribution(currentData),
            TrafficPattern = ClassifyTrafficPattern(currentData)
        };
    }

    private NetworkPerformanceData CalculateAverage()
    {
        lock (_lock)
        {
            if (_history.Count == 0) return new NetworkPerformanceData();

            return new NetworkPerformanceData
            {
                TcpBytesReceived = (long)_history.Average(d => d.TcpBytesReceived),
                TcpBytesSent = (long)_history.Average(d => d.TcpBytesSent),
                UdpBytesReceived = (long)_history.Average(d => d.UdpBytesReceived),
                UdpBytesSent = (long)_history.Average(d => d.UdpBytesSent)
            };
        }
    }

    private TrafficPattern ClassifyTrafficPattern(NetworkPerformanceData data)
    {
        var tcpPercentage = data.BytesTotal > 0 ? (double)data.TcpBytesTotal / data.BytesTotal : 0;
        var receivePercentage = data.BytesTotal > 0 ? (double)data.BytesReceived / data.BytesTotal : 0;

        return (tcpPercentage, receivePercentage) switch
        {
            (> 0.9, > 0.7) => TrafficPattern.WebServerInbound,
            (> 0.9, < 0.3) => TrafficPattern.WebClientOutbound,
            (< 0.5, _) => TrafficPattern.RealTimeUDP,
            (_, var r) when Math.Abs(r - 0.5) < 0.1 => TrafficPattern.BidirectionalBalanced,
            _ => TrafficPattern.Mixed
        };
    }
}

public enum TrafficPattern
{
    WebServerInbound,
    WebClientOutbound,
    RealTimeUDP,
    BidirectionalBalanced,
    Mixed
}
```

## Configuration Examples

### Application Configuration
```json
{
  "NetworkMonitoring": {
    "Enabled": true,
    "EnableUDP": false,
    "CollectionIntervalSeconds": 30,
    "SessionNamePrefix": "MyApp_NetworkMonitor",
    "Thresholds": {
      "WarningBytesPerSecond": 10485760,
      "CriticalBytesPerSecond": 52428800
    },
    "ProcessFiltering": {
      "MonitorChildProcesses": true,
      "ExcludeSystemProcesses": true
    }
  }
}
```

### Dependency Injection Setup
```csharp
public static class NetworkMonitoringServiceExtensions
{
    public static IServiceCollection AddNetworkMonitoring(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var config = configuration.GetSection("NetworkMonitoring").Get<NetworkMonitoringOptions>();
        
        if (config?.Enabled == true)
        {
            services.AddSingleton<NetworkPerformanceReporter>(provider =>
            {
                var processId = Environment.ProcessId;
                var sessionName = $"{config.SessionNamePrefix}_{processId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                
                return NetworkPerformanceReporter.Create(
                    processId: processId,
                    sessionName: sessionName,
                    enableUdp: config.EnableUDP);
            });

            services.AddSingleton<NetworkPerformanceAnalytics>();
            services.AddHostedService<NetworkMonitoringBackgroundService>();
            
            services.AddHealthChecks()
                .AddCheck<NetworkPerformanceHealthCheck>("network_performance");
        }

        return services;
    }
}

public class NetworkMonitoringOptions
{
    public bool Enabled { get; set; } = true;
    public bool EnableUDP { get; set; } = false;
    public int CollectionIntervalSeconds { get; set; } = 30;
    public string SessionNamePrefix { get; set; } = "NetworkMonitor";
    public NetworkThresholds Thresholds { get; set; } = new();
}

public class NetworkThresholds
{
    public long WarningBytesPerSecond { get; set; } = 10_000_000; // 10 MB/s
    public long CriticalBytesPerSecond { get; set; } = 50_000_000; // 50 MB/s
}
```

## Monitoring Integration

### Prometheus Metrics Export
```csharp
public class NetworkPrometheusExporter
{
    private readonly NetworkPerformanceReporter _reporter;

    public async Task WriteMetrics(HttpContext context)
    {
        var data = _reporter.GetNetworkPerformanceData();
        var metrics = new StringBuilder();

        // Basic network metrics
        metrics.AppendLine($"# HELP network_tcp_bytes_received_per_second TCP bytes received per second");
        metrics.AppendLine($"# TYPE network_tcp_bytes_received_per_second gauge");
        metrics.AppendLine($"network_tcp_bytes_received_per_second {data.TcpBytesReceived}");

        metrics.AppendLine($"# HELP network_tcp_bytes_sent_per_second TCP bytes sent per second");
        metrics.AppendLine($"# TYPE network_tcp_bytes_sent_per_second gauge");
        metrics.AppendLine($"network_tcp_bytes_sent_per_second {data.TcpBytesSent}");

        metrics.AppendLine($"# HELP network_udp_bytes_received_per_second UDP bytes received per second");
        metrics.AppendLine($"# TYPE network_udp_bytes_received_per_second gauge");
        metrics.AppendLine($"network_udp_bytes_received_per_second {data.UdpBytesReceived}");

        metrics.AppendLine($"# HELP network_udp_bytes_sent_per_second UDP bytes sent per second");
        metrics.AppendLine($"# TYPE network_udp_bytes_sent_per_second gauge");
        metrics.AppendLine($"network_udp_bytes_sent_per_second {data.UdpBytesSent}");

        metrics.AppendLine($"# HELP network_total_bytes_per_second Total network bytes per second");
        metrics.AppendLine($"# TYPE network_total_bytes_per_second gauge");
        metrics.AppendLine($"network_total_bytes_per_second {data.BytesTotal}");

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(metrics.ToString());
    }
}
```

### Application Insights Integration
```csharp
public class NetworkTelemetryCollector
{
    private readonly NetworkPerformanceReporter _reporter;
    private readonly TelemetryClient _telemetryClient;
    private readonly Timer _collectionTimer;

    public NetworkTelemetryCollector(NetworkPerformanceReporter reporter, TelemetryClient telemetryClient)
    {
        _reporter = reporter;
        _telemetryClient = telemetryClient;
        _collectionTimer = new Timer(CollectTelemetry, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    }

    private void CollectTelemetry(object? state)
    {
        try
        {
            var data = _reporter.GetNetworkPerformanceData();

            // Send individual metrics
            _telemetryClient.TrackMetric("Network.TCP.BytesReceived", data.TcpBytesReceived);
            _telemetryClient.TrackMetric("Network.TCP.BytesSent", data.TcpBytesSent);
            _telemetryClient.TrackMetric("Network.UDP.BytesReceived", data.UdpBytesReceived);
            _telemetryClient.TrackMetric("Network.UDP.BytesSent", data.UdpBytesSent);
            _telemetryClient.TrackMetric("Network.Total.Bytes", data.BytesTotal);

            // Send custom event with all data
            _telemetryClient.TrackEvent("NetworkPerformanceSnapshot", new Dictionary<string, string>
            {
                ["TcpBytesTotal"] = data.TcpBytesTotal.ToString(),
                ["UdpBytesTotal"] = data.UdpBytesTotal.ToString(),
                ["BytesTotal"] = data.BytesTotal.ToString(),
                ["ProcessId"] = Environment.ProcessId.ToString(),
                ["MachineName"] = Environment.MachineName
            });
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
        }
    }
}
```

## Security and Performance

### Security Considerations
1. **Administrative Privileges**: ETW monitoring requires elevated privileges
2. **Data Sensitivity**: Network traffic data may contain sensitive information
3. **Access Control**: Restrict access to network monitoring capabilities
4. **Audit Requirements**: Log network monitoring activities for compliance

### Performance Optimization
1. **Selective Monitoring**: Enable UDP only when necessary
2. **Efficient Event Processing**: Minimize processing time in ETW event handlers
3. **Memory Management**: Regular counter resets to prevent overflow
4. **Resource Monitoring**: Monitor the monitoring overhead itself

### Best Practices
1. **Session Management**: Use unique session names to avoid conflicts
2. **Error Handling**: Implement graceful degradation when monitoring fails
3. **Resource Cleanup**: Properly dispose of ETW sessions and resources
4. **Configuration Validation**: Validate configuration before starting monitoring

## Troubleshooting

### Common Issues

#### Administrative Privilege Errors
```csharp
// Check privileges before attempting to create reporter
if (!(TraceEventSession.IsElevated() ?? false))
{
    throw new InvalidOperationException("Network monitoring requires administrator privileges");
}
```

#### Session Name Conflicts
```csharp
// Generate globally unique session names
public static string GenerateUniqueSessionName(string prefix)
{
    return $"{prefix}_{Environment.MachineName}_{Environment.ProcessId}_{DateTime.UtcNow:yyyyMMddHHmmssff}_{Guid.NewGuid():N}";
}
```

#### Process Lifecycle Issues
```csharp
// Handle process termination gracefully
public class ProcessLifecycleAwareMonitor
{
    private NetworkPerformanceReporter? _reporter;
    private readonly int _processId;

    public NetworkPerformanceData? GetNetworkData()
    {
        try
        {
            // Check if process is still running
            using var process = Process.GetProcessById(_processId);
            if (process.HasExited)
            {
                _reporter?.Dispose();
                _reporter = null;
                return null;
            }

            return _reporter?.GetNetworkPerformanceData();
        }
        catch (ArgumentException)
        {
            // Process no longer exists
            _reporter?.Dispose();
            _reporter = null;
            return null;
        }
    }
}
```

### Diagnostic Utilities
```csharp
public static class NetworkMonitoringDiagnostics
{
    public static void RunDiagnostics()
    {
        Console.WriteLine("Network Monitoring Diagnostics");
        Console.WriteLine("==============================");
        Console.WriteLine($"OS Version: {Environment.OSVersion}");
        Console.WriteLine($"Is Elevated: {TraceEventSession.IsElevated()}");
        Console.WriteLine($"Current Process ID: {Environment.ProcessId}");
        Console.WriteLine($"Machine Name: {Environment.MachineName}");
        Console.WriteLine($".NET Version: {Environment.Version}");
        
        try
        {
            var testSessionName = $"DiagnosticTest_{Guid.NewGuid():N}";
            using var testSession = new TraceEventSession(testSessionName);
            Console.WriteLine($"ETW Session Creation: SUCCESS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ETW Session Creation: FAILED - {ex.Message}");
        }
    }
}
```

## Future Enhancements

### Planned Features
- **Additional Protocols**: ICMP and other protocol support
- **Network Interface Monitoring**: Per-interface traffic analysis
- **Quality of Service Metrics**: Latency and packet loss monitoring
- **Historical Analytics**: Long-term trend analysis and storage

### Integration Roadmap
- **Cloud Monitoring**: Azure Monitor and AWS CloudWatch integration
- **Container Support**: Docker and Kubernetes network monitoring
- **Microservices**: Service mesh network monitoring integration
- **Machine Learning**: Anomaly detection and predictive analytics

## Related Documentation

- **[System Infrastructure](../README.md)** - System-level infrastructure components
- **[Health Checks](../../HealthChecks/README.md)** - Infrastructure health monitoring
- **[Infrastructure Overview](../../README.md)** - Complete infrastructure documentation