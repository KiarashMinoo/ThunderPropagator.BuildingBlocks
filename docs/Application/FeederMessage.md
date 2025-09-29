# FeederMessage

The `FeederMessage` abstract class provides a foundation for building dictionary-based message objects with built-in correlation ID support, casting patterns, and thread-safe operations. It serves as a base class for implementing messaging patterns in distributed systems and event-driven architectures.

## Overview

```csharp
[JsonSerialization(CamelCase = false)]
public abstract class FeederMessage : DisposableObject,
    IDictionary<string, object?>,
    IReadOnlyDictionary<string, object?>,
    ICorrelationIdSupport,
    ICloneable,
    ICloneable<IDictionary<string, object?>>
{
    // Core properties
    public object? this[string key] { get; set; }
    public CastType CastType { get; set; }
    public bool IsDeleted { get; set; }
    public string CorrelationId { get; set; }
    
    // Protected helper methods
    protected void SetValue(object? value, [CallerMemberName] string? key = null);
    protected T GetValue<T>([CallerMemberName] string? key = null);
    protected T? GetValueOrNull<T>([CallerMemberName] string? key = null);
    protected T GetValueOrDefault<T>(T @default, [CallerMemberName] string? key = null);
}
```

The `FeederMessage` class combines the flexibility of a dictionary with strongly-typed property access patterns, making it ideal for scenarios where message structure needs to be both dynamic and type-safe.

## Key Features

### Dictionary-Based Storage
- **Thread-Safe Operations**: Built on `ConcurrentDictionary<string, object?>` for multi-threaded scenarios
- **Dynamic Properties**: Supports arbitrary key-value pairs for flexible message content
- **Type-Safe Access**: Generic helper methods for safe type retrieval and conversion
- **CallerMemberName Integration**: Automatic property name resolution using compiler services

### Messaging Capabilities
- **Correlation ID Support**: Built-in correlation tracking for distributed tracing
- **Cast Type Management**: Support for Multicast, Broadcast, and Unicast delivery patterns
- **Deletion Marking**: Soft deletion capability with `IsDeleted` flag
- **Hash Key Support**: Internal hash key for partitioning and routing

### Serialization Features
- **JSON Serialization**: Configured with `JsonSerialization` attribute for serialization control
- **Cross-Platform Compatibility**: Works with both System.Text.Json and Newtonsoft.Json
- **Attribute-Based Control**: Uses `IgnoreMember` for selective serialization
- **Custom Serialization**: Supports custom serialization patterns through attributes

## Properties

### Core Message Properties

#### `CastType CastType`
Specifies the delivery pattern for the message:
- `Multicast` (default): Delivered to multiple specific recipients
- `Broadcast`: Delivered to all available recipients
- `Unicast`: Delivered to a single specific recipient

#### `bool IsDeleted`
Indicates whether the message has been marked for deletion. Useful for soft deletion patterns and tombstone records.

#### `string CorrelationId`
Unique identifier for correlating related messages across distributed operations. Implements `ICorrelationIdSupport` interface.

### Internal Properties

#### `int? HashKey`
Internal property used for message partitioning and routing in distributed systems. Typically managed by infrastructure code.

## Usage Examples

### Basic Message Implementation

```csharp
public class OrderMessage : FeederMessage
{
    public string OrderId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public decimal Amount
    {
        get => GetValueOrDefault(0m);
        set => SetValue(value);
    }
    
    public DateTime CreatedAt
    {
        get => GetValueOrDefault(DateTime.UtcNow);
        set => SetValue(value);
    }
    
    public string CustomerId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public List<OrderItem> Items
    {
        get => GetValueOrDefault(new List<OrderItem>());
        set => SetValue(value);
    }
    
    public OrderStatus Status
    {
        get => GetValueOrDefault(OrderStatus.Pending);
        set => SetValue(value);
    }
}

public class OrderItem
{
    public string ProductId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

### Message Processing Service

```csharp
public class OrderProcessingService
{
    private readonly ILogger<OrderProcessingService> _logger;
    private readonly IMessagePublisher _publisher;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    
    public OrderProcessingService(
        ILogger<OrderProcessingService> logger,
        IMessagePublisher publisher,
        ICorrelationIdProvider correlationIdProvider)
    {
        _logger = logger;
        _publisher = publisher;
        _correlationIdProvider = correlationIdProvider;
    }
    
    public async Task ProcessOrderAsync(CreateOrderRequest request)
    {
        var correlationId = _correlationIdProvider.GetOrCreateCorrelationId();
        
        // Create order message
        var orderMessage = new OrderMessage
        {
            OrderId = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            Amount = request.Items.Sum(i => i.Price * i.Quantity),
            CreatedAt = DateTime.UtcNow,
            Items = request.Items,
            Status = OrderStatus.Pending,
            CorrelationId = correlationId,
            CastType = CastType.Multicast // Send to inventory, payment, and notification services
        };
        
        _logger.LogInformation("Processing order {OrderId} for customer {CustomerId} with correlation {CorrelationId}",
            orderMessage.OrderId, orderMessage.CustomerId, correlationId);
        
        try
        {
            // Validate order
            await ValidateOrderAsync(orderMessage);
            
            // Publish order created event
            await _publisher.PublishAsync("order.created", orderMessage);
            
            _logger.LogInformation("Order {OrderId} created successfully", orderMessage.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", orderMessage.OrderId);
            
            // Mark order as failed and publish failure event
            orderMessage.Status = OrderStatus.Cancelled;
            orderMessage.CastType = CastType.Broadcast; // Notify all interested services
            
            await _publisher.PublishAsync("order.failed", orderMessage);
            throw;
        }
    }
    
    public async Task HandleOrderStatusUpdateAsync(OrderStatusUpdateMessage statusUpdate)
    {
        using var correlationScope = _correlationIdProvider.BeginScope(statusUpdate.CorrelationId);
        
        _logger.LogInformation("Handling order status update for {OrderId}: {OldStatus} -> {NewStatus}",
            statusUpdate.OrderId, statusUpdate.PreviousStatus, statusUpdate.NewStatus);
        
        // Create status change message
        var statusMessage = new OrderMessage
        {
            OrderId = statusUpdate.OrderId,
            Status = statusUpdate.NewStatus,
            CorrelationId = statusUpdate.CorrelationId,
            CastType = statusUpdate.NotifyCustomer ? CastType.Broadcast : CastType.Multicast
        };
        
        // Add status change metadata
        statusMessage["PreviousStatus"] = statusUpdate.PreviousStatus;
        statusMessage["StatusChangedAt"] = DateTime.UtcNow;
        statusMessage["StatusChangedBy"] = statusUpdate.UpdatedBy;
        statusMessage["StatusChangeReason"] = statusUpdate.Reason;
        
        await _publisher.PublishAsync("order.status.changed", statusMessage);
        
        // Handle specific status transitions
        switch (statusUpdate.NewStatus)
        {
            case OrderStatus.Confirmed:
                await HandleOrderConfirmedAsync(statusMessage);
                break;
                
            case OrderStatus.Shipped:
                await HandleOrderShippedAsync(statusMessage);
                break;
                
            case OrderStatus.Delivered:
                await HandleOrderDeliveredAsync(statusMessage);
                break;
                
            case OrderStatus.Cancelled:
                await HandleOrderCancelledAsync(statusMessage);
                break;
        }
    }
    
    private async Task ValidateOrderAsync(OrderMessage order)
    {
        if (string.IsNullOrEmpty(order.CustomerId))
            throw new ArgumentException("Customer ID is required");
        
        if (order.Items.Count == 0)
            throw new ArgumentException("Order must contain at least one item");
        
        if (order.Amount <= 0)
            throw new ArgumentException("Order amount must be positive");
        
        // Additional validation logic
        await Task.CompletedTask;
    }
    
    private async Task HandleOrderConfirmedAsync(OrderMessage order)
    {
        // Send to inventory service for reservation
        var inventoryMessage = new InventoryReservationMessage
        {
            OrderId = order.OrderId,
            Items = order.Items,
            CorrelationId = order.CorrelationId,
            CastType = CastType.Unicast // Send only to inventory service
        };
        
        await _publisher.PublishAsync("inventory.reserve", inventoryMessage);
    }
    
    private async Task HandleOrderShippedAsync(OrderMessage order)
    {
        // Send tracking notification
        var trackingMessage = new TrackingNotificationMessage
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            TrackingNumber = order.GetValueOrDefault<string>("TrackingNumber", ""),
            CorrelationId = order.CorrelationId,
            CastType = CastType.Broadcast // Notify customer and support systems
        };
        
        await _publisher.PublishAsync("order.tracking", trackingMessage);
    }
    
    private async Task HandleOrderDeliveredAsync(OrderMessage order)
    {
        // Request customer feedback
        var feedbackMessage = new FeedbackRequestMessage
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            CorrelationId = order.CorrelationId,
            CastType = CastType.Unicast // Send only to feedback service
        };
        
        await _publisher.PublishAsync("feedback.request", feedbackMessage);
    }
    
    private async Task HandleOrderCancelledAsync(OrderMessage order)
    {
        // Release inventory and process refund
        var cancellationMessage = new OrderCancellationMessage
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Amount = order.Amount,
            Reason = order.GetValueOrDefault<string>("StatusChangeReason", ""),
            CorrelationId = order.CorrelationId,
            CastType = CastType.Multicast // Send to inventory, payment, and notification services
        };
        
        await _publisher.PublishAsync("order.cancelled", cancellationMessage);
    }
}

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = "";
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderStatusUpdateMessage : FeederMessage
{
    public string OrderId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public OrderStatus PreviousStatus
    {
        get => GetValueOrDefault(OrderStatus.Pending);
        set => SetValue(value);
    }
    
    public OrderStatus NewStatus
    {
        get => GetValueOrDefault(OrderStatus.Pending);
        set => SetValue(value);
    }
    
    public string UpdatedBy
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public string Reason
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public bool NotifyCustomer
    {
        get => GetValueOrDefault(true);
        set => SetValue(value);
    }
}

public class InventoryReservationMessage : FeederMessage
{
    public string OrderId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public List<OrderItem> Items
    {
        get => GetValueOrDefault(new List<OrderItem>());
        set => SetValue(value);
    }
}

public class TrackingNotificationMessage : FeederMessage
{
    public string OrderId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public string CustomerId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public string TrackingNumber
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
}

public class FeedbackRequestMessage : FeederMessage
{
    public string OrderId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public string CustomerId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
}

public class OrderCancellationMessage : FeederMessage
{
    public string OrderId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public string CustomerId
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public decimal Amount
    {
        get => GetValueOrDefault(0m);
        set => SetValue(value);
    }
    
    public string Reason
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
}
```

### Dynamic Message Content

```csharp
public class FlexibleEventMessage : FeederMessage
{
    public string EventType
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public DateTime Timestamp
    {
        get => GetValueOrDefault(DateTime.UtcNow);
        set => SetValue(value);
    }
    
    public string Source
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    // Dynamic property access
    public void SetProperty<T>(string name, T value)
    {
        this[name] = value;
    }
    
    public T? GetProperty<T>(string name)
    {
        return this.TryGetValue(name, out var value) && value is T typedValue ? typedValue : default;
    }
    
    public bool HasProperty(string name)
    {
        return this.ContainsKey(name);
    }
    
    public void RemoveProperty(string name)
    {
        if (this is IDictionary<string, object?> dict)
        {
            dict.Remove(name);
        }
    }
}

public class DynamicEventProcessor
{
    private readonly ILogger<DynamicEventProcessor> _logger;
    
    public DynamicEventProcessor(ILogger<DynamicEventProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task ProcessEventAsync(FlexibleEventMessage eventMessage)
    {
        _logger.LogInformation("Processing event {EventType} from {Source} with correlation {CorrelationId}",
            eventMessage.EventType, eventMessage.Source, eventMessage.CorrelationId);
        
        switch (eventMessage.EventType)
        {
            case "user.registered":
                await ProcessUserRegisteredAsync(eventMessage);
                break;
                
            case "user.login":
                await ProcessUserLoginAsync(eventMessage);
                break;
                
            case "order.placed":
                await ProcessOrderPlacedAsync(eventMessage);
                break;
                
            case "payment.processed":
                await ProcessPaymentProcessedAsync(eventMessage);
                break;
                
            default:
                await ProcessGenericEventAsync(eventMessage);
                break;
        }
    }
    
    private async Task ProcessUserRegisteredAsync(FlexibleEventMessage eventMessage)
    {
        var userId = eventMessage.GetProperty<string>("UserId");
        var email = eventMessage.GetProperty<string>("Email");
        var registrationMethod = eventMessage.GetProperty<string>("RegistrationMethod");
        
        _logger.LogInformation("User {UserId} registered with email {Email} via {Method}",
            userId, email, registrationMethod);
        
        // Create welcome message
        var welcomeMessage = new FlexibleEventMessage
        {
            EventType = "user.welcome",
            Source = "user-service",
            CorrelationId = eventMessage.CorrelationId,
            CastType = CastType.Unicast
        };
        
        welcomeMessage.SetProperty("UserId", userId);
        welcomeMessage.SetProperty("Email", email);
        welcomeMessage.SetProperty("WelcomeTemplate", "new-user-welcome");
        
        await PublishEventAsync(welcomeMessage);
    }
    
    private async Task ProcessUserLoginAsync(FlexibleEventMessage eventMessage)
    {
        var userId = eventMessage.GetProperty<string>("UserId");
        var ipAddress = eventMessage.GetProperty<string>("IpAddress");
        var userAgent = eventMessage.GetProperty<string>("UserAgent");
        var isSuccessful = eventMessage.GetProperty<bool>("IsSuccessful");
        
        if (isSuccessful == true)
        {
            _logger.LogInformation("Successful login for user {UserId} from {IpAddress}",
                userId, ipAddress);
            
            // Check for suspicious activity
            if (await IsLoginSuspiciousAsync(userId, ipAddress, userAgent))
            {
                var securityAlert = new FlexibleEventMessage
                {
                    EventType = "security.suspicious-login",
                    Source = "security-service",
                    CorrelationId = eventMessage.CorrelationId,
                    CastType = CastType.Broadcast
                };
                
                securityAlert.SetProperty("UserId", userId);
                securityAlert.SetProperty("IpAddress", ipAddress);
                securityAlert.SetProperty("UserAgent", userAgent);
                securityAlert.SetProperty("Severity", "Medium");
                
                await PublishEventAsync(securityAlert);
            }
        }
        else
        {
            _logger.LogWarning("Failed login attempt for user {UserId} from {IpAddress}",
                userId, ipAddress);
        }
    }
    
    private async Task ProcessOrderPlacedAsync(FlexibleEventMessage eventMessage)
    {
        var orderId = eventMessage.GetProperty<string>("OrderId");
        var customerId = eventMessage.GetProperty<string>("CustomerId");
        var amount = eventMessage.GetProperty<decimal>("Amount");
        
        _logger.LogInformation("Order {OrderId} placed by customer {CustomerId} for {Amount:C}",
            orderId, customerId, amount);
        
        // Create order confirmation message
        var confirmationMessage = new FlexibleEventMessage
        {
            EventType = "order.confirmation",
            Source = "order-service",
            CorrelationId = eventMessage.CorrelationId,
            CastType = CastType.Multicast
        };
        
        confirmationMessage.SetProperty("OrderId", orderId);
        confirmationMessage.SetProperty("CustomerId", customerId);
        confirmationMessage.SetProperty("Amount", amount);
        confirmationMessage.SetProperty("EstimatedDelivery", DateTime.UtcNow.AddDays(3));
        
        await PublishEventAsync(confirmationMessage);
    }
    
    private async Task ProcessPaymentProcessedAsync(FlexibleEventMessage eventMessage)
    {
        var paymentId = eventMessage.GetProperty<string>("PaymentId");
        var orderId = eventMessage.GetProperty<string>("OrderId");
        var amount = eventMessage.GetProperty<decimal>("Amount");
        var status = eventMessage.GetProperty<string>("Status");
        
        _logger.LogInformation("Payment {PaymentId} for order {OrderId} processed with status {Status}",
            paymentId, orderId, status);
        
        if (status == "Success")
        {
            var paymentConfirmation = new FlexibleEventMessage
            {
                EventType = "payment.confirmed",
                Source = "payment-service",
                CorrelationId = eventMessage.CorrelationId,
                CastType = CastType.Multicast
            };
            
            paymentConfirmation.SetProperty("PaymentId", paymentId);
            paymentConfirmation.SetProperty("OrderId", orderId);
            paymentConfirmation.SetProperty("Amount", amount);
            
            await PublishEventAsync(paymentConfirmation);
        }
    }
    
    private async Task ProcessGenericEventAsync(FlexibleEventMessage eventMessage)
    {
        _logger.LogInformation("Processing generic event {EventType} with {PropertyCount} properties",
            eventMessage.EventType, eventMessage.Count);
        
        // Log all properties for debugging
        foreach (var kvp in eventMessage)
        {
            _logger.LogDebug("Event property: {Key} = {Value}", kvp.Key, kvp.Value);
        }
        
        await Task.CompletedTask;
    }
    
    private async Task<bool> IsLoginSuspiciousAsync(string? userId, string? ipAddress, string? userAgent)
    {
        // Implement suspicious login detection logic
        await Task.Delay(10); // Simulate analysis
        return false; // Simplified for example
    }
    
    private async Task PublishEventAsync(FlexibleEventMessage eventMessage)
    {
        // Simulate event publishing
        await Task.Delay(10);
        _logger.LogDebug("Published event {EventType} with correlation {CorrelationId}",
            eventMessage.EventType, eventMessage.CorrelationId);
    }
}
```

### Message Serialization and Storage

```csharp
public class MessageSerializer
{
    private readonly JsonSerializerOptions _systemTextJsonOptions;
    private readonly JsonSerializerSettings _newtonsoftSettings;
    
    public MessageSerializer()
    {
        _systemTextJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        _newtonsoftSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };
    }
    
    public string SerializeWithSystemTextJson<T>(T message) where T : FeederMessage
    {
        return JsonSerializer.Serialize(message, _systemTextJsonOptions);
    }
    
    public T? DeserializeWithSystemTextJson<T>(string json) where T : FeederMessage
    {
        return JsonSerializer.Deserialize<T>(json, _systemTextJsonOptions);
    }
    
    public string SerializeWithNewtonsoft<T>(T message) where T : FeederMessage
    {
        return JsonConvert.SerializeObject(message, _newtonsoftSettings);
    }
    
    public T? DeserializeWithNewtonsoft<T>(string json) where T : FeederMessage
    {
        return JsonConvert.DeserializeObject<T>(json, _newtonsoftSettings);
    }
}

public class MessageRepository
{
    private readonly ILogger<MessageRepository> _logger;
    private readonly MessageSerializer _serializer;
    private readonly ConcurrentDictionary<string, string> _messageStore;
    
    public MessageRepository(ILogger<MessageRepository> logger, MessageSerializer serializer)
    {
        _logger = logger;
        _serializer = serializer;
        _messageStore = new ConcurrentDictionary<string, string>();
    }
    
    public async Task StoreMessageAsync<T>(T message) where T : FeederMessage
    {
        var messageId = Guid.NewGuid().ToString();
        var json = _serializer.SerializeWithSystemTextJson(message);
        
        _messageStore[messageId] = json;
        
        // Add metadata
        message["MessageId"] = messageId;
        message["StoredAt"] = DateTime.UtcNow;
        
        _logger.LogInformation("Stored message {MessageId} of type {MessageType}",
            messageId, typeof(T).Name);
        
        await Task.CompletedTask;
    }
    
    public async Task<T?> RetrieveMessageAsync<T>(string messageId) where T : FeederMessage
    {
        if (_messageStore.TryGetValue(messageId, out var json))
        {
            var message = _serializer.DeserializeWithSystemTextJson<T>(json);
            
            _logger.LogInformation("Retrieved message {MessageId} of type {MessageType}",
                messageId, typeof(T).Name);
            
            return message;
        }
        
        _logger.LogWarning("Message {MessageId} not found", messageId);
        return default;
    }
    
    public async Task<List<T>> GetMessagesByCorrelationIdAsync<T>(string correlationId) where T : FeederMessage
    {
        var messages = new List<T>();
        
        foreach (var kvp in _messageStore)
        {
            try
            {
                var message = _serializer.DeserializeWithSystemTextJson<T>(kvp.Value);
                if (message?.CorrelationId == correlationId)
                {
                    messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize message {MessageId}", kvp.Key);
            }
        }
        
        _logger.LogInformation("Found {Count} messages with correlation ID {CorrelationId}",
            messages.Count, correlationId);
        
        return await Task.FromResult(messages);
    }
    
    public async Task<MessageStatistics> GetMessageStatisticsAsync()
    {
        var statistics = new MessageStatistics
        {
            TotalMessages = _messageStore.Count,
            MessagesByType = new Dictionary<string, int>(),
            MessagesByCorrelationId = new Dictionary<string, int>()
        };
        
        foreach (var json in _messageStore.Values)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                
                // Extract type information
                if (root.TryGetProperty("$type", out var typeElement))
                {
                    var typeName = typeElement.GetString() ?? "Unknown";
                    statistics.MessagesByType[typeName] = 
                        statistics.MessagesByType.GetValueOrDefault(typeName, 0) + 1;
                }
                
                // Extract correlation ID
                if (root.TryGetProperty("correlationId", out var correlationElement))
                {
                    var correlationId = correlationElement.GetString() ?? "Unknown";
                    statistics.MessagesByCorrelationId[correlationId] = 
                        statistics.MessagesByCorrelationId.GetValueOrDefault(correlationId, 0) + 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to analyze message for statistics");
            }
        }
        
        statistics.UniqueCorrelationIds = statistics.MessagesByCorrelationId.Count;
        statistics.UniqueMessageTypes = statistics.MessagesByType.Count;
        
        return await Task.FromResult(statistics);
    }
}

public class MessageStatistics
{
    public int TotalMessages { get; set; }
    public int UniqueMessageTypes { get; set; }
    public int UniqueCorrelationIds { get; set; }
    public Dictionary<string, int> MessagesByType { get; set; } = new();
    public Dictionary<string, int> MessagesByCorrelationId { get; set; } = new();
}
```

### Message Routing and Delivery

```csharp
public interface IMessageRouter
{
    Task RouteMessageAsync<T>(T message) where T : FeederMessage;
    Task RegisterHandlerAsync<T>(Func<T, Task> handler) where T : FeederMessage;
}

public class MessageRouter : IMessageRouter
{
    private readonly ILogger<MessageRouter> _logger;
    private readonly ConcurrentDictionary<Type, List<Func<FeederMessage, Task>>> _handlers;
    
    public MessageRouter(ILogger<MessageRouter> logger)
    {
        _logger = logger;
        _handlers = new ConcurrentDictionary<Type, List<Func<FeederMessage, Task>>>();
    }
    
    public async Task RouteMessageAsync<T>(T message) where T : FeederMessage
    {
        ArgumentNullException.ThrowIfNull(message);
        
        var messageType = typeof(T);
        
        _logger.LogInformation("Routing message of type {MessageType} with cast type {CastType} and correlation {CorrelationId}",
            messageType.Name, message.CastType, message.CorrelationId);
        
        if (_handlers.TryGetValue(messageType, out var handlerList))
        {
            var handlersToExecute = SelectHandlersBasedOnCastType(handlerList, message.CastType);
            
            _logger.LogInformation("Executing {HandlerCount} handlers for message type {MessageType}",
                handlersToExecute.Count, messageType.Name);
            
            var tasks = handlersToExecute.Select(handler => ExecuteHandlerSafely(handler, message));
            await Task.WhenAll(tasks);
        }
        else
        {
            _logger.LogWarning("No handlers registered for message type {MessageType}", messageType.Name);
        }
    }
    
    public async Task RegisterHandlerAsync<T>(Func<T, Task> handler) where T : FeederMessage
    {
        ArgumentNullException.ThrowIfNull(handler);
        
        var messageType = typeof(T);
        
        // Wrap the typed handler in a generic wrapper
        var wrappedHandler = new Func<FeederMessage, Task>(message => handler((T)message));
        
        _handlers.AddOrUpdate(
            messageType,
            new List<Func<FeederMessage, Task>> { wrappedHandler },
            (key, existing) =>
            {
                existing.Add(wrappedHandler);
                return existing;
            });
        
        _logger.LogInformation("Registered handler for message type {MessageType}", messageType.Name);
        await Task.CompletedTask;
    }
    
    private List<Func<FeederMessage, Task>> SelectHandlersBasedOnCastType(
        List<Func<FeederMessage, Task>> handlers, 
        CastType castType)
    {
        return castType switch
        {
            CastType.Unicast => handlers.Take(1).ToList(), // Only first handler
            CastType.Multicast => handlers.ToList(),       // All handlers
            CastType.Broadcast => handlers.ToList(),       // All handlers
            _ => handlers.ToList()
        };
    }
    
    private async Task ExecuteHandlerSafely(Func<FeederMessage, Task> handler, FeederMessage message)
    {
        try
        {
            await handler(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handler execution failed for message type {MessageType} with correlation {CorrelationId}",
                message.GetType().Name, message.CorrelationId);
        }
    }
}

public class MessageRouterExample
{
    public static async Task DemonstrateMessageRoutingAsync()
    {
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MessageRouter>();
        var router = new MessageRouter(logger);
        
        // Register handlers
        await router.RegisterHandlerAsync<OrderMessage>(async order =>
        {
            Console.WriteLine($"Order handler 1: Processing order {order.OrderId}");
            await Task.Delay(100);
        });
        
        await router.RegisterHandlerAsync<OrderMessage>(async order =>
        {
            Console.WriteLine($"Order handler 2: Validating order {order.OrderId}");
            await Task.Delay(150);
        });
        
        await router.RegisterHandlerAsync<OrderMessage>(async order =>
        {
            Console.WriteLine($"Order handler 3: Logging order {order.OrderId}");
            await Task.Delay(50);
        });
        
        // Create and route messages with different cast types
        var order1 = new OrderMessage
        {
            OrderId = "ORDER-001",
            CustomerId = "CUST-001",
            Amount = 99.99m,
            CastType = CastType.Unicast,
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        var order2 = new OrderMessage
        {
            OrderId = "ORDER-002",
            CustomerId = "CUST-002",
            Amount = 149.99m,
            CastType = CastType.Multicast,
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        Console.WriteLine("Routing order with Unicast (should execute 1 handler):");
        await router.RouteMessageAsync(order1);
        
        Console.WriteLine("\nRouting order with Multicast (should execute all handlers):");
        await router.RouteMessageAsync(order2);
    }
}
```

## Property Access Patterns

### Type-Safe Property Access

```csharp
public static class FeederMessageExtensions
{
    public static void SetTypedProperty<T>(this FeederMessage message, string key, T value)
    {
        message[key] = value;
    }
    
    public static T? GetTypedProperty<T>(this FeederMessage message, string key)
    {
        return message.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
    }
    
    public static T GetTypedPropertyOrDefault<T>(this FeederMessage message, string key, T defaultValue)
    {
        return GetTypedProperty<T>(message, key) ?? defaultValue;
    }
    
    public static bool HasTypedProperty<T>(this FeederMessage message, string key)
    {
        return message.TryGetValue(key, out var value) && value is T;
    }
    
    public static void RemoveProperty(this FeederMessage message, string key)
    {
        if (message is IDictionary<string, object?> dict)
        {
            dict.Remove(key);
        }
    }
    
    public static Dictionary<string, object?> ToStringDictionary(this FeederMessage message)
    {
        return message.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    public static void CopyPropertiesFrom(this FeederMessage target, FeederMessage source, params string[] propertiesToCopy)
    {
        if (propertiesToCopy.Length == 0)
        {
            // Copy all properties except system properties
            var systemProperties = new[] { "CorrelationId", "CastType", "IsDeleted", "HashKey" };
            propertiesToCopy = source.Keys.Except(systemProperties).ToArray();
        }
        
        foreach (var property in propertiesToCopy)
        {
            if (source.TryGetValue(property, out var value))
            {
                target[property] = value;
            }
        }
    }
}
```

## Thread Safety and Performance

### Thread-Safe Operations

```csharp
public class ThreadSafeMessageOperations
{
    public static void DemonstrateThreadSafety()
    {
        var message = new OrderMessage
        {
            OrderId = "ORDER-THREAD-TEST",
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        var tasks = new List<Task>();
        var random = new Random();
        
        // Simulate concurrent property updates
        for (int i = 0; i < 10; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var propertyName = $"Property_{taskId}_{j}";
                    var propertyValue = random.Next(1000);
                    
                    // Thread-safe property setting
                    message[propertyName] = propertyValue;
                    
                    // Thread-safe property getting
                    var retrievedValue = message.GetTypedProperty<int>(propertyName);
                    
                    if (retrievedValue != propertyValue)
                    {
                        Console.WriteLine($"Thread safety issue detected in task {taskId}");
                    }
                    
                    Thread.Sleep(1); // Simulate processing time
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        Console.WriteLine($"Thread safety test completed. Message has {message.Count} properties.");
    }
}
```

### Performance Monitoring

```csharp
public class MessagePerformanceMonitor
{
    private readonly ILogger<MessagePerformanceMonitor> _logger;
    
    public MessagePerformanceMonitor(ILogger<MessagePerformanceMonitor> logger)
    {
        _logger = logger;
    }
    
    public async Task<PerformanceReport> MeasureMessageOperationsAsync()
    {
        var report = new PerformanceReport();
        var stopwatch = Stopwatch.StartNew();
        
        // Measure message creation
        stopwatch.Restart();
        var messages = CreateTestMessages(1000);
        report.MessageCreationTime = stopwatch.Elapsed;
        
        // Measure property access
        stopwatch.Restart();
        foreach (var message in messages)
        {
            _ = message.OrderId;
            _ = message.Amount;
            _ = message.CustomerId;
        }
        report.PropertyAccessTime = stopwatch.Elapsed;
        
        // Measure serialization
        var serializer = new MessageSerializer();
        stopwatch.Restart();
        var serializedMessages = messages.Select(m => serializer.SerializeWithSystemTextJson(m)).ToList();
        report.SerializationTime = stopwatch.Elapsed;
        
        // Measure deserialization
        stopwatch.Restart();
        var deserializedMessages = serializedMessages
            .Select(json => serializer.DeserializeWithSystemTextJson<OrderMessage>(json))
            .ToList();
        report.DeserializationTime = stopwatch.Elapsed;
        
        // Measure dictionary operations
        var testMessage = messages.First();
        stopwatch.Restart();
        for (int i = 0; i < 10000; i++)
        {
            testMessage[$"TestProperty_{i}"] = i;
        }
        report.DictionarySetTime = stopwatch.Elapsed;
        
        stopwatch.Restart();
        for (int i = 0; i < 10000; i++)
        {
            _ = testMessage.GetTypedProperty<int>($"TestProperty_{i}");
        }
        report.DictionaryGetTime = stopwatch.Elapsed;
        
        report.TotalMessages = messages.Count;
        report.AverageMessageSize = serializedMessages.Average(s => s.Length);
        
        _logger.LogInformation("Performance measurement completed: {@Report}", report);
        
        return await Task.FromResult(report);
    }
    
    private List<OrderMessage> CreateTestMessages(int count)
    {
        var messages = new List<OrderMessage>();
        var random = new Random();
        
        for (int i = 0; i < count; i++)
        {
            var message = new OrderMessage
            {
                OrderId = $"ORDER-{i:D6}",
                CustomerId = $"CUST-{random.Next(1000):D4}",
                Amount = random.Next(10, 1000),
                CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(1440)),
                Status = (OrderStatus)random.Next(Enum.GetValues<OrderStatus>().Length),
                CorrelationId = Guid.NewGuid().ToString()
            };
            
            // Add some dynamic properties
            message[$"DynamicProperty_{i}"] = $"Value_{i}";
            message[$"RandomData_{i}"] = random.Next(10000);
            
            messages.Add(message);
        }
        
        return messages;
    }
}

public class PerformanceReport
{
    public TimeSpan MessageCreationTime { get; set; }
    public TimeSpan PropertyAccessTime { get; set; }
    public TimeSpan SerializationTime { get; set; }
    public TimeSpan DeserializationTime { get; set; }
    public TimeSpan DictionarySetTime { get; set; }
    public TimeSpan DictionaryGetTime { get; set; }
    public int TotalMessages { get; set; }
    public double AverageMessageSize { get; set; }
}
```

## Best Practices

### 1. **Property Design Patterns**

```csharp
public class WellDesignedMessage : FeederMessage
{
    // Use descriptive property names
    public string OrderIdentifier
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    // Provide default values for optional properties
    public DateTime CreatedTimestamp
    {
        get => GetValueOrDefault(DateTime.UtcNow);
        set => SetValue(value);
    }
    
    // Use strongly-typed enums for status fields
    public OrderProcessingStatus ProcessingStatus
    {
        get => GetValueOrDefault(OrderProcessingStatus.Pending);
        set => SetValue(value);
    }
    
    // Group related properties with prefixes
    public string ShippingAddressLine1
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    public string ShippingAddressLine2
    {
        get => GetValueOrDefault(string.Empty);
        set => SetValue(value);
    }
    
    // Provide computed properties when appropriate
    public string FullShippingAddress =>
        string.Join(", ", new[] { ShippingAddressLine1, ShippingAddressLine2 }.Where(s => !string.IsNullOrEmpty(s)));
    
    // Use nullable types for optional data
    public DateTime? ShippedAt
    {
        get => GetValueOrNull<DateTime>();
        set => SetValue(value);
    }
    
    // Validate critical properties
    public decimal TotalAmount
    {
        get => GetValueOrDefault(0m);
        set
        {
            if (value < 0)
                throw new ArgumentException("Total amount cannot be negative", nameof(value));
            SetValue(value);
        }
    }
}

public enum OrderProcessingStatus
{
    Pending,
    Validated,
    PaymentProcessing,
    PaymentConfirmed,
    InFulfillment,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}
```

### 2. **Error Handling and Validation**

```csharp
public static class MessageValidation
{
    public static void ValidateRequiredProperties(FeederMessage message, params string[] requiredProperties)
    {
        var missingProperties = new List<string>();
        
        foreach (var property in requiredProperties)
        {
            if (!message.ContainsKey(property) || message[property] == null)
            {
                missingProperties.Add(property);
            }
        }
        
        if (missingProperties.Count > 0)
        {
            throw new ArgumentException($"Required properties missing: {string.Join(", ", missingProperties)}");
        }
    }
    
    public static void ValidateMessageIntegrity<T>(T message) where T : FeederMessage
    {
        if (string.IsNullOrEmpty(message.CorrelationId))
        {
            throw new ArgumentException("CorrelationId is required for all messages");
        }
        
        if (message.IsDeleted && message.CastType != CastType.Broadcast)
        {
            throw new ArgumentException("Deleted messages should use Broadcast cast type");
        }
    }
    
    public static void ValidateBusinessRules<T>(T message) where T : FeederMessage
    {
        // Implement message-specific business rules
        switch (message)
        {
            case OrderMessage order:
                ValidateOrderMessage(order);
                break;
                
            // Add other message types as needed
        }
    }
    
    private static void ValidateOrderMessage(OrderMessage order)
    {
        ValidateRequiredProperties(order, nameof(order.OrderId), nameof(order.CustomerId));
        
        if (order.Amount <= 0)
        {
            throw new ArgumentException("Order amount must be positive");
        }
        
        if (order.Items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item");
        }
    }
}
```

### 3. **Resource Management**

```csharp
public class ManagedMessageProcessor : IDisposable
{
    private readonly List<FeederMessage> _messages;
    private bool _disposed;
    
    public ManagedMessageProcessor()
    {
        _messages = new List<FeederMessage>();
    }
    
    public void ProcessMessage(FeederMessage message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        try
        {
            _messages.Add(message);
            // Process the message
        }
        catch (Exception ex)
        {
            // Ensure message is properly disposed on error
            message.Dispose();
            throw;
        }
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            // Dispose all messages
            foreach (var message in _messages)
            {
                message.Dispose();
            }
            
            _messages.Clear();
            _disposed = true;
        }
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class FeederMessageTests
{
    private OrderMessage _orderMessage;
    
    [SetUp]
    public void Setup()
    {
        _orderMessage = new OrderMessage
        {
            OrderId = "TEST-001",
            CustomerId = "CUST-001",
            Amount = 99.99m,
            CorrelationId = Guid.NewGuid().ToString()
        };
    }
    
    [Test]
    public void Constructor_InitializesWithDefaultCastType()
    {
        var message = new OrderMessage();
        Assert.That(message.CastType, Is.EqualTo(CastType.Multicast));
    }
    
    [Test]
    public void PropertyAccess_GetAndSet_WorksCorrectly()
    {
        // Act
        _orderMessage.OrderId = "UPDATED-001";
        var retrievedOrderId = _orderMessage.OrderId;
        
        // Assert
        Assert.That(retrievedOrderId, Is.EqualTo("UPDATED-001"));
    }
    
    [Test]
    public void DictionaryAccess_GetAndSet_WorksCorrectly()
    {
        // Act
        _orderMessage["CustomProperty"] = "CustomValue";
        var retrievedValue = _orderMessage["CustomProperty"];
        
        // Assert
        Assert.That(retrievedValue, Is.EqualTo("CustomValue"));
    }
    
    [Test]
    public void GetValueOrDefault_WithExistingProperty_ReturnsValue()
    {
        // Arrange
        _orderMessage.Amount = 150.50m;
        
        // Act
        var amount = _orderMessage.GetValueOrDefault(0m);
        
        // Assert
        Assert.That(amount, Is.EqualTo(150.50m));
    }
    
    [Test]
    public void GetValueOrDefault_WithMissingProperty_ReturnsDefault()
    {
        // Act
        var nonExistentValue = _orderMessage.GetValueOrDefault(999m);
        
        // Assert
        Assert.That(nonExistentValue, Is.EqualTo(999m));
    }
    
    [Test]
    public void ThreadSafety_ConcurrentAccess_DoesNotCorruptData()
    {
        // Arrange
        var tasks = new List<Task>();
        var errors = new ConcurrentBag<Exception>();
        
        // Act
        for (int i = 0; i < 10; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        _orderMessage[$"Property_{taskId}_{j}"] = $"Value_{taskId}_{j}";
                        var value = _orderMessage[$"Property_{taskId}_{j}"];
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert
        Assert.That(errors, Is.Empty);
        Assert.That(_orderMessage.Count, Is.GreaterThan(1000)); // Should have many properties
    }
    
    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        // Act
        var clonedDict = ((ICloneable<IDictionary<string, object?>>)_orderMessage).Clone();
        
        // Modify original
        _orderMessage.Amount = 200m;
        
        // Assert
        Assert.That(clonedDict["Amount"], Is.EqualTo(99.99m)); // Clone should be unchanged
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class FeederMessageIntegrationTests
{
    [Test]
    public async Task MessageSerialization_RoundTrip_PreservesAllData()
    {
        // Arrange
        var originalMessage = new OrderMessage
        {
            OrderId = "INT-TEST-001",
            CustomerId = "CUST-INT-001",
            Amount = 199.99m,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Confirmed,
            CorrelationId = Guid.NewGuid().ToString(),
            CastType = CastType.Broadcast
        };
        
        // Add dynamic properties
        originalMessage["CustomProperty"] = "CustomValue";
        originalMessage["NumericProperty"] = 42;
        originalMessage["BooleanProperty"] = true;
        
        var serializer = new MessageSerializer();
        
        // Act
        var json = serializer.SerializeWithSystemTextJson(originalMessage);
        var deserializedMessage = serializer.DeserializeWithSystemTextJson<OrderMessage>(json);
        
        // Assert
        Assert.That(deserializedMessage, Is.Not.Null);
        Assert.That(deserializedMessage.OrderId, Is.EqualTo(originalMessage.OrderId));
        Assert.That(deserializedMessage.Amount, Is.EqualTo(originalMessage.Amount));
        Assert.That(deserializedMessage.CorrelationId, Is.EqualTo(originalMessage.CorrelationId));
        Assert.That(deserializedMessage["CustomProperty"], Is.EqualTo("CustomValue"));
        Assert.That(deserializedMessage["NumericProperty"], Is.EqualTo(42));
        Assert.That(deserializedMessage["BooleanProperty"], Is.EqualTo(true));
    }
    
    [Test]
    public async Task MessageRouting_WithDifferentCastTypes_RoutesCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MessageRouter>();
        var router = new MessageRouter(logger);
        var handlerExecutionCounts = new ConcurrentDictionary<string, int>();
        
        // Register multiple handlers
        await router.RegisterHandlerAsync<OrderMessage>(async order =>
        {
            handlerExecutionCounts.AddOrUpdate("Handler1", 1, (key, value) => value + 1);
            await Task.Delay(10);
        });
        
        await router.RegisterHandlerAsync<OrderMessage>(async order =>
        {
            handlerExecutionCounts.AddOrUpdate("Handler2", 1, (key, value) => value + 1);
            await Task.Delay(10);
        });
        
        await router.RegisterHandlerAsync<OrderMessage>(async order =>
        {
            handlerExecutionCounts.AddOrUpdate("Handler3", 1, (key, value) => value + 1);
            await Task.Delay(10);
        });
        
        // Test Unicast (should execute only 1 handler)
        var unicastMessage = new OrderMessage
        {
            OrderId = "UNICAST-001",
            CastType = CastType.Unicast,
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        await router.RouteMessageAsync(unicastMessage);
        var unicastExecutions = handlerExecutionCounts.Values.Sum();
        
        // Reset counters
        handlerExecutionCounts.Clear();
        
        // Test Multicast (should execute all handlers)
        var multicastMessage = new OrderMessage
        {
            OrderId = "MULTICAST-001",
            CastType = CastType.Multicast,
            CorrelationId = Guid.NewGuid().ToString()
        };
        
        await router.RouteMessageAsync(multicastMessage);
        var multicastExecutions = handlerExecutionCounts.Values.Sum();
        
        // Assert
        Assert.That(unicastExecutions, Is.EqualTo(1));
        Assert.That(multicastExecutions, Is.EqualTo(3));
    }
}
```

## See Also

- [DisposableObject](Objects/DisposableObject.md) - Base disposable object implementation
- [ICorrelationIdSupport](CorrelationId/ICorrelationIdSupport.md) - Correlation ID support interface
- [CastType](Enums/CastType.md) - Message delivery pattern enumeration
- [JsonSerializationAttribute](Attributes/JsonSerializationAttribute.md) - JSON serialization configuration
- [ConcurrentStringBuilder](ConcurrentStringBuilder.md) - Thread-safe string building utility

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing a foundation for dictionary-based messaging with correlation ID support and flexible delivery patterns.*