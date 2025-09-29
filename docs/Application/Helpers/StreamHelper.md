# StreamHelper

The `StreamHelper` class provides stream manipulation utilities for .NET applications. It offers efficient stream conversion operations, byte array handling, and decompression capabilities with built-in telemetry support for performance monitoring.

## Overview

```csharp
public static class StreamHelper
```

`StreamHelper` is a static utility class that provides extension methods and helper functions for working with streams, enabling efficient data conversion and decompression operations.

## Key Features

- **Stream to Byte Array Conversion**: Efficient conversion with position management
- **String to Stream Conversion**: UTF-8 encoding with memory stream creation
- **Compression Support**: Decompression for GZip, Deflate, and Brotli formats
- **Performance Optimization**: Special handling for MemoryStream instances
- **Telemetry Integration**: Built-in activity tracking for performance analysis
- **Position Management**: Automatic stream position handling

## Public API

### Extension Methods

#### ToByteArray(this Stream stream)
Converts a stream to a byte array with optimal performance handling.

```csharp
public static byte[] ToByteArray(this Stream stream)
```

**Parameters:**
- `stream`: The stream to convert

**Returns:** byte[] containing the stream data

**Features:**
- Automatically resets stream position to beginning
- Optimized path for MemoryStream instances  
- Uses BinaryReader for efficient reading
- Includes telemetry tracking

#### ToStream(this string str)
Converts a string to a UTF-8 encoded memory stream.

```csharp
public static Stream ToStream(this string str)
```

**Parameters:**
- `str`: The string to convert

**Returns:** Stream containing UTF-8 encoded string data

#### DecompressStream(this CompressedObject compressedObject, CompressionType compressionType)
Decompresses a compressed object to a readable stream.

```csharp
public static Stream DecompressStream(this CompressedObject compressedObject,
    CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
```

**Parameters:**
- `compressedObject`: The compressed data to decompress
- `compressionType`: The compression algorithm used (GZipStream, DeflateStream, BrotliStream)

**Returns:** Stream containing the decompressed data

**Supported Compression Types:**
- `GZipStream` (default)
- `DeflateStream`
- `BrotliStream`

## Usage Examples

### Basic Stream to Byte Array Conversion

```csharp
public class StreamProcessor
{
    public byte[] ProcessFileStream(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        // Convert stream to byte array
        byte[] data = fileStream.ToByteArray();
        
        Console.WriteLine($"Read {data.Length} bytes from {filePath}");
        return data;
    }
    
    public byte[] ProcessMemoryStream(byte[] data)
    {
        using var memoryStream = new MemoryStream(data);
        
        // Optimized conversion for MemoryStream
        byte[] result = memoryStream.ToByteArray();
        
        return result;
    }
}
```

### String to Stream Conversion

```csharp
public class TextStreamProcessor
{
    public void ProcessTextData(string text)
    {
        // Convert string to stream
        using Stream textStream = text.ToStream();
        
        // Process the stream
        using var reader = new StreamReader(textStream);
        string readback = reader.ReadToEnd();
        
        Console.WriteLine($"Original: {text}");
        Console.WriteLine($"Readback: {readback}");
    }
    
    public async Task SaveStringToFile(string text, string filePath)
    {
        using Stream textStream = text.ToStream();
        using var fileStream = new FileStream(filePath, FileMode.Create);
        
        await textStream.CopyToAsync(fileStream);
    }
}
```

### Decompression Operations

```csharp
public class CompressionHandler
{
    public string DecompressData(CompressedObject compressed)
    {
        // Decompress using default GZip
        using Stream decompressed = compressed.DecompressStream();
        using var reader = new StreamReader(decompressed);
        
        return reader.ReadToEnd();
    }
    
    public byte[] DecompressToBytes(CompressedObject compressed, 
        CompressedObject.CompressionType type)
    {
        using Stream decompressed = compressed.DecompressStream(type);
        return decompressed.ToByteArray();
    }
    
    public async Task ProcessCompressedFile(CompressedObject compressed)
    {
        // Decompress different formats
        var formats = new[]
        {
            CompressedObject.CompressionType.GZipStream,
            CompressedObject.CompressionType.DeflateStream,
            CompressedObject.CompressionType.BrotliStream
        };
        
        foreach (var format in formats)
        {
            try
            {
                using Stream decompressed = compressed.DecompressStream(format);
                byte[] data = decompressed.ToByteArray();
                
                Console.WriteLine($"{format}: {data.Length} bytes decompressed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{format} failed: {ex.Message}");
            }
        }
    }
}
```

### Stream Pipeline Processing

```csharp
public class StreamPipeline
{
    public async Task<byte[]> ProcessDataPipeline(string inputText)
    {
        // Convert string to stream
        using Stream textStream = inputText.ToStream();
        
        // Process through multiple stages
        byte[] stage1 = textStream.ToByteArray();
        
        // Compress data (using ObjectHelper)
        var compressed = stage1.ToCompressed();
        
        // Decompress back to stream
        using Stream decompressed = compressed.DecompressStream();
        
        // Final conversion
        byte[] final = decompressed.ToByteArray();
        
        return final;
    }
    
    public void StreamChaining()
    {
        string originalText = "Hello, World! This is a test message for compression.";
        
        // Chain operations
        byte[] result = originalText
            .ToStream()
            .ToByteArray();
        
        // Verify roundtrip
        string verification = Encoding.UTF8.GetString(result);
        Console.WriteLine($"Original: {originalText}");
        Console.WriteLine($"Result: {verification}");
        Console.WriteLine($"Match: {originalText == verification}");
    }
}
```

## Performance Characteristics

### Memory Stream Optimization
```csharp
public class PerformanceComparison
{
    public void CompareMemoryStreamHandling()
    {
        byte[] testData = Encoding.UTF8.GetBytes("Test data for performance comparison");
        
        // Method 1: Direct MemoryStream.ToArray() (optimized path)
        using var memoryStream1 = new MemoryStream(testData);
        var stopwatch1 = Stopwatch.StartNew();
        byte[] result1 = memoryStream1.ToByteArray(); // Uses optimized path
        stopwatch1.Stop();
        
        // Method 2: Manual BinaryReader approach
        using var memoryStream2 = new MemoryStream(testData);
        var stopwatch2 = Stopwatch.StartNew();
        using var reader = new BinaryReader(memoryStream2);
        byte[] result2 = reader.ReadBytes((int)memoryStream2.Length);
        stopwatch2.Stop();
        
        Console.WriteLine($"Optimized path: {stopwatch1.ElapsedTicks} ticks");
        Console.WriteLine($"Manual approach: {stopwatch2.ElapsedTicks} ticks");
    }
}
```

### Large Stream Handling
```csharp
public class LargeStreamProcessor
{
    public async Task ProcessLargeFile(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        // Check file size before processing
        long fileSize = fileStream.Length;
        Console.WriteLine($"Processing file of {fileSize:N0} bytes");
        
        if (fileSize > 100_000_000) // 100MB
        {
            Console.WriteLine("Large file detected - consider chunked processing");
            await ProcessInChunks(fileStream);
        }
        else
        {
            byte[] data = fileStream.ToByteArray();
            await ProcessData(data);
        }
    }
    
    private async Task ProcessInChunks(Stream stream)
    {
        const int chunkSize = 1024 * 1024; // 1MB chunks
        var buffer = new byte[chunkSize];
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, chunkSize)) > 0)
        {
            // Process chunk
            await ProcessChunk(buffer, bytesRead);
        }
    }
}
```

## Integration with Compression Systems

### Working with CompressedObject
```csharp
public class CompressionIntegration
{
    public void CompressAndDecompress(string data)
    {
        // Convert to stream
        using Stream dataStream = data.ToStream();
        byte[] dataBytes = dataStream.ToByteArray();
        
        // Compress using ObjectHelper
        var compressed = dataBytes.ToCompressed(
            CompressedObject.CompressionType.GZipStream);
        
        // Decompress back to stream
        using Stream decompressed = compressed.DecompressStream(
            CompressedObject.CompressionType.GZipStream);
        
        // Convert back to string
        using var reader = new StreamReader(decompressed);
        string result = reader.ReadToEnd();
        
        Console.WriteLine($"Original: {data}");
        Console.WriteLine($"Roundtrip: {result}");
        Console.WriteLine($"Match: {data == result}");
    }
    
    public void CompareCompressionFormats(string data)
    {
        var dataBytes = data.ToStream().ToByteArray();
        
        var formats = new[]
        {
            CompressedObject.CompressionType.GZipStream,
            CompressedObject.CompressionType.DeflateStream,
            CompressedObject.CompressionType.BrotliStream
        };
        
        foreach (var format in formats)
        {
            // Compress
            var compressed = dataBytes.ToCompressed(format);
            
            // Decompress
            using Stream decompressed = compressed.DecompressStream(format);
            byte[] result = decompressed.ToByteArray();
            
            double ratio = (double)compressed.Length / dataBytes.Length;
            Console.WriteLine($"{format}: {ratio:P2} compression ratio");
        }
    }
}
```

## Error Handling and Edge Cases

### Stream Position Management
```csharp
public class StreamPositionHandling
{
    public void DemonstratePositionHandling()
    {
        byte[] testData = Encoding.UTF8.GetBytes("Test data");
        using var stream = new MemoryStream(testData);
        
        // Move position
        stream.Seek(5, SeekOrigin.Begin);
        Console.WriteLine($"Position before ToByteArray: {stream.Position}");
        
        // ToByteArray automatically resets position
        byte[] result = stream.ToByteArray();
        
        Console.WriteLine($"Position after ToByteArray: {stream.Position}");
        Console.WriteLine($"Full data retrieved: {result.Length == testData.Length}");
    }
    
    public void HandleStreamErrors()
    {
        try
        {
            // Simulate a closed stream
            var stream = new MemoryStream();
            stream.Close();
            
            byte[] result = stream.ToByteArray();
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine($"Stream disposed: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"IO error: {ex.Message}");
        }
    }
}
```

### Decompression Error Handling
```csharp
public class DecompressionErrorHandling
{
    public string SafeDecompress(CompressedObject compressed, 
        CompressedObject.CompressionType expectedType)
    {
        try
        {
            using Stream decompressed = compressed.DecompressStream(expectedType);
            using var reader = new StreamReader(decompressed);
            return reader.ReadToEnd();
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Invalid compression format: {ex.Message}");
            return string.Empty;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Unsupported compression type: {ex.Message}");
            return string.Empty;
        }
    }
    
    public CompressedObject.CompressionType DetectCompressionType(CompressedObject compressed)
    {
        var types = new[]
        {
            CompressedObject.CompressionType.GZipStream,
            CompressedObject.CompressionType.DeflateStream,
            CompressedObject.CompressionType.BrotliStream
        };
        
        foreach (var type in types)
        {
            try
            {
                using Stream test = compressed.DecompressStream(type);
                // If successful, return the type
                return type;
            }
            catch
            {
                // Try next type
                continue;
            }
        }
        
        throw new InvalidOperationException("Unable to detect compression type");
    }
}
```

## Telemetry and Monitoring

### Performance Tracking
```csharp
public class StreamTelemetry
{
    public void MonitorStreamOperations()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                if (activity.Source.Name == "RapidStreamer.BuildingBlocks.Application")
                {
                    Console.WriteLine($"Activity: {activity.DisplayName}");
                    Console.WriteLine($"Duration: {activity.Duration.TotalMilliseconds}ms");
                    
                    foreach (var tag in activity.Tags)
                    {
                        Console.WriteLine($"  {tag.Key}: {tag.Value}");
                    }
                }
            }
        };
        
        ActivitySource.AddActivityListener(listener);
        
        // Perform operations to monitor
        string testData = "Test data for telemetry";
        using Stream stream = testData.ToStream();
        byte[] result = stream.ToByteArray();
    }
}
```

## Testing Strategies

### Unit Tests
```csharp
[Test]
public void ToByteArray_WithValidStream_ReturnsCorrectData()
{
    // Arrange
    byte[] expectedData = Encoding.UTF8.GetBytes("Test data");
    using var stream = new MemoryStream(expectedData);
    
    // Act
    byte[] result = stream.ToByteArray();
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedData));
}

[Test]
public void ToStream_WithValidString_ReturnsCorrectStream()
{
    // Arrange
    string testData = "Test string";
    
    // Act
    using Stream result = testData.ToStream();
    using var reader = new StreamReader(result);
    string readback = reader.ReadToEnd();
    
    // Assert
    Assert.That(readback, Is.EqualTo(testData));
}

[Test]
public void DecompressStream_WithValidData_ReturnsDecompressedStream()
{
    // Arrange
    string originalData = "Test data for compression";
    byte[] dataBytes = Encoding.UTF8.GetBytes(originalData);
    var compressed = dataBytes.ToCompressed(CompressedObject.CompressionType.GZipStream);
    
    // Act
    using Stream decompressed = compressed.DecompressStream(CompressedObject.CompressionType.GZipStream);
    using var reader = new StreamReader(decompressed);
    string result = reader.ReadToEnd();
    
    // Assert
    Assert.That(result, Is.EqualTo(originalData));
}
```

### Performance Tests
```csharp
[Test]
public void ToByteArray_PerformanceTest()
{
    // Arrange
    byte[] largeData = new byte[1_000_000]; // 1MB
    Random.Shared.NextBytes(largeData);
    using var stream = new MemoryStream(largeData);
    
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    byte[] result = stream.ToByteArray();
    
    // Assert
    stopwatch.Stop();
    Assert.That(result.Length, Is.EqualTo(largeData.Length));
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100)); // Should be fast
}
```

## Best Practices

### 1. Proper Stream Disposal
```csharp
// Preferred - Using statement
public byte[] ProcessStream(Stream inputStream)
{
    using Stream stream = inputStream;
    return stream.ToByteArray();
}

// Also acceptable - Explicit disposal
public byte[] ProcessStreamExplicit(Stream inputStream)
{
    try
    {
        return inputStream.ToByteArray();
    }
    finally
    {
        inputStream?.Dispose();
    }
}
```

### 2. Memory Considerations
```csharp
public class MemoryEfficientProcessing
{
    public async Task ProcessLargeStream(Stream largeStream)
    {
        // Check size before loading into memory
        if (largeStream.Length > 50_000_000) // 50MB threshold
        {
            // Process in chunks rather than loading all at once
            await ProcessStreamInChunks(largeStream);
        }
        else
        {
            byte[] data = largeStream.ToByteArray();
            ProcessData(data);
        }
    }
}
```

### 3. Error Handling
```csharp
public byte[] SafeStreamConversion(Stream stream)
{
    try
    {
        return stream.ToByteArray();
    }
    catch (ArgumentNullException)
    {
        throw new ArgumentException("Stream cannot be null");
    }
    catch (ObjectDisposedException)
    {
        throw new InvalidOperationException("Stream has been disposed");
    }
    catch (IOException ex)
    {
        throw new InvalidOperationException($"Error reading stream: {ex.Message}", ex);
    }
}
```

## Integration with Other Helpers

### String Helper Integration
```csharp
public class StringStreamIntegration
{
    public void ConvertStringToStreamToBytes(string data)
    {
        // Method 1: Direct conversion
        byte[] directBytes = data.ToByteArray(); // StringHelper
        
        // Method 2: Via stream
        using Stream stream = data.ToStream(); // StreamHelper
        byte[] streamBytes = stream.ToByteArray(); // StreamHelper
        
        // Both should yield same result
        Console.WriteLine($"Direct: {directBytes.Length} bytes");
        Console.WriteLine($"Via Stream: {streamBytes.Length} bytes");
        Console.WriteLine($"Equal: {directBytes.SequenceEqual(streamBytes)}");
    }
}
```

### Object Helper Integration
```csharp
public class ObjectStreamIntegration
{
    public void ProcessCompressedData<T>(T data) where T : notnull
    {
        // Compress object
        var compressed = data.ToCompressed();
        
        // Decompress to stream
        using Stream decompressed = compressed.DecompressStream();
        
        // Convert to bytes
        byte[] bytes = decompressed.ToByteArray();
        
        // Reconstruct object
        T reconstructed = bytes.FromCompressed<T>();
        
        Console.WriteLine($"Original type: {typeof(T).Name}");
        Console.WriteLine($"Compressed size: {compressed.Length} bytes");
        Console.WriteLine($"Decompressed size: {bytes.Length} bytes");
    }
}
```

## Migration and Upgrades

When upgrading from manual stream handling:

```csharp
// Old approach - Manual implementation
private byte[] ConvertStreamToBytes(Stream stream)
{
    stream.Position = 0; // Manual position reset
    var bytes = new byte[stream.Length];
    stream.Read(bytes, 0, bytes.Length);
    return bytes;
}

// New approach - Using StreamHelper
private byte[] ConvertStreamToBytes(Stream stream)
{
    return stream.ToByteArray(); // Handles position reset automatically
}
```

## See Also

- [StringHelper](StringHelper.md) - String manipulation and encoding utilities
- [ObjectHelper](ObjectHelper.md) - Object compression and manipulation
- [CompressedObject](../Objects/CompressedObject.md) - Compression data structure
- [Telemetry](../Telemetry.md) - Performance monitoring infrastructure

---

*Part of the RapidStreamer.BuildingBlocks.Application.Helpers namespace - providing essential stream manipulation utilities for .NET applications.*