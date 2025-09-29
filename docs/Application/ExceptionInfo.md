# ExceptionInfo

The `ExceptionInfo` class provides a serializable representation of .NET exceptions, enabling safe exception information storage, transmission, and logging across application boundaries. It captures essential exception details while avoiding serialization issues commonly encountered with raw `Exception` objects.

## Overview

```csharp
public sealed class ExceptionInfo
{
    [JsonProperty, JsonInclude] public string Type { get; private set; }
    [JsonProperty, JsonInclude] public string Message { get; private set; }
    [JsonProperty, JsonInclude] public string? Source { get; private set; }
    [JsonProperty, JsonInclude] public ExceptionInfo? InnerException { get; set; }
    
    // Private constructor for JSON serialization
    private ExceptionInfo() { }
    
    // Internal constructor from Exception
    internal ExceptionInfo(Exception exception);
    
    // Explicit conversion operator
    public static explicit operator ExceptionInfo(Exception exception);
}
```

The `ExceptionInfo` class is designed to solve common challenges with exception serialization in distributed systems, logging frameworks, and cross-service communication scenarios.

## Key Features

### Serialization Support
- **JSON Compatible**: Full support for both System.Text.Json and Newtonsoft.Json
- **Cross-Platform**: Serializable across different .NET implementations
- **Version Resilient**: Maintains compatibility across application versions
- **Lightweight**: Contains only essential exception information

### Exception Preservation
- **Type Information**: Preserves the full type name of the original exception
- **Message Content**: Captures the complete exception message
- **Source Context**: Includes the source assembly or component information
- **Inner Exception Chain**: Recursively captures inner exception details (one level deep)

### Safety Features
- **No Sensitive Data**: Avoids capturing potentially sensitive stack trace information
- **Immutable Design**: Read-only properties prevent accidental modification
- **Null Safety**: Proper handling of null values and missing properties

## Constructor and Conversion

### Internal Constructor
```csharp
internal ExceptionInfo(Exception exception)
```
Creates an `ExceptionInfo` instance from an `Exception` object, capturing type, message, source, and first-level inner exception.

### Explicit Conversion Operator
```csharp
public static explicit operator ExceptionInfo(Exception exception)
```
Provides a clean syntax for converting exceptions to serializable form.

## Usage Examples

### Basic Exception Serialization

```csharp
public class ExceptionHandlingService
{
    private readonly ILogger<ExceptionHandlingService> _logger;
    private readonly IExceptionRepository _repository;
    
    public ExceptionHandlingService(ILogger<ExceptionHandlingService> logger, IExceptionRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    
    public async Task<ApiResponse<T>> SafeExecuteAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            var result = await operation();
            return ApiResponse<T>.Success(result);
        }
        catch (Exception ex)
        {
            // Convert exception to serializable form
            var exceptionInfo = (ExceptionInfo)ex;
            
            // Log the exception information
            _logger.LogError("Operation {OperationName} failed: {ExceptionType} - {Message}", 
                operationName, exceptionInfo.Type, exceptionInfo.Message);
            
            // Store exception for analysis
            await _repository.StoreExceptionAsync(operationName, exceptionInfo);
            
            return ApiResponse<T>.Failure(exceptionInfo);
        }
    }
    
    public async Task ProcessMultipleOperationsAsync(IEnumerable<Func<Task>> operations)
    {
        var results = new List<OperationResult>();
        
        foreach (var (operation, index) in operations.Select((op, i) => (op, i)))
        {
            try
            {
                await operation();
                results.Add(new OperationResult 
                { 
                    Index = index, 
                    Success = true 
                });
            }
            catch (Exception ex)
            {
                var exceptionInfo = (ExceptionInfo)ex;
                results.Add(new OperationResult 
                { 
                    Index = index, 
                    Success = false, 
                    Exception = exceptionInfo 
                });
                
                _logger.LogWarning("Operation {Index} failed: {ExceptionType}", index, exceptionInfo.Type);
            }
        }
        
        // Analyze results
        var failedCount = results.Count(r => !r.Success);
        if (failedCount > 0)
        {
            _logger.LogWarning("Batch operation completed with {FailedCount}/{TotalCount} failures", 
                failedCount, results.Count);
            
            // Group exceptions by type for analysis
            var exceptionGroups = results
                .Where(r => !r.Success && r.Exception != null)
                .GroupBy(r => r.Exception!.Type)
                .ToList();
            
            foreach (var group in exceptionGroups)
            {
                _logger.LogInformation("Exception type {Type} occurred {Count} times", 
                    group.Key, group.Count());
            }
        }
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ExceptionInfo? Error { get; set; }
    
    public static ApiResponse<T> Success(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Failure(ExceptionInfo error) => new() { Success = false, Error = error };
}

public class OperationResult
{
    public int Index { get; set; }
    public bool Success { get; set; }
    public ExceptionInfo? Exception { get; set; }
}
```

### JSON Serialization and API Communication

```csharp
public class ExceptionAwareApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExceptionAwareApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ExceptionAwareApiClient(HttpClient httpClient, ILogger<ExceptionAwareApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
    
    public async Task<ApiResult<T>> CallApiAsync<T>(string endpoint, object? request = null)
    {
        try
        {
            HttpResponseMessage response;
            
            if (request != null)
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(endpoint, content);
            }
            else
            {
                response = await _httpClient.GetAsync(endpoint);
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                return ApiResult<T>.Success(data);
            }
            else
            {
                // Try to deserialize error response as ExceptionInfo
                try
                {
                    var errorInfo = JsonSerializer.Deserialize<ExceptionInfo>(responseContent, _jsonOptions);
                    return ApiResult<T>.Failure(errorInfo);
                }
                catch
                {
                    // Fallback to HTTP error
                    var httpException = new HttpRequestException($"HTTP {response.StatusCode}: {responseContent}");
                    return ApiResult<T>.Failure((ExceptionInfo)httpException);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API call to {Endpoint} failed", endpoint);
            return ApiResult<T>.Failure((ExceptionInfo)ex);
        }
    }
    
    public async Task<List<ExceptionInfo>> GetSystemErrorsAsync(DateTime since)
    {
        try
        {
            var endpoint = $"/api/system/errors?since={since:yyyy-MM-ddTHH:mm:ss}";
            var response = await _httpClient.GetAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<List<ExceptionInfo>>(json, _jsonOptions) ?? new List<ExceptionInfo>();
            }
            
            _logger.LogWarning("Failed to retrieve system errors: {StatusCode}", response.StatusCode);
            return new List<ExceptionInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system errors");
            return new List<ExceptionInfo>();
        }
    }
}

public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ExceptionInfo? Error { get; set; }
    
    public static ApiResult<T> Success(T? data) => new() { Success = true, Data = data };
    public static ApiResult<T> Failure(ExceptionInfo error) => new() { Success = false, Error = error };
}
```

### Structured Logging Integration

```csharp
public class StructuredExceptionLogger
{
    private readonly ILogger<StructuredExceptionLogger> _logger;
    
    public StructuredExceptionLogger(ILogger<StructuredExceptionLogger> logger)
    {
        _logger = logger;
    }
    
    public void LogException(Exception exception, string context, Dictionary<string, object>? additionalProperties = null)
    {
        var exceptionInfo = (ExceptionInfo)exception;
        
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Context"] = context,
            ["ExceptionType"] = exceptionInfo.Type,
            ["ExceptionSource"] = exceptionInfo.Source ?? "Unknown"
        });
        
        var logData = new
        {
            Exception = exceptionInfo,
            Context = context,
            Timestamp = DateTime.UtcNow,
            AdditionalProperties = additionalProperties ?? new Dictionary<string, object>()
        };
        
        _logger.LogError("Exception occurred in {Context}: {Message} | Details: {@LogData}", 
            context, exceptionInfo.Message, logData);
        
        // Log inner exception details if present
        if (exceptionInfo.InnerException != null)
        {
            _logger.LogError("Inner exception: {InnerType} - {InnerMessage}", 
                exceptionInfo.InnerException.Type, exceptionInfo.InnerException.Message);
        }
    }
    
    public async Task LogExceptionWithAnalysisAsync(Exception exception, string context)
    {
        var exceptionInfo = (ExceptionInfo)exception;
        
        // Perform exception analysis
        var analysis = AnalyzeException(exceptionInfo);
        
        var enrichedLog = new
        {
            Exception = exceptionInfo,
            Context = context,
            Analysis = analysis,
            Timestamp = DateTime.UtcNow,
            CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        _logger.LogError("Exception with analysis: {Message} | {@EnrichedLog}", 
            exceptionInfo.Message, enrichedLog);
        
        // Store for trend analysis
        await StoreExceptionForTrendAnalysisAsync(exceptionInfo, context, analysis);
    }
    
    private ExceptionAnalysis AnalyzeException(ExceptionInfo exceptionInfo)
    {
        var analysis = new ExceptionAnalysis
        {
            Severity = DetermineSeverity(exceptionInfo.Type),
            Category = CategorizeException(exceptionInfo.Type),
            IsRecoverable = IsRecoverableException(exceptionInfo.Type),
            SuggestedAction = SuggestAction(exceptionInfo.Type, exceptionInfo.Message)
        };
        
        return analysis;
    }
    
    private ExceptionSeverity DetermineSeverity(string exceptionType)
    {
        return exceptionType switch
        {
            var t when t.Contains("OutOfMemory") => ExceptionSeverity.Critical,
            var t when t.Contains("StackOverflow") => ExceptionSeverity.Critical,
            var t when t.Contains("AccessViolation") => ExceptionSeverity.Critical,
            var t when t.Contains("Security") => ExceptionSeverity.High,
            var t when t.Contains("Unauthorized") => ExceptionSeverity.High,
            var t when t.Contains("Timeout") => ExceptionSeverity.Medium,
            var t when t.Contains("Http") => ExceptionSeverity.Medium,
            var t when t.Contains("Argument") => ExceptionSeverity.Low,
            var t when t.Contains("NotFound") => ExceptionSeverity.Low,
            _ => ExceptionSeverity.Medium
        };
    }
    
    private ExceptionCategory CategorizeException(string exceptionType)
    {
        return exceptionType switch
        {
            var t when t.Contains("Sql") || t.Contains("Database") => ExceptionCategory.Database,
            var t when t.Contains("Http") || t.Contains("Network") => ExceptionCategory.Network,
            var t when t.Contains("Security") || t.Contains("Unauthorized") => ExceptionCategory.Security,
            var t when t.Contains("Argument") || t.Contains("Invalid") => ExceptionCategory.Validation,
            var t when t.Contains("NotFound") || t.Contains("Missing") => ExceptionCategory.NotFound,
            var t when t.Contains("Timeout") => ExceptionCategory.Performance,
            _ => ExceptionCategory.General
        };
    }
    
    private bool IsRecoverableException(string exceptionType)
    {
        var nonRecoverableTypes = new[]
        {
            "OutOfMemoryException",
            "StackOverflowException",
            "AccessViolationException",
            "BadImageFormatException"
        };
        
        return !nonRecoverableTypes.Any(type => exceptionType.Contains(type));
    }
    
    private string SuggestAction(string exceptionType, string message)
    {
        return exceptionType switch
        {
            var t when t.Contains("Timeout") => "Consider increasing timeout values or implementing retry logic",
            var t when t.Contains("Network") || t.Contains("Http") => "Check network connectivity and service availability",
            var t when t.Contains("Database") || t.Contains("Sql") => "Verify database connection and query performance",
            var t when t.Contains("Unauthorized") => "Check authentication credentials and permissions",
            var t when t.Contains("NotFound") => "Verify resource existence and path correctness",
            var t when t.Contains("Argument") => "Validate input parameters and business logic",
            _ => "Review error details and application logs for more context"
        };
    }
    
    private async Task StoreExceptionForTrendAnalysisAsync(ExceptionInfo exceptionInfo, string context, ExceptionAnalysis analysis)
    {
        // Simulate storing exception data for trend analysis
        await Task.Delay(10);
        
        _logger.LogDebug("Exception stored for trend analysis: {Type} in {Context}", 
            exceptionInfo.Type, context);
    }
}

public class ExceptionAnalysis
{
    public ExceptionSeverity Severity { get; set; }
    public ExceptionCategory Category { get; set; }
    public bool IsRecoverable { get; set; }
    public string SuggestedAction { get; set; } = "";
}

public enum ExceptionSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum ExceptionCategory
{
    General,
    Database,
    Network,
    Security,
    Validation,
    NotFound,
    Performance
}
```

### Exception Repository and Analytics

```csharp
public interface IExceptionRepository
{
    Task StoreExceptionAsync(string context, ExceptionInfo exceptionInfo);
    Task<List<ExceptionInfo>> GetExceptionsAsync(DateTime from, DateTime to);
    Task<ExceptionStatistics> GetExceptionStatisticsAsync(TimeSpan period);
}

public class ExceptionRepository : IExceptionRepository
{
    private readonly ILogger<ExceptionRepository> _logger;
    private readonly List<StoredExceptionInfo> _exceptions; // In-memory for demo
    
    public ExceptionRepository(ILogger<ExceptionRepository> logger)
    {
        _logger = logger;
        _exceptions = new List<StoredExceptionInfo>();
    }
    
    public async Task StoreExceptionAsync(string context, ExceptionInfo exceptionInfo)
    {
        var stored = new StoredExceptionInfo
        {
            Id = Guid.NewGuid(),
            Context = context,
            Exception = exceptionInfo,
            Timestamp = DateTime.UtcNow,
            ProcessId = Environment.ProcessId,
            MachineName = Environment.MachineName
        };
        
        _exceptions.Add(stored);
        
        _logger.LogDebug("Stored exception {Id} from context {Context}", stored.Id, context);
        
        await Task.CompletedTask;
    }
    
    public async Task<List<ExceptionInfo>> GetExceptionsAsync(DateTime from, DateTime to)
    {
        var filtered = _exceptions
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .Select(e => e.Exception)
            .ToList();
        
        _logger.LogInformation("Retrieved {Count} exceptions between {From} and {To}", 
            filtered.Count, from, to);
        
        return await Task.FromResult(filtered);
    }
    
    public async Task<ExceptionStatistics> GetExceptionStatisticsAsync(TimeSpan period)
    {
        var cutoff = DateTime.UtcNow - period;
        var recentExceptions = _exceptions.Where(e => e.Timestamp >= cutoff).ToList();
        
        var statistics = new ExceptionStatistics
        {
            Period = period,
            TotalCount = recentExceptions.Count,
            UniqueTypes = recentExceptions.Select(e => e.Exception.Type).Distinct().Count(),
            MostCommonType = recentExceptions
                .GroupBy(e => e.Exception.Type)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "None",
            ExceptionsByType = recentExceptions
                .GroupBy(e => e.Exception.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            ExceptionsByContext = recentExceptions
                .GroupBy(e => e.Context)
                .ToDictionary(g => g.Key, g => g.Count()),
            Timeline = GenerateTimeline(recentExceptions, period)
        };
        
        _logger.LogInformation("Generated statistics for {Period}: {Total} exceptions, {Unique} unique types", 
            period, statistics.TotalCount, statistics.UniqueTypes);
        
        return await Task.FromResult(statistics);
    }
    
    private List<TimelineEntry> GenerateTimeline(List<StoredExceptionInfo> exceptions, TimeSpan period)
    {
        var bucketSize = period.TotalMinutes > 60 ? TimeSpan.FromHours(1) : TimeSpan.FromMinutes(10);
        var start = DateTime.UtcNow - period;
        var timeline = new List<TimelineEntry>();
        
        for (var time = start; time <= DateTime.UtcNow; time += bucketSize)
        {
            var bucketEnd = time + bucketSize;
            var count = exceptions.Count(e => e.Timestamp >= time && e.Timestamp < bucketEnd);
            
            timeline.Add(new TimelineEntry
            {
                Timestamp = time,
                Count = count
            });
        }
        
        return timeline;
    }
}

public class StoredExceptionInfo
{
    public Guid Id { get; set; }
    public string Context { get; set; } = "";
    public ExceptionInfo Exception { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public int ProcessId { get; set; }
    public string MachineName { get; set; } = "";
}

public class ExceptionStatistics
{
    public TimeSpan Period { get; set; }
    public int TotalCount { get; set; }
    public int UniqueTypes { get; set; }
    public string MostCommonType { get; set; } = "";
    public Dictionary<string, int> ExceptionsByType { get; set; } = new();
    public Dictionary<string, int> ExceptionsByContext { get; set; } = new();
    public List<TimelineEntry> Timeline { get; set; } = new();
}

public class TimelineEntry
{
    public DateTime Timestamp { get; set; }
    public int Count { get; set; }
}
```

### Cross-Service Exception Propagation

```csharp
public class CrossServiceExceptionHandler
{
    private readonly ILogger<CrossServiceExceptionHandler> _logger;
    private readonly IServiceBusPublisher _serviceBus;
    
    public CrossServiceExceptionHandler(ILogger<CrossServiceExceptionHandler> logger, IServiceBusPublisher serviceBus)
    {
        _logger = logger;
        _serviceBus = serviceBus;
    }
    
    public async Task PropagateExceptionAsync(Exception exception, string sourceService, string operationId)
    {
        var exceptionInfo = (ExceptionInfo)exception;
        
        var exceptionEvent = new ServiceExceptionEvent
        {
            Id = Guid.NewGuid(),
            SourceService = sourceService,
            OperationId = operationId,
            Exception = exceptionInfo,
            Timestamp = DateTime.UtcNow,
            Severity = DetermineSeverity(exceptionInfo.Type)
        };
        
        // Publish exception event to service bus
        await _serviceBus.PublishAsync("service-exceptions", exceptionEvent);
        
        _logger.LogInformation("Exception event {EventId} published for operation {OperationId} from service {SourceService}", 
            exceptionEvent.Id, operationId, sourceService);
    }
    
    public async Task HandleIncomingExceptionEventAsync(ServiceExceptionEvent exceptionEvent)
    {
        _logger.LogWarning("Received exception event from service {SourceService}: {ExceptionType} - {Message}", 
            exceptionEvent.SourceService, exceptionEvent.Exception.Type, exceptionEvent.Exception.Message);
        
        // Take action based on exception severity and type
        switch (exceptionEvent.Severity)
        {
            case ExceptionSeverity.Critical:
                await HandleCriticalExceptionAsync(exceptionEvent);
                break;
                
            case ExceptionSeverity.High:
                await HandleHighSeverityExceptionAsync(exceptionEvent);
                break;
                
            case ExceptionSeverity.Medium:
            case ExceptionSeverity.Low:
                await LogAndMonitorExceptionAsync(exceptionEvent);
                break;
        }
    }
    
    private async Task HandleCriticalExceptionAsync(ServiceExceptionEvent exceptionEvent)
    {
        _logger.LogCritical("Critical exception in service {SourceService}: {ExceptionType}", 
            exceptionEvent.SourceService, exceptionEvent.Exception.Type);
        
        // Implement critical exception handling (alerting, circuit breaking, etc.)
        await NotifyOperationsTeamAsync(exceptionEvent);
        await CheckServiceHealthAsync(exceptionEvent.SourceService);
    }
    
    private async Task HandleHighSeverityExceptionAsync(ServiceExceptionEvent exceptionEvent)
    {
        _logger.LogError("High severity exception in service {SourceService}: {ExceptionType}", 
            exceptionEvent.SourceService, exceptionEvent.Exception.Type);
        
        // Implement high severity handling
        await IncrementErrorCounterAsync(exceptionEvent.SourceService);
        await CheckErrorThresholdAsync(exceptionEvent.SourceService);
    }
    
    private async Task LogAndMonitorExceptionAsync(ServiceExceptionEvent exceptionEvent)
    {
        _logger.LogInformation("Exception logged from service {SourceService}: {ExceptionType}", 
            exceptionEvent.SourceService, exceptionEvent.Exception.Type);
        
        // Regular monitoring and logging
        await UpdateMetricsAsync(exceptionEvent);
    }
    
    private ExceptionSeverity DetermineSeverity(string exceptionType)
    {
        // Implementation similar to previous example
        return exceptionType switch
        {
            var t when t.Contains("OutOfMemory") => ExceptionSeverity.Critical,
            var t when t.Contains("Security") => ExceptionSeverity.High,
            var t when t.Contains("Timeout") => ExceptionSeverity.Medium,
            _ => ExceptionSeverity.Low
        };
    }
    
    private async Task NotifyOperationsTeamAsync(ServiceExceptionEvent exceptionEvent)
    {
        // Simulate alerting operations team
        await Task.Delay(100);
        _logger.LogInformation("Operations team notified about critical exception in {SourceService}", 
            exceptionEvent.SourceService);
    }
    
    private async Task CheckServiceHealthAsync(string serviceName)
    {
        // Simulate health check
        await Task.Delay(50);
        _logger.LogInformation("Health check initiated for service {ServiceName}", serviceName);
    }
    
    private async Task IncrementErrorCounterAsync(string serviceName)
    {
        // Simulate error counter increment
        await Task.Delay(10);
        _logger.LogDebug("Error counter incremented for service {ServiceName}", serviceName);
    }
    
    private async Task CheckErrorThresholdAsync(string serviceName)
    {
        // Simulate threshold checking
        await Task.Delay(20);
        _logger.LogDebug("Error threshold checked for service {ServiceName}", serviceName);
    }
    
    private async Task UpdateMetricsAsync(ServiceExceptionEvent exceptionEvent)
    {
        // Simulate metrics update
        await Task.Delay(5);
        _logger.LogDebug("Metrics updated for exception from {SourceService}", exceptionEvent.SourceService);
    }
}

public class ServiceExceptionEvent
{
    public Guid Id { get; set; }
    public string SourceService { get; set; } = "";
    public string OperationId { get; set; } = "";
    public ExceptionInfo Exception { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public ExceptionSeverity Severity { get; set; }
}

// Mock service bus interface
public interface IServiceBusPublisher
{
    Task PublishAsync<T>(string topic, T message);
}
```

## JSON Serialization Examples

### System.Text.Json

```csharp
public class SystemTextJsonExample
{
    public void DemonstrateSystemTextJsonSerialization()
    {
        try
        {
            throw new InvalidOperationException("Test exception", 
                new ArgumentException("Inner exception"));
        }
        catch (Exception ex)
        {
            var exceptionInfo = (ExceptionInfo)ex;
            
            // Serialize using System.Text.Json
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(exceptionInfo, options);
            Console.WriteLine("System.Text.Json output:");
            Console.WriteLine(json);
            
            // Deserialize back
            var deserialized = JsonSerializer.Deserialize<ExceptionInfo>(json, options);
            Console.WriteLine($"Deserialized Type: {deserialized?.Type}");
            Console.WriteLine($"Deserialized Message: {deserialized?.Message}");
            Console.WriteLine($"Has Inner Exception: {deserialized?.InnerException != null}");
        }
    }
}
```

### Newtonsoft.Json

```csharp
public class NewtonsoftJsonExample
{
    public void DemonstrateNewtonsoftJsonSerialization()
    {
        try
        {
            var innerException = new ArgumentNullException("paramName", "Parameter cannot be null");
            throw new InvalidOperationException("Operation failed due to invalid state", innerException);
        }
        catch (Exception ex)
        {
            var exceptionInfo = (ExceptionInfo)ex;
            
            // Serialize using Newtonsoft.Json
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
            
            var json = JsonConvert.SerializeObject(exceptionInfo, settings);
            Console.WriteLine("Newtonsoft.Json output:");
            Console.WriteLine(json);
            
            // Deserialize back
            var deserialized = JsonConvert.DeserializeObject<ExceptionInfo>(json, settings);
            Console.WriteLine($"Deserialized Type: {deserialized?.Type}");
            Console.WriteLine($"Deserialized Message: {deserialized?.Message}");
            Console.WriteLine($"Inner Exception Type: {deserialized?.InnerException?.Type}");
        }
    }
}
```

## Best Practices

### 1. **Exception Context Preservation**

```csharp
public static class ExceptionInfoExtensions
{
    public static ExceptionInfo WithContext(this ExceptionInfo exceptionInfo, string context)
    {
        // Since ExceptionInfo is immutable, we create contextual metadata separately
        var contextualException = new ContextualExceptionInfo
        {
            Exception = exceptionInfo,
            Context = context,
            Timestamp = DateTime.UtcNow,
            CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        return exceptionInfo;
    }
    
    public static bool IsOfType<T>(this ExceptionInfo exceptionInfo) where T : Exception
    {
        return exceptionInfo.Type == typeof(T).FullName;
    }
    
    public static bool IsTransient(this ExceptionInfo exceptionInfo)
    {
        var transientTypes = new[]
        {
            typeof(TimeoutException).FullName,
            typeof(HttpRequestException).FullName,
            typeof(TaskCanceledException).FullName
        };
        
        return transientTypes.Contains(exceptionInfo.Type);
    }
}

public class ContextualExceptionInfo
{
    public ExceptionInfo Exception { get; set; } = null!;
    public string Context { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; } = "";
}
```

### 2. **Safe Exception Conversion**

```csharp
public static class SafeExceptionConverter
{
    public static ExceptionInfo ToExceptionInfo(Exception? exception)
    {
        if (exception == null)
        {
            return CreateNullExceptionInfo();
        }
        
        try
        {
            return (ExceptionInfo)exception;
        }
        catch (Exception conversionException)
        {
            // Fallback if conversion fails
            return CreateFallbackExceptionInfo(exception, conversionException);
        }
    }
    
    private static ExceptionInfo CreateNullExceptionInfo()
    {
        var nullException = new ArgumentNullException("exception", "Exception was null");
        return (ExceptionInfo)nullException;
    }
    
    private static ExceptionInfo CreateFallbackExceptionInfo(Exception originalException, Exception conversionException)
    {
        var fallbackMessage = $"Failed to convert exception of type {originalException.GetType().Name}: {conversionException.Message}";
        var fallbackException = new InvalidOperationException(fallbackMessage);
        return (ExceptionInfo)fallbackException;
    }
}
```

### 3. **Performance Considerations**

```csharp
public class PerformantExceptionHandling
{
    private static readonly ConcurrentDictionary<Type, string> TypeNameCache = new();
    
    public static string GetCachedTypeName(Type exceptionType)
    {
        return TypeNameCache.GetOrAdd(exceptionType, type => type.FullName ?? type.Name);
    }
    
    public static ExceptionInfo CreateOptimized(Exception exception)
    {
        // Use cached type names for better performance
        var typeName = GetCachedTypeName(exception.GetType());
        
        // Create ExceptionInfo manually for performance-critical scenarios
        // Note: This would require making the constructor public or using reflection
        return (ExceptionInfo)exception;
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class ExceptionInfoTests
{
    [Test]
    public void Constructor_WithSimpleException_CapturesBasicProperties()
    {
        // Arrange
        var originalException = new InvalidOperationException("Test message");
        
        // Act
        var exceptionInfo = (ExceptionInfo)originalException;
        
        // Assert
        Assert.That(exceptionInfo.Type, Is.EqualTo(typeof(InvalidOperationException).FullName));
        Assert.That(exceptionInfo.Message, Is.EqualTo("Test message"));
        Assert.That(exceptionInfo.InnerException, Is.Null);
    }
    
    [Test]
    public void Constructor_WithInnerException_CapturesInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("Inner message");
        var outerException = new InvalidOperationException("Outer message", innerException);
        
        // Act
        var exceptionInfo = (ExceptionInfo)outerException;
        
        // Assert
        Assert.That(exceptionInfo.InnerException, Is.Not.Null);
        Assert.That(exceptionInfo.InnerException.Type, Is.EqualTo(typeof(ArgumentException).FullName));
        Assert.That(exceptionInfo.InnerException.Message, Is.EqualTo("Inner message"));
    }
    
    [Test]
    public void JsonSerialization_SystemTextJson_RoundTripSuccessful()
    {
        // Arrange
        var originalException = new TimeoutException("Operation timed out");
        var exceptionInfo = (ExceptionInfo)originalException;
        
        // Act
        var json = JsonSerializer.Serialize(exceptionInfo);
        var deserialized = JsonSerializer.Deserialize<ExceptionInfo>(json);
        
        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Type, Is.EqualTo(exceptionInfo.Type));
        Assert.That(deserialized.Message, Is.EqualTo(exceptionInfo.Message));
    }
    
    [Test]
    public void JsonSerialization_NewtonsoftJson_RoundTripSuccessful()
    {
        // Arrange
        var originalException = new UnauthorizedAccessException("Access denied");
        var exceptionInfo = (ExceptionInfo)originalException;
        
        // Act
        var json = JsonConvert.SerializeObject(exceptionInfo);
        var deserialized = JsonConvert.DeserializeObject<ExceptionInfo>(json);
        
        // Assert
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Type, Is.EqualTo(exceptionInfo.Type));
        Assert.That(deserialized.Message, Is.EqualTo(exceptionInfo.Message));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class ExceptionInfoIntegrationTests
{
    [Test]
    public async Task ApiExceptionHandling_EndToEnd_PreservesExceptionDetails()
    {
        // Arrange
        var originalException = new HttpRequestException("Network error", 
            new SocketException((int)SocketError.TimedOut));
        
        var exceptionInfo = (ExceptionInfo)originalException;
        
        // Simulate API serialization
        var apiResponse = new { Success = false, Error = exceptionInfo };
        var json = JsonSerializer.Serialize(apiResponse);
        
        // Act - Simulate receiving the response
        var deserializedResponse = JsonSerializer.Deserialize<JsonElement>(json);
        var errorElement = deserializedResponse.GetProperty("Error");
        var receivedExceptionInfo = JsonSerializer.Deserialize<ExceptionInfo>(errorElement.GetRawText());
        
        // Assert
        Assert.That(receivedExceptionInfo, Is.Not.Null);
        Assert.That(receivedExceptionInfo.Type, Contains.Substring("HttpRequestException"));
        Assert.That(receivedExceptionInfo.Message, Is.EqualTo("Network error"));
        Assert.That(receivedExceptionInfo.InnerException, Is.Not.Null);
        Assert.That(receivedExceptionInfo.InnerException.Type, Contains.Substring("SocketException"));
    }
}
```

## See Also

- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON serialization library
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json) - Built-in JSON serialization
- [Exception](https://learn.microsoft.com/en-us/dotnet/api/system.exception) - Base Exception class
- [StructuredLogging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging) - Structured logging in .NET
- [ExceptionHelper](Helpers/ExceptionHelper.md) - Exception handling utilities

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing serializable exception representation for safe cross-boundary exception handling and logging.*