# StringHelper

The `StringHelper` class provides string manipulation utilities for .NET applications. It offers efficient string encoding, Base64 conversion, and decompression operations with built-in telemetry support for performance monitoring.

## Overview

```csharp
public static class StringHelper
```

`StringHelper` is a static utility class that provides extension methods for string manipulation, encoding conversions, and compression operations with UTF-8 encoding as the standard.

## Key Features

- **Byte Array Conversion**: Efficient UTF-8 encoding and decoding
- **ReadOnlyMemory Support**: Modern memory-efficient byte array representation
- **Base64 Operations**: Optimized Base64 encoding and decoding with trimming
- **Decompression Support**: Direct string decompression from compressed objects
- **Telemetry Integration**: Built-in activity tracking for performance analysis
- **UTF-8 Standard**: Consistent UTF-8 encoding across all operations

## Public API

### Extension Methods

#### ToByteArray(this string str)
Converts a string to a UTF-8 encoded byte array.

```csharp
public static byte[] ToByteArray(this string str)
```

**Parameters:**
- `str`: The string to convert

**Returns:** byte[] containing UTF-8 encoded string data

#### ToByteReadOnlyMemory(this string str)
Converts a string to a UTF-8 encoded ReadOnlyMemory<byte> for modern memory management.

```csharp
public static ReadOnlyMemory<byte> ToByteReadOnlyMemory(this string str)
```

**Parameters:**
- `str`: The string to convert

**Returns:** ReadOnlyMemory<byte> containing UTF-8 encoded string data

#### FromByteArray(this byte[] bytes)
Converts a UTF-8 encoded byte array back to a string.

```csharp
public static string FromByteArray(this byte[] bytes)
```

**Parameters:**
- `bytes`: The UTF-8 encoded byte array

**Returns:** string decoded from the byte array

#### ToBase64(this string str)
Converts a string to Base64 representation with optimization.

```csharp
public static string ToBase64(this string str)
```

**Parameters:**
- `str`: The string to encode

**Returns:** Base64 encoded string (with trailing padding removed)

**Note:** The method removes the last 2 characters (`[..^2]`) which are typically padding characters.

#### FromBase64(this string str)
Converts a Base64 encoded string back to the original string.

```csharp
public static string FromBase64(this string str)
```

**Parameters:**
- `str`: The Base64 encoded string

**Returns:** string decoded from Base64

#### DecompressString(this CompressedObject compressedObject, CompressionType compressionType)
Decompresses a compressed object directly to a string.

```csharp
public static string DecompressString(this CompressedObject compressedObject,
    CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
```

**Parameters:**
- `compressedObject`: The compressed data to decompress
- `compressionType`: The compression algorithm used (GZipStream, DeflateStream, BrotliStream)

**Returns:** string containing the decompressed text data

## Usage Examples

### Basic String Encoding Operations

```csharp
public class StringEncodingProcessor
{
    public void DemonstrateBasicOperations()
    {
        string originalText = "Hello, 世界! 🌍";
        
        // Convert to byte array
        byte[] bytes = originalText.ToByteArray();
        Console.WriteLine($"UTF-8 bytes: {bytes.Length}");
        
        // Convert back to string
        string decoded = bytes.FromByteArray();
        Console.WriteLine($"Decoded: {decoded}");
        Console.WriteLine($"Round-trip success: {originalText == decoded}");
        
        // Modern memory-efficient approach
        ReadOnlyMemory<byte> memory = originalText.ToByteReadOnlyMemory();
        Console.WriteLine($"Memory span length: {memory.Length}");
    }
    
    public void ProcessMultilingualText()
    {
        var texts = new[]
        {
            "English text",
            "Español con acentos",
            "中文字符",
            "العربية",
            "🎉 Emoji support"
        };
        
        foreach (string text in texts)
        {
            byte[] encoded = text.ToByteArray();
            string decoded = encoded.FromByteArray();
            
            Console.WriteLine($"Original: {text}");
            Console.WriteLine($"Bytes: {encoded.Length}");
            Console.WriteLine($"Roundtrip: {text == decoded}");
            Console.WriteLine("---");
        }
    }
}
```

### Base64 Operations

```csharp
public class Base64Processor
{
    public void DemonstrateBase64Operations()
    {
        string originalData = "This is a test message for Base64 encoding.";
        
        // Encode to Base64
        string encoded = originalData.ToBase64();
        Console.WriteLine($"Original: {originalData}");
        Console.WriteLine($"Base64: {encoded}");
        
        // Decode from Base64
        string decoded = encoded.FromBase64();
        Console.WriteLine($"Decoded: {decoded}");
        Console.WriteLine($"Match: {originalData == decoded}");
    }
    
    public void CompareBase64WithStandard()
    {
        string testData = "Compare with standard Base64 encoding";
        
        // StringHelper approach (with trimming)
        string customBase64 = testData.ToBase64();
        
        // Standard .NET approach
        byte[] bytes = Encoding.UTF8.GetBytes(testData);
        string standardBase64 = Convert.ToBase64String(bytes);
        
        Console.WriteLine($"Custom Base64: {customBase64}");
        Console.WriteLine($"Standard Base64: {standardBase64}");
        Console.WriteLine($"Length difference: {standardBase64.Length - customBase64.Length}");
        
        // Decode both
        string customDecoded = customBase64.FromBase64();
        string standardDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(standardBase64));
        
        Console.WriteLine($"Both decode correctly: {customDecoded == standardDecoded}");
    }
    
    public void HandleBinaryDataAsBase64()
    {
        // Simulate binary data
        byte[] binaryData = new byte[256];
        for (int i = 0; i < 256; i++)
            binaryData[i] = (byte)i;
        
        // Convert to string representation for Base64
        string binaryAsString = Encoding.Latin1.GetString(binaryData);
        
        // Base64 encode
        string base64 = binaryAsString.ToBase64();
        
        // Decode and verify
        string decoded = base64.FromBase64();
        byte[] result = Encoding.Latin1.GetBytes(decoded);
        
        Console.WriteLine($"Binary data roundtrip: {binaryData.SequenceEqual(result)}");
    }
}
```

### Decompression Operations

```csharp
public class StringDecompressionProcessor
{
    public void DecompressStrings()
    {
        string originalText = "This is a long text that will be compressed and then decompressed back to string format.";
        
        // First compress the text (using ObjectHelper)
        byte[] textBytes = originalText.ToByteArray();
        var compressed = textBytes.ToCompressed(CompressedObject.CompressionType.GZipStream);
        
        // Decompress directly to string
        string decompressed = compressed.DecompressString(CompressedObject.CompressionType.GZipStream);
        
        Console.WriteLine($"Original: {originalText}");
        Console.WriteLine($"Decompressed: {decompressed}");
        Console.WriteLine($"Success: {originalText == decompressed}");
        Console.WriteLine($"Compression ratio: {(double)compressed.Length / textBytes.Length:P2}");
    }
    
    public void CompareCompressionFormats()
    {
        string testData = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                         "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        
        var formats = new[]
        {
            CompressedObject.CompressionType.GZipStream,
            CompressedObject.CompressionType.DeflateStream,
            CompressedObject.CompressionType.BrotliStream
        };
        
        byte[] originalBytes = testData.ToByteArray();
        
        foreach (var format in formats)
        {
            try
            {
                // Compress
                var compressed = originalBytes.ToCompressed(format);
                
                // Decompress to string
                string result = compressed.DecompressString(format);
                
                // Verify
                bool success = testData == result;
                double ratio = (double)compressed.Length / originalBytes.Length;
                
                Console.WriteLine($"{format}:");
                Console.WriteLine($"  Success: {success}");
                Console.WriteLine($"  Ratio: {ratio:P2}");
                Console.WriteLine($"  Size: {compressed.Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{format}: Failed - {ex.Message}");
            }
        }
    }
    
    public async Task ProcessCompressedFiles()
    {
        // Simulate processing multiple compressed text files
        var compressedFiles = new Dictionary<string, CompressedObject>
        {
            ["config.txt"] = "Configuration data here".ToByteArray().ToCompressed(),
            ["log.txt"] = "Log entries and messages".ToByteArray().ToCompressed(),
            ["data.json"] = "{\"key\": \"value\", \"array\": [1,2,3]}".ToByteArray().ToCompressed()
        };
        
        foreach (var file in compressedFiles)
        {
            string content = file.Value.DecompressString();
            Console.WriteLine($"{file.Key}: {content}");
            
            // Process based on file type
            if (file.Key.EndsWith(".json"))
            {
                await ProcessJsonContent(content);
            }
            else if (file.Key.EndsWith(".txt"))
            {
                await ProcessTextContent(content);
            }
        }
    }
}
```

### Memory-Efficient Operations

```csharp
public class MemoryEfficientStringProcessing
{
    public void UseReadOnlyMemory()
    {
        string largeText = string.Join("", Enumerable.Repeat("Sample text data. ", 10000));
        
        // Memory-efficient approach
        ReadOnlyMemory<byte> memory = largeText.ToByteReadOnlyMemory();
        
        // Process in chunks without creating intermediate arrays
        ProcessInChunks(memory);
        
        Console.WriteLine($"Processed {memory.Length} bytes efficiently");
    }
    
    private void ProcessInChunks(ReadOnlyMemory<byte> data)
    {
        const int chunkSize = 1024;
        
        for (int i = 0; i < data.Length; i += chunkSize)
        {
            int currentChunkSize = Math.Min(chunkSize, data.Length - i);
            ReadOnlyMemory<byte> chunk = data.Slice(i, currentChunkSize);
            
            // Process chunk (example: count specific bytes)
            int count = CountSpecificByte(chunk.Span, (byte)' ');
            Console.WriteLine($"Chunk {i / chunkSize}: {count} spaces");
        }
    }
    
    private int CountSpecificByte(ReadOnlySpan<byte> span, byte target)
    {
        int count = 0;
        foreach (byte b in span)
        {
            if (b == target) count++;
        }
        return count;
    }
    
    public void CompareMemoryAllocations()
    {
        string testString = "Test string for memory comparison";
        
        // Method 1: Traditional byte array (allocates memory)
        var stopwatch1 = Stopwatch.StartNew();
        byte[] traditionalArray = testString.ToByteArray();
        stopwatch1.Stop();
        
        // Method 2: ReadOnlyMemory (more efficient)
        var stopwatch2 = Stopwatch.StartNew();
        ReadOnlyMemory<byte> memory = testString.ToByteReadOnlyMemory();
        stopwatch2.Stop();
        
        Console.WriteLine($"Traditional array: {stopwatch1.ElapsedTicks} ticks");
        Console.WriteLine($"ReadOnlyMemory: {stopwatch2.ElapsedTicks} ticks");
        Console.WriteLine($"Same data: {traditionalArray.SequenceEqual(memory.ToArray())}");
    }
}
```

## Performance Characteristics

### Encoding Performance
```csharp
public class StringPerformanceAnalysis
{
    public void BenchmarkEncodingOperations()
    {
        string testData = string.Join("", Enumerable.Repeat("Performance test data. ", 1000));
        const int iterations = 10000;
        
        // Benchmark ToByteArray
        var sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            byte[] _ = testData.ToByteArray();
        }
        sw1.Stop();
        
        // Benchmark ToByteReadOnlyMemory
        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ReadOnlyMemory<byte> _ = testData.ToByteReadOnlyMemory();
        }
        sw2.Stop();
        
        // Benchmark standard Encoding.UTF8.GetBytes
        var sw3 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            byte[] _ = Encoding.UTF8.GetBytes(testData);
        }
        sw3.Stop();
        
        Console.WriteLine($"ToByteArray: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"ToByteReadOnlyMemory: {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"Standard UTF8: {sw3.ElapsedMilliseconds}ms");
    }
    
    public void BenchmarkBase64Operations()
    {
        string testData = "Base64 performance test data";
        const int iterations = 100000;
        
        // StringHelper Base64
        var sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            string encoded = testData.ToBase64();
            string _ = encoded.FromBase64();
        }
        sw1.Stop();
        
        // Standard Base64
        var sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(testData);
            string encoded = Convert.ToBase64String(bytes);
            byte[] decoded = Convert.FromBase64String(encoded);
            string _ = Encoding.UTF8.GetString(decoded);
        }
        sw2.Stop();
        
        Console.WriteLine($"StringHelper Base64: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"Standard Base64: {sw2.ElapsedMilliseconds}ms");
    }
}
```

## Integration with Compression Systems

### Working with Compression Helpers
```csharp
public class CompressionStringIntegration
{
    public void StringCompressionWorkflow()
    {
        string originalText = "This text will go through compression workflow";
        
        // Step 1: String to bytes
        byte[] textBytes = originalText.ToByteArray();
        
        // Step 2: Compress
        var compressed = textBytes.ToCompressed(CompressedObject.CompressionType.BrotliStream);
        
        // Step 3: Direct decompression to string
        string decompressed = compressed.DecompressString(CompressedObject.CompressionType.BrotliStream);
        
        // Verify workflow
        Console.WriteLine($"Original: {originalText}");
        Console.WriteLine($"Decompressed: {decompressed}");
        Console.WriteLine($"Workflow success: {originalText == decompressed}");
        
        // Calculate efficiency
        double compressionRatio = (double)compressed.Length / textBytes.Length;
        Console.WriteLine($"Compression ratio: {compressionRatio:P2}");
    }
    
    public void ProcessMultipleCompressionFormats()
    {
        string data = "Sample data for multi-format compression testing";
        
        var results = new Dictionary<CompressedObject.CompressionType, (int size, string result)>();
        
        foreach (CompressedObject.CompressionType type in Enum.GetValues<CompressedObject.CompressionType>())
        {
            try
            {
                // Compress
                var compressed = data.ToByteArray().ToCompressed(type);
                
                // Decompress to string
                string decompressed = compressed.DecompressString(type);
                
                results[type] = (compressed.Length, decompressed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{type} failed: {ex.Message}");
            }
        }
        
        // Report results
        foreach (var result in results.OrderBy(r => r.Value.size))
        {
            bool success = result.Value.result == data;
            Console.WriteLine($"{result.Key}: {result.Value.size} bytes, Success: {success}");
        }
    }
}
```

## Error Handling and Edge Cases

### Encoding Error Handling
```csharp
public class StringErrorHandling
{
    public void HandleEncodingErrors()
    {
        try
        {
            // Test with null string
            string nullString = null;
            byte[] result = nullString.ToByteArray(); // Will throw NullReferenceException
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine($"Null string error: {ex.Message}");
        }
        
        try
        {
            // Test with invalid Base64
            string invalidBase64 = "Invalid!!!Base64";
            string result = invalidBase64.FromBase64(); // Will throw FormatException
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Invalid Base64 error: {ex.Message}");
        }
    }
    
    public string SafeBase64Decode(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return string.Empty;
        
        try
        {
            return base64String.FromBase64();
        }
        catch (FormatException)
        {
            Console.WriteLine("Invalid Base64 format");
            return string.Empty;
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Invalid Base64 argument");
            return string.Empty;
        }
    }
    
    public string SafeDecompressString(CompressedObject compressed, 
        CompressedObject.CompressionType type)
    {
        try
        {
            return compressed.DecompressString(type);
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Invalid compression data: {ex.Message}");
            return string.Empty;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Unsupported compression type: {ex.Message}");
            return string.Empty;
        }
    }
}
```

### Unicode and Encoding Edge Cases
```csharp
public class UnicodeHandling
{
    public void TestUnicodeEdgeCases()
    {
        var testCases = new[]
        {
            "Simple ASCII",
            "Café with accents",
            "中文字符",
            "🚀🌟💯 Emojis",
            "𝔘𝔫𝔦𝔠𝔬𝔡𝔢", // Mathematical script
            "\u0000\u0001\u0002", // Control characters
            "Mixed: ASCII + 中文 + 🎉"
        };
        
        foreach (string testCase in testCases)
        {
            try
            {
                // Test round-trip
                byte[] bytes = testCase.ToByteArray();
                string decoded = bytes.FromByteArray();
                bool success = testCase == decoded;
                
                // Test Base64 round-trip
                string base64 = testCase.ToBase64();
                string base64Decoded = base64.FromBase64();
                bool base64Success = testCase == base64Decoded;
                
                Console.WriteLine($"'{testCase}': Bytes={success}, Base64={base64Success}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"'{testCase}': Error - {ex.Message}");
            }
        }
    }
}
```

## Telemetry and Monitoring

### Performance Tracking
```csharp
public class StringTelemetry
{
    public void MonitorStringOperations()
    {
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                if (activity.Source.Name == "RapidStreamer.BuildingBlocks.Application" && 
                    activity.DisplayName.Contains("StringHelper"))
                {
                    Console.WriteLine($"StringHelper Activity: {activity.DisplayName}");
                    Console.WriteLine($"Duration: {activity.Duration.TotalMilliseconds}ms");
                    
                    foreach (var tag in activity.Tags)
                    {
                        Console.WriteLine($"  {tag.Key}: {tag.Value}");
                    }
                }
            }
        };
        
        ActivitySource.AddActivityListener(listener);
        
        // Perform monitored operations
        string testData = "Test data for telemetry monitoring";
        byte[] bytes = testData.ToByteArray();
        string base64 = testData.ToBase64();
        ReadOnlyMemory<byte> memory = testData.ToByteReadOnlyMemory();
    }
}
```

## Testing Strategies

### Unit Tests
```csharp
[Test]
public void ToByteArray_WithValidString_ReturnsUTF8Bytes()
{
    // Arrange
    string testString = "Hello, World!";
    byte[] expected = Encoding.UTF8.GetBytes(testString);
    
    // Act
    byte[] result = testString.ToByteArray();
    
    // Assert
    Assert.That(result, Is.EqualTo(expected));
}

[Test]
public void FromByteArray_WithValidBytes_ReturnsOriginalString()
{
    // Arrange
    string original = "Test string with unicode: 测试";
    byte[] bytes = Encoding.UTF8.GetBytes(original);
    
    // Act
    string result = bytes.FromByteArray();
    
    // Assert
    Assert.That(result, Is.EqualTo(original));
}

[Test]
public void ToBase64_FromBase64_RoundTrip_PreservesOriginal()
{
    // Arrange
    string original = "Round trip test data";
    
    // Act
    string base64 = original.ToBase64();
    string result = base64.FromBase64();
    
    // Assert
    Assert.That(result, Is.EqualTo(original));
}

[Test]
public void DecompressString_WithValidCompressedData_ReturnsOriginalString()
{
    // Arrange
    string original = "Compression test data";
    var compressed = original.ToByteArray().ToCompressed();
    
    // Act
    string result = compressed.DecompressString();
    
    // Assert
    Assert.That(result, Is.EqualTo(original));
}
```

### Property-Based Tests
```csharp
[Test]
public void StringOperations_PropertyBasedTest()
{
    var random = new Random(42);
    
    for (int i = 0; i < 1000; i++)
    {
        // Generate random string
        int length = random.Next(1, 1000);
        var chars = new char[length];
        for (int j = 0; j < length; j++)
        {
            chars[j] = (char)random.Next(32, 127); // Printable ASCII
        }
        string testString = new string(chars);
        
        // Test round-trip properties
        byte[] bytes = testString.ToByteArray();
        string decoded = bytes.FromByteArray();
        Assert.That(decoded, Is.EqualTo(testString), $"Byte round-trip failed for: {testString}");
        
        string base64 = testString.ToBase64();
        string base64Decoded = base64.FromBase64();
        Assert.That(base64Decoded, Is.EqualTo(testString), $"Base64 round-trip failed for: {testString}");
    }
}
```

## Best Practices

### 1. Consistent Encoding
```csharp
// Preferred - Use StringHelper for consistent UTF-8 encoding
byte[] bytes = text.ToByteArray();

// Avoid - Mixed encoding approaches
byte[] bytes1 = Encoding.UTF8.GetBytes(text);
byte[] bytes2 = Encoding.ASCII.GetBytes(text); // Inconsistent
```

### 2. Memory Efficiency
```csharp
// For large strings or memory-sensitive scenarios
ReadOnlyMemory<byte> memory = largeString.ToByteReadOnlyMemory();

// For small strings or when you need a byte array
byte[] bytes = smallString.ToByteArray();
```

### 3. Error Handling
```csharp
public string SafeStringOperation(string input)
{
    if (string.IsNullOrWhiteSpace(input))
        return string.Empty;
    
    try
    {
        return input.ToBase64().FromBase64(); // Round-trip test
    }
    catch (Exception ex)
    {
        Logger.LogError($"String operation failed: {ex.Message}");
        return input; // Return original on error
    }
}
```

## Integration with Other Helpers

### Stream Helper Integration
```csharp
public class StringStreamIntegration
{
    public void ConvertStringThroughStream(string data)
    {
        // Method 1: Direct conversion
        byte[] directBytes = data.ToByteArray();
        
        // Method 2: Through stream
        using Stream stream = data.ToStream(); // StreamHelper
        byte[] streamBytes = stream.ToByteArray(); // StreamHelper
        
        // Verify consistency
        bool match = directBytes.SequenceEqual(streamBytes);
        Console.WriteLine($"Direct vs Stream conversion match: {match}");
    }
}
```

### Serialization Helper Integration
```csharp
public class StringSerializationIntegration
{
    public void CompareSerializationSizes<T>(T obj) where T : notnull
    {
        // Get JSON string
        string jsonString = obj.ToJson();
        
        // Compare different representations
        byte[] stringBytes = jsonString.ToByteArray();
        string base64 = jsonString.ToBase64();
        
        // Direct serialization sizes
        byte[] directJsonBytes = obj.ToJsonBytes();
        string directBase64 = obj.ToJsonBase64();
        
        Console.WriteLine($"String->Bytes: {stringBytes.Length}");
        Console.WriteLine($"String->Base64: {base64.Length}");
        Console.WriteLine($"Direct JSON Bytes: {directJsonBytes.Length}");
        Console.WriteLine($"Direct Base64: {directBase64.Length}");
    }
}
```

## Migration and Upgrades

When upgrading from manual string handling:

```csharp
// Old approach - Manual encoding
private byte[] ConvertToBytes(string str)
{
    return Encoding.UTF8.GetBytes(str);
}

private string ConvertFromBytes(byte[] bytes)
{
    return Encoding.UTF8.GetString(bytes);
}

// New approach - Using StringHelper
private byte[] ConvertToBytes(string str)
{
    return str.ToByteArray();
}

private string ConvertFromBytes(byte[] bytes)
{
    return bytes.FromByteArray();
}
```

## See Also

- [StreamHelper](StreamHelper.md) - Stream manipulation and conversion utilities
- [ObjectHelper](ObjectHelper.md) - Object compression and manipulation
- [JsonHelper](JsonHelper.md) - JSON serialization utilities
- [MessagePackHelper](MessagePackHelper.md) - Binary serialization alternatives
- [CompressedObject](../Objects/CompressedObject.md) - Compression data structure

---

*Part of the RapidStreamer.BuildingBlocks.Application.Helpers namespace - providing essential string manipulation utilities for .NET applications.*