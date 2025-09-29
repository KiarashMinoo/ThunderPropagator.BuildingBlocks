# CastType

The `CastType` enum defines message distribution patterns in the RapidStreamer BuildingBlocks framework. It specifies how messages should be delivered to recipients, controlling whether messages are sent to one, some, or all available targets in a messaging system.

## Overview

The `CastType` enum is primarily used by the `FeederMessage` class and messaging infrastructure to determine message routing and delivery patterns. It provides standardized delivery semantics that are commonly used in distributed messaging systems.

## Enum Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.Enums
{
    public enum CastType
    {
        Multicast = 0,
        Broadcast = 1,
        Unicast = 2
    }
}
```

## Values

### Multicast
- **Value**: `0` (Default)
- **Description**: Send message to multiple specific recipients or subscribers
- **Use Case**: Targeted group messaging, subscriber notifications, event publishing to interested parties
- **Delivery**: Message is delivered to a specific subset of available recipients
- **Performance**: Moderate - only selected recipients receive the message

### Broadcast
- **Value**: `1`
- **Description**: Send message to all available recipients in the system
- **Use Case**: System-wide announcements, emergency notifications, global state changes
- **Delivery**: Message is delivered to every connected recipient regardless of subscription
- **Performance**: Highest overhead - all recipients receive the message

### Unicast
- **Value**: `2`
- **Description**: Send message to exactly one specific recipient
- **Use Case**: Point-to-point communication, direct responses, targeted commands
- **Delivery**: Message is delivered to a single, specifically identified recipient
- **Performance**: Lowest overhead - most efficient delivery pattern

## Usage Examples

### FeederMessage Integration

```csharp
using RapidStreamer.BuildingBlocks.Application;
using RapidStreamer.BuildingBlocks.Application.Enums;

public class OrderNotificationMessage : FeederMessage
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

// Default multicast behavior (set in FeederMessage constructor)
var orderNotification = new OrderNotificationMessage
{
    OrderId = "ORD-001",
    CustomerId = "CUST-123",
    Amount = 299.99m,
    Status = "Confirmed"
};
// CastType is automatically set to Multicast (default)
Console.WriteLine($"Cast Type: {orderNotification.CastType}"); // Output: Multicast

// Unicast for direct customer notification
var directNotification = new OrderNotificationMessage
{
    OrderId = "ORD-002",
    CustomerId = "CUST-456",
    Amount = 149.99m,
    Status = "Shipped",
    CastType = CastType.Unicast
};

// Broadcast for system-wide alerts
var systemAlert = new OrderNotificationMessage
{
    OrderId = "SYS-001",
    CustomerId = "SYSTEM",
    Amount = 0,
    Status = "System Maintenance Starting",
    CastType = CastType.Broadcast
};
```

### Message Routing Service

```csharp
public class MessageRoutingService
{
    private readonly IMessageBroker _messageBroker;
    private readonly IRecipientRegistry _recipientRegistry;
    private readonly ILogger<MessageRoutingService> _logger;
    
    public MessageRoutingService(IMessageBroker messageBroker, 
        IRecipientRegistry recipientRegistry,
        ILogger<MessageRoutingService> logger)
    {
        _messageBroker = messageBroker;
        _recipientRegistry = recipientRegistry;
        _logger = logger;
    }
    
    public async Task<DeliveryResult> SendMessageAsync(FeederMessage message)
    {
        var recipients = await GetRecipientsForCastType(message.CastType, message);
        
        _logger.LogInformation(
            "Sending message with cast type {CastType} to {RecipientCount} recipients [{CorrelationId}]",
            message.CastType, recipients.Count, message.CorrelationId);
        
        return await DeliverMessage(message, recipients);
    }
    
    private async Task<List<MessageRecipient>> GetRecipientsForCastType(
        CastType castType, FeederMessage message)
    {
        switch (castType)
        {
            case CastType.Unicast:
                return await GetUnicastRecipient(message);
                
            case CastType.Multicast:
                return await GetMulticastRecipients(message);
                
            case CastType.Broadcast:
                return await GetAllRecipients();
                
            default:
                throw new ArgumentOutOfRangeException(nameof(castType), castType, 
                    "Unknown cast type");
        }
    }
    
    private async Task<List<MessageRecipient>> GetUnicastRecipient(FeederMessage message)
    {
        // For unicast, determine the specific recipient
        // This could be based on message content, routing key, or explicit recipient ID
        
        var targetRecipient = await DetermineUnicastTarget(message);
        
        if (targetRecipient == null)
        {
            _logger.LogWarning(
                "No unicast recipient found for message [{CorrelationId}]", 
                message.CorrelationId);
            return new List<MessageRecipient>();
        }
        
        return new List<MessageRecipient> { targetRecipient };
    }
    
    private async Task<MessageRecipient?> DetermineUnicastTarget(FeederMessage message)
    {
        // Example: Route based on message content
        if (message is OrderNotificationMessage orderMsg)
        {
            return await _recipientRegistry.GetRecipientByCustomerIdAsync(orderMsg.CustomerId);
        }
        
        // Example: Route based on explicit recipient field
        if (message.ContainsKey("RecipientId"))
        {
            var recipientId = message.GetValueOrDefault<string>("RecipientId");
            return await _recipientRegistry.GetRecipientByIdAsync(recipientId);
        }
        
        return null;
    }
    
    private async Task<List<MessageRecipient>> GetMulticastRecipients(FeederMessage message)
    {
        // For multicast, get recipients based on subscription or interest
        
        var messageType = message.GetType().Name;
        var subscribers = await _recipientRegistry.GetSubscribersAsync(messageType);
        
        // Apply additional filtering based on message content
        if (message is OrderNotificationMessage orderMsg)
        {
            // Only send to recipients interested in this customer's orders
            subscribers = subscribers.Where(r => 
                r.IsInterestedInCustomer(orderMsg.CustomerId)).ToList();
        }
        
        _logger.LogDebug(
            "Found {SubscriberCount} multicast recipients for message type {MessageType}",
            subscribers.Count, messageType);
        
        return subscribers;
    }
    
    private async Task<List<MessageRecipient>> GetAllRecipients()
    {
        // For broadcast, get all connected recipients
        var allRecipients = await _recipientRegistry.GetAllActiveRecipientsAsync();
        
        _logger.LogDebug(
            "Broadcasting to {RecipientCount} recipients", 
            allRecipients.Count);
        
        return allRecipients;
    }
    
    private async Task<DeliveryResult> DeliverMessage(FeederMessage message, 
        List<MessageRecipient> recipients)
    {
        var deliveryTasks = recipients.Select(async recipient =>
        {
            try
            {
                await _messageBroker.SendToRecipientAsync(message, recipient);
                return new RecipientDeliveryResult 
                { 
                    Recipient = recipient, 
                    Success = true 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to deliver message to recipient {RecipientId} [{CorrelationId}]",
                    recipient.Id, message.CorrelationId);
                    
                return new RecipientDeliveryResult 
                { 
                    Recipient = recipient, 
                    Success = false, 
                    Error = ex.Message 
                };
            }
        });
        
        var results = await Task.WhenAll(deliveryTasks);
        
        return new DeliveryResult
        {
            CastType = message.CastType,
            TotalRecipients = recipients.Count,
            SuccessfulDeliveries = results.Count(r => r.Success),
            FailedDeliveries = results.Count(r => !r.Success),
            Results = results.ToList()
        };
    }
}

public class DeliveryResult
{
    public CastType CastType { get; set; }
    public int TotalRecipients { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public List<RecipientDeliveryResult> Results { get; set; } = new();
}

public class RecipientDeliveryResult
{
    public MessageRecipient Recipient { get; set; } = null!;
    public bool Success { get; set; }
    public string? Error { get; set; }
}
```

### Event Publishing System

```csharp
public class EventPublisher
{
    private readonly MessageRoutingService _routingService;
    
    public EventPublisher(MessageRoutingService routingService)
    {
        _routingService = routingService;
    }
    
    // Unicast: Direct user notification
    public async Task NotifyUserDirectlyAsync(string userId, string message)
    {
        var notification = new UserNotificationMessage
        {
            UserId = userId,
            Message = message,
            Timestamp = DateTime.UtcNow,
            CastType = CastType.Unicast
        };
        
        await _routingService.SendMessageAsync(notification);
    }
    
    // Multicast: Notify interested subscribers
    public async Task PublishOrderEventAsync(OrderEvent orderEvent)
    {
        var message = new OrderEventMessage
        {
            OrderId = orderEvent.OrderId,
            EventType = orderEvent.EventType,
            CustomerId = orderEvent.CustomerId,
            Data = orderEvent.Data,
            CastType = CastType.Multicast // Send to order event subscribers
        };
        
        await _routingService.SendMessageAsync(message);
    }
    
    // Broadcast: System-wide announcements
    public async Task BroadcastSystemAnnouncementAsync(string announcement)
    {
        var message = new SystemAnnouncementMessage
        {
            Announcement = announcement,
            Timestamp = DateTime.UtcNow,
            Priority = AnnouncementPriority.High,
            CastType = CastType.Broadcast // Send to all connected users
        };
        
        await _routingService.SendMessageAsync(message);
    }
}
```

### Message Processing Pipeline

```csharp
public class CastTypeAwareProcessor
{
    public async Task<ProcessingResult> ProcessMessageAsync(FeederMessage message)
    {
        var processingStrategy = GetProcessingStrategy(message.CastType);
        
        using var activity = Activity.StartActivity($"ProcessMessage.{message.CastType}");
        activity?.SetTag("cast.type", message.CastType.ToString());
        activity?.SetTag("correlation.id", message.CorrelationId);
        
        var result = await processingStrategy.ProcessAsync(message);
        
        // Log processing metrics based on cast type
        LogProcessingMetrics(message.CastType, result);
        
        return result;
    }
    
    private IMessageProcessingStrategy GetProcessingStrategy(CastType castType)
    {
        return castType switch
        {
            CastType.Unicast => new UnicastProcessingStrategy(),
            CastType.Multicast => new MulticastProcessingStrategy(),
            CastType.Broadcast => new BroadcastProcessingStrategy(),
            _ => throw new ArgumentOutOfRangeException(nameof(castType), castType, 
                "Unsupported cast type")
        };
    }
    
    private void LogProcessingMetrics(CastType castType, ProcessingResult result)
    {
        var metrics = new
        {
            CastType = castType.ToString(),
            Duration = result.ProcessingTime,
            Success = result.Success,
            RecipientCount = result.RecipientCount
        };
        
        // Log to metrics system
        Console.WriteLine($"Processing metrics: {JsonSerializer.Serialize(metrics)}");
    }
}

public interface IMessageProcessingStrategy
{
    Task<ProcessingResult> ProcessAsync(FeederMessage message);
}

public class UnicastProcessingStrategy : IMessageProcessingStrategy
{
    public async Task<ProcessingResult> ProcessAsync(FeederMessage message)
    {
        // Optimized for single recipient
        // - Fast routing
        // - Minimal overhead
        // - Direct delivery
        
        var startTime = DateTime.UtcNow;
        
        // Process message for single recipient
        await ProcessForSingleRecipient(message);
        
        return new ProcessingResult
        {
            Success = true,
            ProcessingTime = DateTime.UtcNow - startTime,
            RecipientCount = 1,
            CastType = CastType.Unicast
        };
    }
    
    private async Task ProcessForSingleRecipient(FeederMessage message)
    {
        // Implementation for unicast processing
        await Task.Delay(10); // Simulate processing
    }
}

public class MulticastProcessingStrategy : IMessageProcessingStrategy
{
    public async Task<ProcessingResult> ProcessAsync(FeederMessage message)
    {
        // Optimized for multiple specific recipients
        // - Subscriber lookup
        // - Filtered delivery
        // - Parallel processing
        
        var startTime = DateTime.UtcNow;
        
        var recipients = await GetSubscribers(message);
        await ProcessForMultipleRecipients(message, recipients);
        
        return new ProcessingResult
        {
            Success = true,
            ProcessingTime = DateTime.UtcNow - startTime,
            RecipientCount = recipients.Count,
            CastType = CastType.Multicast
        };
    }
    
    private async Task<List<string>> GetSubscribers(FeederMessage message)
    {
        // Get subscribers for this message type
        await Task.Delay(5); // Simulate subscriber lookup
        return new List<string> { "subscriber1", "subscriber2", "subscriber3" };
    }
    
    private async Task ProcessForMultipleRecipients(FeederMessage message, List<string> recipients)
    {
        // Parallel processing for multiple recipients
        var tasks = recipients.Select(async recipient =>
        {
            await Task.Delay(10); // Simulate processing for each recipient
        });
        
        await Task.WhenAll(tasks);
    }
}

public class BroadcastProcessingStrategy : IMessageProcessingStrategy
{
    public async Task<ProcessingResult> ProcessAsync(FeederMessage message)
    {
        // Optimized for all recipients
        // - System-wide delivery
        // - Maximum parallelization
        // - Failure tolerance
        
        var startTime = DateTime.UtcNow;
        
        var allRecipients = await GetAllRecipients();
        await ProcessForAllRecipients(message, allRecipients);
        
        return new ProcessingResult
        {
            Success = true,
            ProcessingTime = DateTime.UtcNow - startTime,
            RecipientCount = allRecipients.Count,
            CastType = CastType.Broadcast
        };
    }
    
    private async Task<List<string>> GetAllRecipients()
    {
        // Get all connected recipients
        await Task.Delay(15); // Simulate getting all recipients
        return new List<string> { "user1", "user2", "user3", "user4", "user5" };
    }
    
    private async Task ProcessForAllRecipients(FeederMessage message, List<string> recipients)
    {
        // Broadcast processing with failure tolerance
        var tasks = recipients.Select(async recipient =>
        {
            try
            {
                await Task.Delay(5); // Simulate processing
            }
            catch
            {
                // Log but don't fail the entire broadcast
            }
        });
        
        await Task.WhenAll(tasks);
    }
}
```

## Configuration and Patterns

### Message Configuration

```csharp
public class MessageConfiguration
{
    public CastType DefaultCastType { get; set; } = CastType.Multicast;
    public Dictionary<string, CastType> MessageTypeCastTypes { get; set; } = new();
    public bool AllowCastTypeOverride { get; set; } = true;
    public int MaxBroadcastRecipients { get; set; } = 1000;
    public TimeSpan BroadcastTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

public class ConfigurableMessageService
{
    private readonly MessageConfiguration _config;
    
    public ConfigurableMessageService(MessageConfiguration config)
    {
        _config = config;
    }
    
    public FeederMessage CreateMessage<T>(T messageData) where T : FeederMessage
    {
        // Apply default cast type based on configuration
        var messageType = typeof(T).Name;
        
        if (_config.MessageTypeCastTypes.TryGetValue(messageType, out var configuredCastType))
        {
            messageData.CastType = configuredCastType;
        }
        else
        {
            messageData.CastType = _config.DefaultCastType;
        }
        
        return messageData;
    }
    
    public bool ValidateCastType(FeederMessage message)
    {
        switch (message.CastType)
        {
            case CastType.Broadcast:
                // Validate broadcast is allowed and within limits
                return ValidateBroadcastLimits();
                
            case CastType.Multicast:
            case CastType.Unicast:
                return true;
                
            default:
                return false;
        }
    }
    
    private bool ValidateBroadcastLimits()
    {
        // Check if broadcast is within configured limits
        // This could check current system load, recipient count, etc.
        return true; // Simplified for example
    }
}
```

### Performance Monitoring

```csharp
public class CastTypeMetrics
{
    public class MetricsCollector
    {
        private readonly IMetricsLogger _metricsLogger;
        
        public MetricsCollector(IMetricsLogger metricsLogger)
        {
            _metricsLogger = metricsLogger;
        }
        
        public void RecordMessageProcessing(CastType castType, TimeSpan duration, 
            int recipientCount, bool success)
        {
            var tags = new Dictionary<string, string>
            {
                ["cast_type"] = castType.ToString().ToLowerInvariant(),
                ["success"] = success.ToString().ToLowerInvariant()
            };
            
            _metricsLogger.RecordHistogram("message_processing_duration", 
                duration.TotalMilliseconds, tags);
            _metricsLogger.RecordCounter("message_processing_total", 1, tags);
            _metricsLogger.RecordGauge("message_recipient_count", recipientCount, tags);
        }
        
        public void RecordCastTypeDistribution(CastType castType)
        {
            var tags = new Dictionary<string, string>
            {
                ["cast_type"] = castType.ToString().ToLowerInvariant()
            };
            
            _metricsLogger.RecordCounter("cast_type_usage", 1, tags);
        }
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[TestClass]
public class CastTypeTests
{
    [TestMethod]
    public void CastType_HasExpectedValues()
    {
        Assert.AreEqual(0, (int)CastType.Multicast);
        Assert.AreEqual(1, (int)CastType.Broadcast);
        Assert.AreEqual(2, (int)CastType.Unicast);
    }
    
    [TestMethod]
    public void FeederMessage_DefaultsToMulticast()
    {
        var message = new TestFeederMessage();
        
        Assert.AreEqual(CastType.Multicast, message.CastType);
    }
    
    [TestMethod]
    public void FeederMessage_AllowsCastTypeOverride()
    {
        var message = new TestFeederMessage
        {
            CastType = CastType.Unicast
        };
        
        Assert.AreEqual(CastType.Unicast, message.CastType);
    }
    
    [TestMethod]
    public void CastType_CanBeConvertedToString()
    {
        Assert.AreEqual("Multicast", CastType.Multicast.ToString());
        Assert.AreEqual("Broadcast", CastType.Broadcast.ToString());
        Assert.AreEqual("Unicast", CastType.Unicast.ToString());
    }
    
    private class TestFeederMessage : FeederMessage
    {
        public string TestData { get; set; } = string.Empty;
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class MessageRoutingIntegrationTests
{
    private MessageRoutingService _routingService;
    private Mock<IRecipientRegistry> _recipientRegistryMock;
    private Mock<IMessageBroker> _messageBrokerMock;
    
    [TestInitialize]
    public void Setup()
    {
        _recipientRegistryMock = new Mock<IRecipientRegistry>();
        _messageBrokerMock = new Mock<IMessageBroker>();
        
        _routingService = new MessageRoutingService(
            _messageBrokerMock.Object,
            _recipientRegistryMock.Object,
            Mock.Of<ILogger<MessageRoutingService>>());
    }
    
    [TestMethod]
    public async Task UnicastMessage_DeliveredToSingleRecipient()
    {
        // Arrange
        var recipient = new MessageRecipient { Id = "user1" };
        _recipientRegistryMock
            .Setup(r => r.GetRecipientByIdAsync("user1"))
            .ReturnsAsync(recipient);
        
        var message = new TestMessage 
        { 
            CastType = CastType.Unicast,
            ["RecipientId"] = "user1"
        };
        
        // Act
        var result = await _routingService.SendMessageAsync(message);
        
        // Assert
        Assert.AreEqual(1, result.TotalRecipients);
        Assert.AreEqual(1, result.SuccessfulDeliveries);
        _messageBrokerMock.Verify(
            b => b.SendToRecipientAsync(message, recipient), 
            Times.Once);
    }
    
    [TestMethod]
    public async Task BroadcastMessage_DeliveredToAllRecipients()
    {
        // Arrange
        var recipients = new List<MessageRecipient>
        {
            new() { Id = "user1" },
            new() { Id = "user2" },
            new() { Id = "user3" }
        };
        
        _recipientRegistryMock
            .Setup(r => r.GetAllActiveRecipientsAsync())
            .ReturnsAsync(recipients);
        
        var message = new TestMessage { CastType = CastType.Broadcast };
        
        // Act
        var result = await _routingService.SendMessageAsync(message);
        
        // Assert
        Assert.AreEqual(3, result.TotalRecipients);
        Assert.AreEqual(3, result.SuccessfulDeliveries);
        
        foreach (var recipient in recipients)
        {
            _messageBrokerMock.Verify(
                b => b.SendToRecipientAsync(message, recipient), 
                Times.Once);
        }
    }
    
    private class TestMessage : FeederMessage
    {
        public string TestProperty { get; set; } = string.Empty;
    }
}
```

## Performance Considerations

### Cast Type Performance Characteristics

| Cast Type | Latency | Throughput | Resource Usage | Scalability |
|-----------|---------|------------|----------------|-------------|
| Unicast   | Lowest  | Highest    | Minimal        | Excellent   |
| Multicast | Medium  | Medium     | Moderate       | Good        |
| Broadcast | Highest | Lowest     | Maximum        | Limited     |

### Optimization Strategies

```csharp
public class OptimizedCastTypeHandler
{
    public async Task<DeliveryResult> OptimizedDelivery(FeederMessage message, 
        List<MessageRecipient> recipients)
    {
        switch (message.CastType)
        {
            case CastType.Unicast:
                // Use direct delivery for single recipient
                return await DeliverDirect(message, recipients.First());
                
            case CastType.Multicast:
                // Use batch delivery for multiple recipients
                return await DeliverBatch(message, recipients);
                
            case CastType.Broadcast:
                // Use parallel delivery with concurrency limits
                return await DeliverParallel(message, recipients, maxConcurrency: 10);
                
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private async Task<DeliveryResult> DeliverDirect(FeederMessage message, 
        MessageRecipient recipient)
    {
        // Optimized single-recipient delivery
        await Task.Delay(1); // Simulate fast delivery
        return new DeliveryResult { TotalRecipients = 1, SuccessfulDeliveries = 1 };
    }
    
    private async Task<DeliveryResult> DeliverBatch(FeederMessage message, 
        List<MessageRecipient> recipients)
    {
        // Batch delivery for multiple recipients
        var batchSize = 5;
        var batches = recipients.Chunk(batchSize);
        
        var results = new List<RecipientDeliveryResult>();
        
        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(async recipient =>
            {
                await Task.Delay(2); // Simulate batch processing
                return new RecipientDeliveryResult { Recipient = recipient, Success = true };
            });
            
            var batchResults = await Task.WhenAll(batchTasks);
            results.AddRange(batchResults);
        }
        
        return new DeliveryResult 
        { 
            TotalRecipients = recipients.Count,
            SuccessfulDeliveries = results.Count(r => r.Success),
            Results = results
        };
    }
    
    private async Task<DeliveryResult> DeliverParallel(FeederMessage message, 
        List<MessageRecipient> recipients, int maxConcurrency)
    {
        using var semaphore = new SemaphoreSlim(maxConcurrency);
        
        var deliveryTasks = recipients.Select(async recipient =>
        {
            await semaphore.WaitAsync();
            try
            {
                await Task.Delay(3); // Simulate parallel processing
                return new RecipientDeliveryResult { Recipient = recipient, Success = true };
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        var results = await Task.WhenAll(deliveryTasks);
        
        return new DeliveryResult 
        { 
            TotalRecipients = recipients.Count,
            SuccessfulDeliveries = results.Count(r => r.Success),
            Results = results.ToList()
        };
    }
}
```

## Best Practices

1. **Default to Multicast**: Use multicast as the default for most messaging scenarios
2. **Unicast for Direct Communication**: Use unicast for point-to-point messages and responses
3. **Broadcast Sparingly**: Reserve broadcast for system-wide announcements and critical notifications
4. **Performance Monitoring**: Monitor delivery metrics for each cast type
5. **Failure Handling**: Implement appropriate failure handling for each cast type
6. **Resource Management**: Implement concurrency limits and rate limiting for broadcasts
7. **Testing**: Test all cast types in your message processing pipeline

## Related Components

- **[FeederMessage](../Application/FeederMessage.md)**: Base message class that uses CastType
- **Message Routing Services**: Components that implement cast type routing logic
- **Recipient Management**: Services that manage message recipients and subscriptions
- **Performance Monitoring**: Systems that track delivery metrics by cast type

## See Also

- [Enums System Overview](README.md)
- [FeederMessage Documentation](../Application/FeederMessage.md)
- [Message Routing Patterns](../Patterns/MessageRouting.md)