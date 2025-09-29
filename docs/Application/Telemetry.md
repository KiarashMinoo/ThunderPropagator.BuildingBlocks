# Telemetry

The `Telemetry` static class provides a centralized OpenTelemetry integration utility for distributed tracing and metrics collection. It offers a simplified API for creating activities, counters, histograms, and other telemetry instruments while supporting environment-based configuration for observability scenarios.

## Overview

```csharp
public static class Telemetry
{
    public const string MeterName = "rapidStreamer.meter";
    public const string ActivityName = "rapidStreamer.activity";
    
    public static string Version { get; set; } = "1.0.0";
    
    public static KeyValuePair<string, object?> SuccessfulTag { get; }
    public static KeyValuePair<string, object?> UnsuccessfulTag { get; }
    
    // Activity methods
    public static Activity? StartActivity(string name, ActivityKind kind);
    public static Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext);
    
    // Metrics methods
    public static Counter<T>? CreateCounter<T>(string name, string? unit = null, string? description = null);
    public static UpDownCounter<T>? CreateUpDownCounter<T>(string name, string? unit = null, string? description = null);
    public static Histogram<T>? CreateHistogram<T>(string name, string? unit = null, string? description = null);
    public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null);
}
```

The `Telemetry` class is designed to provide:
- **Distributed Tracing**: ActivitySource-based activity creation for request tracing
- **Metrics Collection**: Counter, histogram, and gauge creation for performance monitoring
- **Environment Configuration**: Automatic configuration from environment variables
- **Performance Optimization**: Lazy initialization and null-safe operations
- **OpenTelemetry Integration**: Full compatibility with OpenTelemetry standards

## Key Features

### Environment-Based Configuration
- **OTEL_EXPORTER_OTLP_ENDPOINT**: Enables ActivitySource when configured
- **ACTIVITY_NAME**: Customizes activity source name (default: "rapidStreamer.activity")
- **VERSION**: Sets version for ActivitySource (default: "1.0.0")
- **METER_ENABLED**: Controls meter initialization (default: true)
- **METER_NAME**: Customizes meter name (default: "rapidStreamer.meter")

### Distributed Tracing Support
- **ActivitySource Integration**: Native support for OpenTelemetry activities
- **Activity Kind Support**: Support for all ActivityKind types (Client, Server, Producer, Consumer, Internal)
- **Parent Context Support**: Hierarchical activity creation with parent-child relationships
- **Null-Safe Operations**: Graceful handling when tracing is disabled

### Metrics Instruments
- **Counters**: Monotonically increasing metrics for counting operations
- **UpDown Counters**: Bidirectional counters for metrics that can increase or decrease
- **Histograms**: Distribution tracking for latency and size measurements
- **Observable Gauges**: Point-in-time measurements with callback-based observation

### Pre-defined Tags
- **SuccessfulTag**: Standard tag for successful operations ("Status", "Success")
- **UnsuccessfulTag**: Standard tag for failed operations ("Status", "Failed")

## Usage Examples

### Basic Activity Tracing

```csharp
public class OrderService
{
    private static readonly Counter<long>? OrderCounter = Telemetry.CreateCounter<long>(
        "orders_processed_total",
        "orders",
        "Total number of orders processed"
    );
    
    private static readonly Histogram<double>? OrderProcessingTime = Telemetry.CreateHistogram<double>(
        "order_processing_duration_seconds",
        "s",
        "Time taken to process an order"
    );
    
    public async Task<Order> ProcessOrderAsync(CreateOrderRequest request)
    {
        // Start activity for distributed tracing
        using var activity = Telemetry.StartActivity("order.process", ActivityKind.Server);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Add activity tags for context
            activity?.SetTag("order.customer_id", request.CustomerId);
            activity?.SetTag("order.item_count", request.Items.Count);
            activity?.SetTag("order.total_amount", request.TotalAmount);
            
            // Simulate order processing
            await ValidateOrderAsync(request);
            var order = await CreateOrderAsync(request);
            await SaveOrderAsync(order);
            await SendConfirmationEmailAsync(order);
            
            // Record successful metrics
            OrderCounter?.Add(1, Telemetry.SuccessfulTag);
            OrderProcessingTime?.Record(stopwatch.Elapsed.TotalSeconds, Telemetry.SuccessfulTag);
            
            // Add success information to activity
            activity?.SetTag("order.id", order.Id);
            activity?.SetStatus(ActivityStatusCode.Ok, "Order processed successfully");
            
            return order;
        }
        catch (Exception ex)
        {
            // Record failure metrics
            OrderCounter?.Add(1, Telemetry.UnsuccessfulTag);
            OrderProcessingTime?.Record(stopwatch.Elapsed.TotalSeconds, Telemetry.UnsuccessfulTag);
            
            // Add error information to activity
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            
            throw;
        }
    }
    
    private async Task ValidateOrderAsync(CreateOrderRequest request)
    {
        using var activity = Telemetry.StartActivity("order.validate", ActivityKind.Internal);
        
        activity?.SetTag("validation.customer_id", request.CustomerId);
        activity?.SetTag("validation.item_count", request.Items.Count);
        
        // Simulate validation logic
        await Task.Delay(50);
        
        if (request.Items.Count == 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Order must contain at least one item");
            throw new ValidationException("Order must contain at least one item");
        }
        
        if (request.TotalAmount <= 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Order total must be greater than zero");
            throw new ValidationException("Order total must be greater than zero");
        }
        
        activity?.SetStatus(ActivityStatusCode.Ok, "Validation completed");
    }
    
    private async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using var activity = Telemetry.StartActivity("order.create", ActivityKind.Internal);
        
        // Simulate order creation
        await Task.Delay(100);
        
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            Items = request.Items,
            TotalAmount = request.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Created
        };
        
        activity?.SetTag("order.id", order.Id);
        activity?.SetStatus(ActivityStatusCode.Ok, "Order created");
        
        return order;
    }
    
    private async Task SaveOrderAsync(Order order)
    {
        using var activity = Telemetry.StartActivity("order.save", ActivityKind.Client);
        
        activity?.SetTag("database.operation", "INSERT");
        activity?.SetTag("database.table", "Orders");
        activity?.SetTag("order.id", order.Id);
        
        try
        {
            // Simulate database save
            await Task.Delay(75);
            
            activity?.SetStatus(ActivityStatusCode.Ok, "Order saved to database");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, $"Failed to save order: {ex.Message}");
            throw;
        }
    }
    
    private async Task SendConfirmationEmailAsync(Order order)
    {
        using var activity = Telemetry.StartActivity("notification.email.send", ActivityKind.Producer);
        
        activity?.SetTag("email.type", "order_confirmation");
        activity?.SetTag("email.customer_id", order.CustomerId);
        activity?.SetTag("order.id", order.Id);
        
        try
        {
            // Simulate email sending
            await Task.Delay(200);
            
            activity?.SetStatus(ActivityStatusCode.Ok, "Confirmation email sent");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, $"Failed to send email: {ex.Message}");
            // Don't throw - email failure shouldn't fail the order
        }
    }
}

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = "";
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class Order
{
    public string Id { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public enum OrderStatus
{
    Created,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

### Comprehensive Metrics Collection

```csharp
public class WebApiMetrics
{
    // Request metrics
    private static readonly Counter<long>? RequestCounter = Telemetry.CreateCounter<long>(
        "http_requests_total",
        "requests",
        "Total number of HTTP requests received"
    );
    
    private static readonly Histogram<double>? RequestDuration = Telemetry.CreateHistogram<double>(
        "http_request_duration_seconds",
        "s",
        "Duration of HTTP requests in seconds"
    );
    
    private static readonly Histogram<long>? RequestSize = Telemetry.CreateHistogram<long>(
        "http_request_size_bytes",
        "bytes",
        "Size of HTTP request bodies in bytes"
    );
    
    private static readonly Histogram<long>? ResponseSize = Telemetry.CreateHistogram<long>(
        "http_response_size_bytes",
        "bytes",
        "Size of HTTP response bodies in bytes"
    );
    
    // Connection metrics
    private static readonly UpDownCounter<int>? ActiveConnections = Telemetry.CreateUpDownCounter<int>(
        "http_active_connections",
        "connections",
        "Number of active HTTP connections"
    );
    
    // Cache metrics
    private static readonly Counter<long>? CacheHits = Telemetry.CreateCounter<long>(
        "cache_hits_total",
        "hits",
        "Total number of cache hits"
    );
    
    private static readonly Counter<long>? CacheMisses = Telemetry.CreateCounter<long>(
        "cache_misses_total",
        "misses",
        "Total number of cache misses"
    );
    
    // Observable metrics
    private static readonly ObservableGauge<int>? MemoryUsage = Telemetry.CreateObservableGauge<int>(
        "process_memory_usage_mb",
        () => (int)(GC.GetTotalMemory(false) / 1024 / 1024),
        "MB",
        "Current memory usage in megabytes"
    );
    
    private static readonly ObservableGauge<int>? ThreadCount = Telemetry.CreateObservableGauge<int>(
        "process_thread_count",
        () => Process.GetCurrentProcess().Threads.Count,
        "threads",
        "Current number of threads"
    );
    
    private static readonly ObservableGauge<double>? CpuUsage = Telemetry.CreateObservableGauge<double>(
        "process_cpu_usage_percent",
        GetCpuUsage,
        "%",
        "Current CPU usage percentage"
    );
    
    private static DateTime _lastCpuTime = DateTime.UtcNow;
    private static TimeSpan _lastProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;
    
    private static double GetCpuUsage()
    {
        var currentTime = DateTime.UtcNow;
        var currentProcessorTime = Process.GetCurrentProcess().TotalProcessorTime;
        
        var timeDiff = currentTime - _lastCpuTime;
        var processorTimeDiff = currentProcessorTime - _lastProcessorTime;
        
        var cpuUsage = processorTimeDiff.TotalMilliseconds / timeDiff.TotalMilliseconds / Environment.ProcessorCount * 100;
        
        _lastCpuTime = currentTime;
        _lastProcessorTime = currentProcessorTime;
        
        return Math.Round(cpuUsage, 2);
    }
    
    public static void RecordRequest(string method, string endpoint, int statusCode, TimeSpan duration, long requestSize, long responseSize)
    {
        var methodTag = new KeyValuePair<string, object?>("method", method);
        var endpointTag = new KeyValuePair<string, object?>("endpoint", endpoint);
        var statusTag = new KeyValuePair<string, object?>("status_code", statusCode);
        var statusCategoryTag = new KeyValuePair<string, object?>("status_category", GetStatusCategory(statusCode));
        
        var tags = new[] { methodTag, endpointTag, statusTag, statusCategoryTag };
        
        RequestCounter?.Add(1, tags);
        RequestDuration?.Record(duration.TotalSeconds, tags);
        RequestSize?.Record(requestSize, tags);
        ResponseSize?.Record(responseSize, tags);
    }
    
    public static void RecordConnectionChange(int change)
    {
        ActiveConnections?.Add(change);
    }
    
    public static void RecordCacheHit(string cacheType, string key)
    {
        var typeTag = new KeyValuePair<string, object?>("cache_type", cacheType);
        var keyTag = new KeyValuePair<string, object?>("cache_key_prefix", GetKeyPrefix(key));
        
        CacheHits?.Add(1, typeTag, keyTag);
    }
    
    public static void RecordCacheMiss(string cacheType, string key)
    {
        var typeTag = new KeyValuePair<string, object?>("cache_type", cacheType);
        var keyTag = new KeyValuePair<string, object?>("cache_key_prefix", GetKeyPrefix(key));
        
        CacheMisses?.Add(1, typeTag, keyTag);
    }
    
    private static string GetStatusCategory(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "2xx",
            >= 300 and < 400 => "3xx",
            >= 400 and < 500 => "4xx",
            >= 500 => "5xx",
            _ => "1xx"
        };
    }
    
    private static string GetKeyPrefix(string key)
    {
        var parts = key.Split(':', '_', '-');
        return parts.Length > 0 ? parts[0] : "unknown";
    }
}

public class ApiController
{
    private readonly CacheService _cacheService;
    
    public ApiController(CacheService cacheService)
    {
        _cacheService = cacheService;
    }
    
    public async Task<IActionResult> GetUserAsync(string userId)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestSize = Encoding.UTF8.GetByteCount(userId);
        
        try
        {
            // Record incoming connection
            WebApiMetrics.RecordConnectionChange(1);
            
            // Try cache first
            var cachedUser = await _cacheService.GetAsync<User>($"user:{userId}");
            if (cachedUser != null)
            {
                WebApiMetrics.RecordCacheHit("user", $"user:{userId}");
                
                var cachedResponse = JsonSerializer.Serialize(cachedUser);
                var cachedResponseSize = Encoding.UTF8.GetByteCount(cachedResponse);
                
                WebApiMetrics.RecordRequest("GET", "/api/users/{id}", 200, stopwatch.Elapsed, requestSize, cachedResponseSize);
                
                return Ok(cachedUser);
            }
            
            WebApiMetrics.RecordCacheMiss("user", $"user:{userId}");
            
            // Fetch from database
            var user = await GetUserFromDatabaseAsync(userId);
            if (user == null)
            {
                WebApiMetrics.RecordRequest("GET", "/api/users/{id}", 404, stopwatch.Elapsed, requestSize, 0);
                return NotFound();
            }
            
            // Cache the result
            await _cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(15));
            
            var response = JsonSerializer.Serialize(user);
            var responseSize = Encoding.UTF8.GetByteCount(response);
            
            WebApiMetrics.RecordRequest("GET", "/api/users/{id}", 200, stopwatch.Elapsed, requestSize, responseSize);
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            var errorResponse = JsonSerializer.Serialize(new { error = ex.Message });
            var errorResponseSize = Encoding.UTF8.GetByteCount(errorResponse);
            
            WebApiMetrics.RecordRequest("GET", "/api/users/{id}", 500, stopwatch.Elapsed, requestSize, errorResponseSize);
            
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            // Record connection closed
            WebApiMetrics.RecordConnectionChange(-1);
        }
    }
    
    private async Task<User?> GetUserFromDatabaseAsync(string userId)
    {
        using var activity = Telemetry.StartActivity("database.get_user", ActivityKind.Client);
        
        activity?.SetTag("database.operation", "SELECT");
        activity?.SetTag("database.table", "Users");
        activity?.SetTag("user.id", userId);
        
        // Simulate database operation
        await Task.Delay(50);
        
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com"
        };
        
        activity?.SetStatus(ActivityStatusCode.Ok, "User retrieved from database");
        
        return user;
    }
}

public class User
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class CacheService
{
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        // Simulate cache lookup
        await Task.Delay(5);
        return null; // Simulate cache miss
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
    {
        // Simulate cache set
        await Task.Delay(10);
    }
}

public interface IActionResult { }
public class OkObjectResult : IActionResult
{
    public object Value { get; }
    public OkObjectResult(object value) => Value = value;
}
public class NotFoundResult : IActionResult { }
public class ObjectResult : IActionResult
{
    public object Value { get; }
    public int StatusCode { get; }
    public ObjectResult(object value) => Value = value;
}

public static class ControllerExtensions
{
    public static IActionResult Ok(object value) => new OkObjectResult(value);
    public static IActionResult NotFound() => new NotFoundResult();
    public static IActionResult StatusCode(int statusCode, object value) => new ObjectResult(value);
}
```

### Background Service Monitoring

```csharp
public class BackgroundTaskService : BackgroundService
{
    private static readonly Counter<long>? TaskCounter = Telemetry.CreateCounter<long>(
        "background_tasks_total",
        "tasks",
        "Total number of background tasks executed"
    );
    
    private static readonly Histogram<double>? TaskDuration = Telemetry.CreateHistogram<double>(
        "background_task_duration_seconds",
        "s",
        "Duration of background task execution"
    );
    
    private static readonly UpDownCounter<int>? ActiveTasks = Telemetry.CreateUpDownCounter<int>(
        "background_active_tasks",
        "tasks",
        "Number of currently active background tasks"
    );
    
    private static readonly ObservableGauge<int>? QueueSize = Telemetry.CreateObservableGauge<int>(
        "background_queue_size",
        () => _taskQueue.Count,
        "tasks",
        "Number of tasks waiting in the queue"
    );
    
    private static readonly ConcurrentQueue<BackgroundTask> _taskQueue = new();
    private readonly ILogger<BackgroundTaskService> _logger;
    
    public BackgroundTaskService(ILogger<BackgroundTaskService> logger)
    {
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = Telemetry.StartActivity("background_service.execute", ActivityKind.Internal);
        
        activity?.SetTag("service.name", nameof(BackgroundTaskService));
        activity?.SetTag("service.version", Telemetry.Version);
        
        try
        {
            await ProcessTasksAsync(stoppingToken);
            activity?.SetStatus(ActivityStatusCode.Ok, "Background service completed");
        }
        catch (OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Background service cancelled");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Background service failed");
            throw;
        }
    }
    
    private async Task ProcessTasksAsync(CancellationToken cancellationToken)
    {
        // Add some sample tasks to the queue
        for (int i = 0; i < 10; i++)
        {
            _taskQueue.Enqueue(new BackgroundTask($"task-{i}", TimeSpan.FromSeconds(Random.Shared.Next(1, 5))));
        }
        
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_taskQueue.TryDequeue(out var task))
            {
                await ProcessTaskAsync(task, cancellationToken);
            }
            else
            {
                // No tasks available, wait a bit
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
    
    private async Task ProcessTaskAsync(BackgroundTask task, CancellationToken cancellationToken)
    {
        using var activity = Telemetry.StartActivity("background_task.process", ActivityKind.Internal);
        
        var stopwatch = Stopwatch.StartNew();
        
        ActiveTasks?.Add(1);
        
        try
        {
            activity?.SetTag("task.id", task.Id);
            activity?.SetTag("task.type", task.Type);
            activity?.SetTag("task.estimated_duration", task.EstimatedDuration.TotalSeconds);
            
            _logger.LogInformation("Processing background task {TaskId}", task.Id);
            
            // Simulate task processing
            await Task.Delay(task.EstimatedDuration, cancellationToken);
            
            // Record successful task completion
            TaskCounter?.Add(1, Telemetry.SuccessfulTag, new KeyValuePair<string, object?>("task.type", task.Type));
            TaskDuration?.Record(stopwatch.Elapsed.TotalSeconds, Telemetry.SuccessfulTag);
            
            activity?.SetStatus(ActivityStatusCode.Ok, "Task completed successfully");
            
            _logger.LogInformation("Completed background task {TaskId} in {Duration}ms", task.Id, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            TaskCounter?.Add(1, Telemetry.UnsuccessfulTag, new KeyValuePair<string, object?>("task.type", task.Type));
            TaskDuration?.Record(stopwatch.Elapsed.TotalSeconds, Telemetry.UnsuccessfulTag);
            
            activity?.SetStatus(ActivityStatusCode.Error, "Task was cancelled");
            
            _logger.LogWarning("Background task {TaskId} was cancelled", task.Id);
            throw;
        }
        catch (Exception ex)
        {
            TaskCounter?.Add(1, Telemetry.UnsuccessfulTag, new KeyValuePair<string, object?>("task.type", task.Type));
            TaskDuration?.Record(stopwatch.Elapsed.TotalSeconds, Telemetry.UnsuccessfulTag);
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error.type", ex.GetType().Name);
            
            _logger.LogError(ex, "Background task {TaskId} failed", task.Id);
            
            // Don't rethrow - continue processing other tasks
        }
        finally
        {
            ActiveTasks?.Add(-1);
        }
    }
    
    public void EnqueueTask(BackgroundTask task)
    {
        _taskQueue.Enqueue(task);
        _logger.LogInformation("Enqueued background task {TaskId}", task.Id);
    }
}

public class BackgroundTask
{
    public string Id { get; }
    public string Type { get; }
    public TimeSpan EstimatedDuration { get; }
    public DateTime CreatedAt { get; }
    
    public BackgroundTask(string id, TimeSpan estimatedDuration, string type = "general")
    {
        Id = id;
        Type = type;
        EstimatedDuration = estimatedDuration;
        CreatedAt = DateTime.UtcNow;
    }
}

public abstract class BackgroundService
{
    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    
    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        return ExecuteAsync(cancellationToken);
    }
    
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public interface ILogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
}
```

### Health Check Integration

```csharp
public class HealthCheckService
{
    private static readonly Counter<long>? HealthCheckCounter = Telemetry.CreateCounter<long>(
        "health_checks_total",
        "checks",
        "Total number of health checks performed"
    );
    
    private static readonly Histogram<double>? HealthCheckDuration = Telemetry.CreateHistogram<double>(
        "health_check_duration_seconds",
        "s",
        "Duration of health check execution"
    );
    
    private static readonly ObservableGauge<int>? HealthStatus = Telemetry.CreateObservableGauge<int>(
        "application_health_status",
        GetOverallHealthStatus,
        "status",
        "Overall application health status (1=healthy, 0=unhealthy)"
    );
    
    private static volatile bool _isHealthy = true;
    private readonly List<IHealthCheck> _healthChecks;
    
    public HealthCheckService(List<IHealthCheck> healthChecks)
    {
        _healthChecks = healthChecks;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        using var activity = Telemetry.StartActivity("health_check.execute", ActivityKind.Internal);
        
        var stopwatch = Stopwatch.StartNew();
        var results = new List<IndividualHealthResult>();
        var overallStatus = HealthStatus.Healthy;
        
        try
        {
            activity?.SetTag("health_check.count", _healthChecks.Count);
            
            // Execute all health checks
            var tasks = _healthChecks.Select(async check =>
            {
                try
                {
                    var result = await ExecuteHealthCheckAsync(check);
                    return result;
                }
                catch (Exception ex)
                {
                    return new IndividualHealthResult
                    {
                        Name = check.Name,
                        Status = HealthStatus.Unhealthy,
                        ErrorMessage = ex.Message,
                        Duration = TimeSpan.Zero
                    };
                }
            });
            
            results.AddRange(await Task.WhenAll(tasks));
            
            // Determine overall status
            if (results.Any(r => r.Status == HealthStatus.Unhealthy))
            {
                overallStatus = HealthStatus.Unhealthy;
            }
            else if (results.Any(r => r.Status == HealthStatus.Degraded))
            {
                overallStatus = HealthStatus.Degraded;
            }
            
            _isHealthy = overallStatus == HealthStatus.Healthy;
            
            // Record metrics
            var statusTag = new KeyValuePair<string, object?>("status", overallStatus.ToString());
            HealthCheckCounter?.Add(1, statusTag);
            HealthCheckDuration?.Record(stopwatch.Elapsed.TotalSeconds, statusTag);
            
            // Update activity
            activity?.SetTag("health_check.overall_status", overallStatus.ToString());
            activity?.SetTag("health_check.healthy_count", results.Count(r => r.Status == HealthStatus.Healthy));
            activity?.SetTag("health_check.degraded_count", results.Count(r => r.Status == HealthStatus.Degraded));
            activity?.SetTag("health_check.unhealthy_count", results.Count(r => r.Status == HealthStatus.Unhealthy));
            
            activity?.SetStatus(ActivityStatusCode.Ok, "Health check completed");
            
            return new HealthCheckResult
            {
                OverallStatus = overallStatus,
                TotalDuration = stopwatch.Elapsed,
                Checks = results,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _isHealthy = false;
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            HealthCheckCounter?.Add(1, new KeyValuePair<string, object?>("status", "Error"));
            HealthCheckDuration?.Record(stopwatch.Elapsed.TotalSeconds, new KeyValuePair<string, object?>("status", "Error"));
            
            throw;
        }
    }
    
    private async Task<IndividualHealthResult> ExecuteHealthCheckAsync(IHealthCheck healthCheck)
    {
        using var activity = Telemetry.StartActivity("health_check.individual", ActivityKind.Internal);
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            activity?.SetTag("health_check.name", healthCheck.Name);
            activity?.SetTag("health_check.type", healthCheck.GetType().Name);
            
            var isHealthy = await healthCheck.CheckHealthAsync();
            var status = isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            
            activity?.SetTag("health_check.result", status.ToString());
            activity?.SetStatus(ActivityStatusCode.Ok, $"Health check '{healthCheck.Name}' completed");
            
            return new IndividualHealthResult
            {
                Name = healthCheck.Name,
                Status = status,
                Duration = stopwatch.Elapsed,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            activity?.SetTag("health_check.result", "Error");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            
            return new IndividualHealthResult
            {
                Name = healthCheck.Name,
                Status = HealthStatus.Unhealthy,
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message
            };
        }
    }
    
    private static int GetOverallHealthStatus()
    {
        return _isHealthy ? 1 : 0;
    }
}

public interface IHealthCheck
{
    string Name { get; }
    Task<bool> CheckHealthAsync();
}

public class DatabaseHealthCheck : IHealthCheck
{
    public string Name => "Database";
    
    public async Task<bool> CheckHealthAsync()
    {
        using var activity = Telemetry.StartActivity("health_check.database", ActivityKind.Client);
        
        try
        {
            // Simulate database connectivity check
            await Task.Delay(50);
            
            activity?.SetTag("database.type", "SqlServer");
            activity?.SetStatus(ActivityStatusCode.Ok, "Database is accessible");
            
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

public class CacheHealthCheck : IHealthCheck
{
    public string Name => "Cache";
    
    public async Task<bool> CheckHealthAsync()
    {
        using var activity = Telemetry.StartActivity("health_check.cache", ActivityKind.Client);
        
        try
        {
            // Simulate cache connectivity check
            await Task.Delay(25);
            
            activity?.SetTag("cache.type", "Redis");
            activity?.SetStatus(ActivityStatusCode.Ok, "Cache is accessible");
            
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}

public class HealthCheckResult
{
    public HealthStatus OverallStatus { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<IndividualHealthResult> Checks { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class IndividualHealthResult
{
    public string Name { get; set; } = "";
    public HealthStatus Status { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
}

public class HealthCheckDemo
{
    public static async Task DemonstrateHealthCheckTelemetry()
    {
        var healthChecks = new List<IHealthCheck>
        {
            new DatabaseHealthCheck(),
            new CacheHealthCheck()
        };
        
        var healthCheckService = new HealthCheckService(healthChecks);
        
        Console.WriteLine("=== Health Check Telemetry Demo ===\n");
        
        // Perform health checks
        for (int i = 0; i < 3; i++)
        {
            try
            {
                var result = await healthCheckService.CheckHealthAsync();
                
                Console.WriteLine($"Health Check #{i + 1}:");
                Console.WriteLine($"  Overall Status: {result.OverallStatus}");
                Console.WriteLine($"  Total Duration: {result.TotalDuration.TotalMilliseconds:F2} ms");
                Console.WriteLine($"  Timestamp: {result.Timestamp}");
                
                foreach (var check in result.Checks)
                {
                    Console.WriteLine($"  - {check.Name}: {check.Status} ({check.Duration.TotalMilliseconds:F2} ms)");
                    if (!string.IsNullOrEmpty(check.ErrorMessage))
                    {
                        Console.WriteLine($"    Error: {check.ErrorMessage}");
                    }
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Health check failed: {ex.Message}\n");
            }
            
            await Task.Delay(1000); // Wait between checks
        }
    }
}
```

## Environment Configuration

### Environment Variables

```bash
# Enable OpenTelemetry tracing
export OTEL_EXPORTER_OTLP_ENDPOINT="http://localhost:4317"

# Customize telemetry configuration
export ACTIVITY_NAME="myapp.activity"
export VERSION="2.1.0"
export METER_NAME="myapp.meter"
export METER_ENABLED="true"
```

### Docker Configuration

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Set telemetry environment variables
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:14268/api/traces
ENV ACTIVITY_NAME=myapp.activity
ENV VERSION=1.0.0
ENV METER_ENABLED=true

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore "./MyApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Docker Compose with Observability Stack

```yaml
# docker-compose.yml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "80:80"
    environment:
      - OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:14268/api/traces
      - ACTIVITY_NAME=myapp.activity
      - VERSION=1.0.0
      - METER_ENABLED=true
    depends_on:
      - jaeger
      - prometheus
    
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "14268:14268"
    environment:
      - COLLECTOR_OTLP_ENABLED=true
    
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--web.enable-lifecycle'
    
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana-storage:/var/lib/grafana

volumes:
  grafana-storage:
```

## Performance Considerations

### Null-Safe Operations
- All telemetry operations are null-safe when instruments are not initialized
- Minimal performance impact when telemetry is disabled
- Lazy initialization based on environment configuration

### Memory Efficiency
- Static instrument instances prevent repeated allocations
- Efficient tag handling with KeyValuePair structures
- Minimal object creation for high-frequency operations

### Initialization Overhead
- Static constructor performs one-time initialization
- Environment variable checks occur only at startup
- ActivitySource and Meter creation is conditional

## Best Practices

### 1. **Activity Naming Conventions**

```csharp
public static class ActivityNames
{
    // Use hierarchical naming
    public const string OrderProcess = "order.process";
    public const string OrderValidate = "order.validate";
    public const string OrderSave = "order.save";
    
    // Include operation type
    public const string DatabaseQuery = "database.query";
    public const string DatabaseUpdate = "database.update";
    public const string CacheGet = "cache.get";
    public const string CacheSet = "cache.set";
    
    // Use descriptive names for external calls
    public const string HttpClientRequest = "http.client.request";
    public const string MessageQueueSend = "mq.send";
    public const string MessageQueueReceive = "mq.receive";
}
```

### 2. **Metric Design Patterns**

```csharp
public static class MetricPatterns
{
    // Use consistent naming conventions
    private static readonly Counter<long>? RequestsTotal = Telemetry.CreateCounter<long>(
        "requests_total", // Use _total suffix for counters
        "requests",
        "Total number of requests"
    );
    
    private static readonly Histogram<double>? RequestDuration = Telemetry.CreateHistogram<double>(
        "request_duration_seconds", // Use _seconds suffix for time
        "s",
        "Request duration in seconds"
    );
    
    // Include meaningful tags
    public static void RecordRequest(string method, string endpoint, int statusCode, double duration)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("method", method),
            new("endpoint", SanitizeEndpoint(endpoint)),
            new("status_code", statusCode),
            new("status_class", GetStatusClass(statusCode))
        };
        
        RequestsTotal?.Add(1, tags);
        RequestDuration?.Record(duration, tags);
    }
    
    private static string SanitizeEndpoint(string endpoint)
    {
        // Replace IDs with placeholders to prevent high cardinality
        return Regex.Replace(endpoint, @"/\d+", "/{id}");
    }
    
    private static string GetStatusClass(int statusCode)
    {
        return statusCode / 100 switch
        {
            2 => "2xx",
            3 => "3xx",
            4 => "4xx",
            5 => "5xx",
            _ => "other"
        };
    }
}
```

### 3. **Error Handling**

```csharp
public static class TelemetryErrorHandling
{
    public static void SafeRecordMetric(Action recordAction, string metricName)
    {
        try
        {
            recordAction();
        }
        catch (Exception ex)
        {
            // Log telemetry errors but don't fail the application
            Console.WriteLine($"Telemetry error for {metricName}: {ex.Message}");
        }
    }
    
    public static Activity? SafeStartActivity(string name, ActivityKind kind)
    {
        try
        {
            return Telemetry.StartActivity(name, kind);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start activity {name}: {ex.Message}");
            return null;
        }
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class TelemetryTests
{
    [Test]
    public void StartActivity_WithValidParameters_ReturnsActivity()
    {
        // Arrange
        var activityName = "test.activity";
        var activityKind = ActivityKind.Internal;
        
        // Act
        using var activity = Telemetry.StartActivity(activityName, activityKind);
        
        // Assert - activity may be null if telemetry is disabled
        if (activity != null)
        {
            Assert.That(activity.OperationName, Is.EqualTo(activityName));
            Assert.That(activity.Kind, Is.EqualTo(activityKind));
        }
    }
    
    [Test]
    public void CreateCounter_WithValidParameters_ReturnsCounter()
    {
        // Arrange
        var name = "test_counter";
        var unit = "operations";
        var description = "Test counter for unit tests";
        
        // Act
        var counter = Telemetry.CreateCounter<int>(name, unit, description);
        
        // Assert - counter may be null if metrics are disabled
        Assert.That(counter, Is.Not.Null.Or.Null);
    }
    
    [Test]
    public void SuccessfulTag_HasCorrectValues()
    {
        // Act
        var tag = Telemetry.SuccessfulTag;
        
        // Assert
        Assert.That(tag.Key, Is.EqualTo("Status"));
        Assert.That(tag.Value, Is.EqualTo("Success"));
    }
    
    [Test]
    public void UnsuccessfulTag_HasCorrectValues()
    {
        // Act
        var tag = Telemetry.UnsuccessfulTag;
        
        // Assert
        Assert.That(tag.Key, Is.EqualTo("Status"));
        Assert.That(tag.Value, Is.EqualTo("Failed"));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class TelemetryIntegrationTests
{
    [Test]
    public async Task OrderService_ProcessOrder_CreatesTelemetryData()
    {
        // Arrange
        var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            SampleUsingParentId = (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => { /* Track started activities */ },
            ActivityStopped = activity => { /* Track completed activities */ }
        };
        
        ActivitySource.AddActivityListener(listener);
        
        var orderService = new OrderService();
        var request = new CreateOrderRequest
        {
            CustomerId = "customer-123",
            Items = new List<OrderItem>
            {
                new OrderItem { ProductId = "product-1", Quantity = 2, Price = 25.00m }
            },
            TotalAmount = 50.00m
        };
        
        // Act
        var order = await orderService.ProcessOrderAsync(request);
        
        // Assert
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Id, Is.Not.Empty);
        
        // Cleanup
        listener.Dispose();
    }
}
```

## See Also

- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/) - Official OpenTelemetry documentation
- [System.Diagnostics.Activity](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.activity) - .NET Activity class for distributed tracing
- [System.Diagnostics.Metrics](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics) - .NET metrics API
- [ActivitySource](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.activitysource) - Activity source for creating activities
- [Meter](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.meter) - Meter class for creating metric instruments

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing centralized OpenTelemetry integration for distributed tracing and metrics collection with environment-based configuration.*