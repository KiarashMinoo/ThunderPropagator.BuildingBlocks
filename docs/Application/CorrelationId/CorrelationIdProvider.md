# CorrelationIdProvider

The `CorrelationIdProvider` is a static utility class that generates unique correlation IDs for any object type. It creates deterministic, time-stamped correlation IDs that combine temporal information, object hash codes, and type information to ensure uniqueness while maintaining traceability.

## Overview

The `CorrelationIdProvider` generates correlation IDs in the format: `{timestamp}-{hashcode}-{encoded-type-name}`, providing a unique identifier that includes contextual information about when and from what type of object the ID was generated.

## Class Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.CorrelationId
{
    public static class CorrelationIdProvider
    {
        public static string GenerateCorrelationId<T>(this T input) where T : notnull;
    }
}
```

## Key Features

- **Universal Generation**: Works with any non-null object type
- **Time-based Uniqueness**: Includes Unix timestamp for temporal ordering
- **Deterministic Hashing**: Uses object hash codes for repeatability
- **Type Information**: Encodes type name for debugging and traceability
- **Telemetry Integration**: Automatic activity tracking for observability
- **Special Handling**: Optimized behavior for `FeederMessage` types

## Usage Examples

### Basic Correlation ID Generation

```csharp
using RapidStreamer.BuildingBlocks.Application.CorrelationId;

// Generate correlation ID for any object
var customer = new Customer { Id = 123, Name = "John Doe" };
string correlationId = customer.GenerateCorrelationId();
Console.WriteLine(correlationId); 
// Output: "1703123456789-456789123-Q3VzdG9tZXI"

// Generate for different object types
var order = new Order { OrderId = "ORD-001", Amount = 299.99m };
string orderCorrelationId = order.GenerateCorrelationId();

var product = new Product { SKU = "LAPTOP-001", Price = 999.99m };
string productCorrelationId = product.GenerateCorrelationId();
```

### Consistent ID Generation

```csharp
// Same object generates same correlation ID (within same millisecond)
var user = new User { Id = 42, Email = "user@example.com" };

string id1 = user.GenerateCorrelationId();
string id2 = user.GenerateCorrelationId();

// IDs are identical if generated in same millisecond with same object
Assert.Equal(id1, id2);
```

### FeederMessage Special Handling

```csharp
// FeederMessage uses HashKey if available, otherwise falls back to GetHashCode()
var message = new CustomFeederMessage();
message.HashKey = 12345; // Custom hash key

string correlationId = message.GenerateCorrelationId();
// Uses HashKey value (12345) instead of GetHashCode()

// If HashKey is null or 0, uses standard GetHashCode()
message.HashKey = null;
string fallbackId = message.GenerateCorrelationId();
// Uses message.GetHashCode() value
```

### Collection and Batch Processing

```csharp
// Generate correlation IDs for collections
var orders = new List<Order>
{
    new Order { OrderId = "ORD-001", CustomerId = 123 },
    new Order { OrderId = "ORD-002", CustomerId = 456 },
    new Order { OrderId = "ORD-003", CustomerId = 789 }
};

var correlatedOrders = orders.Select(order => new
{
    Order = order,
    CorrelationId = order.GenerateCorrelationId(),
    Timestamp = DateTime.UtcNow
}).ToList();

foreach (var item in correlatedOrders)
{
    Console.WriteLine($"Order {item.Order.OrderId}: {item.CorrelationId}");
}
```

### Integration with Logging and Tracing

```csharp
public class OrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;
    
    public OrderProcessor(ILogger<OrderProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task<ProcessingResult> ProcessOrderAsync(Order order)
    {
        // Generate correlation ID for tracking
        string correlationId = order.GenerateCorrelationId();
        
        _logger.LogInformation("Processing order {OrderId} with correlation ID {CorrelationId}", 
            order.OrderId, correlationId);
        
        try
        {
            // Process order with correlation ID context
            var result = await ProcessOrderInternal(order, correlationId);
            
            _logger.LogInformation("Successfully processed order {OrderId} [{CorrelationId}]", 
                order.OrderId, correlationId);
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId} [{CorrelationId}]", 
                order.OrderId, correlationId);
            throw;
        }
    }
    
    private async Task<ProcessingResult> ProcessOrderInternal(Order order, string correlationId)
    {
        // Use correlation ID for distributed tracing
        using var activity = Activity.StartActivity("ProcessOrder");
        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("order.id", order.OrderId);
        
        // Process order logic here
        return new ProcessingResult { CorrelationId = correlationId };
    }
}
```

## Correlation ID Format

The generated correlation ID follows this structure:

```
{unix-timestamp}-{hash-code}-{base64-encoded-type-name}
```

### Components Explained

1. **Unix Timestamp**: Milliseconds since Unix epoch for temporal uniqueness
2. **Hash Code**: Object's hash code (or FeederMessage.HashKey if available)
3. **Type Name**: Base64-encoded type name (without padding) for type identification

### Example Breakdown

```csharp
var customer = new Customer { Id = 123 };
string correlationId = customer.GenerateCorrelationId();
// Result: "1703123456789-456789123-Q3VzdG9tZXI"

// Breaking down the components:
// 1703123456789 = Unix timestamp (December 21, 2023 12:30:56.789 UTC)
// 456789123 = customer.GetHashCode() result
// Q3VzdG9tZXI = Base64 encoding of "Customer" (type name)
```

## Performance Considerations

### Efficient Generation

```csharp
// Prefer batch generation for multiple objects
public static class CorrelationIdBatch
{
    public static Dictionary<T, string> GenerateCorrelationIds<T>(IEnumerable<T> items) 
        where T : notnull
    {
        return items.ToDictionary(item => item, item => item.GenerateCorrelationId());
    }
    
    public static void AttachCorrelationIds<T>(IList<T> items, Action<T, string> setter) 
        where T : notnull
    {
        foreach (var item in items)
        {
            string correlationId = item.GenerateCorrelationId();
            setter(item, correlationId);
        }
    }
}

// Usage
var products = LoadProducts();
var correlationMap = CorrelationIdBatch.GenerateCorrelationIds(products);

// Or attach directly to objects
var orders = LoadOrders();
CorrelationIdBatch.AttachCorrelationIds(orders, (order, id) => order.TrackingId = id);
```

### Memory Optimization

```csharp
// For high-volume scenarios, consider caching
public class CorrelationIdCache
{
    private readonly ConcurrentDictionary<int, string> _cache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
    
    public string GetOrGenerateCorrelationId<T>(T input) where T : notnull
    {
        int key = input.GetHashCode();
        
        return _cache.GetOrAdd(key, _ => input.GenerateCorrelationId());
    }
    
    public void ClearExpiredEntries()
    {
        // Implement cache expiry logic based on timestamp parsing
    }
}
```

## Integration Patterns

### Dependency Injection Setup

```csharp
// Register correlation ID services
services.AddScoped<ICorrelationIdService, CorrelationIdService>();
services.AddSingleton<CorrelationIdCache>();

public interface ICorrelationIdService
{
    string GenerateId<T>(T input) where T : notnull;
    string GenerateIdWithPrefix<T>(T input, string prefix) where T : notnull;
}

public class CorrelationIdService : ICorrelationIdService
{
    private readonly CorrelationIdCache _cache;
    
    public CorrelationIdService(CorrelationIdCache cache)
    {
        _cache = cache;
    }
    
    public string GenerateId<T>(T input) where T : notnull
    {
        return input.GenerateCorrelationId();
    }
    
    public string GenerateIdWithPrefix<T>(T input, string prefix) where T : notnull
    {
        string baseId = input.GenerateCorrelationId();
        return $"{prefix}-{baseId}";
    }
}
```

### Middleware Integration

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID for request
        string correlationId = context.GenerateCorrelationId();
        
        // Add to response headers
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        // Add to logging context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[Test]
public void GenerateCorrelationId_WithSameObject_ReturnsSameId()
{
    // Arrange
    var testObject = new TestClass { Value = 42 };
    
    // Act
    string id1 = testObject.GenerateCorrelationId();
    string id2 = testObject.GenerateCorrelationId();
    
    // Assert
    Assert.Equal(id1, id2);
}

[Test]
public void GenerateCorrelationId_WithDifferentObjects_ReturnsDifferentIds()
{
    // Arrange
    var obj1 = new TestClass { Value = 1 };
    var obj2 = new TestClass { Value = 2 };
    
    // Act
    string id1 = obj1.GenerateCorrelationId();
    string id2 = obj2.GenerateCorrelationId();
    
    // Assert
    Assert.NotEqual(id1, id2);
}

[Test]
public void GenerateCorrelationId_FormatValidation()
{
    // Arrange
    var testObject = new TestClass();
    
    // Act
    string correlationId = testObject.GenerateCorrelationId();
    
    // Assert - should match pattern: timestamp-hashcode-encodedtype
    var parts = correlationId.Split('-');
    Assert.Equal(3, parts.Length);
    Assert.True(long.TryParse(parts[0], out _)); // Timestamp
    Assert.True(int.TryParse(parts[1], out _));  // Hash code
    Assert.NotEmpty(parts[2]); // Encoded type name
}
```

### Integration Testing

```csharp
[Test]
public async Task ProcessWorkflow_WithCorrelationId_MaintainsTracing()
{
    // Arrange
    var order = new Order { OrderId = "TEST-001" };
    string correlationId = order.GenerateCorrelationId();
    
    // Act
    var result = await _orderProcessor.ProcessOrderAsync(order);
    
    // Assert
    Assert.Equal(correlationId, result.CorrelationId);
    
    // Verify telemetry/logs contain correlation ID
    var logEntries = _testLogger.GetLogEntries();
    Assert.All(logEntries, entry => 
        Assert.Contains(correlationId, entry.Message));
}
```

## Best Practices

1. **Consistent Usage**: Always generate correlation IDs at system boundaries
2. **Early Generation**: Create correlation IDs as early as possible in request processing
3. **Propagation**: Pass correlation IDs through all system layers
4. **Logging Integration**: Include correlation IDs in all log messages
5. **Error Handling**: Preserve correlation IDs in exception handling and error responses
6. **Performance**: Consider caching for high-volume scenarios with repeated objects
7. **Validation**: Always validate that correlation IDs are properly formatted

## Related Components

- **[CorrelationIdSupportHelper](CorrelationIdSupportHelper.md)**: Helper methods for objects implementing ICorrelationIdSupport
- **[ICorrelationIdSupport](ICorrelationIdSupport.md)**: Interface for objects that can store correlation IDs
- **[FeederMessage](../Application/FeederMessage.md)**: Base class that uses correlation IDs for message tracking

## See Also

- [CorrelationId System Overview](README.md)
- [Telemetry Integration](../Application/Telemetry.md)
- [Distributed Tracing Patterns](../Patterns/DistributedTracing.md)