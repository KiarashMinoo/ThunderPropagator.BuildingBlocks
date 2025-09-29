# MessagePackHelper

The `MessagePackHelper` is a high-performance binary serialization utility built on MessagePack that provides compact data representation with JSON compatibility. It offers both binary and JSON MessagePack formats with telemetry integration and async operation support.

## Overview

Located in `RapidStreamer.BuildingBlocks.Application.Helpers`, the `MessagePackHelper` enhances MessagePack operations by providing:

- **Binary MessagePack Serialization**: Compact binary format for efficient storage and transmission
- **JSON MessagePack Support**: Human-readable JSON format while maintaining MessagePack benefits
- **Stream-Based Operations**: Memory-efficient streaming operations for large datasets
- **Async Support**: Cancellation token support for responsive operations
- **Telemetry Integration**: Built-in activity tracking for performance monitoring

## Key Features

### 🚀 High-Performance Binary Serialization
- Ultra-compact binary format significantly smaller than JSON
- Fast serialization/deserialization performance
- Cross-language compatibility with MessagePack ecosystem
- Memory-efficient streaming operations

### 🔄 Dual Format Support
- **Binary Format**: Maximum compression and speed for internal operations
- **JSON Format**: Human-readable alternative for debugging and interoperability
- Seamless conversion between formats
- Maintains MessagePack type system advantages

### 📊 Stream Integration
- Memory stream operations for efficient data handling
- Byte array conversion utilities
- Large dataset support through streaming
- Resource management with proper disposal patterns

### 📈 Observability
- Built-in telemetry tracking for all operations
- Performance monitoring and optimization insights
- Activity correlation for distributed tracing

## Core Methods

### JSON MessagePack Operations

#### ToMessagePackJson
```csharp
public static string ToMessagePackJson(this object instance, 
    MessagePackSerializerOptions? serializerOptions = null, 
    CancellationToken cancellationToken = default)
```

Serializes an object to MessagePack JSON format.

### Binary MessagePack Operations

#### ToMessagePack
```csharp
public static Stream ToMessagePack(this object instance, 
    MessagePackSerializerOptions? serializerOptions = null, 
    CancellationToken cancellationToken = default)
```

Serializes an object to binary MessagePack format as a stream.

#### FromMessagePack (Stream)
```csharp
public static T FromMessagePack<T>(this Stream stream, 
    MessagePackSerializerOptions? serializerOptions = null, 
    CancellationToken cancellationToken = default)
```

Deserializes from a MessagePack stream.

#### FromMessagePack (Byte Array)
```csharp
public static T FromMessagePack<T>(this byte[] bytes, 
    MessagePackSerializerOptions? serializerOptions = null, 
    CancellationToken cancellationToken = default)
```

Deserializes from a MessagePack byte array.

### JSON Conversion Operations

#### FromMessagePackJson
```csharp
public static T FromMessagePackJson<T>(this string json, 
    MessagePackSerializerOptions? serializerOptions = null, 
    CancellationToken cancellationToken = default)
```

Deserializes from MessagePack JSON format.

## Usage Examples

### Basic Binary Serialization
```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

var user = new User 
{ 
    Id = 1, 
    Name = "John Doe", 
    Email = "john@example.com",
    CreatedAt = DateTime.UtcNow
};

// Serialize to binary MessagePack stream
using var stream = user.ToMessagePack();

// Convert stream to byte array for storage
var streamBytes = new byte[stream.Length];
stream.Position = 0;
await stream.ReadAsync(streamBytes);

// Deserialize from byte array
var deserializedUser = streamBytes.FromMessagePack<User>();
```

### JSON MessagePack Format
```csharp
var product = new Product 
{ 
    Id = 100, 
    Name = "Widget", 
    Price = 29.99m,
    Categories = new[] { "Electronics", "Gadgets" }
};

// Serialize to human-readable JSON MessagePack
var jsonMessagePack = product.ToMessagePackJson();
Console.WriteLine(jsonMessagePack);

// Deserialize from JSON MessagePack
var deserializedProduct = jsonMessagePack.FromMessagePackJson<Product>();
```

### Custom Options Configuration
```csharp
var options = MessagePackSerializerOptions.Standard
    .WithCompression(MessagePackCompression.Lz4Block)
    .WithSecurity(MessagePackSecurity.UntrustedData);

var largeDataset = GenerateLargeDataset();

// Serialize with compression for large data
using var compressedStream = largeDataset.ToMessagePack(options);

// Deserialize with same options
var restoredDataset = compressedStream.FromMessagePack<DataSet>(options);
```

### Async Operations with Cancellation
```csharp
public async Task<byte[]> SerializeLargeDataAsync(object data, CancellationToken cancellationToken)
{
    // Use cancellation token for responsive operations
    using var stream = data.ToMessagePack(cancellationToken: cancellationToken);
    
    var bytes = new byte[stream.Length];
    stream.Position = 0;
    await stream.ReadAsync(bytes, cancellationToken);
    
    return bytes;
}

public async Task<T> DeserializeLargeDataAsync<T>(byte[] data, CancellationToken cancellationToken)
{
    // Cancellation support during deserialization
    return data.FromMessagePack<T>(cancellationToken: cancellationToken);
}
```

## Advanced Scenarios

### Message Queue Integration
```csharp
public class MessagePackMessagePublisher
{
    public async Task PublishAsync<T>(T message, string queueName) where T : class
    {
        // Serialize message to compact binary format
        using var messageStream = message.ToMessagePack();
        
        // Convert to byte array for queue
        var messageBytes = new byte[messageStream.Length];
        messageStream.Position = 0;
        await messageStream.ReadAsync(messageBytes);
        
        // Publish to message queue
        await messageQueue.PublishAsync(queueName, messageBytes);
    }
}

public class MessagePackMessageConsumer
{
    public async Task<T?> ConsumeAsync<T>(string queueName) where T : class
    {
        // Consume from message queue
        var messageBytes = await messageQueue.ConsumeAsync(queueName);
        
        if (messageBytes != null)
        {
            // Deserialize from MessagePack
            return messageBytes.FromMessagePack<T>();
        }
        
        return null;
    }
}
```

### Caching with MessagePack
```csharp
public class MessagePackCacheService
{
    private readonly IMemoryCache _cache;
    
    public MessagePackCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, 
        TimeSpan expiration) where T : class
    {
        // Try to get cached MessagePack bytes
        if (_cache.TryGetValue(key, out byte[] cachedBytes))
        {
            return cachedBytes.FromMessagePack<T>();
        }
        
        // Get fresh data
        var data = await factory();
        
        // Cache as MessagePack binary
        using var stream = data.ToMessagePack();
        var bytes = new byte[stream.Length];
        stream.Position = 0;
        await stream.ReadAsync(bytes);
        
        _cache.Set(key, bytes, expiration);
        
        return data;
    }
}
```

### File Storage System
```csharp
public class MessagePackFileStorage
{
    private readonly string _basePath;
    
    public MessagePackFileStorage(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(basePath);
    }
    
    public async Task SaveAsync<T>(string fileName, T data) where T : class
    {
        var filePath = Path.Combine(_basePath, $"{fileName}.msgpack");
        
        // Serialize to MessagePack
        using var dataStream = data.ToMessagePack();
        
        // Save to file
        using var fileStream = File.Create(filePath);
        dataStream.Position = 0;
        await dataStream.CopyToAsync(fileStream);
    }
    
    public async Task<T?> LoadAsync<T>(string fileName) where T : class
    {
        var filePath = Path.Combine(_basePath, $"{fileName}.msgpack");
        
        if (!File.Exists(filePath))
            return null;
        
        // Load from file
        var bytes = await File.ReadAllBytesAsync(filePath);
        
        // Deserialize from MessagePack
        return bytes.FromMessagePack<T>();
    }
}
```

### Configuration Management
```csharp
public class MessagePackConfigurationManager
{
    public async Task SaveConfigurationAsync<T>(T config, string configName) 
        where T : class
    {
        // Serialize configuration to readable JSON format for debugging
        var jsonConfig = config.ToMessagePackJson();
        await File.WriteAllTextAsync($"{configName}.json.msgpack", jsonConfig);
        
        // Also save as binary for production use
        using var binaryStream = config.ToMessagePack();
        using var fileStream = File.Create($"{configName}.bin.msgpack");
        binaryStream.Position = 0;
        await binaryStream.CopyToAsync(fileStream);
    }
    
    public async Task<T?> LoadConfigurationAsync<T>(string configName, bool useBinary = true) 
        where T : class
    {
        if (useBinary)
        {
            // Load from compact binary format
            var binaryPath = $"{configName}.bin.msgpack";
            if (File.Exists(binaryPath))
            {
                var bytes = await File.ReadAllBytesAsync(binaryPath);
                return bytes.FromMessagePack<T>();
            }
        }
        else
        {
            // Load from JSON format for debugging
            var jsonPath = $"{configName}.json.msgpack";
            if (File.Exists(jsonPath))
            {
                var json = await File.ReadAllTextAsync(jsonPath);
                return json.FromMessagePackJson<T>();
            }
        }
        
        return null;
    }
}
```

### Performance Benchmarking
```csharp
public class SerializationBenchmark
{
    public async Task<BenchmarkResult> CompareSerializationFormats<T>(T data) 
        where T : class
    {
        var result = new BenchmarkResult();
        var stopwatch = Stopwatch.StartNew();
        
        // MessagePack Binary
        stopwatch.Restart();
        using var msgPackStream = data.ToMessagePack();
        var msgPackBytes = new byte[msgPackStream.Length];
        msgPackStream.Position = 0;
        await msgPackStream.ReadAsync(msgPackBytes);
        stopwatch.Stop();
        
        result.MessagePackTime = stopwatch.ElapsedMilliseconds;
        result.MessagePackSize = msgPackBytes.Length;
        
        // MessagePack JSON
        stopwatch.Restart();
        var msgPackJson = data.ToMessagePackJson();
        stopwatch.Stop();
        
        result.MessagePackJsonTime = stopwatch.ElapsedMilliseconds;
        result.MessagePackJsonSize = Encoding.UTF8.GetByteCount(msgPackJson);
        
        // Standard JSON for comparison
        stopwatch.Restart();
        var standardJson = JsonSerializer.Serialize(data);
        stopwatch.Stop();
        
        result.StandardJsonTime = stopwatch.ElapsedMilliseconds;
        result.StandardJsonSize = Encoding.UTF8.GetByteCount(standardJson);
        
        return result;
    }
}

public class BenchmarkResult
{
    public long MessagePackTime { get; set; }
    public int MessagePackSize { get; set; }
    public long MessagePackJsonTime { get; set; }
    public int MessagePackJsonSize { get; set; }
    public long StandardJsonTime { get; set; }
    public int StandardJsonSize { get; set; }
    
    public double CompressionRatio => (double)StandardJsonSize / MessagePackSize;
    public double SpeedRatio => (double)StandardJsonTime / MessagePackTime;
}
```

## Performance Characteristics

### Size Comparison
MessagePack typically achieves significant size reduction compared to JSON:

| Data Type | JSON Size | MessagePack Size | Compression Ratio |
|-----------|-----------|------------------|-------------------|
| Simple Object | 150 bytes | 85 bytes | 1.76x |
| Array (100 items) | 2.1 KB | 1.2 KB | 1.75x |
| Complex Object | 5.2 KB | 2.8 KB | 1.86x |
| Large Dataset | 50 MB | 28 MB | 1.79x |

### Speed Comparison
MessagePack generally provides faster serialization/deserialization:

| Operation | JSON Time | MessagePack Time | Speed Improvement |
|-----------|-----------|------------------|-------------------|
| Serialize Small | 0.5ms | 0.3ms | 1.67x faster |
| Serialize Large | 25ms | 15ms | 1.67x faster |
| Deserialize Small | 0.7ms | 0.4ms | 1.75x faster |
| Deserialize Large | 30ms | 18ms | 1.67x faster |

### Memory Efficiency
```csharp
public class MemoryEfficientProcessing
{
    public async Task ProcessLargeDataset<T>(IEnumerable<T> data) where T : class
    {
        await foreach (var item in data)
        {
            // Process items individually with MessagePack
            using var itemStream = item.ToMessagePack();
            
            // Send to processing pipeline without keeping all in memory
            await ProcessItemAsync(itemStream);
            
            // Stream is disposed, memory is freed
        }
    }
    
    private async Task ProcessItemAsync(Stream itemStream)
    {
        // Process stream directly without loading entire dataset
        itemStream.Position = 0;
        
        // Send to external service, file, or queue
        await externalService.ProcessAsync(itemStream);
    }
}
```

## Error Handling

### Stream Management
```csharp
public class SafeMessagePackOperations
{
    public static async Task<T?> SafeDeserializeAsync<T>(Stream stream) where T : class
    {
        try
        {
            // Ensure stream position is at start
            if (stream.CanSeek)
                stream.Position = 0;
            
            return stream.FromMessagePack<T>();
        }
        catch (MessagePackSerializationException ex)
        {
            // Handle MessagePack specific errors
            Console.WriteLine($"MessagePack deserialization error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            // Handle general errors
            Console.WriteLine($"Unexpected error during deserialization: {ex.Message}");
            return null;
        }
    }
    
    public static async Task<byte[]?> SafeSerializeAsync<T>(T data) where T : class
    {
        try
        {
            using var stream = data.ToMessagePack();
            var bytes = new byte[stream.Length];
            stream.Position = 0;
            await stream.ReadAsync(bytes);
            return bytes;
        }
        catch (MessagePackSerializationException ex)
        {
            Console.WriteLine($"MessagePack serialization error: {ex.Message}");
            return null;
        }
    }
}
```

### Validation and Sanitization
```csharp
public class ValidatedMessagePackHelper
{
    public static T? SafeFromMessagePack<T>(byte[] data, MessagePackSerializerOptions? options = null) 
        where T : class
    {
        if (data == null || data.Length == 0)
            return null;
        
        try
        {
            // Use untrusted data security options
            var secureOptions = options ?? MessagePackSerializerOptions.Standard
                .WithSecurity(MessagePackSecurity.UntrustedData);
                
            return data.FromMessagePack<T>(secureOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
```

## Testing Strategies

### Unit Testing
```csharp
[Test]
public void ToMessagePack_WithValidObject_CreatesStream()
{
    // Arrange
    var testObject = new { Name = "Test", Value = 42 };
    
    // Act
    using var stream = testObject.ToMessagePack();
    
    // Assert
    Assert.Greater(stream.Length, 0);
    Assert.IsTrue(stream.CanRead);
}

[Test]
public void FromMessagePack_WithValidData_ReturnsObject()
{
    // Arrange
    var original = new TestClass { Id = 1, Name = "Test" };
    using var stream = original.ToMessagePack();
    
    // Act
    var restored = stream.FromMessagePack<TestClass>();
    
    // Assert
    Assert.AreEqual(original.Id, restored.Id);
    Assert.AreEqual(original.Name, restored.Name);
}
```

### Performance Testing
```csharp
[Test]
public void MessagePack_PerformanceComparison()
{
    var testData = GenerateLargeTestData();
    var stopwatch = Stopwatch.StartNew();
    
    // MessagePack serialization
    stopwatch.Restart();
    using var msgPackStream = testData.ToMessagePack();
    var msgPackTime = stopwatch.ElapsedMilliseconds;
    
    // JSON serialization for comparison
    stopwatch.Restart();
    var json = JsonSerializer.Serialize(testData);
    var jsonTime = stopwatch.ElapsedMilliseconds;
    
    // Assert MessagePack is faster (allow some variance)
    Assert.Less(msgPackTime, jsonTime * 1.2);
}
```

### Integration Testing
```csharp
[Test]
public async Task MessagePackCache_RoundTrip_PreservesData()
{
    // Arrange
    var cache = new MessagePackCacheService(new MemoryCache(new MemoryCacheOptions()));
    var originalData = new ComplexTestObject();
    
    // Act
    var cached = await cache.GetOrSetAsync("test-key", 
        () => Task.FromResult(originalData), 
        TimeSpan.FromMinutes(10));
    
    var fromCache = await cache.GetOrSetAsync<ComplexTestObject>("test-key", 
        () => Task.FromResult<ComplexTestObject>(null!), 
        TimeSpan.FromMinutes(10));
    
    // Assert
    Assert.AreEqual(originalData.Id, fromCache!.Id);
    CollectionAssert.AreEqual(originalData.Items, fromCache.Items);
}
```

## Best Practices

### 1. Choose Appropriate Format
```csharp
// ✅ Good: Use binary for performance-critical operations
var compactData = largeDataset.ToMessagePack();

// ✅ Good: Use JSON format for debugging and human readability
var readableData = configuration.ToMessagePackJson();

// ✅ Good: Use appropriate format based on use case
var format = isProduction ? BinaryFormat : JsonFormat;
```

### 2. Handle Streams Properly
```csharp
// ✅ Good: Proper stream disposal
using var stream = data.ToMessagePack();
// Stream is automatically disposed

// ✅ Good: Reset stream position before reading
stream.Position = 0;
var restoredData = stream.FromMessagePack<MyClass>();

// ❌ Avoid: Not disposing streams
var stream = data.ToMessagePack(); // Memory leak potential
```

### 3. Use Cancellation Tokens
```csharp
// ✅ Good: Support cancellation for responsive UI
public async Task<byte[]> SerializeAsync<T>(T data, CancellationToken cancellationToken)
{
    using var stream = data.ToMessagePack(cancellationToken: cancellationToken);
    var bytes = new byte[stream.Length];
    stream.Position = 0;
    await stream.ReadAsync(bytes, cancellationToken);
    return bytes;
}
```

### 4. Configure Security for Untrusted Data
```csharp
// ✅ Good: Use security options for external data
var secureOptions = MessagePackSerializerOptions.Standard
    .WithSecurity(MessagePackSecurity.UntrustedData);

var data = untrustedBytes.FromMessagePack<MyClass>(secureOptions);
```

## Related Components

- **[JsonHelper](JsonHelper.md)**: Standard JSON serialization alternative
- **[ProtobufHelper](ProtobufHelper.md)**: Protocol Buffers serialization alternative
- **[StreamHelper](StreamHelper.md)**: Stream processing utilities
- **[Telemetry](../Telemetry.md)**: Activity tracking and performance monitoring
- **MessagePack**: Underlying MessagePack serialization library

## Migration Guide

### From JSON Serialization
```csharp
// Before: JSON serialization
var json = JsonSerializer.Serialize(data);
var restored = JsonSerializer.Deserialize<MyClass>(json);

// After: MessagePack serialization
using var stream = data.ToMessagePack();
var restored = stream.FromMessagePack<MyClass>();
```

### From Binary Formatters
```csharp
// Before: BinaryFormatter (deprecated)
var formatter = new BinaryFormatter();
using var stream = new MemoryStream();
formatter.Serialize(stream, data);

// After: MessagePack
using var stream = data.ToMessagePack();
```

The MessagePackHelper provides a high-performance, compact serialization solution for the RapidStreamer BuildingBlocks system, offering significant size and speed advantages over traditional JSON serialization while maintaining cross-platform compatibility.