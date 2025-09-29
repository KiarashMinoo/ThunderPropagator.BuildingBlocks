# ICorrelationIdSupport

The `ICorrelationIdSupport` interface defines a contract for objects that can store and manage correlation IDs. It provides a standardized way to add correlation ID tracking capabilities to any class, enabling distributed tracing and request correlation across system boundaries.

## Overview

The `ICorrelationIdSupport` interface is a simple but powerful contract that allows objects to participate in correlation ID-based tracking systems. Any class implementing this interface can leverage the correlation ID infrastructure provided by `CorrelationIdProvider` and `CorrelationIdSupportHelper`.

## Interface Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.CorrelationId
{
    public interface ICorrelationIdSupport
    {
        string CorrelationId { get; protected internal set; }
    }
}
```

## Key Features

- **Simple Contract**: Single property defining correlation ID storage
- **Protected Setter**: Controlled access to correlation ID modification
- **Universal Application**: Can be implemented by any class needing correlation tracking
- **Framework Integration**: Seamlessly works with correlation ID utilities
- **Type Safety**: Enables generic constraints for correlation ID operations

## Implementation Examples

### Basic Implementation

```csharp
using RapidStreamer.BuildingBlocks.Application.CorrelationId;

public class OrderRequest : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}

// Usage
var order = new OrderRequest
{
    OrderId = "ORD-001",
    Amount = 299.99m,
    CustomerId = "CUST-123"
}.GenerateCorrelationId(); // Extension method available due to interface
```

### Entity Framework Integration

```csharp
public abstract class BaseEntity : ICorrelationIdSupport
{
    public int Id { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Order> Orders { get; set; } = new();
}

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}

// Usage with EF Core
public class ApplicationDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically generate correlation IDs for new entities
        var newEntities = ChangeTracker.Entries<ICorrelationIdSupport>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .Where(e => string.IsNullOrEmpty(e.CorrelationId));
            
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
public class MessageEnvelope : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string MessageType { get; set; } = string.Empty;
    public object Payload { get; set; } = null!;
    public Dictionary<string, string> Headers { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class OrderCreatedEvent : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Usage in message publishing
public class MessagePublisher
{
    public async Task PublishAsync<T>(T message) where T : ICorrelationIdSupport
    {
        // Ensure correlation ID exists
        if (string.IsNullOrEmpty(message.CorrelationId))
        {
            message.GenerateCorrelationId();
        }
        
        var envelope = new MessageEnvelope
        {
            MessageType = typeof(T).Name,
            Payload = message,
            Headers = { ["CorrelationId"] = message.CorrelationId }
        }.SetCorrelationId(message.CorrelationId);
        
        await PublishToQueue(envelope);
    }
}
```

### Web API Integration

```csharp
public class ApiRequest : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
}

public class ApiResponse : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse>> CreateOrder(CreateOrderRequest request)
    {
        // Generate correlation ID for request tracking
        request.GenerateCorrelationId();
        
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            
            var response = new ApiResponse
            {
                Success = true,
                Message = "Order created successfully",
                Data = order
            }.SetCorrelationId(request.CorrelationId);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new ApiResponse
            {
                Success = false,
                Message = "Failed to create order",
                Errors = { ex.Message }
            }.SetCorrelationId(request.CorrelationId);
            
            return BadRequest(errorResponse);
        }
    }
}

public class CreateOrderRequest : ApiRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public string ShippingAddress { get; set; } = string.Empty;
}
```

### Background Processing Integration

```csharp
public abstract class BackgroundJob : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class EmailSendingJob : BackgroundJob
{
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
}

public class ReportGenerationJob : BackgroundJob
{
    public string ReportType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string OutputFormat { get; set; } = "PDF";
}

// Background job processor
public class JobProcessor<T> where T : BackgroundJob
{
    public async Task<JobResult> ProcessAsync(T job)
    {
        // Ensure correlation ID for tracking
        if (string.IsNullOrEmpty(job.CorrelationId))
        {
            job.GenerateCorrelationId();
        }
        
        using var activity = Activity.StartActivity($"ProcessJob.{typeof(T).Name}");
        activity?.SetTag("correlation.id", job.CorrelationId);
        activity?.SetTag("job.id", job.JobId);
        
        try
        {
            job.Status = JobStatus.Running;
            job.StartedAt = DateTime.UtcNow;
            
            var result = await ProcessJobInternal(job);
            
            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            
            return new JobResult
            {
                CorrelationId = job.CorrelationId,
                Success = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            
            return new JobResult
            {
                CorrelationId = job.CorrelationId,
                Success = false,
                Error = ex.Message
            };
        }
    }
}
```

## Advanced Implementation Patterns

### Hierarchical Correlation IDs

```csharp
public interface IHierarchicalCorrelationIdSupport : ICorrelationIdSupport
{
    string? ParentCorrelationId { get; set; }
    List<string> ChildCorrelationIds { get; set; }
}

public class ProcessingWorkflow : IHierarchicalCorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string? ParentCorrelationId { get; set; }
    public List<string> ChildCorrelationIds { get; set; } = new();
    
    public string WorkflowId { get; set; } = string.Empty;
    public List<ProcessingStep> Steps { get; set; } = new();
}

public class ProcessingStep : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    
    public string StepName { get; set; } = string.Empty;
    public StepStatus Status { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Extension methods for hierarchical correlation
public static class HierarchicalCorrelationExtensions
{
    public static T CreateChildStep<T>(this IHierarchicalCorrelationIdSupport parent, T child) 
        where T : ICorrelationIdSupport
    {
        child.GenerateCorrelationId();
        parent.ChildCorrelationIds.Add(child.CorrelationId);
        
        if (child is IHierarchicalCorrelationIdSupport hierarchicalChild)
        {
            hierarchicalChild.ParentCorrelationId = parent.CorrelationId;
        }
        
        return child;
    }
}
```

### Conditional Correlation ID Support

```csharp
public abstract class ConditionalCorrelationEntity : ICorrelationIdSupport
{
    private string _correlationId = string.Empty;
    
    public virtual string CorrelationId 
    { 
        get => _correlationId;
        set => _correlationId = ShouldTrackCorrelation() ? value : string.Empty;
    }
    
    protected abstract bool ShouldTrackCorrelation();
}

public class CriticalOrder : ConditionalCorrelationEntity
{
    public decimal Amount { get; set; }
    public OrderPriority Priority { get; set; }
    
    protected override bool ShouldTrackCorrelation()
    {
        // Only track correlation for high-value or high-priority orders
        return Amount > 1000m || Priority == OrderPriority.Critical;
    }
}
```

### Interface Composition

```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}

public interface ITrackable : ICorrelationIdSupport, IAuditable
{
    TrackingMetadata Metadata { get; set; }
}

public class TrackingMetadata
{
    public string Source { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class AuditableEntity : ITrackable
{
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public TrackingMetadata Metadata { get; set; } = new();
}

public class CustomerProfile : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public CustomerTier Tier { get; set; }
}
```

## Testing Strategies

### Interface Implementation Testing

```csharp
[Test]
public void ImplementsICorrelationIdSupport()
{
    // Arrange
    var entity = new TestEntity();
    
    // Act & Assert
    Assert.IsAssignableFrom<ICorrelationIdSupport>(entity);
    Assert.NotNull(entity.CorrelationId);
}

[Test]
public void CorrelationId_CanBeSetAndRetrieved()
{
    // Arrange
    var entity = new TestEntity();
    const string testId = "TEST-CORRELATION-ID";
    
    // Act
    entity.CorrelationId = testId;
    
    // Assert
    Assert.Equal(testId, entity.CorrelationId);
}

[Test]
public void CorrelationId_WorksWithHelperMethods()
{
    // Arrange
    var entity = new TestEntity();
    
    // Act
    var result = entity.GenerateCorrelationId();
    
    // Assert
    Assert.Same(entity, result);
    Assert.NotEmpty(entity.CorrelationId);
}

private class TestEntity : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
```

### Generic Constraint Testing

```csharp
[Test]
public void GenericMethod_WorksWithICorrelationIdSupport()
{
    // Arrange
    var entities = new List<ICorrelationIdSupport>
    {
        new TestEntity(),
        new AnotherTestEntity()
    };
    
    // Act
    var results = ProcessEntities(entities);
    
    // Assert
    Assert.All(results, entity => Assert.NotEmpty(entity.CorrelationId));
}

private static List<T> ProcessEntities<T>(IEnumerable<T> entities) 
    where T : ICorrelationIdSupport
{
    return entities.Select(entity => entity.GenerateCorrelationId()).ToList();
}
```

### Mock Testing

```csharp
[Test]
public async Task Service_PropagatesCorrelationId()
{
    // Arrange
    var mockEntity = new Mock<ICorrelationIdSupport>();
    mockEntity.Setup(x => x.CorrelationId).Returns("TEST-ID");
    
    var service = new TestService();
    
    // Act
    var result = await service.ProcessAsync(mockEntity.Object);
    
    // Assert
    Assert.Equal("TEST-ID", result.CorrelationId);
    mockEntity.Verify(x => x.CorrelationId, Times.AtLeastOnce);
}
```

## Performance Considerations

### Memory Efficiency

```csharp
// Use struct for lightweight correlation ID containers
public readonly struct CorrelationContext : IEquatable<CorrelationContext>
{
    public string CorrelationId { get; }
    
    public CorrelationContext(string correlationId)
    {
        CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
    }
    
    public bool Equals(CorrelationContext other) => 
        string.Equals(CorrelationId, other.CorrelationId, StringComparison.Ordinal);
    
    public override bool Equals(object? obj) => 
        obj is CorrelationContext other && Equals(other);
    
    public override int GetHashCode() => 
        CorrelationId.GetHashCode(StringComparison.Ordinal);
}

// Extension methods for struct usage
public static class CorrelationContextExtensions
{
    public static CorrelationContext ToContext(this ICorrelationIdSupport entity)
    {
        return new CorrelationContext(entity.CorrelationId);
    }
}
```

### Batch Operations

```csharp
public static class BatchCorrelationOperations
{
    public static void EnsureCorrelationIds<T>(this ICollection<T> entities) 
        where T : ICorrelationIdSupport
    {
        Parallel.ForEach(entities.Where(e => string.IsNullOrEmpty(e.CorrelationId)), 
            entity => entity.GenerateCorrelationId());
    }
    
    public static Dictionary<string, T> IndexByCorrelationId<T>(this IEnumerable<T> entities) 
        where T : ICorrelationIdSupport
    {
        return entities.ToDictionary(e => e.CorrelationId, e => e);
    }
}
```

## Best Practices

1. **Consistent Implementation**: Always implement the interface properly with appropriate access modifiers
2. **Early Assignment**: Generate or assign correlation IDs as early as possible in object lifecycle
3. **Immutability**: Consider making correlation IDs immutable once set for data integrity
4. **Validation**: Validate correlation ID format and requirements in critical paths
5. **Logging Integration**: Always include correlation IDs in log entries for traceability
6. **Error Propagation**: Maintain correlation IDs through exception handling and error responses
7. **Testing**: Verify interface implementation and correlation ID behavior in unit tests

## Related Components

- **[CorrelationIdProvider](CorrelationIdProvider.md)**: Utility for generating correlation IDs
- **[CorrelationIdSupportHelper](CorrelationIdSupportHelper.md)**: Extension methods for ICorrelationIdSupport objects
- **[FeederMessage](../Application/FeederMessage.md)**: Base message class implementing this interface

## See Also

- [CorrelationId System Overview](README.md)
- [Distributed Tracing Patterns](../Patterns/DistributedTracing.md)
- [Interface Design Guidelines](../Guidelines/InterfaceDesign.md)