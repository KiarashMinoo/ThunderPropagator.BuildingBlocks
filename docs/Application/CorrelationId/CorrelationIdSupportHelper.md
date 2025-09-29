# CorrelationIdSupportHelper

The `CorrelationIdSupportHelper` is a static utility class that provides extension methods for objects implementing the `ICorrelationIdSupport` interface. It simplifies correlation ID management by offering convenient methods to generate and set correlation IDs on supported objects.

## Overview

The `CorrelationIdSupportHelper` acts as a bridge between the `CorrelationIdProvider` and objects that implement `ICorrelationIdSupport`, providing a fluent API for correlation ID management while maintaining object instances for method chaining.

## Class Definition

```csharp
namespace RapidStreamer.BuildingBlocks.Application.CorrelationId
{
    public static class CorrelationIdSupportHelper
    {
        public static T GenerateCorrelationId<T>(this T input) where T : class, ICorrelationIdSupport;
        public static T SetCorrelationId<T>(this T input, string correlationId) where T : class, ICorrelationIdSupport;
    }
}
```

## Key Features

- **Fluent Interface**: Returns the original object for method chaining
- **Type Safety**: Constrained to classes implementing `ICorrelationIdSupport`
- **Automatic Generation**: Integrates with `CorrelationIdProvider` for ID generation
- **Telemetry Integration**: Automatic activity tracking for observability
- **Manual Assignment**: Allows setting custom correlation IDs

## Usage Examples

### Basic Correlation ID Generation

```csharp
using RapidStreamer.BuildingBlocks.Application.CorrelationId;

public class OrderRequest : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// Generate correlation ID automatically
var orderRequest = new OrderRequest 
{ 
    OrderId = "ORD-001", 
    Amount = 299.99m 
}
.GenerateCorrelationId(); // Fluent method chaining

Console.WriteLine($"Generated ID: {orderRequest.CorrelationId}");
// Output: Generated ID: 1703123456789-456789123-T3JkZXJSZXF1ZXN0
```

### Manual Correlation ID Assignment

```csharp
public class UserSession : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
}

// Set custom correlation ID
var session = new UserSession 
{ 
    UserId = "user123", 
    LoginTime = DateTime.UtcNow 
}
.SetCorrelationId("SESSION-abc123-xyz789");

Console.WriteLine($"Session ID: {session.CorrelationId}");
// Output: Session ID: SESSION-abc123-xyz789
```

### Method Chaining and Fluent Configuration

```csharp
public class ProcessingContext : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string ProcessId { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Fluent configuration with correlation ID generation
var context = new ProcessingContext()
    .GenerateCorrelationId()
    .Configure(ctx =>
    {
        ctx.ProcessId = "PROC-001";
        ctx.Status = ProcessingStatus.Starting;
        ctx.CreatedAt = DateTime.UtcNow;
    });

// Custom extension method for configuration
public static class ProcessingContextExtensions
{
    public static T Configure<T>(this T context, Action<T> configure) where T : ProcessingContext
    {
        configure(context);
        return context;
    }
}
```

### FeederMessage Integration

```csharp
public class CustomMessage : FeederMessage
{
    public string MessageType { get; set; } = string.Empty;
    public object Payload { get; set; } = null!;
}

// FeederMessage already implements ICorrelationIdSupport
var message = new CustomMessage
{
    MessageType = "ORDER_CREATED",
    Payload = new { OrderId = "ORD-001", Amount = 299.99m }
}
.GenerateCorrelationId(); // Uses CorrelationIdSupportHelper

// The correlation ID is automatically set
Console.WriteLine($"Message Correlation ID: {message.CorrelationId}");
```

### Collection Processing

```csharp
public class DocumentProcessingRequest : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public ProcessingPriority Priority { get; set; }
}

// Process collection with correlation IDs
var documents = new[]
{
    new DocumentProcessingRequest { DocumentId = "DOC-001", FilePath = "/docs/file1.pdf" },
    new DocumentProcessingRequest { DocumentId = "DOC-002", FilePath = "/docs/file2.pdf" },
    new DocumentProcessingRequest { DocumentId = "DOC-003", FilePath = "/docs/file3.pdf" }
};

var processedDocuments = documents
    .Select(doc => doc.GenerateCorrelationId())
    .ToList();

foreach (var doc in processedDocuments)
{
    Console.WriteLine($"Document {doc.DocumentId}: {doc.CorrelationId}");
}
```

### Correlation ID Propagation

```csharp
public class RequestContext : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

public class ResponseContext : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string ResponseId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}

public class RequestProcessor
{
    public ResponseContext ProcessRequest(RequestContext request)
    {
        // Propagate correlation ID from request to response
        var response = new ResponseContext
        {
            ResponseId = Guid.NewGuid().ToString(),
            StatusCode = 200
        }
        .SetCorrelationId(request.CorrelationId); // Maintain traceability
        
        return response;
    }
}
```

### Advanced Usage Patterns

```csharp
public class ProcessingPipeline
{
    public async Task<ProcessingResult> ProcessAsync<T>(T input) 
        where T : class, ICorrelationIdSupport
    {
        // Ensure correlation ID exists
        if (string.IsNullOrEmpty(input.CorrelationId))
        {
            input.GenerateCorrelationId();
        }
        
        var correlationId = input.CorrelationId;
        
        using var activity = Activity.StartActivity("ProcessingPipeline.Process");
        activity?.SetTag("correlation.id", correlationId);
        
        try
        {
            // Stage 1: Validation
            var validatedInput = await ValidateAsync(input);
            
            // Stage 2: Processing  
            var processedResult = await ProcessInternalAsync(validatedInput);
            
            // Stage 3: Finalization
            var finalResult = await FinalizeAsync(processedResult);
            
            return new ProcessingResult
            {
                CorrelationId = correlationId,
                Status = ProcessingStatus.Completed,
                Result = finalResult
            };
        }
        catch (Exception ex)
        {
            return new ProcessingResult
            {
                CorrelationId = correlationId,
                Status = ProcessingStatus.Failed,
                Error = ex.Message
            };
        }
    }
}
```

## Integration Patterns

### Dependency Injection Setup

```csharp
// Register correlation ID services
services.AddScoped<ICorrelationContextService, CorrelationContextService>();

public interface ICorrelationContextService
{
    T CreateWithCorrelationId<T>() where T : class, ICorrelationIdSupport, new();
    T EnsureCorrelationId<T>(T item) where T : class, ICorrelationIdSupport;
}

public class CorrelationContextService : ICorrelationContextService
{
    public T CreateWithCorrelationId<T>() where T : class, ICorrelationIdSupport, new()
    {
        return new T().GenerateCorrelationId();
    }
    
    public T EnsureCorrelationId<T>(T item) where T : class, ICorrelationIdSupport
    {
        if (string.IsNullOrEmpty(item.CorrelationId))
        {
            item.GenerateCorrelationId();
        }
        return item;
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
        // Check for existing correlation ID in headers
        string? correlationId = context.Request.Headers["X-Correlation-ID"];
        
        if (string.IsNullOrEmpty(correlationId))
        {
            // Generate new correlation ID using context
            correlationId = context.GenerateCorrelationId();
        }
        
        // Create request context
        var requestContext = new RequestContext
        {
            RequestId = context.TraceIdentifier,
            Method = context.Request.Method,
            Path = context.Request.Path
        }
        .SetCorrelationId(correlationId);
        
        // Store in HttpContext for downstream access
        context.Items["CorrelationContext"] = requestContext;
        
        // Add to response headers
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        await _next(context);
    }
}
```

### Repository Pattern Integration

```csharp
public interface IRepository<T> where T : class, ICorrelationIdSupport
{
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<T?> GetByCorrelationIdAsync(string correlationId);
}

public class BaseRepository<T> : IRepository<T> where T : class, ICorrelationIdSupport
{
    private readonly DbContext _context;
    
    public BaseRepository(DbContext context)
    {
        _context = context;
    }
    
    public async Task<T> CreateAsync(T entity)
    {
        // Ensure correlation ID is set before persisting
        entity.GenerateCorrelationId();
        
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }
    
    public async Task<T> UpdateAsync(T entity)
    {
        // Maintain existing correlation ID
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }
    
    public async Task<T?> GetByCorrelationIdAsync(string correlationId)
    {
        return await _context.Set<T>()
            .FirstOrDefaultAsync(e => e.CorrelationId == correlationId);
    }
}
```

## Performance Considerations

### Efficient Batch Processing

```csharp
public static class BatchCorrelationIdHelper
{
    public static IEnumerable<T> GenerateCorrelationIds<T>(this IEnumerable<T> items) 
        where T : class, ICorrelationIdSupport
    {
        return items.Select(item => item.GenerateCorrelationId());
    }
    
    public static void EnsureCorrelationIds<T>(this IList<T> items) 
        where T : class, ICorrelationIdSupport
    {
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.CorrelationId))
            {
                item.GenerateCorrelationId();
            }
        }
    }
}

// Usage
var requests = LoadProcessingRequests();
requests.EnsureCorrelationIds(); // Efficiently ensure all have correlation IDs

var newRequests = CreateNewRequests()
    .GenerateCorrelationIds()
    .ToList();
```

### Memory Optimization

```csharp
public class CorrelationIdPool
{
    private readonly ConcurrentQueue<StringBuilder> _stringBuilders = new();
    
    public T GenerateOptimizedCorrelationId<T>(T input) where T : class, ICorrelationIdSupport
    {
        if (!_stringBuilders.TryDequeue(out StringBuilder? sb))
        {
            sb = new StringBuilder(64); // Pre-size for typical correlation ID length
        }
        
        try
        {
            // Custom correlation ID generation logic using pooled StringBuilder
            var correlationId = GenerateWithStringBuilder(input, sb);
            input.SetCorrelationId(correlationId);
            return input;
        }
        finally
        {
            sb.Clear();
            _stringBuilders.Enqueue(sb);
        }
    }
    
    private string GenerateWithStringBuilder<T>(T input, StringBuilder sb) where T : notnull
    {
        // Implement optimized correlation ID generation
        return input.GenerateCorrelationId(); // Fallback to standard method
    }
}
```

## Testing Strategies

### Unit Testing

```csharp
[Test]
public void GenerateCorrelationId_SetsCorrelationIdProperty()
{
    // Arrange
    var request = new TestRequest();
    
    // Act
    var result = request.GenerateCorrelationId();
    
    // Assert
    Assert.NotNull(result.CorrelationId);
    Assert.NotEmpty(result.CorrelationId);
    Assert.Same(request, result); // Returns same instance
}

[Test]
public void SetCorrelationId_SetsSpecifiedValue()
{
    // Arrange
    var request = new TestRequest();
    const string testId = "TEST-CORRELATION-ID";
    
    // Act
    var result = request.SetCorrelationId(testId);
    
    // Assert
    Assert.Equal(testId, result.CorrelationId);
    Assert.Same(request, result); // Returns same instance
}

[Test]
public void GenerateCorrelationId_WithExistingId_OverwritesId()
{
    // Arrange
    var request = new TestRequest();
    request.CorrelationId = "existing-id";
    
    // Act
    var result = request.GenerateCorrelationId();
    
    // Assert
    Assert.NotEqual("existing-id", result.CorrelationId);
    Assert.NotEmpty(result.CorrelationId);
}

private class TestRequest : ICorrelationIdSupport
{
    public string CorrelationId { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}
```

### Integration Testing

```csharp
[Test]
public async Task ProcessWorkflow_MaintainsCorrelationId()
{
    // Arrange
    var processor = new WorkflowProcessor();
    var request = new WorkflowRequest { Data = "test" }
        .GenerateCorrelationId();
    
    var originalCorrelationId = request.CorrelationId;
    
    // Act
    var result = await processor.ProcessAsync(request);
    
    // Assert
    Assert.Equal(originalCorrelationId, result.CorrelationId);
}
```

## Best Practices

1. **Early Generation**: Generate correlation IDs as early as possible in the request lifecycle
2. **Consistent Propagation**: Always propagate correlation IDs through system boundaries
3. **Immutable IDs**: Avoid changing correlation IDs once generated unless explicitly required
4. **Logging Integration**: Include correlation IDs in all log messages for traceability
5. **Error Handling**: Preserve correlation IDs in exception handling and error responses
6. **Testing**: Always verify correlation ID generation and propagation in tests
7. **Performance**: Use batch methods for processing large collections

## Related Components

- **[CorrelationIdProvider](CorrelationIdProvider.md)**: Core correlation ID generation utility
- **[ICorrelationIdSupport](ICorrelationIdSupport.md)**: Interface defining correlation ID support contract
- **[FeederMessage](../Application/FeederMessage.md)**: Base message class implementing ICorrelationIdSupport

## See Also

- [CorrelationId System Overview](README.md)
- [Distributed Tracing Patterns](../Patterns/DistributedTracing.md)
- [Telemetry Integration](../Application/Telemetry.md)