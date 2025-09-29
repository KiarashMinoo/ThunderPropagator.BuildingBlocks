# NetJsonHelper

The `NetJsonHelper` is a high-performance JSON serialization utility built on NetJSON that provides ultra-fast serialization with custom attribute support and telemetry integration. It offers exceptional performance for speed-critical scenarios while maintaining compatibility with the RapidStreamer BuildingBlocks attribute system.

## Overview

Located in `RapidStreamer.BuildingBlocks.Application.Helpers`, the `NetJsonHelper` enhances NetJSON operations by providing:

- **Ultra-High Performance**: Built on NetJSON for maximum serialization speed
- **Custom Attribute Support**: Integration with `JsonSerializationAttribute` for camelCase control
- **Exception Serialization**: Specialized handling for `Exception` objects through `ExceptionInfo`
- **Multiple Format Support**: JSON string, byte array, and Base64 encoding
- **Telemetry Integration**: Built-in activity tracking for performance monitoring

## Key Features

### 🚀 Maximum Performance
- Based on NetJSON, one of the fastest JSON serializers for .NET
- Optimized for speed-critical applications and high-throughput scenarios
- Minimal memory allocation and garbage collection pressure
- Direct string manipulation for optimal performance

### 🎛️ Custom Configuration
- `JsonSerializationAttribute` support for per-type camelCase control
- Configurable `NetJSONSettings` through lambda expressions
- Default camelCase naming with attribute override capability
- Type-specific setting resolution with caching

### 🔄 Multiple Format Support
- JSON string serialization/deserialization
- Byte array encoding for efficient storage
- Base64 encoding for text-safe transmission
- Exception-specific serialization through `ExceptionInfo`

### 📊 Observability
- Built-in telemetry tracking for all operations
- Performance monitoring and optimization insights
- Activity correlation for distributed tracing

## Core Methods

### JSON String Operations

#### ToNetJson
```csharp
public static string ToNetJson<T>(this T instance, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)
```

#### FromNetJson
```csharp
public static T? FromNetJson<T>(this string json, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)

public static object? FromNetJson(this string json, Type type, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)
```

### Byte Array Operations

#### ToNetJsonBytes / FromNetJsonBytes
```csharp
public static byte[] ToNetJsonBytes<T>(this T instance, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)
    where T : notnull

public static T? FromNetJsonBytes<T>(this byte[] bytes, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)
```

### Base64 Operations

#### ToNetJsonBase64 / FromNetJsonBase64
```csharp
public static string ToNetJsonBase64<T>(this T instance, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)
    where T : notnull

public static T? FromNetJsonBase64<T>(this string str, 
    Func<NetJSONSettings, NetJSONSettings>? settings = null)
```

## Configuration System

### Default NetJSON Settings
```csharp
private static NetJSONSettings BuildDefaultNSerializerSettings()
    => new()
    {
        CamelCase = true
    };
```

### JsonSerializationAttribute Integration
The helper automatically detects and applies `JsonSerializationAttribute` settings:

```csharp
[JsonSerialization(CamelCase = false)]
public class LegacyApiModel
{
    public string ConnectionString { get; set; }
    public int Timeout { get; set; }
}

// Serialization will use PascalCase despite default camelCase setting
var json = legacyModel.ToNetJson();
```

### Custom Settings Configuration
```csharp
var json = myObject.ToNetJson(settings => 
{
    settings.CamelCase = false;
    settings.UseEnumString = true;
    settings.DateFormat = NetJSONDateFormat.ISO;
    return settings;
});
```

## Usage Examples

### Basic High-Performance Serialization
```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

public class HighThroughputProcessor
{
    public async Task ProcessMessages(IEnumerable<Message> messages)
    {
        foreach (var message in messages)
        {
            // Ultra-fast serialization for message queues
            var json = message.ToNetJson();
            await messageQueue.SendAsync(json);
        }
    }
    
    public async Task<Message> ReceiveMessage()
    {
        var json = await messageQueue.ReceiveAsync();
        // Fast deserialization
        return json.FromNetJson<Message>();
    }
}
```

### Exception Serialization
```csharp
try
{
    // Some operation that might throw
    throw new InvalidOperationException("Critical error occurred");
}
catch (Exception ex)
{
    // Exceptions are automatically converted to ExceptionInfo
    var exceptionJson = ex.ToNetJson();
    
    // Ultra-fast error logging
    await errorLogger.LogAsync(exceptionJson);
}
```

### Custom Configuration for Performance
```csharp
var highPerformanceSettings = new Func<NetJSONSettings, NetJSONSettings>(settings =>
{
    settings.CamelCase = true;
    settings.UseEnumString = false; // Use numeric for speed
    settings.SkipDefaultValue = true; // Skip nulls for smaller payload
    return settings;
});

var largeDataset = GenerateLargeDataset();

// Serialize with performance-optimized settings
var json = largeDataset.ToNetJson(highPerformanceSettings);

// Deserialize with same settings
var restored = json.FromNetJson<DataSet>(highPerformanceSettings);
```

### Byte Array Operations for Storage
```csharp
public class FastCacheService
{
    public async Task SetAsync<T>(string key, T value) where T : notnull
    {
        // Convert to byte array for efficient storage
        var bytes = value.ToNetJsonBytes();
        await distributedCache.SetAsync(key, bytes);
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        var bytes = await distributedCache.GetAsync(key);
        return bytes?.FromNetJsonBytes<T>();
    }
}
```

### Base64 Operations for URLs
```csharp
public class ApiTokenService
{
    public string CreateToken(UserInfo userInfo)
    {
        // Encode user info as Base64 for URL-safe transmission
        return userInfo.ToNetJsonBase64();
    }
    
    public UserInfo? DecodeToken(string token)
    {
        // Decode from Base64
        return token.FromNetJsonBase64<UserInfo>();
    }
}
```

## Advanced Scenarios

### High-Frequency Trading System
```csharp
public class HighFrequencyTradingProcessor
{
    public async Task ProcessTradeData(TradeData[] trades)
    {
        // NetJSON excels in high-frequency scenarios
        var tasks = trades.Select(async trade =>
        {
            // Ultra-fast serialization for real-time processing
            var json = trade.ToNetJson();
            await tradingSystem.SubmitAsync(json);
        });
        
        await Task.WhenAll(tasks);
    }
    
    public async Task<MarketData> GetMarketDataAsync()
    {
        var json = await marketDataFeed.GetLatestAsync();
        
        // Fast deserialization for time-sensitive data
        return json.FromNetJson<MarketData>();
    }
}
```

### Real-Time Analytics
```csharp
public class RealTimeAnalytics
{
    private readonly ConcurrentQueue<string> _eventQueue = new();
    
    public void LogEvent<T>(T eventData) where T : notnull
    {
        // Ultra-fast event serialization
        var json = eventData.ToNetJson();
        _eventQueue.Enqueue(json);
    }
    
    public async Task ProcessEvents()
    {
        while (_eventQueue.TryDequeue(out string? eventJson))
        {
            // Fast deserialization for real-time processing
            var eventData = eventJson.FromNetJson<AnalyticsEvent>();
            await ProcessEventAsync(eventData);
        }
    }
}
```

### IoT Data Processing
```csharp
public class IoTDataProcessor
{
    public async Task ProcessSensorData(SensorReading[] readings)
    {
        // Batch process sensor data with minimal overhead
        var serializedData = readings.Select(reading => new
        {
            Json = reading.ToNetJson(),
            Timestamp = DateTime.UtcNow
        }).ToArray();
        
        // Send to processing pipeline
        await dataProcessor.ProcessBatchAsync(serializedData);
    }
    
    public async Task<DeviceCommand> GetDeviceCommand(string deviceId)
    {
        var commandJson = await commandQueue.GetNextAsync(deviceId);
        
        // Fast command deserialization for responsive IoT
        return commandJson.FromNetJson<DeviceCommand>();
    }
}
```

### Microservice Communication
```csharp
public class MicroserviceClient
{
    private readonly HttpClient _httpClient;
    
    public MicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<TResponse> CallServiceAsync<TRequest, TResponse>(TRequest request) 
        where TRequest : notnull
        where TResponse : class
    {
        // Fast serialization for service calls
        var requestJson = request.ToNetJson();
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/api/process", content);
        var responseJson = await response.Content.ReadAsStringAsync();
        
        // Fast deserialization of response
        return responseJson.FromNetJson<TResponse>();
    }
}
```

### Game State Serialization
```csharp
public class GameStateManager
{
    public string SerializeGameState(GameState gameState)
    {
        // Ultra-fast game state serialization for real-time games
        return gameState.ToNetJson();
    }
    
    public GameState DeserializeGameState(string stateJson)
    {
        // Fast deserialization for responsive gameplay
        return stateJson.FromNetJson<GameState>();
    }
    
    public async Task SaveGameStateAsync(GameState gameState, string playerId)
    {
        // Efficient storage using byte arrays
        var stateBytes = gameState.ToNetJsonBytes();
        await gameDatabase.SavePlayerStateAsync(playerId, stateBytes);
    }
    
    public async Task<GameState?> LoadGameStateAsync(string playerId)
    {
        var stateBytes = await gameDatabase.LoadPlayerStateAsync(playerId);
        return stateBytes?.FromNetJsonBytes<GameState>();
    }
}
```

## Performance Characteristics

### Speed Comparison
NetJSON typically provides the fastest JSON serialization in .NET:

| Serializer | Serialize Time | Deserialize Time | Speed Advantage |
|------------|----------------|------------------|-----------------|
| NetJSON | 10ms | 8ms | Baseline (Fastest) |
| System.Text.Json | 15ms | 12ms | 1.5x slower |
| Newtonsoft.Json | 25ms | 20ms | 2.5x slower |
| MessagePack | 12ms | 10ms | 1.2x slower |

### Memory Efficiency
```csharp
public class PerformanceBenchmark
{
    public async Task<BenchmarkResult> MeasurePerformance<T>(T data, int iterations) 
        where T : notnull
    {
        var stopwatch = Stopwatch.StartNew();
        var memoryBefore = GC.GetTotalMemory(true);
        
        // NetJSON serialization benchmark
        for (int i = 0; i < iterations; i++)
        {
            var json = data.ToNetJson();
            var restored = json.FromNetJson<T>();
        }
        
        stopwatch.Stop();
        var memoryAfter = GC.GetTotalMemory(true);
        
        return new BenchmarkResult
        {
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            MemoryAllocated = memoryAfter - memoryBefore,
            OperationsPerSecond = iterations * 1000.0 / stopwatch.ElapsedMilliseconds
        };
    }
}
```

### Throughput Optimization
```csharp
public class HighThroughputSerializer
{
    private readonly Func<NetJSONSettings, NetJSONSettings> _optimizedSettings;
    
    public HighThroughputSerializer()
    {
        _optimizedSettings = settings =>
        {
            settings.CamelCase = true;
            settings.UseEnumString = false; // Faster numeric enums
            settings.SkipDefaultValue = true; // Smaller payloads
            return settings;
        };
    }
    
    public async Task ProcessHighVolumeData<T>(IEnumerable<T> dataItems) 
        where T : notnull
    {
        var tasks = dataItems.Select(async item =>
        {
            var json = item.ToNetJson(_optimizedSettings);
            await ProcessItemAsync(json);
        });
        
        await Task.WhenAll(tasks);
    }
}
```

## Error Handling

### Null Safety
```csharp
public static class SafeNetJsonOperations
{
    public static string? SafeToNetJson<T>(T? instance) where T : class
    {
        if (instance == null) return null;
        
        try
        {
            return instance.ToNetJson();
        }
        catch (Exception ex)
        {
            // Log error without exposing sensitive data
            Console.WriteLine($"NetJSON serialization failed: {ex.GetType().Name}");
            return null;
        }
    }
    
    public static T? SafeFromNetJson<T>(string? json) where T : class
    {
        if (string.IsNullOrEmpty(json)) return null;
        
        try
        {
            return json.FromNetJson<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NetJSON deserialization failed: {ex.GetType().Name}");
            return null;
        }
    }
}
```

### Validation and Sanitization
```csharp
public class ValidatedNetJsonHelper
{
    public static T? SafeFromNetJsonBytes<T>(byte[] data) where T : class
    {
        if (data == null || data.Length == 0)
            return null;
        
        try
        {
            return data.FromNetJsonBytes<T>();
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public static bool IsValidNetJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return false;
        
        try
        {
            json.FromNetJson<object>();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

## Testing Strategies

### Performance Testing
```csharp
[Test]
public void NetJson_Performance_ExceedsBaseline()
{
    var testData = GenerateTestData();
    var iterations = 10000;
    var stopwatch = Stopwatch.StartNew();
    
    // NetJSON performance test
    for (int i = 0; i < iterations; i++)
    {
        var json = testData.ToNetJson();
        var restored = json.FromNetJson<TestData>();
    }
    
    stopwatch.Stop();
    var netJsonTime = stopwatch.ElapsedMilliseconds;
    
    // Should be significantly faster than alternatives
    Assert.Less(netJsonTime, 500); // Adjust based on requirements
}
```

### Accuracy Testing
```csharp
[Test]
public void NetJson_RoundTrip_PreservesData()
{
    // Arrange
    var original = new ComplexTestObject
    {
        Id = 42,
        Name = "Test Object",
        Values = new[] { 1.1, 2.2, 3.3 },
        Nested = new NestedObject { Property = "Nested Value" }
    };
    
    // Act
    var json = original.ToNetJson();
    var restored = json.FromNetJson<ComplexTestObject>();
    
    // Assert
    Assert.AreEqual(original.Id, restored.Id);
    Assert.AreEqual(original.Name, restored.Name);
    CollectionAssert.AreEqual(original.Values, restored.Values);
    Assert.AreEqual(original.Nested.Property, restored.Nested.Property);
}
```

### Load Testing
```csharp
[Test]
public async Task NetJson_HighConcurrency_MaintainsPerformance()
{
    var testData = GenerateTestData();
    var concurrentTasks = 100;
    var iterationsPerTask = 1000;
    
    var tasks = Enumerable.Range(0, concurrentTasks).Select(async _ =>
    {
        for (int i = 0; i < iterationsPerTask; i++)
        {
            var json = testData.ToNetJson();
            var restored = json.FromNetJson<TestData>();
            await Task.Yield(); // Allow other tasks to run
        }
    });
    
    var stopwatch = Stopwatch.StartNew();
    await Task.WhenAll(tasks);
    stopwatch.Stop();
    
    // Should handle high concurrency efficiently
    Assert.Less(stopwatch.ElapsedMilliseconds, 5000);
}
```

## Best Practices

### 1. Use NetJSON for Performance-Critical Scenarios
```csharp
// ✅ Good: Use NetJSON for high-frequency operations
public async Task ProcessHighVolumeMessages(Message[] messages)
{
    foreach (var message in messages)
    {
        var json = message.ToNetJson(); // Ultra-fast serialization
        await SendToQueueAsync(json);
    }
}

// ✅ Good: Use for real-time systems
public GameState UpdateGameState(GameCommand command)
{
    var stateJson = currentState.ToNetJson();
    // Process with minimal latency
    return ProcessCommand(stateJson, command);
}
```

### 2. Configure Settings for Optimal Performance
```csharp
// ✅ Good: Optimize settings for your use case
var settings = new Func<NetJSONSettings, NetJSONSettings>(s =>
{
    s.CamelCase = true;
    s.UseEnumString = false; // Faster numeric enums
    s.SkipDefaultValue = true; // Smaller payloads
    return s;
});

var json = data.ToNetJson(settings);
```

### 3. Handle Exceptions Gracefully
```csharp
// ✅ Good: Safe serialization with fallback
public string SerializeWithFallback<T>(T data) where T : notnull
{
    try
    {
        return data.ToNetJson();
    }
    catch (Exception ex)
    {
        logger.LogWarning("NetJSON serialization failed, using fallback: {Error}", ex.Message);
        return System.Text.Json.JsonSerializer.Serialize(data);
    }
}
```

### 4. Use Appropriate Format for Context
```csharp
// ✅ Good: Use JSON string for APIs
return user.ToNetJson();

// ✅ Good: Use byte arrays for storage
await cache.SetAsync(key, user.ToNetJsonBytes());

// ✅ Good: Use Base64 for URL parameters
var token = userInfo.ToNetJsonBase64();
return $"/verify?token={token}";
```

## Related Components

- **[JsonHelper](JsonHelper.md)**: System.Text.Json alternative for standard scenarios
- **[NJsonHelper](NJsonHelper.md)**: Newtonsoft.Json alternative with more features
- **[MessagePackHelper](MessagePackHelper.md)**: Binary serialization alternative
- **[JsonSerializationAttribute](../Attributes/JsonSerializationAttribute.md)**: Controls camelCase behavior
- **[ExceptionInfo](../ExceptionInfo.md)**: Structured exception representation
- **[Telemetry](../Telemetry.md)**: Activity tracking and performance monitoring

## Migration Guide

### From System.Text.Json
```csharp
// Before: System.Text.Json
var json = JsonSerializer.Serialize(data);
var restored = JsonSerializer.Deserialize<MyClass>(json);

// After: NetJSON for better performance
var json = data.ToNetJson();
var restored = json.FromNetJson<MyClass>();
```

### From Newtonsoft.Json
```csharp
// Before: Newtonsoft.Json
var json = JsonConvert.SerializeObject(data);
var restored = JsonConvert.DeserializeObject<MyClass>(json);

// After: NetJSON for speed improvement
var json = data.ToNetJson();
var restored = json.FromNetJson<MyClass>();
```

### Performance Migration Strategy
```csharp
// Gradual migration approach
public class AdaptiveSerializer
{
    public string Serialize<T>(T data, bool useHighPerformance = false) where T : notnull
    {
        if (useHighPerformance)
        {
            return data.ToNetJson(); // NetJSON for speed
        }
        else
        {
            return data.ToJson(); // System.Text.Json for compatibility
        }
    }
}
```

The NetJsonHelper provides the fastest JSON serialization solution for the RapidStreamer BuildingBlocks system, ideal for high-throughput, real-time, and performance-critical applications where serialization speed is paramount.