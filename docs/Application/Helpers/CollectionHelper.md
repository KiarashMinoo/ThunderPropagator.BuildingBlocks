# CollectionHelper

The `CollectionHelper` class is a static utility class in the RapidStreamer BuildingBlocks that provides powerful extension methods for working with collections, arrays, and enumerable objects. It offers high-performance operations for filtering, transformation, iteration, and data manipulation with optimized memory usage and telemetry integration.

## Purpose

This helper serves as:
- A high-performance collection manipulation utility
- An extension provider for IEnumerable, arrays, and ArraySegments
- A creator of memory-efficient `LinkedArray<T>` instances through filtering
- A bridge between different collection types and transformation operations
- A telemetry-aware collection processor for performance monitoring

## Key Features

- **High-Performance Filtering**: Creates `LinkedArray<T>` instances without copying data
- **Type Conversion**: Seamless conversion between collection element types
- **Array Splicing**: Efficient chunking of large collections into manageable segments
- **Multiple ForEach Variants**: Optimized iteration for different collection types
- **Memory Optimization**: Uses unsafe code and Span<T> for maximum performance
- **Telemetry Integration**: Built-in activity tracking for performance monitoring
- **Null Safety**: Robust null checking and safe operations

## Methods

### Filter<T>
Creates a memory-efficient `LinkedArray<T>` containing only elements that match the specified predicate.

```csharp
public static LinkedArray<T> Filter<T>(this IEnumerable<T>? enumerable, Func<T, bool> func)
```

**Key Benefits:**
- No data copying - creates index-based references to original array
- Telemetry tracking for performance monitoring
- Returns `LinkedArray<T>.Empty` for empty inputs
- Uses unsafe pointer arithmetic for maximum performance

### Convert<T, TR>
Transforms array elements from one type to another using a conversion function.

```csharp
public static TR[]? Convert<T, TR>(this T[]? array, Func<T, TR> func)
public static IEnumerable<T> Convert<T>(this IEnumerable<IConvertible<T>> enumerable)
```

**Features:**
- Efficient array transformation with pre-allocated result arrays
- Built-in support for `IConvertible<T>` interface
- Null-safe operations returning null for null inputs
- Span-based iteration for optimal performance

### Splice<T>
Splits large collections into smaller chunks of specified size.

```csharp
public static IEnumerable<ArraySegment<T>> Splice<T>(this IEnumerable<T> enumerable, int count)
```

**Capabilities:**
- Lazy evaluation using yield return
- Handles remainder elements in final segment
- Returns ArraySegment<T> for zero-copy chunking
- Automatic sizing for collections smaller than chunk size

### ForEach Variants
Provides optimized iteration methods for different collection types.

```csharp
// For IEnumerable<T>
public static void ForEach<T>(this IEnumerable<T>? collection, Action<T> action)
public static void ForEach<T>(this IEnumerable<T>? collection, Action<int, T> action)

// For Arrays
public static void ForEach<T>(this T[]? array, Action<T> execution)
public static void ForEach<T>(this T[]? array, Action<int, T> execution)

// For ArraySegment<T>
public static void ForEach<T>(this ArraySegment<T> array, Action<T> execution)
public static void ForEach<T>(this ArraySegment<T> array, Action<int, T> execution)
```

### IsEquals<T>
Performs deep equality comparison between two enumerables.

```csharp
public static bool IsEquals<T>(this IEnumerable<T>? enumerable, IEnumerable<T>? other)
```

## Usage Examples

### High-Performance Filtering

```csharp
// Large dataset - no copying occurs
var salesData = LoadMillionSalesRecords();

// Filter creates LinkedArray with index references only
LinkedArray<SalesRecord> highValueSales = salesData.Filter(record => 
    record.Amount > 10000 && record.Date >= DateTime.Today.AddDays(-30));

Console.WriteLine($"Found {highValueSales.Count} high-value sales from {salesData.Length} total records");

// Chain operations efficiently
var topCustomers = highValueSales
    .ForEach(sale => sale.CustomerId)
    .Distinct()
    .Take(10);
```

### Type Conversion Operations

```csharp
// Convert array of strings to integers
var stringNumbers = new[] { "1", "2", "3", "4", "5" };
var integers = stringNumbers.Convert(s => int.Parse(s));

// Convert using IConvertible interface
var convertibleItems = new List<IConvertible<Product>>();
var products = convertibleItems.Convert().ToArray();

// Null-safe conversion
string[]? nullArray = null;
var result = nullArray.Convert(s => s.ToUpper()); // Returns null safely
```

### Array Splicing for Batch Processing

```csharp
var largeDataset = Enumerable.Range(1, 10000).ToArray();

// Process in chunks of 100
foreach (var chunk in largeDataset.Splice(100))
{
    Console.WriteLine($"Processing chunk: {chunk.Offset} to {chunk.Offset + chunk.Count - 1}");
    
    // Process chunk using ArraySegment (no copying)
    chunk.ForEach((index, value) => 
        Console.WriteLine($"Item {index}: {value}"));
}
```

### Optimized Iteration

```csharp
var products = LoadProductCatalog();

// Simple iteration
products.ForEach(product => Console.WriteLine(product.Name));

// Indexed iteration with position tracking
products.ForEach((index, product) => 
    Console.WriteLine($"{index + 1}. {product.Name} - ${product.Price:F2}"));

// ArraySegment iteration (zero-copy)
var segment = new ArraySegment<Product>(products, 100, 50);
segment.ForEach((index, product) => ProcessProduct(index, product));
```

### Collection Comparison

```csharp
var list1 = new[] { 1, 2, 3, 4, 5 };
var list2 = new[] { 1, 2, 3, 4, 5 };
var list3 = new[] { 1, 2, 3, 4, 6 };

bool areEqual1 = list1.IsEquals(list2); // true
bool areEqual2 = list1.IsEquals(list3); // false

// Handles null cases safely
int[]? nullList = null;
bool nullComparison = nullList.IsEquals(null); // true
```

## Real-World Applications

### Data Processing Pipeline

```csharp
public class DataProcessor
{
    public ProcessingResult ProcessLargeDataset(DataRecord[] dataset)
    {
        // Filter active records (no copying)
        var activeRecords = dataset.Filter(record => record.IsActive);
        
        // Process in manageable chunks
        var results = new List<ProcessedData>();
        
        foreach (var chunk in activeRecords.Splice(1000))
        {
            var chunkResults = new ProcessedData[chunk.Count];
            
            chunk.ForEach((index, record) =>
            {
                chunkResults[index] = ProcessSingleRecord(record);
            });
            
            results.AddRange(chunkResults);
        }
        
        return new ProcessingResult { ProcessedItems = results };
    }
}
```

### Inventory Management System

```csharp
public class InventoryManager
{
    public InventoryReport GenerateReport(InventoryItem[] inventory)
    {
        // Find low stock items efficiently
        var lowStockItems = inventory.Filter(item => 
            item.Quantity <= item.MinimumStock && item.IsActive);
        
        // Convert to report DTOs
        var reportItems = lowStockItems.Convert(item => new ReportItemDto
        {
            ProductId = item.Id,
            ProductName = item.Name,
            CurrentStock = item.Quantity,
            MinimumStock = item.MinimumStock,
            ReorderQuantity = item.ReorderQuantity
        });
        
        return new InventoryReport
        {
            LowStockItems = reportItems,
            TotalItemsChecked = inventory.Length,
            LowStockCount = lowStockItems.Count
        };
    }
}
```

### Performance Analytics

```csharp
public class PerformanceAnalyzer
{
    public AnalysisResult AnalyzeMetrics(MetricData[] metrics)
    {
        // Filter recent metrics
        var recentMetrics = metrics.Filter(m => 
            m.Timestamp >= DateTime.UtcNow.AddHours(-24));
        
        // Calculate statistics in chunks
        var statistics = new List<ChunkStatistics>();
        
        foreach (var hourlyChunk in recentMetrics.Splice(60)) // 60 metrics per hour
        {
            var stats = new ChunkStatistics();
            
            hourlyChunk.ForEach((index, metric) =>
            {
                stats.TotalValue += metric.Value;
                stats.Count++;
                
                if (metric.Value > stats.MaxValue)
                    stats.MaxValue = metric.Value;
                    
                if (metric.Value < stats.MinValue || stats.MinValue == 0)
                    stats.MinValue = metric.Value;
            });
            
            stats.AverageValue = stats.TotalValue / stats.Count;
            statistics.Add(stats);
        }
        
        return new AnalysisResult { HourlyStatistics = statistics };
    }
}
```

## Performance Considerations

### Memory Efficiency
- `Filter` operations create `LinkedArray<T>` instances that reference original data without copying
- `Splice` returns `ArraySegment<T>` instances for zero-copy chunking
- ForEach methods use Span<T> and unsafe code for optimal iteration performance

### Telemetry Integration
```csharp
// All major operations include telemetry tracking
var data = largeDataset.Filter(predicate); // Automatically tracked
// Activity: "CollectionHelper_Filter" with array length tag

foreach (var chunk in data.Splice(100)) // Tracked operation
{
    chunk.ForEach(ProcessItem); // Tracked with chunk size
}
```

### Optimization Techniques
- Uses `CollectionsMarshal.AsSpan()` for List<T> operations
- Leverages `MemoryMarshal.GetReference()` for direct memory access
- Employs `Unsafe.Add()` for pointer arithmetic without bounds checking
- Pre-allocates arrays when final size is known

## Thread Safety

- **Static Methods**: All methods are thread-safe as they are stateless
- **Input Collections**: Thread safety depends on the source collection's implementation
- **Concurrent Access**: Multiple threads can safely call these methods simultaneously
- **LinkedArray Creation**: The created `LinkedArray<T>` instances are thread-safe for read operations

## Best Practices

1. **Use Filter for Large Datasets**: Leverage `Filter` instead of LINQ `Where()` for large collections to avoid copying
2. **Batch Processing**: Use `Splice` for processing large datasets in manageable chunks
3. **Type-Specific ForEach**: Choose the appropriate ForEach overload for your collection type
4. **Memory Management**: Be aware that `LinkedArray<T>` holds references to source arrays
5. **Null Safety**: Leverage built-in null checking - methods handle null inputs gracefully

## Error Handling

```csharp
// Safe filtering with null handling
var result = nullableCollection?.Filter(predicate) ?? LinkedArray<T>.Empty;

// Safe conversion with validation
var converted = sourceArray?.Convert(item => 
{
    try 
    {
        return ConvertItem(item);
    }
    catch (Exception ex)
    {
        Logger.LogWarning($"Conversion failed for item: {ex.Message}");
        return defaultValue;
    }
});

// Safe splicing with size validation
var chunks = dataset.Splice(Math.Max(1, chunkSize));
```

## Integration with LinkedArray

The `CollectionHelper` is tightly integrated with the `LinkedArray<T>` system:

```csharp
// Filter creates LinkedArray instances
LinkedArray<Product> filteredProducts = allProducts.Filter(p => p.IsAvailable);

// LinkedArray provides its own ForEach methods
var results = filteredProducts.ForEach(product => new ProductDto
{
    Id = product.Id,
    Name = product.Name,
    Price = product.Price
});

// Chain operations efficiently
var topProducts = inventory
    .Filter(item => item.Category == "Electronics")
    .ForEach(item => item.Product)
    .OrderByDescending(p => p.Rating)
    .Take(10);
```

## Testing

The helper methods are extensively tested for:
- Filtering accuracy and performance
- Type conversion correctness
- Splicing boundary conditions
- ForEach operation completeness
- Null input handling
- Memory efficiency validation

## Related Components

- **[LinkedArray<T>](../Collections/LinkedArray.md)**: Primary return type for Filter operations
- **[IConvertible<T>](../Application/IConvertible.md)**: Interface used by Convert operations
- **[Telemetry](../Application/Telemetry.md)**: Provides activity tracking for performance monitoring
- **[Collections System](../Collections/README.md)**: Part of the broader collection utilities

The `CollectionHelper` provides essential collection manipulation capabilities with a focus on performance, memory efficiency, and developer productivity, making it a cornerstone utility for data processing operations in RapidStreamer applications.