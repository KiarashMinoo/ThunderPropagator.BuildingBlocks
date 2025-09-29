# CorrelationId System

The CorrelationId system provides a comprehensive solution for tracking and correlating requests, messages, and operations across distributed systems. It enables end-to-end traceability through unique identifiers that can be generated, stored, and propagated throughout the application lifecycle.

## System Overview

The CorrelationId system consists of three main components that work together to provide seamless correlation ID management:

- **[ICorrelationIdSupport](ICorrelationIdSupport.md)**: Interface defining correlation ID storage contract
- **[CorrelationIdProvider](CorrelationIdProvider.md)**: Utility for generating unique correlation IDs
- **[CorrelationIdSupportHelper](CorrelationIdSupportHelper.md)**: Extension methods for fluent correlation ID management

## Architecture

```mermaid
graph TD
    A[ICorrelationIdSupport Interface] --> B[Objects implementing interface]
    C[CorrelationIdProvider] --> D[Generate unique IDs]
    E[CorrelationIdSupportHelper] --> F[Fluent management]
    
    B --> G[FeederMessage]
    B --> H[Entity Classes]
    B --> I[Request/Response Objects]
    
    D --> J[Time-based uniqueness]
    D --> K[Type information]
    D --> L[Hash-based determinism]
    
    F --> M[GenerateCorrelationId()]
    F --> N[SetCorrelationId()]
    
    G --> O[Message Processing]
    H --> P[Data Persistence]
    I --> Q[Web APIs]
    
    O --> R[Distributed Tracing]
    P --> R
    Q --> R
```

## Quick Start Guide

### Basic Usage

```csharp
using RapidStreamer.BuildingBlocks.Application.CorrelationId;

// 1. Implement the interface
public class OrderRequest : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// 2. Generate correlation ID
var request = new OrderRequest { OrderId = "ORD-001", Amount = 299.99m }
    .GenerateCorrelationId(); // Fluent extension method

// 3. Use for tracking
Console.WriteLine($"Processing order with correlation ID: {request.CorrelationId}");
```

### Message Processing

```csharp
// FeederMessage already implements ICorrelationIdSupport
public class OrderCreatedMessage : FeederMessage
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// Automatic correlation ID generation
var message = new OrderCreatedMessage 
{ 
    OrderId = "ORD-001", 
    Amount = 299.99m 
}
.GenerateCorrelationId();

// The correlation ID is now set and can be used for tracing
await messageProcessor.ProcessAsync(message);
```

### Web API Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        // Generate correlation ID for request tracking
        request.GenerateCorrelationId();
        
        // Process with correlation context
        var result = await _orderService.ProcessAsync(request);
        
        // Return with same correlation ID
        return Ok(new ApiResponse 
        { 
            Data = result 
        }.SetCorrelationId(request.CorrelationId));
    }
}
```

## Component Details

### ICorrelationIdSupport Interface

The foundation interface that enables any class to participate in correlation ID tracking:

```csharp
public interface ICorrelationIdSupport
{
    string CorrelationId { get; protected internal set; }
}
```

**Key Features:**
- Simple property contract for correlation ID storage
- Protected setter ensures controlled access
- Universal application to any class type

**[Read full documentation →](ICorrelationIdSupport.md)**

### CorrelationIdProvider

Static utility class that generates unique correlation IDs with temporal and type information:

```csharp
public static class CorrelationIdProvider
{
    public static string GenerateCorrelationId<T>(this T input) where T : notnull;
}
```

**Key Features:**
- Time-stamped unique ID generation
- Type information encoding
- Special handling for FeederMessage objects
- Telemetry integration

**[Read full documentation →](CorrelationIdProvider.md)**

### CorrelationIdSupportHelper

Extension methods providing fluent API for correlation ID management:

```csharp
public static class CorrelationIdSupportHelper
{
    public static T GenerateCorrelationId<T>(this T input) where T : class, ICorrelationIdSupport;
    public static T SetCorrelationId<T>(this T input, string correlationId) where T : class, ICorrelationIdSupport;
}
```

**Key Features:**
- Fluent method chaining
- Automatic ID generation and assignment
- Manual correlation ID setting
- Returns original object for chaining

**[Read full documentation →](CorrelationIdSupportHelper.md)**

## Integration Patterns

### Dependency Injection Configuration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationContextService, CorrelationContextService>();
        services.AddSingleton<CorrelationIdCache>();
        services.AddTransient<CorrelationIdMiddleware>();
        
        return services;
    }
}

public interface ICorrelationContextService
{
    T CreateWithCorrelationId<T>() where T : class, ICorrelationIdSupport, new();
    T EnsureCorrelationId<T>(T item) where T : class, ICorrelationIdSupport;
    string GetCurrentCorrelationId();
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
        // Extract or generate correlation ID
        string correlationId = context.Request.Headers["X-Correlation-ID"]
            .FirstOrDefault() ?? context.GenerateCorrelationId();
        
        // Add to response headers
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        // Store in context for downstream use
        context.Items["CorrelationId"] = correlationId;
        
        await _next(context);
    }
}
```

### Entity Framework Integration

```csharp
public abstract class BaseEntity : ICorrelationIdSupport
{
    public int Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationDbContext : DbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-generate correlation IDs for new entities
        var newEntities = ChangeTracker.Entries<ICorrelationIdSupport>()
            .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.CorrelationId))
            .Select(e => e.Entity);
            
        foreach (var entity in newEntities)
        {
            entity.GenerateCorrelationId();
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

### Message Queue Integration

```csharp
public class MessagePublisher<T> where T : ICorrelationIdSupport
{
    public async Task PublishAsync(T message, string? correlationId = null)
    {
        // Ensure correlation ID
        if (!string.IsNullOrEmpty(correlationId))
        {
            message.SetCorrelationId(correlationId);
        }
        else if (string.IsNullOrEmpty(message.CorrelationId))
        {
            message.GenerateCorrelationId();
        }
        
        // Create envelope with correlation tracking
        var envelope = new MessageEnvelope
        {
            MessageId = Guid.NewGuid().ToString(),
            MessageType = typeof(T).Name,
            CorrelationId = message.CorrelationId,
            Payload = message,
            Timestamp = DateTime.UtcNow
        };
        
        await PublishToQueue(envelope);
    }
}
```

## Usage Examples

### End-to-End Request Tracking

```csharp
// 1. Web API receives request
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    // Generate correlation ID at entry point
    request.GenerateCorrelationId();
    
    _logger.LogInformation("Processing order creation [{CorrelationId}]", 
        request.CorrelationId);
    
    // 2. Pass to service layer
    var result = await _orderService.CreateAsync(request);
    
    return Ok(result);
}

// 3. Service processes with same correlation ID
public class OrderService
{
    public async Task<OrderResult> CreateAsync(CreateOrderRequest request)
    {
        using var activity = Activity.StartActivity("OrderService.Create");
        activity?.SetTag("correlation.id", request.CorrelationId);
        
        // 4. Create domain entity with correlation ID
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerId = request.CustomerId,
            Items = request.Items
        }.SetCorrelationId(request.CorrelationId);
        
        // 5. Persist with correlation tracking
        await _repository.SaveAsync(order);
        
        // 6. Publish event with correlation ID
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Amount = order.TotalAmount
        }.SetCorrelationId(request.CorrelationId);
        
        await _eventPublisher.PublishAsync(orderCreatedEvent);
        
        return new OrderResult 
        { 
            OrderId = order.Id 
        }.SetCorrelationId(request.CorrelationId);
    }
}
```

### Batch Processing

```csharp
public class BatchProcessor<T> where T : class, ICorrelationIdSupport
{
    public async Task<BatchResult> ProcessBatchAsync(IEnumerable<T> items)
    {
        // Ensure all items have correlation IDs
        var itemsList = items.ToList();
        itemsList.EnsureCorrelationIds();
        
        var results = new List<ProcessingResult>();
        
        await Parallel.ForEachAsync(itemsList, async (item, ct) =>
        {
            using var activity = Activity.StartActivity("BatchProcessor.ProcessItem");
            activity?.SetTag("correlation.id", item.CorrelationId);
            
            try
            {
                var result = await ProcessItemAsync(item);
                results.Add(new ProcessingResult 
                { 
                    Success = true, 
                    Item = item 
                }.SetCorrelationId(item.CorrelationId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process item [{CorrelationId}]", 
                    item.CorrelationId);
                    
                results.Add(new ProcessingResult 
                { 
                    Success = false, 
                    Error = ex.Message, 
                    Item = item 
                }.SetCorrelationId(item.CorrelationId));
            }
        });
        
        return new BatchResult
        {
            TotalItems = itemsList.Count,
            SuccessfulItems = results.Count(r => r.Success),
            FailedItems = results.Count(r => !r.Success),
            Results = results
        };
    }
}
```

### Error Handling and Recovery

```csharp
public class ErrorHandlingService
{
    public async Task<ProcessingResult> ProcessWithRecovery<T>(T item) 
        where T : class, ICorrelationIdSupport
    {
        const int maxRetries = 3;
        var correlationId = item.CorrelationId;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Processing attempt {Attempt} [{CorrelationId}]", 
                    attempt, correlationId);
                
                var result = await ProcessItemAsync(item);
                
                _logger.LogInformation("Successfully processed on attempt {Attempt} [{CorrelationId}]", 
                    attempt, correlationId);
                
                return new ProcessingResult 
                { 
                    Success = true, 
                    Attempts = attempt 
                }.SetCorrelationId(correlationId);
            }
            catch (TransientException ex)
            {
                _logger.LogWarning(ex, "Transient error on attempt {Attempt} [{CorrelationId}]", 
                    attempt, correlationId);
                
                if (attempt == maxRetries)
                {
                    return new ProcessingResult 
                    { 
                        Success = false, 
                        Error = $"Max retries exceeded: {ex.Message}",
                        Attempts = attempt 
                    }.SetCorrelationId(correlationId);
                }
                
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Permanent error on attempt {Attempt} [{CorrelationId}]", 
                    attempt, correlationId);
                
                return new ProcessingResult 
                { 
                    Success = false, 
                    Error = ex.Message,
                    Attempts = attempt 
                }.SetCorrelationId(correlationId);
            }
        }
        
        // Should never reach here
        throw new InvalidOperationException("Unexpected code path");
    }
}
```

## Performance Considerations

### Efficient ID Generation

```csharp
// Use object pooling for high-volume scenarios
public class CorrelationIdPool
{
    private readonly ConcurrentQueue<StringBuilder> _builders = new();
    
    public string GenerateOptimized<T>(T input) where T : notnull
    {
        if (!_builders.TryDequeue(out var sb))
        {
            sb = new StringBuilder(64);
        }
        
        try
        {
            // Generate using pooled StringBuilder
            return BuildCorrelationId(input, sb);
        }
        finally
        {
            sb.Clear();
            _builders.Enqueue(sb);
        }
    }
}
```

### Batch Operations

```csharp
public static class BatchCorrelationExtensions
{
    public static void EnsureCorrelationIds<T>(this IList<T> items) 
        where T : ICorrelationIdSupport
    {
        Parallel.ForEach(
            items.Where(i => string.IsNullOrEmpty(i.CorrelationId)),
            item => item.GenerateCorrelationId()
        );
    }
    
    public static Dictionary<string, T> IndexByCorrelationId<T>(this IEnumerable<T> items) 
        where T : ICorrelationIdSupport
    {
        return items.ToDictionary(i => i.CorrelationId, i => i);
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[TestClass]
public class CorrelationIdSystemTests
{
    [TestMethod]
    public void GenerateCorrelationId_CreatesUniqueIds()
    {
        var item1 = new TestItem().GenerateCorrelationId();
        var item2 = new TestItem().GenerateCorrelationId();
        
        Assert.AreNotEqual(item1.CorrelationId, item2.CorrelationId);
    }
    
    [TestMethod]
    public void SetCorrelationId_MaintainsCustomId()
    {
        const string customId = "CUSTOM-ID-123";
        var item = new TestItem().SetCorrelationId(customId);
        
        Assert.AreEqual(customId, item.CorrelationId);
    }
    
    [TestMethod]
    public void CorrelationId_PropagatesThoughPipeline()
    {
        var request = new TestRequest().GenerateCorrelationId();
        var response = ProcessRequest(request);
        
        Assert.AreEqual(request.CorrelationId, response.CorrelationId);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class CorrelationIdIntegrationTests
{
    [TestMethod]
    public async Task EndToEndProcessing_MaintainsCorrelationId()
    {
        // Arrange
        var request = new OrderRequest { Amount = 100m }.GenerateCorrelationId();
        var originalId = request.CorrelationId;
        
        // Act
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/orders", request);
        
        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.AreEqual(originalId, response.Headers.GetValues("X-Correlation-ID").First());
    }
}
```

## Best Practices

1. **Early Generation**: Generate correlation IDs at system boundaries (API entry points, message handlers)
2. **Consistent Propagation**: Always pass correlation IDs through all layers and external calls
3. **Logging Integration**: Include correlation IDs in all log messages for end-to-end traceability
4. **Error Preservation**: Maintain correlation IDs through exception handling and error responses
5. **Performance Optimization**: Use batch operations for high-volume scenarios
6. **Testing**: Verify correlation ID generation, propagation, and persistence in tests
7. **Documentation**: Document correlation ID usage patterns and requirements

## Troubleshooting

### Common Issues

**Missing Correlation IDs**
```csharp
// Problem: Correlation ID not being set
var item = new MyItem();
// Solution: Always generate or set correlation ID
var item = new MyItem().GenerateCorrelationId();
```

**Correlation ID Loss**
```csharp
// Problem: Not propagating correlation ID
var response = ProcessRequest(request);
// Solution: Explicitly propagate correlation ID
var response = ProcessRequest(request).SetCorrelationId(request.CorrelationId);
```

**Performance Issues**
```csharp
// Problem: Generating many correlation IDs
foreach (var item in items)
{
    item.GenerateCorrelationId();
}
// Solution: Use batch operations
items.EnsureCorrelationIds();
```

## Related Documentation

- **[FeederMessage Documentation](../Application/FeederMessage.md)**: Base message class using correlation IDs
- **[Telemetry Integration](../Application/Telemetry.md)**: Activity tracking and observability
- **[Distributed Tracing Patterns](../Patterns/DistributedTracing.md)**: Advanced tracing scenarios

## Contributing

When extending the CorrelationId system:

1. Maintain backward compatibility with existing interfaces
2. Follow naming conventions for correlation ID properties
3. Include comprehensive unit and integration tests
4. Update documentation with usage examples
5. Consider performance implications for high-volume scenarios

## Version History

- **v1.0**: Initial implementation with basic correlation ID generation
- **v1.1**: Added CorrelationIdSupportHelper for fluent API
- **v1.2**: Enhanced FeederMessage integration and telemetry support
- **v1.3**: Performance optimizations and batch processing capabilities