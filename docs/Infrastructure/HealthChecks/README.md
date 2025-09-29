# Health Checks

## Overview

The Health Checks module provides comprehensive health monitoring capabilities for infrastructure components in RapidStreamer applications. This module integrates with the .NET health check framework to deliver real-time monitoring, early fault detection, and operational visibility.

## Purpose

- **Infrastructure Monitoring**: Monitor critical dependencies and services
- **Early Fault Detection**: Identify issues before they impact users
- **Operational Visibility**: Provide real-time status information
- **Integration Ready**: Seamless integration with monitoring and alerting systems

## Components

### ActiveMQ Health Monitoring

Complete health check implementation for Apache ActiveMQ message brokers:

- **[ActiveMQHealthCheckOptions](ActiveMQHealthCheckOptions.md)** - Configuration settings for ActiveMQ health checks
- **[ActiveMQHealthCheck](ActiveMQHealthCheck.md)** - Core health check implementation
- **[ActiveMQHealthCheckExtensions](ActiveMQHealthCheckExtensions.md)** - Dependency injection and registration helpers

## Features

### Comprehensive Monitoring
- **Connection Testing**: Verify broker connectivity and authentication
- **Message Operations**: Test actual message sending capabilities
- **Resource Validation**: Check queue availability and permissions
- **Performance Metrics**: Monitor response times and connection health

### Flexible Configuration
- **Multiple Brokers**: Support for monitoring multiple ActiveMQ instances
- **Authentication Options**: Support for anonymous and authenticated connections
- **SSL/TLS Support**: Secure connection monitoring
- **Custom Timeouts**: Configurable timeout settings per environment

### Integration Capabilities
- **ASP.NET Core**: Built-in endpoint support
- **Background Services**: Continuous monitoring capabilities
- **External Monitoring**: Export to monitoring systems
- **Alerting Integration**: Connect to notification systems

## Quick Start

### Basic Setup
```csharp
// Program.cs or Startup.cs
services.AddHealthChecks()
    .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
    {
        BrokerUri = "tcp://localhost:61616",
        Queue = "health.check"
    });

// Configure endpoint
app.UseHealthChecks("/health");
```

### Production Setup
```csharp
services.AddHealthChecks()
    .AddActiveMQHealthCheck(
        new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://activemq.production.com:61616",
            ClientId = "api-health-checker",
            UserName = Configuration["ActiveMQ:Username"],
            Password = Configuration["ActiveMQ:Password"],
            Queue = "system.health"
        },
        name: "Production ActiveMQ",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "messaging", "critical" },
        timeout: TimeSpan.FromSeconds(30)
    );
```

## Usage Patterns

### Environment-Specific Configuration
```csharp
// Development
if (env.IsDevelopment())
{
    services.AddHealthChecks()
        .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://localhost:61616",
            Queue = "dev.health"
        });
}

// Production
if (env.IsProduction())
{
    services.AddHealthChecks()
        .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
        {
            BrokerUri = Configuration["ActiveMQ:ProductionUri"],
            ClientId = Configuration["ActiveMQ:ClientId"],
            UserName = Configuration["ActiveMQ:Username"],
            Password = Configuration["ActiveMQ:Password"],
            Queue = Configuration["ActiveMQ:HealthQueue"]
        },
        timeout: TimeSpan.FromSeconds(60));
}
```

### Multi-Instance Monitoring
```csharp
services.AddHealthChecks()
    .AddActiveMQHealthCheck(primaryOptions, "Primary ActiveMQ", tags: new[] { "primary" })
    .AddActiveMQHealthCheck(secondaryOptions, "Secondary ActiveMQ", tags: new[] { "secondary" })
    .AddActiveMQHealthCheck(analyticsOptions, "Analytics ActiveMQ", tags: new[] { "analytics" });
```

### Custom Health Check Endpoints
```csharp
// General health
app.UseHealthChecks("/health");

// Messaging-specific health
app.UseHealthChecks("/health/messaging", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("messaging")
});

// Detailed health information
app.UseHealthChecks("/health/detailed", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    status = kvp.Value.Status.ToString(),
                    duration = kvp.Value.Duration,
                    exception = kvp.Value.Exception?.Message,
                    description = kvp.Value.Description
                })
        };
        
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});
```

## Configuration Examples

### Configuration File Setup
```json
// appsettings.json
{
  "HealthChecks": {
    "ActiveMQ": {
      "Primary": {
        "BrokerUri": "tcp://activemq-primary.company.com:61616",
        "ClientId": "health-primary",
        "UserName": "monitoring_user",
        "Password": "secure_password",
        "Queue": "health.primary"
      },
      "Secondary": {
        "BrokerUri": "tcp://activemq-secondary.company.com:61616",
        "ClientId": "health-secondary",
        "UserName": "monitoring_user",
        "Password": "secure_password",
        "Queue": "health.secondary"
      }
    }
  }
}
```

```csharp
// Configuration binding
var primaryOptions = new ActiveMQHealthCheckOptions();
Configuration.GetSection("HealthChecks:ActiveMQ:Primary").Bind(primaryOptions);

var secondaryOptions = new ActiveMQHealthCheckOptions();
Configuration.GetSection("HealthChecks:ActiveMQ:Secondary").Bind(secondaryOptions);

services.AddHealthChecks()
    .AddActiveMQHealthCheck(primaryOptions, "Primary ActiveMQ")
    .AddActiveMQHealthCheck(secondaryOptions, "Secondary ActiveMQ");
```

### Secure Configuration
```csharp
// Using Azure Key Vault or similar secret management
services.AddHealthChecks()
    .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
    {
        BrokerUri = Configuration["ActiveMQ:SecureBrokerUri"],
        ClientId = "secure-health-checker",
        UserName = await secretClient.GetSecretAsync("activemq-username"),
        Password = await secretClient.GetSecretAsync("activemq-password"),
        Queue = "secure.health"
    });
```

## Monitoring Integration

### Background Health Monitoring
```csharp
public class HealthMonitoringService : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthMonitoringService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var report = await _healthCheckService.CheckHealthAsync(stoppingToken);
            
            foreach (var check in report.Entries)
            {
                if (check.Value.Status != HealthStatus.Healthy)
                {
                    _logger.LogWarning("Health check {CheckName} failed: {Status} - {Description}",
                        check.Key, check.Value.Status, check.Value.Description);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### External Monitoring Integration
```csharp
// Custom health check result writer for monitoring systems
public static class HealthCheckWriters
{
    public static async Task WritePrometheusMetrics(HttpContext context, HealthReport report)
    {
        var metrics = new StringBuilder();
        
        foreach (var check in report.Entries)
        {
            var status = check.Value.Status == HealthStatus.Healthy ? 1 : 0;
            metrics.AppendLine($"health_check{{name=\"{check.Key}\"}} {status}");
            metrics.AppendLine($"health_check_duration{{name=\"{check.Key}\"}} {check.Value.Duration.TotalMilliseconds}");
        }

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(metrics.ToString());
    }
}

// Usage
app.UseHealthChecks("/metrics", new HealthCheckOptions
{
    ResponseWriter = HealthCheckWriters.WritePrometheusMetrics
});
```

## Best Practices

### Configuration Management
1. **Environment Variables**: Use environment-specific configurations
2. **Secret Management**: Store credentials securely
3. **Validation**: Validate configuration at startup
4. **Documentation**: Document all configuration options

### Performance Optimization
1. **Appropriate Timeouts**: Set realistic timeout values
2. **Check Frequency**: Balance monitoring needs with resource usage
3. **Resource Cleanup**: Ensure proper disposal of connections
4. **Caching**: Cache expensive checks when appropriate

### Monitoring Strategy
1. **Layered Monitoring**: Different endpoints for different audiences
2. **Tag-Based Filtering**: Use tags for organized monitoring
3. **Alerting Integration**: Connect to alerting systems
4. **Trend Analysis**: Monitor health check trends over time

### Security Considerations
1. **Credential Protection**: Secure storage and transmission
2. **Network Security**: Use encrypted connections
3. **Access Control**: Limit access to health endpoints
4. **Audit Logging**: Track health check activities

## Troubleshooting

### Common Issues

#### Connection Problems
- **Broker Unavailable**: Verify broker status and network connectivity
- **Authentication Failures**: Check credentials and user permissions
- **Timeout Issues**: Adjust timeout settings for network conditions
- **SSL/Certificate Problems**: Verify certificate configuration

#### Configuration Issues
- **Invalid URIs**: Ensure proper URI format and accessibility
- **Missing Queues**: Verify queue existence and permissions
- **Service Registration**: Check dependency injection setup
- **Duplicate Names**: Ensure unique health check names

#### Performance Problems
- **Slow Health Checks**: Optimize timeout settings and network configuration
- **Resource Leaks**: Monitor connection pooling and disposal
- **Memory Usage**: Check for proper cleanup of health check resources
- **CPU Usage**: Monitor health check frequency and execution time

### Diagnostic Tools

#### Health Check Status
```bash
# Check basic health
curl http://localhost:5000/health

# Check detailed health
curl http://localhost:5000/health/detailed

# Check specific component
curl http://localhost:5000/health/messaging
```

#### Logging Configuration
```csharp
services.AddLogging(builder =>
{
    builder.AddFilter("Microsoft.Extensions.Diagnostics.HealthChecks", LogLevel.Debug);
    builder.AddFilter("RapidStreamer.BuildingBlocks.Infrastructure.HealthChecks", LogLevel.Debug);
});
```

## Future Enhancements

### Planned Features
- **Database Health Checks**: SQL Server, PostgreSQL, MongoDB support
- **Redis Health Checks**: Cache and session store monitoring
- **Web Service Health Checks**: HTTP endpoint monitoring
- **File System Health Checks**: Disk space and permission monitoring

### Integration Roadmap
- **Kubernetes Integration**: Readiness and liveness probe support
- **Docker Health Checks**: Container health monitoring
- **Cloud Provider Integration**: AWS, Azure, GCP health services
- **Metrics Export**: Enhanced Prometheus and Grafana integration

## Related Documentation

### Infrastructure Components
- **System Resource Monitor**: System-level monitoring capabilities
- **Configuration Management**: Application configuration handling
- **Logging Framework**: Structured logging and diagnostics

### Application Components
- **[Correlation ID Support](../Application/CorrelationId/README.md)**: Request tracing integration
- **[Telemetry](../Application/Telemetry.md)**: Application metrics and monitoring
- **[Exception Handling](../Application/ExceptionInfo.md)**: Error tracking and reporting