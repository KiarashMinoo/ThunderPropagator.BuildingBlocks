# Infrastructure Building Blocks

## Overview

The Infrastructure module provides essential infrastructure-level building blocks for robust, scalable applications. These components handle cross-cutting concerns such as health monitoring, system resource management, and operational infrastructure that applications depend on.

## Purpose

- **Infrastructure Monitoring**: Comprehensive health checks and system monitoring
- **Operational Excellence**: Tools for maintaining application reliability
- **System Integration**: Components for system-level operations
- **Performance Monitoring**: Resource utilization and performance tracking

## Components

### Health Checks
Comprehensive health monitoring capabilities for critical infrastructure dependencies:

**[Health Checks](HealthChecks/README.md)** - Complete health monitoring solution
- **[ActiveMQHealthCheckOptions](HealthChecks/ActiveMQHealthCheckOptions.md)** - Configuration for ActiveMQ health monitoring
- **[ActiveMQHealthCheck](HealthChecks/ActiveMQHealthCheck.md)** - ActiveMQ broker health check implementation
- **[ActiveMQHealthCheckExtensions](HealthChecks/ActiveMQHealthCheckExtensions.md)** - Registration and dependency injection helpers

### System Resource Monitoring
System-level monitoring and resource management capabilities:

**[System Components](System/README.md)** - System-level monitoring and performance analysis
- **[Network Performance Monitoring](System/Network/README.md)** - Real-time network traffic monitoring
  - **[NetworkPerformanceData](System/Network/NetworkPerformanceData.md)** - Network performance metrics data model
  - **[NetworkPerformanceReporter](System/Network/NetworkPerformanceReporter.md)** - ETW-based network monitoring implementation
- **[System Resource Monitor](SystemResourceMonitor/README.md)** - Monitor CPU, memory, disk, and network resources
- **Performance Analytics** - Resource threshold monitoring and alerting
- **Trend Analysis** - Performance baseline establishment and usage patterns

## Features

### Health Monitoring
- **Multi-Component Support**: Monitor databases, message brokers, web services, and more
- **Custom Health Checks**: Extensible framework for custom health monitoring
- **Integration Ready**: Built-in support for ASP.NET Core health check endpoints
- **Alerting Integration**: Connect to monitoring and alerting systems

### System Resource Management
- **Performance Monitoring**: Track CPU, memory, and disk utilization
- **Threshold Management**: Configure alerts for resource limits
- **Trend Analysis**: Historical resource usage patterns
- **Capacity Planning**: Data for infrastructure scaling decisions

### Operational Excellence
- **Early Warning Systems**: Detect issues before they impact users
- **Comprehensive Logging**: Detailed operational logs and metrics
- **Configuration Management**: Environment-specific infrastructure settings
- **Security Monitoring**: Track security-related infrastructure events

## Architecture

### Health Check Framework
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Application   │───▶│   Health Check   │───▶│   Monitoring    │
│   Components    │    │    Framework     │    │    Systems      │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Infrastructure│    │   Health Check   │    │    Alerting     │
│   Dependencies  │    │    Results       │    │    Systems      │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### System Resource Monitor
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   System APIs   │───▶│   Resource       │───▶│   Metrics       │
│   & Counters    │    │   Collectors     │    │   Storage       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Raw System    │    │   Processed      │    │   Dashboards &  │
│   Metrics       │    │   Metrics        │    │   Reports       │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

### Network Performance Monitoring
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   ETW Network   │───▶│  Network Perf    │───▶│  Performance    │
│    Events       │    │   Reporter       │    │     Data        │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│ Process-Specific│    │ Protocol-Level   │    │   Monitoring    │
│ Network Traffic │    │  Analysis        │    │   Integration   │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## Getting Started

### Health Check Setup
```csharp
// Basic health checks setup
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://localhost:61616",
            Queue = "health.check"
        });
}

public void Configure(IApplicationBuilder app)
{
    app.UseHealthChecks("/health");
}
```

### Advanced Health Monitoring
```csharp
// Comprehensive health monitoring
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        // ActiveMQ monitoring
        .AddActiveMQHealthCheck(
            Configuration.GetSection("ActiveMQ").Get<ActiveMQHealthCheckOptions>(),
            name: "Message Broker",
            tags: new[] { "messaging", "critical" })
        
        // Database monitoring (future)
        .AddSqlServer(Configuration.GetConnectionString("DefaultConnection"))
        
        // Redis monitoring (future)
        .AddRedis(Configuration.GetConnectionString("Redis"));

    // Custom health check endpoints
    app.UseHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    
    app.UseHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });
}
```

### System Resource Monitoring
```csharp
// System resource monitoring setup
public void ConfigureServices(IServiceCollection services)
{
    // Network performance monitoring
    services.AddSingleton<NetworkPerformanceReporter>(provider =>
    {
        var processId = Environment.ProcessId;
        var sessionName = $"WebApp_NetworkMonitor_{processId}";
        
        return NetworkPerformanceReporter.Create(
            processId: processId,
            sessionName: sessionName,
            enableUdp: false); // Web applications typically use TCP
    });

    // System resource monitoring
    services.AddSystemResourceMonitor(options =>
    {
        options.EnableCpuMonitoring = true;
        options.EnableMemoryMonitoring = true;
        options.EnableDiskMonitoring = true;
        options.EnableNetworkMonitoring = true;
        options.CollectionInterval = TimeSpan.FromSeconds(30);
    });
}
```

## Usage Patterns

### Production Health Monitoring
```csharp
// Production-ready health monitoring setup
services.AddHealthChecks()
    .AddActiveMQHealthCheck(
        new ActiveMQHealthCheckOptions
        {
            BrokerUri = Configuration["ActiveMQ:ProductionUri"],
            ClientId = Environment.MachineName + "-health",
            UserName = Configuration["ActiveMQ:MonitoringUser"],
            Password = Configuration["ActiveMQ:MonitoringPassword"],
            Queue = "production.health"
        },
        name: "Production Message Broker",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "messaging", "production", "critical" },
        timeout: TimeSpan.FromSeconds(30)
    );

// Kubernetes-style endpoints
app.UseHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.UseHealthChecks("/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
```

### Multi-Environment Configuration
```csharp
// Environment-specific infrastructure setup
public void ConfigureServices(IServiceCollection services)
{
    if (Environment.IsDevelopment())
    {
        services.AddHealthChecks()
            .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
            {
                BrokerUri = "tcp://localhost:61616",
                Queue = "dev.health"
            }, "Development ActiveMQ");
    }
    else if (Environment.IsStaging())
    {
        services.AddHealthChecks()
            .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
            {
                BrokerUri = "tcp://staging-activemq.company.com:61616",
                ClientId = "staging-health-checker",
                UserName = Configuration["ActiveMQ:StagingUser"],
                Password = Configuration["ActiveMQ:StagingPassword"],
                Queue = "staging.health"
            }, "Staging ActiveMQ");
    }
    else // Production
    {
        services.AddHealthChecks()
            .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
            {
                BrokerUri = Configuration["ActiveMQ:ProductionUri"],
                ClientId = $"{Environment.MachineName}-health",
                UserName = Configuration["ActiveMQ:ProductionUser"],
                Password = Configuration["ActiveMQ:ProductionPassword"],
                Queue = "production.health"
            },
            name: "Production ActiveMQ",
            failureStatus: HealthStatus.Unhealthy,
            timeout: TimeSpan.FromMinutes(1));
    }
}
```

### Monitoring Integration
```csharp
// Integration with monitoring systems
public class HealthMonitoringBackgroundService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IMetricsLogger _metricsLogger;
    private readonly IAlertingService _alertingService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var report = await _healthCheckService.CheckHealthAsync(stoppingToken);
            
            // Log metrics
            await _metricsLogger.LogHealthMetrics(report);
            
            // Send alerts for failures
            foreach (var check in report.Entries.Where(x => x.Value.Status != HealthStatus.Healthy))
            {
                await _alertingService.SendAlert(new HealthCheckAlert
                {
                    CheckName = check.Key,
                    Status = check.Value.Status,
                    Exception = check.Value.Exception,
                    Duration = check.Value.Duration,
                    Timestamp = DateTime.UtcNow
                });
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Configuration Examples

### Complete Configuration File
```json
{
  "Infrastructure": {
    "HealthChecks": {
      "ActiveMQ": {
        "Primary": {
          "BrokerUri": "tcp://activemq-primary.company.com:61616",
          "ClientId": "primary-health-checker",
          "UserName": "monitoring_user",
          "Password": "secure_password",
          "Queue": "health.primary"
        },
        "Secondary": {
          "BrokerUri": "tcp://activemq-secondary.company.com:61616",
          "ClientId": "secondary-health-checker",
          "UserName": "monitoring_user",
          "Password": "secure_password",
          "Queue": "health.secondary"
        }
      },
      "Database": {
        "ConnectionString": "Server=db.company.com;Database=AppDB;Trusted_Connection=true;",
        "Timeout": "00:00:30"
      }
    },
    "SystemResourceMonitor": {
      "EnableCpuMonitoring": true,
      "EnableMemoryMonitoring": true,
      "EnableDiskMonitoring": true,
      "EnableNetworkMonitoring": false,
      "CollectionInterval": "00:00:30",
      "Thresholds": {
        "CpuUsagePercent": 80,
        "MemoryUsagePercent": 85,
        "DiskUsagePercent": 90
      }
    },
    "NetworkMonitoring": {
      "Enabled": true,
      "EnableUDP": false,
      "CollectionIntervalSeconds": 30,
      "SessionNamePrefix": "WebApp_NetworkMonitor",
      "Thresholds": {
        "WarningBytesPerSecond": 10485760,
        "CriticalBytesPerSecond": 52428800
      }
    }
  }
}
```

### Docker Compose Integration
```yaml
# docker-compose.yml
version: '3.8'
services:
  app:
    image: myapp:latest
    environment:
      - Infrastructure__HealthChecks__ActiveMQ__Primary__BrokerUri=tcp://activemq:61616
      - Infrastructure__HealthChecks__ActiveMQ__Primary__Queue=health.check
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/healthz"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    depends_on:
      activemq:
        condition: service_healthy

  activemq:
    image: symptoma/activemq:latest
    ports:
      - "61616:61616"
    healthcheck:
      test: ["CMD-SHELL", "netstat -ln | grep :61616"]
      interval: 30s
      timeout: 10s
      retries: 3
```

## Best Practices

### Health Check Strategy
1. **Layered Monitoring**: Different health checks for different concerns (liveness, readiness, health)
2. **Appropriate Timeouts**: Set realistic timeouts based on network and system characteristics
3. **Dependency Isolation**: Ensure health check failures don't cascade
4. **Resource Efficiency**: Minimize resource usage of health checks themselves

### System Resource Monitoring
1. **Baseline Establishment**: Establish performance baselines for normal operation
2. **Threshold Management**: Set appropriate thresholds for alerts
3. **Trend Analysis**: Monitor trends rather than just point-in-time metrics
4. **Capacity Planning**: Use monitoring data for infrastructure scaling decisions

### Configuration Management
1. **Environment Separation**: Different configurations for different environments
2. **Secret Management**: Secure storage of credentials and sensitive configuration
3. **Validation**: Validate configuration at application startup
4. **Documentation**: Document all configuration options and their purposes

### Security Considerations
1. **Credential Protection**: Secure storage and transmission of monitoring credentials
2. **Network Security**: Use encrypted connections where appropriate
3. **Access Control**: Limit access to health check and monitoring endpoints
4. **Audit Logging**: Track access to monitoring endpoints and configuration changes

## Integration with Monitoring Systems

### Prometheus Integration
```csharp
// Custom Prometheus metrics endpoint
app.UseHealthChecks("/metrics", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var metrics = new StringBuilder();
        
        // Overall health status
        metrics.AppendLine($"application_health_status {(report.Status == HealthStatus.Healthy ? 1 : 0)}");
        
        // Individual health checks
        foreach (var check in report.Entries)
        {
            var status = check.Value.Status == HealthStatus.Healthy ? 1 : 0;
            var duration = check.Value.Duration.TotalMilliseconds;
            
            metrics.AppendLine($"health_check_status{{name=\"{check.Key}\"}} {status}");
            metrics.AppendLine($"health_check_duration_ms{{name=\"{check.Key}\"}} {duration}");
        }

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(metrics.ToString());
    }
});
```

### Azure Application Insights
```csharp
// Application Insights health monitoring
public class ApplicationInsightsHealthPublisher : IHostedService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly TelemetryClient _telemetryClient;
    private Timer _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(PublishHealthMetrics, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async void PublishHealthMetrics(object state)
    {
        var report = await _healthCheckService.CheckHealthAsync();
        
        _telemetryClient.TrackMetric("HealthCheck.OverallStatus", 
            report.Status == HealthStatus.Healthy ? 1 : 0);
        
        foreach (var check in report.Entries)
        {
            _telemetryClient.TrackMetric($"HealthCheck.{check.Key}.Status",
                check.Value.Status == HealthStatus.Healthy ? 1 : 0);
            _telemetryClient.TrackMetric($"HealthCheck.{check.Key}.Duration",
                check.Value.Duration.TotalMilliseconds);
        }
    }
}
```

## Troubleshooting

### Common Issues
1. **Health Check Timeouts**: Adjust timeout settings based on network conditions
2. **Authentication Failures**: Verify credentials and user permissions
3. **Resource Leaks**: Ensure proper disposal of connections and resources
4. **Configuration Errors**: Validate configuration at startup

### Diagnostic Commands
```bash
# Test health endpoints
curl http://localhost:5000/health
curl http://localhost:5000/health/ready
curl http://localhost:5000/health/live

# Check specific components
curl http://localhost:5000/health/activemq
curl http://localhost:5000/health/database

# Get detailed metrics
curl http://localhost:5000/metrics
```

### Logging Configuration
```csharp
// Enhanced logging for troubleshooting
services.AddLogging(builder =>
{
    builder.AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.Information);
    builder.AddFilter("RapidStreamer.BuildingBlocks.Infrastructure", LogLevel.Debug);
});
```

## Future Roadmap

### Planned Components
- **Database Health Checks**: SQL Server, PostgreSQL, MongoDB
- **Cache Health Checks**: Redis, Memcached
- **Web Service Health Checks**: HTTP endpoint monitoring
- **File System Health Checks**: Disk space and permission monitoring
- **Network Health Checks**: Connectivity and latency monitoring
- **System Performance Monitoring**: Enhanced CPU, memory, and disk monitoring
- **Resource Analytics**: Advanced performance analytics and predictions

### Integration Enhancements
- **Kubernetes Integration**: Enhanced probe support
- **Cloud Provider Integration**: AWS, Azure, GCP health services
- **Container Orchestration**: Docker Swarm, Kubernetes health integration
- **Service Mesh Integration**: Istio, Linkerd health monitoring

## Related Documentation

### Application Components
- **[Application Building Blocks](../Application/README.md)** - Application-level components
- **[Telemetry](../Application/Telemetry.md)** - Application metrics and monitoring
- **[Correlation ID Support](../Application/CorrelationId/README.md)** - Request tracing

### Development Tools
- **Build Configuration**: Project build and packaging settings
- **Testing Framework**: Unit and integration testing components
- **Documentation Tools**: API documentation and code analysis