# ActiveMQHealthCheck

## Overview

The `ActiveMQHealthCheck` class implements the `IHealthCheck` interface to provide health monitoring capabilities for Apache ActiveMQ message brokers. This component performs actual connectivity tests by establishing connections and sending test messages to verify broker availability and functionality.

## Purpose

- **Broker Connectivity**: Test connection establishment to ActiveMQ brokers
- **Message Operations**: Verify ability to send messages to configured queues
- **Health Monitoring**: Integration with .NET health check framework
- **Fault Detection**: Early detection of ActiveMQ broker issues

## Class Declaration

```csharp
internal sealed class ActiveMQHealthCheck : IHealthCheck
{
    private readonly ActiveMQHealthCheckOptions _activeMQHealthCheckOptions;

    public ActiveMQHealthCheck(ActiveMQHealthCheckOptions activeMQHealthCheckOptions)
    {
        _activeMQHealthCheckOptions = activeMQHealthCheckOptions;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Implementation details
    }
}
```

## Implementation Details

### Constructor Injection
The health check receives configuration through dependency injection:
- **[ActiveMQHealthCheckOptions](ActiveMQHealthCheckOptions.md)**: Configuration for broker connection and queue settings

### Health Check Process

The `CheckHealthAsync` method performs the following steps:

1. **Connection Factory Creation**: Creates `ConnectionFactory` with broker URI
2. **Authentication Setup**: Configures credentials if provided
3. **Connection Establishment**: Creates and opens connection to broker
4. **Session Creation**: Establishes message session
5. **Queue Setup**: Creates reference to target queue
6. **Producer Creation**: Sets up message producer
7. **Message Configuration**: Creates test message with TTL
8. **Message Sending**: Sends test message to verify functionality
9. **Resource Cleanup**: Properly disposes all resources

### Message Configuration

```csharp
producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
producer.Priority = MsgPriority.AboveLow;

var message = new ActiveMQMessage { NMSTimeToLive = TimeSpan.FromMilliseconds(1000) };
await producer.SendAsync(message);
```

**Key Settings:**
- **Non-Persistent**: Messages don't survive broker restarts (appropriate for health checks)
- **Above Low Priority**: Slightly elevated priority without impacting business messages
- **1-Second TTL**: Short time-to-live to prevent message accumulation

## Usage Examples

### Basic Registration
```csharp
// In Startup.cs or Program.cs
services.AddHealthChecks()
    .AddActiveMQHealthCheck(new ActiveMQHealthCheckOptions
    {
        BrokerUri = "tcp://localhost:61616",
        Queue = "health.check"
    });
```

### Advanced Configuration
```csharp
services.AddHealthChecks()
    .AddActiveMQHealthCheck(
        options: new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://activemq.production.com:61616",
            ClientId = "api-health-checker",
            UserName = "monitoring_user",
            Password = "secure_password",
            Queue = "system.health"
        },
        name: "ActiveMQ Production",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "messaging", "activemq", "production" },
        timeout: TimeSpan.FromSeconds(10)
    );
```

### Multiple Broker Monitoring
```csharp
services.AddHealthChecks()
    .AddActiveMQHealthCheck(
        new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://activemq-primary.com:61616",
            Queue = "health.primary"
        },
        name: "ActiveMQ Primary")
    .AddActiveMQHealthCheck(
        new ActiveMQHealthCheckOptions
        {
            BrokerUri = "tcp://activemq-secondary.com:61616",
            Queue = "health.secondary"
        },
        name: "ActiveMQ Secondary");
```

## Health Check Results

### Healthy State
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "ActiveMQHealthCheck": {
      "status": "Healthy",
      "duration": "00:00:00.1234567"
    }
  }
}
```

### Unhealthy State
```json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:00.5000000",
  "entries": {
    "ActiveMQHealthCheck": {
      "status": "Unhealthy",
      "duration": "00:00:00.5000000",
      "exception": "Connection refused: localhost:61616",
      "description": "Connection refused: localhost:61616"
    }
  }
}
```

## Error Handling

### Common Failure Scenarios

1. **Connection Failures**
   - Broker unavailable or unreachable
   - Invalid broker URI configuration
   - Network connectivity issues

2. **Authentication Failures**
   - Invalid credentials
   - User account disabled or expired
   - Insufficient permissions

3. **Queue Operation Failures**
   - Queue doesn't exist
   - Insufficient permissions to send messages
   - Broker resource limitations

4. **Timeout Issues**
   - Slow network connections
   - Broker under heavy load
   - Resource contention

### Exception Handling Strategy

```csharp
try
{
    // Health check operations
    return HealthCheckResult.Healthy();
}
catch (Exception exception)
{
    return HealthCheckResult.Unhealthy(exception.Message, exception);
}
```

**Benefits:**
- **Detailed Error Information**: Full exception details captured
- **Troubleshooting Support**: Stack traces available for debugging
- **Monitoring Integration**: Compatible with health check monitoring systems

## Performance Considerations

### Resource Management
- **Using Statements**: Proper disposal of connections, sessions, and producers
- **Connection Lifecycle**: New connection per health check (stateless)
- **Memory Efficiency**: Minimal object creation and immediate cleanup

### Network Optimization
- **Non-Persistent Messages**: Reduces broker storage overhead
- **Short TTL**: Prevents message accumulation in queues
- **Lightweight Payloads**: Minimal message content for efficiency

### Monitoring Best Practices
1. **Check Frequency**: Balance between responsiveness and resource usage
2. **Timeout Configuration**: Set appropriate timeouts for your network environment
3. **Queue Management**: Use dedicated health check queues
4. **Resource Monitoring**: Monitor connection pool usage and broker resources

## Integration Points

### Health Check Framework
- **IHealthCheck Interface**: Standard .NET health check contract
- **HealthCheckContext**: Access to health check metadata and cancellation
- **HealthCheckResult**: Standardized result format

### Dependency Injection
- **Scoped Registration**: Health check instances created per request
- **Configuration Injection**: Options pattern for flexible configuration
- **Service Lifetime**: Managed by health check framework

### Monitoring Systems
- **ASP.NET Core Integration**: Built-in health check endpoints
- **Custom Monitoring**: Export to external monitoring systems
- **Alerting Integration**: Trigger alerts on health check failures

## Related Components

- **[ActiveMQ Health Check Options](ActiveMQHealthCheckOptions.md)** - Configuration settings
- **[ActiveMQ Health Check Extensions](ActiveMQHealthCheckExtensions.md)** - Registration helpers
- **[Health Checks Overview](README.md)** - Complete health checks documentation

## Security Considerations

### Credential Management
- **Configuration Security**: Store credentials securely
- **Principle of Least Privilege**: Use dedicated monitoring accounts
- **Credential Rotation**: Support for credential updates without restart

### Network Security
- **SSL/TLS Support**: Encrypted connections for production
- **Firewall Configuration**: Ensure health check network access
- **VPN Requirements**: Consider network security policies

### Audit and Compliance
- **Health Check Logging**: Monitor health check execution
- **Access Auditing**: Track health check user activities
- **Compliance Reporting**: Document monitoring capabilities

## Troubleshooting Guide

### Connection Issues
1. **Verify Broker URI**: Ensure correct protocol, host, and port
2. **Network Connectivity**: Test basic network reachability
3. **Firewall Rules**: Verify port accessibility
4. **Broker Status**: Confirm ActiveMQ broker is running

### Authentication Problems
1. **Credential Validation**: Verify username and password
2. **User Permissions**: Check broker user configuration
3. **Client ID Conflicts**: Ensure unique client identifiers
4. **Account Status**: Verify user account is active

### Performance Issues
1. **Timeout Configuration**: Adjust health check timeouts
2. **Broker Load**: Monitor broker resource utilization
3. **Network Latency**: Consider network performance
4. **Queue Configuration**: Verify queue settings and permissions

### Message Failures
1. **Queue Existence**: Verify target queue exists
2. **Queue Permissions**: Check send permissions
3. **Message Size Limits**: Ensure message within broker limits
4. **Broker Capacity**: Monitor broker storage and memory