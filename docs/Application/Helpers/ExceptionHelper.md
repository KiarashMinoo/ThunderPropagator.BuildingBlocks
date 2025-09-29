# ExceptionHelper

The `ExceptionHelper` class is a static utility class in the RapidStreamer BuildingBlocks that provides essential exception handling and description utilities. It offers convenient extension methods for Exception objects to create detailed, hierarchical descriptions of exception chains including inner exceptions.

## Purpose

This helper serves as:
- An exception description generator for detailed error reporting
- A hierarchical exception traversal utility that includes inner exceptions
- A logging and debugging aid for comprehensive error analysis
- A user-friendly error message formatter with customizable separators
- A foundation for error handling patterns in enterprise applications

## Key Features

- **Exception Chain Traversal**: Automatically traverses the complete exception hierarchy
- **Customizable Separators**: Configurable separator strings for formatting output
- **Inner Exception Support**: Comprehensive handling of nested exception structures
- **Memory Efficient**: Optimized string building without excessive allocations
- **Pattern Matching**: Uses modern C# pattern matching for clean, readable code
- **Null Safety**: Robust handling of null exception scenarios

## Method

### Describe
Creates a comprehensive description of an exception including all inner exceptions in the chain.

```csharp
public static string Describe(this Exception exception, string separator = " => ")
```

**Implementation:**
```csharp
public static string Describe(this Exception exception, string separator = " => ")
{
    var rtn = "";

    var ex = exception;
    while (ex is not null)
    {
        rtn += string.IsNullOrWhiteSpace(rtn) switch
        {
            false => $"{separator}{ex.Message}",
            _ => ex.Message
        };

        ex = ex.InnerException;
    }

    return rtn;
}
```

**Key Features:**
- **Chain Traversal**: Iterates through the complete exception hierarchy
- **Custom Separators**: Allows customization of the separator between exception messages
- **Pattern Matching**: Uses switch expressions for clean conditional logic
- **Memory Efficient**: Builds the description string incrementally
- **Hierarchical Display**: Shows the progression from outer to inner exceptions

## Usage Examples

### Basic Exception Description

```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

try
{
    // Simulate a complex operation that might throw nested exceptions
    ThrowNestedExceptions();
}
catch (Exception ex)
{
    // Get comprehensive exception description
    string description = ex.Describe();
    Console.WriteLine($"Error occurred: {description}");
    
    // Custom separator
    string detailedDescription = ex.Describe(" --> ");
    Console.WriteLine($"Detailed error: {detailedDescription}");
}

void ThrowNestedExceptions()
{
    try
    {
        try
        {
            throw new InvalidOperationException("Database connection failed");
        }
        catch (Exception inner)
        {
            throw new DataException("Failed to retrieve user data", inner);
        }
    }
    catch (Exception inner)
    {
        throw new ApplicationException("Service operation failed", inner);
    }
}

// Output:
// Error occurred: Service operation failed => Failed to retrieve user data => Database connection failed
// Detailed error: Service operation failed --> Failed to retrieve user data --> Database connection failed
```

### Logging Integration

```csharp
public class ErrorLogger
{
    private readonly ILogger _logger;
    
    public ErrorLogger(ILogger logger)
    {
        _logger = logger;
    }
    
    public void LogError(Exception exception, string context = "")
    {
        var errorDescription = exception.Describe();
        var logMessage = string.IsNullOrEmpty(context) 
            ? $"Error: {errorDescription}"
            : $"Error in {context}: {errorDescription}";
            
        _logger.LogError(exception, logMessage);
    }
    
    public void LogErrorWithSeparator(Exception exception, string separator, string context = "")
    {
        var errorDescription = exception.Describe(separator);
        var logMessage = string.IsNullOrEmpty(context) 
            ? $"Error: {errorDescription}"
            : $"Error in {context}: {errorDescription}";
            
        _logger.LogError(exception, logMessage);
    }
    
    public ErrorSummary CreateErrorSummary(Exception exception)
    {
        return new ErrorSummary
        {
            MainError = exception.Message,
            FullDescription = exception.Describe(),
            ExceptionType = exception.GetType().Name,
            InnerExceptionCount = CountInnerExceptions(exception),
            StackTrace = exception.StackTrace ?? string.Empty,
            Timestamp = DateTime.UtcNow
        };
    }
    
    private int CountInnerExceptions(Exception exception)
    {
        int count = 0;
        var current = exception.InnerException;
        
        while (current != null)
        {
            count++;
            current = current.InnerException;
        }
        
        return count;
    }
}

public class ErrorSummary
{
    public string MainError { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public int InnerExceptionCount { get; set; }
    public string StackTrace { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

// Usage
var logger = new ErrorLogger(serviceProvider.GetService<ILogger>());

try
{
    await ProcessComplexOperation();
}
catch (Exception ex)
{
    logger.LogError(ex, "ProcessComplexOperation");
    logger.LogErrorWithSeparator(ex, " | ", "ProcessComplexOperation");
    
    var summary = logger.CreateErrorSummary(ex);
    Console.WriteLine($"Error summary: {summary.FullDescription}");
}
```

### API Error Response Generation

```csharp
public class ApiErrorHandler
{
    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? TraceId { get; set; }
    }
    
    public ErrorResponse CreateErrorResponse(Exception exception, string? traceId = null)
    {
        return new ErrorResponse
        {
            Error = exception.Message,
            Details = exception.Describe(" | "),
            ErrorCode = exception.GetType().Name,
            TraceId = traceId
        };
    }
    
    public ErrorResponse CreateUserFriendlyErrorResponse(Exception exception, string? traceId = null)
    {
        // Create user-friendly message while preserving technical details
        var userMessage = GetUserFriendlyMessage(exception);
        var technicalDetails = exception.Describe();
        
        return new ErrorResponse
        {
            Error = userMessage,
            Details = technicalDetails,
            ErrorCode = GenerateErrorCode(exception),
            TraceId = traceId
        };
    }
    
    private string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Invalid input provided",
            UnauthorizedAccessException => "Access denied",
            FileNotFoundException => "Requested resource not found",
            TimeoutException => "Operation timed out",
            InvalidOperationException => "Operation cannot be completed at this time",
            _ => "An error occurred while processing your request"
        };
    }
    
    private string GenerateErrorCode(Exception exception)
    {
        var baseCode = exception.GetType().Name.Replace("Exception", "");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        return $"{baseCode}_{timestamp}";
    }
}

// Usage in Web API Controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApiErrorHandler _errorHandler;
    
    public UsersController(ApiErrorHandler errorHandler)
    {
        _errorHandler = errorHandler;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await userService.GetUserAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            var traceId = HttpContext.TraceIdentifier;
            var errorResponse = _errorHandler.CreateUserFriendlyErrorResponse(ex, traceId);
            
            return ex switch
            {
                ArgumentException => BadRequest(errorResponse),
                UnauthorizedAccessException => Unauthorized(errorResponse),
                NotFoundException => NotFound(errorResponse),
                _ => StatusCode(500, errorResponse)
            };
        }
    }
}
```

### Diagnostic and Debug Utilities

```csharp
public class DiagnosticHelper
{
    public static class ExceptionAnalyzer
    {
        public static ExceptionAnalysis Analyze(Exception exception)
        {
            var analysis = new ExceptionAnalysis
            {
                OriginalException = exception,
                FullDescription = exception.Describe(),
                ExceptionChain = BuildExceptionChain(exception),
                Severity = DetermineSeverity(exception),
                Category = CategorizeException(exception),
                Recommendations = GenerateRecommendations(exception)
            };
            
            return analysis;
        }
        
        private static List<ExceptionDetail> BuildExceptionChain(Exception exception)
        {
            var chain = new List<ExceptionDetail>();
            var current = exception;
            int level = 0;
            
            while (current != null)
            {
                chain.Add(new ExceptionDetail
                {
                    Level = level,
                    ExceptionType = current.GetType().Name,
                    Message = current.Message,
                    Source = current.Source ?? "Unknown",
                    StackTrace = current.StackTrace ?? string.Empty
                });
                
                current = current.InnerException;
                level++;
            }
            
            return chain;
        }
        
        private static ExceptionSeverity DetermineSeverity(Exception exception)
        {
            return exception switch
            {
                ArgumentException or ArgumentNullException => ExceptionSeverity.Warning,
                UnauthorizedAccessException => ExceptionSeverity.Warning,
                FileNotFoundException => ExceptionSeverity.Warning,
                TimeoutException => ExceptionSeverity.Error,
                OutOfMemoryException => ExceptionSeverity.Critical,
                StackOverflowException => ExceptionSeverity.Critical,
                AccessViolationException => ExceptionSeverity.Critical,
                _ => ExceptionSeverity.Error
            };
        }
        
        private static ExceptionCategory CategorizeException(Exception exception)
        {
            return exception switch
            {
                ArgumentException or ArgumentNullException => ExceptionCategory.Validation,
                UnauthorizedAccessException => ExceptionCategory.Security,
                FileNotFoundException or DirectoryNotFoundException => ExceptionCategory.IO,
                SqlException or DbException => ExceptionCategory.Database,
                HttpRequestException => ExceptionCategory.Network,
                TimeoutException => ExceptionCategory.Performance,
                OutOfMemoryException => ExceptionCategory.Memory,
                _ => ExceptionCategory.General
            };
        }
        
        private static List<string> GenerateRecommendations(Exception exception)
        {
            var recommendations = new List<string>();
            
            switch (exception)
            {
                case ArgumentException:
                    recommendations.Add("Validate input parameters before processing");
                    recommendations.Add("Add proper input validation at API boundaries");
                    break;
                    
                case UnauthorizedAccessException:
                    recommendations.Add("Check user permissions and authentication status");
                    recommendations.Add("Verify role-based access controls");
                    break;
                    
                case FileNotFoundException:
                    recommendations.Add("Verify file path and existence before access");
                    recommendations.Add("Implement proper file handling with try-catch blocks");
                    break;
                    
                case TimeoutException:
                    recommendations.Add("Increase timeout values if appropriate");
                    recommendations.Add("Implement retry mechanisms with exponential backoff");
                    recommendations.Add("Check network connectivity and service availability");
                    break;
                    
                case OutOfMemoryException:
                    recommendations.Add("Review memory usage patterns and optimize allocations");
                    recommendations.Add("Implement proper dispose patterns for IDisposable objects");
                    recommendations.Add("Consider streaming approaches for large data sets");
                    break;
                    
                default:
                    recommendations.Add("Review exception details and stack trace");
                    recommendations.Add("Implement appropriate error handling");
                    break;
            }
            
            return recommendations;
        }
    }
    
    public enum ExceptionSeverity { Warning, Error, Critical }
    public enum ExceptionCategory { Validation, Security, IO, Database, Network, Performance, Memory, General }
    
    public class ExceptionAnalysis
    {
        public Exception OriginalException { get; set; } = null!;
        public string FullDescription { get; set; } = string.Empty;
        public List<ExceptionDetail> ExceptionChain { get; set; } = new();
        public ExceptionSeverity Severity { get; set; }
        public ExceptionCategory Category { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }
    
    public class ExceptionDetail
    {
        public int Level { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
    }
}

// Usage
try
{
    // Complex operation that might fail
    await ProcessComplexWorkflow();
}
catch (Exception ex)
{
    var analysis = DiagnosticHelper.ExceptionAnalyzer.Analyze(ex);
    
    Console.WriteLine($"Exception Analysis:");
    Console.WriteLine($"Description: {analysis.FullDescription}");
    Console.WriteLine($"Severity: {analysis.Severity}");
    Console.WriteLine($"Category: {analysis.Category}");
    Console.WriteLine($"Chain Length: {analysis.ExceptionChain.Count}");
    
    Console.WriteLine("\nException Chain:");
    foreach (var detail in analysis.ExceptionChain)
    {
        Console.WriteLine($"  Level {detail.Level}: {detail.ExceptionType} - {detail.Message}");
    }
    
    Console.WriteLine("\nRecommendations:");
    foreach (var recommendation in analysis.Recommendations)
    {
        Console.WriteLine($"  - {recommendation}");
    }
}
```

## Real-World Applications

### Enterprise Error Handling System

```csharp
public class EnterpriseErrorHandler
{
    private readonly ILogger<EnterpriseErrorHandler> _logger;
    private readonly IMetricsCollector _metrics;
    
    public EnterpriseErrorHandler(ILogger<EnterpriseErrorHandler> logger, IMetricsCollector metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }
    
    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception exception, ErrorContext context)
    {
        var errorId = Guid.NewGuid().ToString();
        var description = exception.Describe();
        
        // Log error with full description
        _logger.LogError(exception, "Error {ErrorId} in {Context}: {Description}", 
            errorId, context.OperationName, description);
        
        // Collect metrics
        _metrics.IncrementCounter("errors.total", new[] { 
            ("exception_type", exception.GetType().Name),
            ("operation", context.OperationName),
            ("severity", DetermineSeverity(exception).ToString())
        });
        
        // Create error report
        var errorReport = new ErrorReport
        {
            ErrorId = errorId,
            Timestamp = DateTime.UtcNow,
            Context = context,
            Description = description,
            ExceptionType = exception.GetType().FullName ?? "Unknown",
            Severity = DetermineSeverity(exception),
            UserMessage = GenerateUserMessage(exception),
            TechnicalDetails = exception.ToString()
        };
        
        // Store error for analysis
        await StoreErrorReportAsync(errorReport);
        
        return new ErrorHandlingResult
        {
            ErrorId = errorId,
            UserMessage = errorReport.UserMessage,
            ShouldRetry = ShouldRetry(exception),
            RecommendedAction = GetRecommendedAction(exception)
        };
    }
    
    private ExceptionSeverity DetermineSeverity(Exception exception)
    {
        // Implementation similar to previous examples
        return exception switch
        {
            ArgumentException => ExceptionSeverity.Warning,
            TimeoutException => ExceptionSeverity.Error,
            OutOfMemoryException => ExceptionSeverity.Critical,
            _ => ExceptionSeverity.Error
        };
    }
    
    private string GenerateUserMessage(Exception exception)
    {
        var rootCause = GetRootCause(exception);
        
        return rootCause switch
        {
            ArgumentException => "Please check your input and try again",
            UnauthorizedAccessException => "You don't have permission to perform this action",
            TimeoutException => "The operation is taking longer than expected. Please try again",
            _ => "An unexpected error occurred. Please contact support if the problem persists"
        };
    }
    
    private Exception GetRootCause(Exception exception)
    {
        var current = exception;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current;
    }
    
    private bool ShouldRetry(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            SqlException sqlEx => sqlEx.Number == 2, // Connection timeout
            _ => false
        };
    }
    
    private string GetRecommendedAction(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Validate input parameters",
            UnauthorizedAccessException => "Check user permissions",
            TimeoutException => "Retry operation or increase timeout",
            FileNotFoundException => "Verify file exists",
            _ => "Contact technical support"
        };
    }
    
    private async Task StoreErrorReportAsync(ErrorReport report)
    {
        // Store in database, send to monitoring system, etc.
        await Task.Delay(1); // Placeholder
    }
}

public class ErrorContext
{
    public string OperationName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class ErrorReport
{
    public string ErrorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ErrorContext Context { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public ExceptionSeverity Severity { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public string TechnicalDetails { get; set; } = string.Empty;
}

public class ErrorHandlingResult
{
    public string ErrorId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public bool ShouldRetry { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}
```

### Health Check and Monitoring Integration

```csharp
public class HealthCheckErrorReporter
{
    public HealthCheckResult ReportHealthCheckFailure(Exception exception, string healthCheckName)
    {
        var description = exception.Describe(" | ");
        var severity = DetermineHealthImpact(exception);
        
        return new HealthCheckResult
        {
            Status = HealthStatus.Unhealthy,
            Description = $"Health check '{healthCheckName}' failed: {description}",
            Exception = exception,
            Data = new Dictionary<string, object>
            {
                ["error_description"] = description,
                ["exception_type"] = exception.GetType().Name,
                ["severity"] = severity.ToString(),
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }
    
    private HealthImpact DetermineHealthImpact(Exception exception)
    {
        return exception switch
        {
            SqlException => HealthImpact.Critical,  // Database down
            HttpRequestException => HealthImpact.Major,  // External service down
            TimeoutException => HealthImpact.Minor,  // Slow response
            _ => HealthImpact.Major
        };
    }
}

public enum HealthImpact { Minor, Major, Critical }
```

## Integration with ExceptionInfo

The `ExceptionHelper` works with the `ExceptionInfo` class for serialization scenarios:

```csharp
public static class ExtendedExceptionHelper
{
    public static ExceptionInfo ToExceptionInfo(this Exception exception)
    {
        return new ExceptionInfo(exception);
    }
    
    public static string DescribeExceptionInfo(this ExceptionInfo exceptionInfo)
    {
        var description = exceptionInfo.Message;
        var current = exceptionInfo.InnerException;
        
        while (current != null)
        {
            description += $" => {current.Message}";
            current = current.InnerException;
        }
        
        return description;
    }
    
    public static ExceptionSummary CreateSummary(this Exception exception)
    {
        return new ExceptionSummary
        {
            Description = exception.Describe(),
            ExceptionInfo = exception.ToExceptionInfo(),
            RootCause = GetRootCause(exception).Message,
            ExceptionCount = CountExceptions(exception)
        };
    }
    
    private static Exception GetRootCause(Exception exception)
    {
        var current = exception;
        while (current.InnerException != null)
        {
            current = current.InnerException;
        }
        return current;
    }
    
    private static int CountExceptions(Exception exception)
    {
        int count = 1;
        var current = exception.InnerException;
        
        while (current != null)
        {
            count++;
            current = current.InnerException;
        }
        
        return count;
    }
}

public class ExceptionSummary
{
    public string Description { get; set; } = string.Empty;
    public ExceptionInfo ExceptionInfo { get; set; } = null!;
    public string RootCause { get; set; } = string.Empty;
    public int ExceptionCount { get; set; }
}
```

## Performance Considerations

### Memory Efficiency
- Uses incremental string building instead of StringBuilder for small exception chains
- Pattern matching compiles to efficient IL code
- Minimal object allocations during description generation

### Optimization for Large Exception Chains
```csharp
public static class OptimizedExceptionHelper
{
    public static string DescribeOptimized(this Exception exception, string separator = " => ")
    {
        // Pre-calculate required capacity
        var capacity = CalculateRequiredCapacity(exception, separator);
        var sb = new StringBuilder(capacity);
        
        var current = exception;
        bool isFirst = true;
        
        while (current != null)
        {
            if (!isFirst)
            {
                sb.Append(separator);
            }
            
            sb.Append(current.Message);
            current = current.InnerException;
            isFirst = false;
        }
        
        return sb.ToString();
    }
    
    private static int CalculateRequiredCapacity(Exception exception, string separator)
    {
        int totalLength = 0;
        var current = exception;
        bool isFirst = true;
        
        while (current != null)
        {
            if (!isFirst)
            {
                totalLength += separator.Length;
            }
            
            totalLength += current.Message.Length;
            current = current.InnerException;
            isFirst = false;
        }
        
        return totalLength;
    }
}
```

## Thread Safety

- **Static Method**: Thread-safe as it's a stateless static extension method
- **String Immutability**: Works with immutable strings, ensuring thread safety
- **No Shared State**: Each method call operates independently

## Testing Strategies

```csharp
[Test]
public void Describe_WithSingleException_ReturnsMessage()
{
    // Arrange
    var exception = new InvalidOperationException("Test error");
    
    // Act
    string result = exception.Describe();
    
    // Assert
    Assert.Equal("Test error", result);
}

[Test]
public void Describe_WithNestedExceptions_ReturnsChainedMessage()
{
    // Arrange
    var innerException = new ArgumentException("Inner error");
    var outerException = new InvalidOperationException("Outer error", innerException);
    
    // Act
    string result = outerException.Describe();
    
    // Assert
    Assert.Equal("Outer error => Inner error", result);
}

[Test]
public void Describe_WithCustomSeparator_UsesCustomSeparator()
{
    // Arrange
    var innerException = new ArgumentException("Inner error");
    var outerException = new InvalidOperationException("Outer error", innerException);
    
    // Act
    string result = outerException.Describe(" | ");
    
    // Assert
    Assert.Equal("Outer error | Inner error", result);
}

[Test]
public void Describe_WithDeepNesting_HandlesAllLevels()
{
    // Arrange
    var level3 = new ArgumentException("Level 3 error");
    var level2 = new InvalidOperationException("Level 2 error", level3);
    var level1 = new ApplicationException("Level 1 error", level2);
    
    // Act
    string result = level1.Describe();
    
    // Assert
    Assert.Equal("Level 1 error => Level 2 error => Level 3 error", result);
}
```

## Best Practices

1. **Use for Logging**: Leverage `Describe()` for comprehensive error logging that captures the full exception context
2. **Custom Separators**: Choose separators that work well with your logging format and readability requirements
3. **Error Reporting**: Include the full description in error reports for debugging and support purposes
4. **User Messages**: Use the description for technical logs, but create user-friendly messages for UI display
5. **Integration**: Combine with structured logging for better error tracking and analysis

## Error Handling

```csharp
public static class SafeExceptionHelper
{
    public static string SafeDescribe(this Exception? exception, string separator = " => ", string defaultMessage = "Unknown error")
    {
        if (exception == null)
        {
            return defaultMessage;
        }
        
        try
        {
            return exception.Describe(separator);
        }
        catch
        {
            return defaultMessage;
        }
    }
    
    public static bool TryDescribe(this Exception exception, out string description, string separator = " => ")
    {
        description = string.Empty;
        
        try
        {
            description = exception.Describe(separator);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

## Related Components

- **[ExceptionInfo](../ExceptionInfo.md)**: Provides serializable exception information for JSON/binary formats
- **[JsonHelper](JsonHelper.md)**: Uses ExceptionInfo for exception serialization in JSON operations
- **[GuardClauseHelper](GuardClauseHelper.md)**: Integrates with validation frameworks for comprehensive error reporting
- **[Telemetry](../Telemetry.md)**: Supports error tracking and monitoring in distributed systems

The `ExceptionHelper` provides essential exception handling capabilities with a focus on clarity, completeness, and ease of integration, making it a valuable tool for robust error handling in enterprise applications.