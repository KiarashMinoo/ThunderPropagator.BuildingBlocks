# Size Helper

The `Size` class provides memory footprint calculation utilities for managed objects in .NET applications. It offers accurate memory consumption analysis by recursively traversing object hierarchies and calculating the total memory footprint including all referenced objects.

## Overview

```csharp
public unsafe class Size : DisposableObject
```

`Size` is an unsafe class that extends `DisposableObject` and provides memory footprint calculation capabilities for any .NET managed object. It performs deep memory analysis by examining all fields (including private and protected) and following object references while preventing circular reference issues.

## Key Features

- **Deep Memory Analysis**: Recursively calculates memory footprint including all referenced objects
- **Circular Reference Protection**: Maintains reference tracking to prevent infinite loops
- **Platform-Aware Calculations**: Automatically adjusts pointer size based on platform architecture (32-bit/64-bit)
- **Comprehensive Type Support**: Handles primitives, collections, custom objects, and complex hierarchies
- **Asynchronous Operations**: Provides async methods for non-blocking memory analysis
- **JSON Size Estimation**: Offers JSON serialization size calculation for comparison

## Public API

### Static Properties

```csharp
public static readonly int PointerSize
```
Platform-specific pointer size (4 bytes on 32-bit, 8 bytes on 64-bit systems).

### Static Methods

#### Calculate<T>(T obj)
Calculates the optimistic memory footprint of any managed object.

```csharp
public static Task<long> Calculate<T>(T obj) where T : notnull
```

**Parameters:**
- `obj`: The object to analyze (must be non-null)

**Returns:** Task<long> representing the total memory footprint in bytes

**What's Counted:**
- All instance fields (including auto-generated, private, and protected)
- All referenced objects in the hierarchy
- Base class fields through inheritance chain

**What's NOT Counted:**
- Static fields
- Properties
- Methods and functions
- Member methods

#### CalculateJsonify<T>(T obj)
Calculates the memory footprint of an object when serialized to JSON Base64 format.

```csharp
public static Task<int> CalculateJsonify<T>(T obj) where T : notnull
```

**Parameters:**
- `obj`: The object to serialize and measure

**Returns:** Task<int> representing the JSON Base64 string length

## Memory Calculation Details

### Type-Specific Calculations

The `Size` class handles different types with specific memory calculation strategies:

#### Primitive Types
```csharp
// Examples of primitive type sizes
bool, byte, sbyte => 1 byte
short, ushort => 2 bytes
int, uint => 4 bytes
long, ulong => 8 bytes
float => 4 bytes
double => 8 bytes
decimal => 16 bytes
```

#### Special Types
```csharp
char => Calculated based on string context (1 or 2 bytes)
Enum => 4 bytes (int backing)
Pointer => Platform-specific (4 or 8 bytes)
DateTime => 8 bytes
IntPtr, UIntPtr => Platform-specific pointer size
string => char size * string length
```

#### Collections
For `IEnumerable` types, the class recursively calculates the sum of all contained objects.

#### Complex Objects
For custom classes and structures, the calculation traverses the entire inheritance hierarchy, examining all fields at each level.

## Usage Examples

### Basic Memory Analysis

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<string> Hobbies { get; set; }
}

// Calculate memory footprint
var person = new Person
{
    Name = "John Doe",
    Age = 30,
    Hobbies = new List<string> { "Reading", "Gaming", "Cooking" }
};

long memoryFootprint = await Size.Calculate(person);
Console.WriteLine($"Person object uses {memoryFootprint} bytes");
```

### Comparing Memory Representations

```csharp
public class DataAnalysis
{
    public async Task CompareMemoryUsage<T>(T data) where T : notnull
    {
        // Calculate actual object memory footprint
        long objectSize = await Size.Calculate(data);
        
        // Calculate JSON serialized size
        int jsonSize = await Size.CalculateJsonify(data);
        
        Console.WriteLine($"Object Memory: {objectSize} bytes");
        Console.WriteLine($"JSON Size: {jsonSize} bytes");
        Console.WriteLine($"Compression Ratio: {(double)jsonSize / objectSize:P2}");
    }
}
```

### Memory Profiling for Collections

```csharp
public class MemoryProfiler
{
    public async Task ProfileCollectionGrowth()
    {
        var data = new List<string>();
        
        // Profile memory growth
        for (int i = 0; i < 1000; i += 100)
        {
            // Add 100 items
            for (int j = 0; j < 100; j++)
                data.Add($"Item_{i + j}");
            
            long currentSize = await Size.Calculate(data);
            Console.WriteLine($"Items: {data.Count}, Memory: {currentSize} bytes");
        }
    }
}
```

### Memory Analysis for Hierarchical Objects

```csharp
public class Company
{
    public string Name { get; set; }
    public List<Department> Departments { get; set; }
}

public class Department
{
    public string Name { get; set; }
    public List<Employee> Employees { get; set; }
}

public class Employee
{
    public string Name { get; set; }
    public decimal Salary { get; set; }
    public Employee Manager { get; set; } // Potential circular reference
}

// Analyze complex hierarchy
var company = BuildComplexCompanyStructure();
long totalMemory = await Size.Calculate(company);
Console.WriteLine($"Total company data uses {totalMemory} bytes");
```

## Performance Characteristics

### Memory Analysis Performance
- **Time Complexity**: O(n) where n is the total number of objects in the hierarchy
- **Space Complexity**: O(d) where d is the maximum depth of object hierarchy
- **Reference Tracking**: Uses List<object> to prevent circular references

### Optimization Strategies
```csharp
public class OptimizedMemoryAnalysis
{
    // For repeated analysis, consider caching results
    private readonly Dictionary<Type, long> _typeSizeCache = new();
    
    public async Task<long> AnalyzeWithCaching<T>(T obj) where T : notnull
    {
        var type = typeof(T);
        
        // For simple types, use cached results
        if (type.IsPrimitive || type == typeof(string))
        {
            if (!_typeSizeCache.TryGetValue(type, out long cachedSize))
            {
                cachedSize = await Size.Calculate(obj);
                _typeSizeCache[type] = cachedSize;
            }
            return cachedSize;
        }
        
        // For complex types, always calculate
        return await Size.Calculate(obj);
    }
}
```

## Memory Optimization Use Cases

### 1. Memory Leak Detection
```csharp
public class MemoryLeakDetector
{
    private readonly Dictionary<string, long> _baselineMemory = new();
    
    public async Task EstablishBaseline(string key, object obj)
    {
        _baselineMemory[key] = await Size.Calculate(obj);
    }
    
    public async Task<bool> DetectLeak(string key, object obj, double threshold = 1.5)
    {
        if (!_baselineMemory.TryGetValue(key, out long baseline))
            return false;
        
        long current = await Size.Calculate(obj);
        return current > baseline * threshold;
    }
}
```

### 2. Cache Size Management
```csharp
public class MemoryAwareCache<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _cache = new();
    private readonly long _maxMemoryBytes;
    private long _currentMemoryUsage;
    
    public MemoryAwareCache(long maxMemoryBytes)
    {
        _maxMemoryBytes = maxMemoryBytes;
    }
    
    public async Task<bool> TryAdd(TKey key, TValue value)
    {
        long valueSize = await Size.Calculate(value);
        
        if (_currentMemoryUsage + valueSize > _maxMemoryBytes)
        {
            await EvictLeastRecentlyUsed();
        }
        
        if (_currentMemoryUsage + valueSize <= _maxMemoryBytes)
        {
            _cache[key] = value;
            _currentMemoryUsage += valueSize;
            return true;
        }
        
        return false;
    }
}
```

### 3. Data Structure Optimization
```csharp
public class DataStructureAnalyzer
{
    public async Task CompareImplementations()
    {
        // Compare different data structures
        var list = Enumerable.Range(0, 1000).ToList();
        var array = Enumerable.Range(0, 1000).ToArray();
        var hashSet = Enumerable.Range(0, 1000).ToHashSet();
        
        long listSize = await Size.Calculate(list);
        long arraySize = await Size.Calculate(array);
        long hashSetSize = await Size.Calculate(hashSet);
        
        Console.WriteLine($"List<int>: {listSize} bytes");
        Console.WriteLine($"int[]: {arraySize} bytes");
        Console.WriteLine($"HashSet<int>: {hashSetSize} bytes");
    }
}
```

## Integration with Other Helpers

### JSON Comparison
```csharp
public class SerializationAnalyzer
{
    public async Task AnalyzeSerialization<T>(T data) where T : notnull
    {
        // Memory footprint
        long memorySize = await Size.Calculate(data);
        
        // JSON serialization sizes
        int jsonSize = await Size.CalculateJsonify(data);
        int systemJsonSize = data.ToJson().Length;
        int newtonsoftSize = data.ToNJson().Length;
        
        // MessagePack size
        var messagePackBytes = data.ToMessagePackBytes();
        int messagePackSize = messagePackBytes.Length;
        
        Console.WriteLine($"Memory: {memorySize} bytes");
        Console.WriteLine($"JSON (Base64): {jsonSize} bytes");
        Console.WriteLine($"System.Text.Json: {systemJsonSize} bytes");
        Console.WriteLine($"Newtonsoft.Json: {newtonsoftSize} bytes");
        Console.WriteLine($"MessagePack: {messagePackSize} bytes");
    }
}
```

### Object Helper Integration
```csharp
public class ComprehensiveAnalysis
{
    public async Task AnalyzeObject<T>(T obj) where T : notnull
    {
        // Memory analysis
        long memoryFootprint = await Size.Calculate(obj);
        
        // Object properties
        bool isDisposable = obj.IsDisposable();
        string objectHash = obj.ToObjectHashCode();
        
        // Compression analysis
        var compressed = obj.ToCompressed();
        long compressedSize = await Size.Calculate(compressed);
        
        Console.WriteLine($"Original Memory: {memoryFootprint} bytes");
        Console.WriteLine($"Compressed Memory: {compressedSize} bytes");
        Console.WriteLine($"Is Disposable: {isDisposable}");
        Console.WriteLine($"Object Hash: {objectHash}");
    }
}
```

## Error Handling and Edge Cases

### Null Reference Handling
```csharp
// The Calculate method includes null protection
public async Task HandleNullReferences()
{
    try
    {
        object nullObj = null;
        // This will throw ArgumentNullException
        await Size.Calculate(nullObj);
    }
    catch (ArgumentNullException ex)
    {
        Console.WriteLine($"Null object detected: {ex.Message}");
    }
}
```

### Circular Reference Protection
```csharp
public class CircularReferenceExample
{
    public class Node
    {
        public string Name { get; set; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; } = new();
    }
    
    public async Task TestCircularReferences()
    {
        var parent = new Node { Name = "Parent" };
        var child = new Node { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        
        // Size calculation handles circular references
        long size = await Size.Calculate(parent);
        Console.WriteLine($"Circular structure size: {size} bytes");
    }
}
```

## Testing Strategies

### Unit Testing
```csharp
[Test]
public async Task Size_Calculate_ShouldReturnCorrectMemoryFootprint()
{
    // Arrange
    var testString = "Hello, World!";
    var expectedMinSize = testString.Length * sizeof(char);
    
    // Act
    long actualSize = await Size.Calculate(testString);
    
    // Assert
    Assert.That(actualSize, Is.GreaterThanOrEqualTo(expectedMinSize));
}

[Test]
public async Task Size_CalculateJsonify_ShouldReturnJsonSize()
{
    // Arrange
    var testObject = new { Name = "Test", Value = 42 };
    var expectedJsonLength = testObject.ToJsonBase64().Length;
    
    // Act
    int actualSize = await Size.CalculateJsonify(testObject);
    
    // Assert
    Assert.That(actualSize, Is.EqualTo(expectedJsonLength));
}
```

### Performance Testing
```csharp
[Test]
public async Task Size_Calculate_PerformanceTest()
{
    // Arrange
    var largeCollection = Enumerable.Range(0, 10000)
        .Select(i => new { Id = i, Name = $"Item_{i}" })
        .ToList();
    
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    long size = await Size.Calculate(largeCollection);
    
    // Assert
    stopwatch.Stop();
    Console.WriteLine($"Calculated {size} bytes in {stopwatch.ElapsedMilliseconds}ms");
    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000)); // Should complete within 5 seconds
}
```

## Best Practices

### 1. Use Async/Await Pattern
```csharp
// Preferred - Non-blocking
long size = await Size.Calculate(largeObject);

// Avoid - Blocking
long size = Size.Calculate(largeObject).Result;
```

### 2. Consider Memory Impact
```csharp
// For large objects, consider memory impact of analysis itself
public async Task AnalyzeLargeObjects<T>(T obj) where T : notnull
{
    if (await Size.Calculate(obj) > 100_000_000) // 100MB
    {
        Console.WriteLine("Warning: Analyzing very large object");
    }
}
```

### 3. Dispose Resources
```csharp
// The Size class implements IDisposable through DisposableObject
// Resources are automatically cleaned up, but be aware of the lifecycle
public async Task ProperUsage()
{
    var data = CreateLargeDataStructure();
    long size = await Size.Calculate(data);
    
    // Size class handles its own disposal
    // Focus on disposing your own large objects
    if (data is IDisposable disposable)
        disposable.Dispose();
}
```

## Platform Considerations

### Architecture Awareness
```csharp
public class PlatformAwareAnalysis
{
    public async Task ShowPlatformDifferences()
    {
        Console.WriteLine($"Pointer Size: {Size.PointerSize} bytes");
        Console.WriteLine($"Platform: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
        
        var pointerArray = new IntPtr[100];
        long arraySize = await Size.Calculate(pointerArray);
        Console.WriteLine($"100 pointers use: {arraySize} bytes");
    }
}
```

## Migration and Upgrades

When upgrading from direct memory measurement approaches:

```csharp
// Old approach - Manual calculation
private long CalculateManualSize(object obj)
{
    // Complex manual reflection code
    return 0; // Simplified
}

// New approach - Using Size helper
private async Task<long> CalculateWithHelper(object obj)
{
    return await Size.Calculate(obj);
}
```

## See Also

- [ObjectHelper](ObjectHelper.md) - Object manipulation and utility operations
- [JsonHelper](JsonHelper.md) - JSON serialization for size comparison
- [MessagePackHelper](MessagePackHelper.md) - Binary serialization alternatives
- [Collections](../Collections/README.md) - Collection data structures and their memory characteristics

---

*Part of the RapidStreamer.BuildingBlocks.Application.Helpers namespace - providing essential memory analysis utilities for .NET applications.*