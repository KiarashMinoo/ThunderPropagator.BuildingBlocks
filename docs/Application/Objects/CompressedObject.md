# CompressedObject

The `CompressedObject` struct provides a readonly container for compressed data with support for multiple compression formats. It offers efficient storage and transfer of compressed binary data with built-in conversion capabilities and type safety.

## Overview

```csharp
public readonly struct CompressedObject
```

`CompressedObject` is a readonly struct that encapsulates compressed byte data, providing implicit conversions, length tracking, and Base64 string representation for efficient data handling in compression scenarios.

## Key Features

- **Readonly Struct Design**: Immutable container ensuring data integrity
- **Multiple Compression Format Support**: Support for GZip, Deflate, Brotli, BZip2, and GZip formats
- **Implicit Conversions**: Seamless conversion to/from byte arrays and strings
- **Base64 String Representation**: Built-in Base64 encoding for storage and transmission
- **Memory Efficient**: Struct-based design minimizes memory overhead
- **Type Safety**: Strong typing prevents accidental misuse of compressed data

## Compression Types

### CompressionType Enumeration
Defines the supported compression algorithms:

```csharp
public enum CompressionType
{
    GZipStream,    // Standard GZip compression using .NET GZipStream
    DeflateStream, // Deflate compression using .NET DeflateStream
    BrotliStream,  // Brotli compression using .NET BrotliStream
    BZip2,         // BZip2 compression (external library support)
    GZip           // Alternative GZip implementation
}
```

**Format Characteristics:**
- **GZipStream**: Best general-purpose compression with good balance of speed and ratio
- **DeflateStream**: Fast compression with slightly lower compression ratios
- **BrotliStream**: Modern compression with excellent ratios, ideal for web scenarios
- **BZip2**: High compression ratios, slower but excellent for archival
- **GZip**: Alternative GZip implementation for specific use cases

## Public API

### Properties

#### Length
Gets the length of the compressed data in bytes.

```csharp
public int Length => _value.Length;
```

**Returns:** int representing the number of bytes in the compressed data

### Methods

#### ToString()
Converts the compressed data to a Base64 string representation.

```csharp
public override string ToString() => Convert.ToBase64String(_value);
```

**Returns:** Base64 encoded string of the compressed data
**Use Case:** Storage in databases, JSON serialization, or text-based transmission

### Implicit Conversions

#### From String (Base64)
Converts a Base64 string to a CompressedObject.

```csharp
public static implicit operator CompressedObject(string value) => Convert.FromBase64String(value);
```

#### To String (Base64)
Converts a CompressedObject to its Base64 string representation.

```csharp
public static implicit operator string(CompressedObject compressedObject) => compressedObject.ToString();
```

#### From Byte Array
Converts a byte array to a CompressedObject.

```csharp
public static implicit operator CompressedObject(byte[] bytes) => new(bytes);
```

#### To Byte Array
Converts a CompressedObject to a byte array.

```csharp
public static implicit operator byte[](CompressedObject compressedObject) => compressedObject._value;
```

## Usage Examples

### Basic Compression and Storage

```csharp
public class CompressionExample
{
    public void DemonstrateBasicUsage()
    {
        // Create sample data
        string originalData = "This is a sample text that will be compressed for storage efficiency.";
        byte[] originalBytes = Encoding.UTF8.GetBytes(originalData);
        
        // Compress data (using ObjectHelper extension method)
        CompressedObject compressed = originalBytes.ToCompressed(CompressedObject.CompressionType.GZipStream);
        
        // Display compression information
        Console.WriteLine($"Original size: {originalBytes.Length} bytes");
        Console.WriteLine($"Compressed size: {compressed.Length} bytes");
        Console.WriteLine($"Compression ratio: {(double)compressed.Length / originalBytes.Length:P2}");
        
        // Convert to Base64 for storage
        string base64Representation = compressed; // Implicit conversion
        Console.WriteLine($"Base64 representation: {base64Representation}");
        
        // Restore from Base64
        CompressedObject restored = base64Representation; // Implicit conversion
        Console.WriteLine($"Restored size: {restored.Length} bytes");
        
        // Decompress back to original data
        string decompressed = restored.DecompressString();
        Console.WriteLine($"Decompressed: {decompressed}");
        Console.WriteLine($"Round-trip success: {originalData == decompressed}");
    }
}
```

### Working with Different Compression Formats

```csharp
public class CompressionFormatComparison
{
    public void CompareCompressionFormats()
    {
        string testData = File.ReadAllText("large-text-file.txt");
        byte[] originalBytes = Encoding.UTF8.GetBytes(testData);
        
        var formats = new[]
        {
            CompressedObject.CompressionType.GZipStream,
            CompressedObject.CompressionType.DeflateStream,
            CompressedObject.CompressionType.BrotliStream
        };
        
        Console.WriteLine($"Original size: {originalBytes.Length:N0} bytes");
        Console.WriteLine();
        
        foreach (var format in formats)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Compress
            CompressedObject compressed = originalBytes.ToCompressed(format);
            stopwatch.Stop();
            
            // Calculate metrics
            double compressionRatio = (double)compressed.Length / originalBytes.Length;
            double spaceSaved = 1 - compressionRatio;
            
            Console.WriteLine($"{format}:");
            Console.WriteLine($"  Compressed size: {compressed.Length:N0} bytes");
            Console.WriteLine($"  Compression ratio: {compressionRatio:P2}");
            Console.WriteLine($"  Space saved: {spaceSaved:P2}");
            Console.WriteLine($"  Compression time: {stopwatch.ElapsedMilliseconds}ms");
            
            // Test decompression
            var decompressWatch = Stopwatch.StartNew();
            string decompressed = compressed.DecompressString(format);
            decompressWatch.Stop();
            
            Console.WriteLine($"  Decompression time: {decompressWatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Round-trip success: {testData == decompressed}");
            Console.WriteLine();
        }
    }
}
```

### Database Storage Integration

```csharp
public class DatabaseCompressionService
{
    private readonly IDbConnection _connection;
    
    public DatabaseCompressionService(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task SaveCompressedDataAsync(int id, string data, CompressedObject.CompressionType compressionType)
    {
        // Compress the data
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        CompressedObject compressed = dataBytes.ToCompressed(compressionType);
        
        // Store as Base64 string in database
        string base64Data = compressed; // Implicit conversion
        
        const string sql = @"
            INSERT INTO CompressedData (Id, Data, CompressionType, OriginalSize, CompressedSize)
            VALUES (@Id, @Data, @CompressionType, @OriginalSize, @CompressedSize)";
        
        await _connection.ExecuteAsync(sql, new
        {
            Id = id,
            Data = base64Data,
            CompressionType = compressionType.ToString(),
            OriginalSize = dataBytes.Length,
            CompressedSize = compressed.Length
        });
        
        Console.WriteLine($"Saved compressed data: {compressed.Length} bytes (originally {dataBytes.Length} bytes)");
    }
    
    public async Task<string> LoadCompressedDataAsync(int id)
    {
        const string sql = @"
            SELECT Data, CompressionType 
            FROM CompressedData 
            WHERE Id = @Id";
        
        var result = await _connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
        
        if (result == null)
            throw new InvalidOperationException($"No data found for ID {id}");
        
        // Restore CompressedObject from Base64
        CompressedObject compressed = (string)result.Data; // Implicit conversion
        
        // Parse compression type
        if (!Enum.TryParse<CompressedObject.CompressionType>(result.CompressionType, out var compressionType))
            compressionType = CompressedObject.CompressionType.GZipStream;
        
        // Decompress and return
        return compressed.DecompressString(compressionType);
    }
    
    public async Task<CompressionStatistics> GetCompressionStatisticsAsync()
    {
        const string sql = @"
            SELECT 
                CompressionType,
                COUNT(*) as Count,
                AVG(CAST(CompressedSize AS FLOAT) / OriginalSize) as AvgCompressionRatio,
                SUM(OriginalSize) as TotalOriginalSize,
                SUM(CompressedSize) as TotalCompressedSize
            FROM CompressedData 
            GROUP BY CompressionType";
        
        var results = await _connection.QueryAsync(sql);
        
        return new CompressionStatistics
        {
            FormatStats = results.ToDictionary(
                r => Enum.Parse<CompressedObject.CompressionType>(r.CompressionType),
                r => new FormatStatistics
                {
                    Count = r.Count,
                    AverageCompressionRatio = r.AvgCompressionRatio,
                    TotalOriginalSize = r.TotalOriginalSize,
                    TotalCompressedSize = r.TotalCompressedSize
                })
        };
    }
}

public class CompressionStatistics
{
    public Dictionary<CompressedObject.CompressionType, FormatStatistics> FormatStats { get; set; } = new();
}

public class FormatStatistics
{
    public int Count { get; set; }
    public double AverageCompressionRatio { get; set; }
    public long TotalOriginalSize { get; set; }
    public long TotalCompressedSize { get; set; }
    public double SpaceSaved => 1 - AverageCompressionRatio;
}
```

### Web API Integration

```csharp
public class CompressionApiController : ControllerBase
{
    [HttpPost("compress")]
    public async Task<IActionResult> CompressData([FromBody] CompressionRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(request.Data))
                return BadRequest("Data cannot be empty");
            
            // Compress data
            byte[] dataBytes = Encoding.UTF8.GetBytes(request.Data);
            CompressedObject compressed = dataBytes.ToCompressed(request.CompressionType);
            
            // Return compressed data info
            var response = new CompressionResponse
            {
                CompressedData = compressed, // Implicit conversion to string (Base64)
                OriginalSize = dataBytes.Length,
                CompressedSize = compressed.Length,
                CompressionRatio = (double)compressed.Length / dataBytes.Length,
                CompressionType = request.CompressionType
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Compression failed: {ex.Message}");
        }
    }
    
    [HttpPost("decompress")]
    public async Task<IActionResult> DecompressData([FromBody] DecompressionRequest request)
    {
        try
        {
            // Restore CompressedObject from Base64
            CompressedObject compressed = request.CompressedData; // Implicit conversion
            
            // Decompress data
            string decompressed = compressed.DecompressString(request.CompressionType);
            
            var response = new DecompressionResponse
            {
                DecompressedData = decompressed,
                CompressedSize = compressed.Length,
                DecompressedSize = Encoding.UTF8.GetBytes(decompressed).Length
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest($"Decompression failed: {ex.Message}");
        }
    }
}

public class CompressionRequest
{
    public string Data { get; set; } = string.Empty;
    public CompressedObject.CompressionType CompressionType { get; set; } = CompressedObject.CompressionType.GZipStream;
}

public class CompressionResponse
{
    public string CompressedData { get; set; } = string.Empty;
    public int OriginalSize { get; set; }
    public int CompressedSize { get; set; }
    public double CompressionRatio { get; set; }
    public CompressedObject.CompressionType CompressionType { get; set; }
}

public class DecompressionRequest
{
    public string CompressedData { get; set; } = string.Empty;
    public CompressedObject.CompressionType CompressionType { get; set; } = CompressedObject.CompressionType.GZipStream;
}

public class DecompressionResponse
{
    public string DecompressedData { get; set; } = string.Empty;
    public int CompressedSize { get; set; }
    public int DecompressedSize { get; set; }
}
```

### Caching Integration

```csharp
public class CompressedCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    
    public CompressedCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public async Task SetCompressedAsync<T>(string key, T value, TimeSpan? expiration = null) where T : notnull
    {
        // Serialize and compress
        string json = value.ToJson();
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        CompressedObject compressed = jsonBytes.ToCompressed(CompressedObject.CompressionType.BrotliStream);
        
        // Store compressed data
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
        };
        
        _cache.Set(key, compressed, options);
        
        Console.WriteLine($"Cached {key}: {jsonBytes.Length} → {compressed.Length} bytes ({(double)compressed.Length / jsonBytes.Length:P2})");
    }
    
    public async Task<T?> GetCompressedAsync<T>(string key)
    {
        if (!_cache.TryGetValue(key, out object? cachedValue) || cachedValue is not CompressedObject compressed)
            return default;
        
        try
        {
            // Decompress and deserialize
            string json = compressed.DecompressString(CompressedObject.CompressionType.BrotliStream);
            return json.FromJson<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decompress cached value for key {key}: {ex.Message}");
            _cache.Remove(key); // Remove corrupted entry
            return default;
        }
    }
    
    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        var stats = new CacheStatistics();
        
        // This would require access to internal cache implementation
        // For demonstration purposes, we'll simulate the statistics
        if (_cache is MemoryCache memCache)
        {
            var field = typeof(MemoryCache).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field?.GetValue(memCache) is IDictionary entries)
            {
                stats.TotalEntries = entries.Count;
                
                foreach (DictionaryEntry entry in entries)
                {
                    if (entry.Value?.GetType().GetProperty("Value")?.GetValue(entry.Value) is CompressedObject compressed)
                    {
                        stats.TotalCompressedSize += compressed.Length;
                        stats.CompressedEntries++;
                    }
                }
            }
        }
        
        return stats;
    }
}

public class CacheStatistics
{
    public int TotalEntries { get; set; }
    public int CompressedEntries { get; set; }
    public long TotalCompressedSize { get; set; }
    public double CompressionCoverage => TotalEntries > 0 ? (double)CompressedEntries / TotalEntries : 0;
}
```

## Performance Characteristics

### Memory Efficiency

```csharp
public class CompressionPerformanceAnalyzer
{
    public async Task AnalyzeMemoryUsage()
    {
        var testSizes = new[] { 1024, 10240, 102400, 1024000 }; // 1KB to 1MB
        
        foreach (int size in testSizes)
        {
            // Generate test data
            byte[] testData = new byte[size];
            Random.Shared.NextBytes(testData);
            
            // Measure original size
            long originalMemory = await Size.Calculate(testData);
            
            // Compress with different formats
            var formats = Enum.GetValues<CompressedObject.CompressionType>()
                .Where(f => f != CompressedObject.CompressionType.BZip2) // Skip if not available
                .ToArray();
            
            Console.WriteLine($"\nTest data size: {size:N0} bytes");
            Console.WriteLine($"Original memory footprint: {originalMemory:N0} bytes");
            
            foreach (var format in formats)
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    CompressedObject compressed = testData.ToCompressed(format);
                    stopwatch.Stop();
                    
                    long compressedMemory = await Size.Calculate(compressed);
                    double memoryRatio = (double)compressedMemory / originalMemory;
                    double compressionRatio = (double)compressed.Length / size;
                    
                    Console.WriteLine($"{format}:");
                    Console.WriteLine($"  Compressed size: {compressed.Length:N0} bytes");
                    Console.WriteLine($"  Memory footprint: {compressedMemory:N0} bytes");
                    Console.WriteLine($"  Memory ratio: {memoryRatio:P2}");
                    Console.WriteLine($"  Compression ratio: {compressionRatio:P2}");
                    Console.WriteLine($"  Compression time: {stopwatch.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{format}: Error - {ex.Message}");
                }
            }
        }
    }
}
```

### Benchmarking Different Scenarios

```csharp
public class CompressionBenchmarks
{
    [Benchmark]
    public CompressedObject CompressGZip()
    {
        return _testData.ToCompressed(CompressedObject.CompressionType.GZipStream);
    }
    
    [Benchmark]
    public CompressedObject CompressDeflate()
    {
        return _testData.ToCompressed(CompressedObject.CompressionType.DeflateStream);
    }
    
    [Benchmark]
    public CompressedObject CompressBrotli()
    {
        return _testData.ToCompressed(CompressedObject.CompressionType.BrotliStream);
    }
    
    [Benchmark]
    public string DecompressGZip()
    {
        return _compressedGZip.DecompressString(CompressedObject.CompressionType.GZipStream);
    }
    
    [Benchmark]
    public string DecompressDeflate()
    {
        return _compressedDeflate.DecompressString(CompressedObject.CompressionType.DeflateStream);
    }
    
    [Benchmark]
    public string DecompressBrotli()
    {
        return _compressedBrotli.DecompressString(CompressedObject.CompressionType.BrotliStream);
    }
    
    private readonly byte[] _testData;
    private readonly CompressedObject _compressedGZip;
    private readonly CompressedObject _compressedDeflate;
    private readonly CompressedObject _compressedBrotli;
    
    public CompressionBenchmarks()
    {
        _testData = Encoding.UTF8.GetBytes(GenerateTestData(10000));
        _compressedGZip = _testData.ToCompressed(CompressedObject.CompressionType.GZipStream);
        _compressedDeflate = _testData.ToCompressed(CompressedObject.CompressionType.DeflateStream);
        _compressedBrotli = _testData.ToCompressed(CompressedObject.CompressionType.BrotliStream);
    }
    
    private static string GenerateTestData(int length)
    {
        var random = new Random(42); // Fixed seed for consistent benchmarks
        var chars = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            chars[i] = (char)random.Next(32, 127); // Printable ASCII
        }
        
        return new string(chars);
    }
}
```

## Error Handling and Edge Cases

### Robust Compression Handling

```csharp
public class SafeCompressionService
{
    public static CompressedObject? TryCompress(byte[] data, CompressedObject.CompressionType compressionType)
    {
        if (data == null || data.Length == 0)
            return null;
        
        try
        {
            return data.ToCompressed(compressionType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Compression failed for {compressionType}: {ex.Message}");
            return null;
        }
    }
    
    public static string? TryDecompress(CompressedObject compressed, CompressedObject.CompressionType compressionType)
    {
        if (compressed.Length == 0)
            return null;
        
        try
        {
            return compressed.DecompressString(compressionType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Decompression failed for {compressionType}: {ex.Message}");
            return null;
        }
    }
    
    public static bool ValidateCompressedData(CompressedObject compressed, CompressedObject.CompressionType compressionType)
    {
        try
        {
            // Attempt decompression to validate
            compressed.DecompressString(compressionType);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static CompressedObject CompressWithFallback(byte[] data, params CompressedObject.CompressionType[] preferredFormats)
    {
        foreach (var format in preferredFormats)
        {
            var compressed = TryCompress(data, format);
            if (compressed.HasValue)
                return compressed.Value;
        }
        
        // Final fallback to GZipStream
        return data.ToCompressed(CompressedObject.CompressionType.GZipStream);
    }
}
```

## Integration with BuildingBlocks Helpers

### Serialization Integration

```csharp
public class SerializationCompressionService
{
    public CompressedObject CompressObject<T>(T obj, CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.BrotliStream) where T : notnull
    {
        // Try different serialization formats and choose most efficient
        var serializations = new[]
        {
            ("JSON", obj.ToJsonBytes()),
            ("MessagePack", obj.ToMessagePackBytes()),
            ("Protobuf", obj.ToProtobufBytes())
        };
        
        var bestSerialization = serializations.OrderBy(s => s.Item2.Length).First();
        Console.WriteLine($"Best serialization for compression: {bestSerialization.Item1} ({bestSerialization.Item2.Length} bytes)");
        
        return bestSerialization.Item2.ToCompressed(compressionType);
    }
    
    public T? DecompressObject<T>(CompressedObject compressed, string serializationFormat, CompressedObject.CompressionType compressionType)
    {
        try
        {
            byte[] decompressedBytes = compressed.DecompressStream(compressionType).ToByteArray();
            
            return serializationFormat.ToLower() switch
            {
                "json" => decompressedBytes.FromJsonBytes<T>(),
                "messagepack" => decompressedBytes.FromMessagePackBytes<T>(),
                "protobuf" => decompressedBytes.FromProtobufBytes<T>(),
                _ => throw new NotSupportedException($"Serialization format not supported: {serializationFormat}")
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to decompress object: {ex.Message}");
            return default;
        }
    }
}
```

### String Helper Integration

```csharp
public static class CompressedObjectExtensions
{
    public static CompressedObject ToCompressedFromString(this string str, CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
    {
        byte[] bytes = str.ToByteArray(); // Using StringHelper
        return bytes.ToCompressed(compressionType);
    }
    
    public static string DecompressToString(this CompressedObject compressed, CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
    {
        byte[] decompressed = compressed.DecompressStream(compressionType).ToByteArray();
        return decompressed.FromByteArray(); // Using StringHelper
    }
    
    public static CompressedObject FromBase64Compressed(this string base64)
    {
        return base64.FromBase64().ToByteArray().ToCompressed(); // Chain StringHelper methods
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[Test]
public void CompressedObject_ImplicitConversions_WorkCorrectly()
{
    // Arrange
    byte[] originalData = Encoding.UTF8.GetBytes("Test data for compression");
    string base64Data = Convert.ToBase64String(originalData);
    
    // Act - Test byte array conversion
    CompressedObject fromBytes = originalData;
    byte[] backToBytes = fromBytes;
    
    // Act - Test string conversion
    CompressedObject fromString = base64Data;
    string backToString = fromString;
    
    // Assert
    Assert.That(backToBytes, Is.EqualTo(originalData));
    Assert.That(backToString, Is.EqualTo(base64Data));
    Assert.That(fromBytes.Length, Is.EqualTo(originalData.Length));
}

[Test]
public void CompressedObject_Length_ReturnsCorrectValue()
{
    // Arrange
    byte[] testData = new byte[1000];
    Random.Shared.NextBytes(testData);
    
    // Act
    CompressedObject compressed = testData;
    
    // Assert
    Assert.That(compressed.Length, Is.EqualTo(testData.Length));
}

[Test]
public void CompressedObject_ToString_ReturnsBase64()
{
    // Arrange
    byte[] testData = { 1, 2, 3, 4, 5 };
    string expectedBase64 = Convert.ToBase64String(testData);
    
    // Act
    CompressedObject compressed = testData;
    string result = compressed.ToString();
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedBase64));
}
```

### Integration Tests

```csharp
[Test]
public void CompressedObject_RoundTripWithCompression_PreservesData()
{
    // Arrange
    string originalData = "This is test data that will be compressed and decompressed";
    byte[] originalBytes = Encoding.UTF8.GetBytes(originalData);
    
    var compressionTypes = new[]
    {
        CompressedObject.CompressionType.GZipStream,
        CompressedObject.CompressionType.DeflateStream,
        CompressedObject.CompressionType.BrotliStream
    };
    
    foreach (var compressionType in compressionTypes)
    {
        // Act
        CompressedObject compressed = originalBytes.ToCompressed(compressionType);
        string decompressed = compressed.DecompressString(compressionType);
        
        // Assert
        Assert.That(decompressed, Is.EqualTo(originalData), $"Round-trip failed for {compressionType}");
        Assert.That(compressed.Length, Is.LessThan(originalBytes.Length), $"No compression achieved with {compressionType}");
    }
}
```

## Best Practices

### 1. Choose Appropriate Compression Format
```csharp
public static class CompressionFormatSelector
{
    public static CompressedObject.CompressionType SelectBestFormat(byte[] data, CompressionPriority priority)
    {
        return priority switch
        {
            CompressionPriority.Speed => CompressedObject.CompressionType.DeflateStream,
            CompressionPriority.Size => CompressedObject.CompressionType.BrotliStream,
            CompressionPriority.Compatibility => CompressedObject.CompressionType.GZipStream,
            _ => CompressedObject.CompressionType.GZipStream
        };
    }
}

public enum CompressionPriority
{
    Speed,
    Size,
    Compatibility
}
```

### 2. Handle Large Data Efficiently
```csharp
public class LargeDataCompressionHandler
{
    private const int MaxDataSize = 10 * 1024 * 1024; // 10MB threshold
    
    public CompressedObject CompressLargeData(byte[] data, CompressedObject.CompressionType compressionType)
    {
        if (data.Length > MaxDataSize)
        {
            Console.WriteLine($"Warning: Compressing large data ({data.Length:N0} bytes)");
            // Consider chunked compression for very large data
        }
        
        return data.ToCompressed(compressionType);
    }
}
```

### 3. Validate Compressed Data
```csharp
public static bool IsValidCompressedData(CompressedObject compressed, CompressedObject.CompressionType expectedType)
{
    try
    {
        compressed.DecompressString(expectedType);
        return true;
    }
    catch
    {
        return false;
    }
}
```

## See Also

- [ObjectHelper](../Helpers/ObjectHelper.md) - Object compression and manipulation utilities
- [StreamHelper](../Helpers/StreamHelper.md) - Stream decompression operations
- [StringHelper](../Helpers/StringHelper.md) - String decompression utilities
- [DisposableObject](DisposableObject.md) - Base class for proper resource disposal
- [Size](../Helpers/Size.md) - Memory footprint calculation

---

*Part of the RapidStreamer.BuildingBlocks.Application.Objects namespace - providing efficient compressed data container infrastructure for .NET applications.*