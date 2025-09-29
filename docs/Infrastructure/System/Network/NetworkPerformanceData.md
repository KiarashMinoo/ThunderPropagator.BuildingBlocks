# NetworkPerformanceData

## Overview

The `NetworkPerformanceData` class provides a comprehensive data model for network performance metrics, specifically designed to capture and organize TCP and UDP traffic statistics. This class serves as the primary data transfer object for network monitoring and performance analysis in RapidStreamer applications.

## Purpose

- **Network Metrics Collection**: Structured storage of network performance data
- **Protocol Separation**: Distinct tracking of TCP and UDP traffic
- **Traffic Analysis**: Bidirectional traffic monitoring (sent/received)
- **Performance Monitoring**: Real-time network performance assessment

## Class Declaration

```csharp
public sealed class NetworkPerformanceData
{
    // TCP Traffic Properties
    public long TcpBytesReceived { get; set; }
    public long TcpBytesSent { get; set; }
    public long TcpBytesTotal => TcpBytesReceived + TcpBytesSent;

    // UDP Traffic Properties  
    public long UdpBytesReceived { get; set; }
    public long UdpBytesSent { get; set; }
    public long UdpBytesTotal => UdpBytesReceived + UdpBytesSent;

    // Combined Traffic Properties
    public long BytesReceived => TcpBytesReceived + UdpBytesReceived;
    public long BytesSent => TcpBytesSent + UdpBytesSent;
    public long BytesTotal => BytesReceived + BytesSent;
}
```

## Properties

### TCP Traffic Metrics

#### TcpBytesReceived
- **Type**: `long`
- **Description**: Number of bytes received via TCP protocol
- **Unit**: Bytes per second (when collected by NetworkPerformanceReporter)
- **Range**: 0 to Long.MaxValue

#### TcpBytesSent
- **Type**: `long`
- **Description**: Number of bytes sent via TCP protocol  
- **Unit**: Bytes per second (when collected by NetworkPerformanceReporter)
- **Range**: 0 to Long.MaxValue

#### TcpBytesTotal
- **Type**: `long` (computed property)
- **Description**: Total TCP traffic (sent + received)
- **Formula**: `TcpBytesReceived + TcpBytesSent`
- **Read-Only**: Calculated automatically

### UDP Traffic Metrics

#### UdpBytesReceived
- **Type**: `long`
- **Description**: Number of bytes received via UDP protocol
- **Unit**: Bytes per second (when collected by NetworkPerformanceReporter)
- **Range**: 0 to Long.MaxValue

#### UdpBytesSent
- **Type**: `long`
- **Description**: Number of bytes sent via UDP protocol
- **Unit**: Bytes per second (when collected by NetworkPerformanceReporter)
- **Range**: 0 to Long.MaxValue

#### UdpBytesTotal
- **Type**: `long` (computed property)
- **Description**: Total UDP traffic (sent + received)
- **Formula**: `UdpBytesReceived + UdpBytesSent`
- **Read-Only**: Calculated automatically

### Combined Traffic Metrics

#### BytesReceived
- **Type**: `long` (computed property)
- **Description**: Total bytes received across all protocols
- **Formula**: `TcpBytesReceived + UdpBytesReceived`
- **Read-Only**: Calculated automatically

#### BytesSent
- **Type**: `long` (computed property)
- **Description**: Total bytes sent across all protocols
- **Formula**: `TcpBytesSent + UdpBytesSent`
- **Read-Only**: Calculated automatically

#### BytesTotal
- **Type**: `long` (computed property)
- **Description**: Total network traffic across all protocols and directions
- **Formula**: `BytesReceived + BytesSent`
- **Read-Only**: Calculated automatically

## Usage Examples

### Basic Data Access
```csharp
// Get network performance data from reporter
var performanceData = networkReporter.GetNetworkPerformanceData();

// Access individual metrics
Console.WriteLine($"TCP Received: {performanceData.TcpBytesReceived:N0} bytes/sec");
Console.WriteLine($"TCP Sent: {performanceData.TcpBytesSent:N0} bytes/sec");
Console.WriteLine($"UDP Received: {performanceData.UdpBytesReceived:N0} bytes/sec");
Console.WriteLine($"UDP Sent: {performanceData.UdpBytesSent:N0} bytes/sec");

// Access calculated totals
Console.WriteLine($"Total TCP Traffic: {performanceData.TcpBytesTotal:N0} bytes/sec");
Console.WriteLine($"Total UDP Traffic: {performanceData.UdpBytesTotal:N0} bytes/sec");
Console.WriteLine($"Total Network Traffic: {performanceData.BytesTotal:N0} bytes/sec");
```

### Performance Analysis
```csharp
public class NetworkAnalyzer
{
    public NetworkAnalysisResult AnalyzePerformance(NetworkPerformanceData data)
    {
        return new NetworkAnalysisResult
        {
            // Protocol distribution
            TcpPercentage = (double)data.TcpBytesTotal / data.BytesTotal * 100,
            UdpPercentage = (double)data.UdpBytesTotal / data.BytesTotal * 100,
            
            // Traffic direction analysis
            InboundPercentage = (double)data.BytesReceived / data.BytesTotal * 100,
            OutboundPercentage = (double)data.BytesSent / data.BytesTotal * 100,
            
            // Performance indicators
            IsHighTraffic = data.BytesTotal > 1_000_000, // 1MB/sec threshold
            IsTcpDominant = data.TcpBytesTotal > data.UdpBytesTotal,
            IsBalanced = Math.Abs(data.BytesReceived - data.BytesSent) < data.BytesTotal * 0.1
        };
    }
}
```

### Monitoring Dashboard
```csharp
public class NetworkMonitoringService : BackgroundService
{
    private readonly NetworkPerformanceReporter _reporter;
    private readonly ILogger<NetworkMonitoringService> _logger;
    private readonly IMetricsCollector _metrics;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var data = _reporter.GetNetworkPerformanceData();
            
            // Log performance metrics
            _logger.LogInformation("Network Performance - TCP: {TcpTotal:N0} B/s, UDP: {UdpTotal:N0} B/s, Total: {Total:N0} B/s",
                data.TcpBytesTotal, data.UdpBytesTotal, data.BytesTotal);
            
            // Send to metrics system
            await _metrics.GaugeAsync("network.tcp.bytes_received", data.TcpBytesReceived);
            await _metrics.GaugeAsync("network.tcp.bytes_sent", data.TcpBytesSent);
            await _metrics.GaugeAsync("network.udp.bytes_received", data.UdpBytesReceived);
            await _metrics.GaugeAsync("network.udp.bytes_sent", data.UdpBytesSent);
            await _metrics.GaugeAsync("network.total.bytes", data.BytesTotal);
            
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

## Data Interpretation

### Traffic Patterns

#### High TCP Traffic
```csharp
if (data.TcpBytesTotal > data.UdpBytesTotal * 10)
{
    // Indicates web traffic, database connections, file transfers
    Console.WriteLine("Application appears to be TCP-heavy (web services, databases)");
}
```

#### High UDP Traffic
```csharp
if (data.UdpBytesTotal > data.TcpBytesTotal)
{
    // Indicates streaming, gaming, DNS, or real-time communication
    Console.WriteLine("Application appears to be UDP-heavy (streaming, real-time)");
}
```

#### Balanced Traffic
```csharp
var receiveSendRatio = (double)data.BytesReceived / data.BytesSent;
if (receiveSendRatio >= 0.8 && receiveSendRatio <= 1.2)
{
    Console.WriteLine("Balanced bidirectional traffic pattern");
}
```

### Performance Thresholds
```csharp
public static class NetworkThresholds
{
    public const long LowTraffic = 100_000;      // 100 KB/s
    public const long ModerateTraffic = 1_000_000;   // 1 MB/s
    public const long HighTraffic = 10_000_000;      // 10 MB/s
    public const long VeryHighTraffic = 100_000_000; // 100 MB/s

    public static string GetTrafficLevel(NetworkPerformanceData data)
    {
        return data.BytesTotal switch
        {
            < LowTraffic => "Low",
            < ModerateTraffic => "Moderate", 
            < HighTraffic => "High",
            < VeryHighTraffic => "Very High",
            _ => "Extreme"
        };
    }
}
```

## Integration Examples

### ASP.NET Core Health Checks
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
                ["tcp_bytes_total"] = data.TcpBytesTotal,
                ["udp_bytes_total"] = data.UdpBytesTotal,
                ["total_bytes"] = data.BytesTotal,
                ["traffic_level"] = NetworkThresholds.GetTrafficLevel(data)
            };

            // Check if traffic exceeds configured thresholds
            if (data.BytesTotal > _options.MaxBytesPerSecond)
            {
                return HealthCheckResult.Degraded($"High network traffic: {data.BytesTotal:N0} bytes/sec", data: healthData);
            }

            return HealthCheckResult.Healthy("Network performance within normal ranges", healthData);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to collect network performance data", ex);
        }
    }
}
```

### Metrics Export
```csharp
public class NetworkMetricsExporter
{
    public async Task ExportToPrometheus(NetworkPerformanceData data, HttpContext context)
    {
        var metrics = new StringBuilder();
        
        // TCP metrics
        metrics.AppendLine($"network_tcp_bytes_received {data.TcpBytesReceived}");
        metrics.AppendLine($"network_tcp_bytes_sent {data.TcpBytesSent}");
        metrics.AppendLine($"network_tcp_bytes_total {data.TcpBytesTotal}");
        
        // UDP metrics  
        metrics.AppendLine($"network_udp_bytes_received {data.UdpBytesReceived}");
        metrics.AppendLine($"network_udp_bytes_sent {data.UdpBytesSent}");
        metrics.AppendLine($"network_udp_bytes_total {data.UdpBytesTotal}");
        
        // Combined metrics
        metrics.AppendLine($"network_bytes_received {data.BytesReceived}");
        metrics.AppendLine($"network_bytes_sent {data.BytesSent}");
        metrics.AppendLine($"network_bytes_total {data.BytesTotal}");

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(metrics.ToString());
    }
}
```

## Performance Considerations

### Memory Efficiency
- **Value Type Properties**: All properties are value types for minimal memory overhead
- **Computed Properties**: Calculated properties avoid storing redundant data
- **No Collections**: Simple flat structure for high-performance scenarios

### Thread Safety
- **Immutable After Creation**: Properties should be set once after collection
- **Read-Only Calculations**: Computed properties are thread-safe for reading
- **Snapshot Pattern**: Represents a point-in-time snapshot of network performance

### Precision and Accuracy
- **Long Type**: Sufficient range for high-traffic scenarios (up to 9.2 exabytes)
- **Bytes Per Second**: When used with NetworkPerformanceReporter, values represent rate
- **Time-Based Calculations**: Accuracy depends on measurement interval

## Related Components

- **[NetworkPerformanceReporter](NetworkPerformanceReporter.md)** - ETW-based network performance collection
- **[Network Performance Overview](README.md)** - Complete network monitoring documentation
- **[System Infrastructure](../README.md)** - System-level infrastructure components

## Best Practices

### Data Collection
1. **Regular Intervals**: Collect data at consistent intervals for accurate trends
2. **Appropriate Frequency**: Balance monitoring overhead with data granularity
3. **Data Validation**: Verify data ranges and handle edge cases
4. **Error Handling**: Gracefully handle collection failures

### Performance Monitoring
1. **Baseline Establishment**: Establish normal traffic patterns for comparison
2. **Threshold Configuration**: Set appropriate alerts based on application requirements
3. **Trend Analysis**: Monitor changes over time rather than absolute values
4. **Context Awareness**: Consider application activity when interpreting metrics

### Integration Patterns
1. **Logging Integration**: Include network metrics in structured logs
2. **Metrics Systems**: Export to monitoring platforms like Prometheus
3. **Alerting**: Configure alerts for abnormal traffic patterns
4. **Dashboard Visualization**: Create real-time dashboards for operations teams

## Troubleshooting

### Common Issues
1. **Zero Values**: May indicate ETW session not running or insufficient privileges
2. **Negative Calculations**: Check for counter resets or overflow conditions
3. **Inconsistent Data**: Verify measurement intervals and timing accuracy
4. **Performance Impact**: Monitor the overhead of data collection itself

### Validation Techniques
```csharp
public static class NetworkDataValidator
{
    public static bool IsValid(NetworkPerformanceData data)
    {
        // Check for negative values
        if (data.TcpBytesReceived < 0 || data.TcpBytesSent < 0 ||
            data.UdpBytesReceived < 0 || data.UdpBytesSent < 0)
            return false;

        // Verify computed properties
        if (data.TcpBytesTotal != data.TcpBytesReceived + data.TcpBytesSent)
            return false;

        if (data.BytesTotal != data.TcpBytesTotal + data.UdpBytesTotal)
            return false;

        return true;
    }
}
```