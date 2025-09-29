# JsonHelper

The `JsonHelper` is a comprehensive JSON serialization utility built on `System.Text.Json` that provides high-performance JSON operations with custom attribute support, telemetry integration, and specialized exception handling. It offers a fluent API for serialization, deserialization, and format conversion operations.

## Overview

Located in `RapidStreamer.BuildingBlocks.Application.Helpers`, the `JsonHelper` enhances JSON operations by providing:

- **Custom Attribute Support**: Integration with `JsonSerializationAttribute` for camelCase control
- **Exception Serialization**: Specialized handling for `Exception` objects through `ExceptionInfo`
- **Multiple Format Support**: JSON string, byte array, and Base64 encoding
- **Performance Optimization**: Built-in telemetry tracking and optimized serialization paths
- **Flexible Configuration**: Configurable `JsonSerializerOptions` through lambda expressions

## Key Features

### 🚀 High-Performance Serialization
- Built on `System.Text.Json` for optimal performance
- Telemetry integration for monitoring and optimization
- Concurrent dictionary caching for attribute resolution
- Memory-efficient byte array and Base64 operations

### 🎛️ Custom Configuration
- `JsonSerializationAttribute` support for per-type camelCase control
- Configurable serializer options through lambda expressions
- Default options with camelCase naming and cycle handling
- Type-specific option resolution

### 🔄 Multiple Format Support
- JSON string serialization/deserialization
- Byte array encoding for efficient storage
- Base64 encoding for text-safe transmission
- Exception-specific serialization through `ExceptionInfo`

### 📊 Observability
- Built-in activity tracking for all operations
- Performance monitoring through telemetry
- Detailed operation metrics and timing

## Core Methods

### JSON String Operations

#### ToJson
```csharp
public static string ToJson<T>(this T instance, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
```

#### FromJson
```csharp
public static T? FromJson<T>(this string json, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)

public static object? FromJson(this string json, Type type, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
```

### Byte Array Operations

#### ToJsonBytes / FromJsonBytes
```csharp
public static byte[] ToJsonBytes<T>(this T instance, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
    where T : notnull

public static T? FromJsonBytes<T>(this byte[] bytes, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
```

### Base64 Operations

#### ToJsonBase64 / FromJsonBase64
```csharp
public static string ToJsonBase64<T>(this T instance, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
    where T : notnull

public static T? FromJsonBase64<T>(this string str, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
```

## Configuration System

### Default Serializer Options
```csharp
private static JsonSerializerOptions BuildDefaultSerializerOptions()
    => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };
```

### JsonSerializationAttribute Integration
The helper automatically detects and applies `JsonSerializationAttribute` settings:

```csharp
[JsonSerialization(CamelCase = false)]
public class DatabaseModel
{
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
}

// Serialization will use PascalCase despite default camelCase policy
var json = dbModel.ToJson();
```

### Custom Options Configuration
```csharp
var json = myObject.ToJson(options => 
{
    options.WriteIndented = true;
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    return options;
});
```

## Usage Examples

### Basic Serialization
```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };

// Serialize to JSON string
var json = user.ToJson();
// Output: {"id":1,"name":"John Doe","email":"john@example.com"}

// Deserialize from JSON string
var deserializedUser = json.FromJson<User>();
```

### Exception Serialization
```csharp
try
{
    // Some operation that might throw
    throw new InvalidOperationException("Something went wrong");
}
catch (Exception ex)
{
    // Exceptions are automatically converted to ExceptionInfo
    var exceptionJson = ex.ToJson();
    
    // Contains structured exception information
    var exceptionInfo = exceptionJson.FromJson<ExceptionInfo>();
    Console.WriteLine($"Exception: {exceptionInfo.Message}");
    Console.WriteLine($"Stack Trace: {exceptionInfo.StackTrace}");
}
```

### Custom Configuration
```csharp
var product = new Product { Name = "Widget", Price = 99.99m };

// Custom serialization options
var prettyJson = product.ToJson(options =>
{
    options.WriteIndented = true;
    options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    return options;
});

// Custom deserialization options
var deserializedProduct = prettyJson.FromJson<Product>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    return options;
});
```

### Byte Array Operations
```csharp
var largeObject = GenerateLargeDataSet();

// Convert to byte array for efficient storage
var bytes = largeObject.ToJsonBytes();
await File.WriteAllBytesAsync("data.json", bytes);

// Read back from byte array
var loadedBytes = await File.ReadAllBytesAsync("data.json");
var restoredObject = loadedBytes.FromJsonBytes<DataSet>();
```

### Base64 Operations
```csharp
var sensitiveData = new { UserId = 123, Token = "secret" };

// Encode to Base64 for URL-safe transmission
var base64Json = sensitiveData.ToJsonBase64();
var url = $"https://api.example.com/callback?data={base64Json}";

// Decode from Base64
var receivedData = base64Json.FromJsonBase64<dynamic>();
```

## Advanced Scenarios

### API Response Handling
```csharp
public class ApiController : ControllerBase
{
    [HttpGet("user/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        try
        {
            var user = await userService.GetUserAsync(id);
            
            // Custom JSON response with specific formatting
            var json = user.ToJson(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                return options;
            });
            
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            // Structured exception response
            var errorJson = ex.ToJson();
            return BadRequest(errorJson);
        }
    }
}
```

### Message Queue Integration
```csharp
public class MessagePublisher
{
    public async Task PublishAsync<T>(T message) where T : notnull
    {
        // Serialize message for queue
        var messageBytes = message.ToJsonBytes(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            return options;
        });
        
        await messageQueue.SendAsync(messageBytes);
    }
}

public class MessageConsumer
{
    public async Task<T?> ConsumeAsync<T>()
    {
        var messageBytes = await messageQueue.ReceiveAsync();
        return messageBytes.FromJsonBytes<T>();
    }
}
```

### Configuration Management
```csharp
public class ConfigurationManager
{
    public async Task SaveConfigurationAsync<T>(T config, string filePath) 
        where T : notnull
    {
        var json = config.ToJson(options =>
        {
            options.WriteIndented = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return options;
        });
        
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task<T?> LoadConfigurationAsync<T>(string filePath)
    {
        if (!File.Exists(filePath)) return default;
        
        var json = await File.ReadAllTextAsync(filePath);
        return json.FromJson<T>(options =>
        {
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            return options;
        });
    }
}
```

### Caching Integration
```csharp
public class CacheService
{
    private readonly IMemoryCache _cache;
    
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, 
        TimeSpan expiration) where T : notnull
    {
        // Try to get from cache as Base64
        if (_cache.TryGetValue(key, out string cachedBase64))
        {
            return cachedBase64.FromJsonBase64<T>();
        }
        
        // Get fresh data
        var data = await factory();
        
        // Store as Base64 in cache
        var base64 = data.ToJsonBase64();
        _cache.Set(key, base64, expiration);
        
        return data;
    }
}
```

## Performance Optimization

### Attribute Caching
The helper uses a concurrent dictionary to cache `JsonSerializationAttribute` lookups:

```csharp
private static readonly ConcurrentDictionary<Type, JsonSerializationAttribute?> 
    JsonSerializationAttributes = new();
```

### Telemetry Integration
All operations include telemetry tracking:

```csharp
const string activityName = $"{nameof(JsonHelper)}_{nameof(ToJson)}";
using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
```

### Memory Efficiency
```csharp
// Efficient byte array operations
public static byte[] ToJsonBytes<T>(this T instance, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
    where T : notnull
{
    var jsonStr = instance.ToJson(options);
    var bytes = Encoding.UTF8.GetBytes(jsonStr);
    return bytes;
}
```

## Error Handling

### Null Safety
```csharp
public static T? FromJsonBytes<T>(this byte[] bytes, 
    Func<JsonSerializerOptions, JsonSerializerOptions>? options = null)
{
    if (bytes.Length == 0)
    {
        return default;
    }

    var jsonStr = Encoding.UTF8.GetString(bytes);
    if (string.IsNullOrWhiteSpace(jsonStr))
    {
        return default;
    }

    return jsonStr.FromJson<T>(options);
}
```

### Exception Information
When serializing exceptions, the helper automatically converts them to `ExceptionInfo`:

```csharp
if (instance is Exception exception)
{
    ExceptionInfo exceptionInfo = new(exception);
    return JsonSerializer.Serialize(exceptionInfo, JsonSerializerOptions<T>(serializerOptions));
}
```

## Testing Strategies

### Unit Testing
```csharp
[Test]
public void ToJson_WithSimpleObject_ReturnsValidJson()
{
    // Arrange
    var obj = new { Name = "Test", Value = 42 };
    
    // Act
    var json = obj.ToJson();
    
    // Assert
    Assert.That(json, Contains.Substring("\"name\":\"Test\""));
    Assert.That(json, Contains.Substring("\"value\":42"));
}

[Test]
public void FromJson_WithValidJson_ReturnsObject()
{
    // Arrange
    var json = "{\"name\":\"Test\",\"value\":42}";
    
    // Act
    var obj = json.FromJson<dynamic>();
    
    // Assert
    Assert.IsNotNull(obj);
}
```

### Performance Testing
```csharp
[Test]
public void ToJsonBytes_WithLargeObject_PerformsWithinTimeLimit()
{
    // Arrange
    var largeObject = GenerateLargeTestObject();
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    var bytes = largeObject.ToJsonBytes();
    stopwatch.Stop();
    
    // Assert
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
    Assert.That(bytes.Length, Is.GreaterThan(0));
}
```

### Integration Testing
```csharp
[Test]
public void RoundTrip_WithComplexObject_PreservesData()
{
    // Arrange
    var original = new ComplexTestObject();
    
    // Act
    var json = original.ToJson();
    var restored = json.FromJson<ComplexTestObject>();
    
    // Assert
    Assert.AreEqual(original.Id, restored.Id);
    Assert.AreEqual(original.Name, restored.Name);
    CollectionAssert.AreEqual(original.Items, restored.Items);
}
```

## Best Practices

### 1. Use Appropriate Format for Use Case
```csharp
// ✅ Good: Use JSON string for API responses
return user.ToJson();

// ✅ Good: Use byte arrays for file storage
await File.WriteAllBytesAsync(path, data.ToJsonBytes());

// ✅ Good: Use Base64 for URL parameters
var url = $"/api/data?payload={data.ToJsonBase64()}";
```

### 2. Configure Options Appropriately
```csharp
// ✅ Good: Configure for specific needs
var apiJson = data.ToJson(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    return options;
});

// ✅ Good: Configure for human readability
var configJson = config.ToJson(options =>
{
    options.WriteIndented = true;
    return options;
});
```

### 3. Handle Exceptions Appropriately
```csharp
// ✅ Good: Structured exception serialization
try
{
    var result = await riskyOperation();
    return result.ToJson();
}
catch (Exception ex)
{
    logger.LogError("Operation failed: {Exception}", ex.ToJson());
    throw;
}
```

### 4. Use JsonSerializationAttribute for Type-Specific Control
```csharp
// ✅ Good: Control serialization per type
[JsonSerialization(CamelCase = false)]
public class LegacyApiModel
{
    public string PropertyName { get; set; }
}

[JsonSerialization(CamelCase = true)]
public class ModernApiModel
{
    public string PropertyName { get; set; }
}
```

## Related Components

- **[JsonSerializationAttribute](../Attributes/JsonSerializationAttribute.md)**: Controls camelCase behavior per type
- **[ExceptionInfo](../ExceptionInfo.md)**: Structured exception representation
- **[Telemetry](../Telemetry.md)**: Activity tracking and performance monitoring
- **[ExceptionHelper](ExceptionHelper.md)**: Complementary exception handling utilities

## Migration Guide

### From Newtonsoft.Json
```csharp
// Before: Newtonsoft.Json
var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
var obj = JsonConvert.DeserializeObject<MyClass>(json);

// After: JsonHelper
var json = obj.ToJson(options => 
{
    options.WriteIndented = true;
    return options;
});
var obj = json.FromJson<MyClass>();
```

### From System.Text.Json Direct
```csharp
// Before: Direct System.Text.Json
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};
var json = JsonSerializer.Serialize(obj, options);

// After: JsonHelper
var json = obj.ToJson(); // Uses default camelCase options
```

The JsonHelper provides a robust, high-performance foundation for JSON operations throughout the RapidStreamer BuildingBlocks system, with built-in observability, custom attribute support, and flexible configuration options.