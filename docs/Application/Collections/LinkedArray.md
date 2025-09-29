# LinkedArray\<T>

`LinkedArray<T>` is a high-performance, readonly collection that provides a unique approach to array access through index-based linking. It allows you to create virtual views of existing arrays by maintaining a list of indices that reference elements in the original array, enabling efficient filtering, reordering, and processing without copying the underlying data.

## Overview

The `LinkedArray<T>` is designed for scenarios where you need to work with subsets or reordered views of large arrays without the memory overhead of copying elements. It provides:
- **Zero-copy operations** - references existing array data without duplication
- **Flexible element selection** - build collections using array indices
- **High-performance enumeration** - optimized with unsafe memory operations and spans
- **Functional programming support** - comprehensive ForEach operations with transformations
- **Immutable semantics** - readonly after construction, thread-safe for concurrent reads
- **Integration with existing arrays** - seamless interoperability with standard array operations

## Key Features

### 1. Index-Based Linking
Elements are accessed through a list of indices that point to positions in the original array.

### 2. Memory Efficiency
No element copying - maintains references to the original array data.

### 3. High-Performance Operations
Utilizes spans, unsafe memory access, and optimized enumeration patterns.

### 4. Functional Transformations
Rich set of ForEach operations supporting both actions and projections.

### 5. Thread-Safe Reads
Immutable after construction, making it safe for concurrent read operations.

## Structure Declaration

```csharp
public readonly struct LinkedArray<T> : 
    IList<T>,
    IReadOnlyList<T>,
    ICollection<T>,
    IReadOnlyCollection<T>
{
    public static LinkedArray<T> Empty { get; } = new([]);
    
    // Read-only properties
    public int Count { get; }
    public bool IsReadOnly => true;
    public T this[int index] { get; }
}
```

## Constructor and Creation

```csharp
// Create from existing array
var sourceArray = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
var linkedArray = new LinkedArray<int>(sourceArray);

// Empty linked array
var emptyArray = LinkedArray<string>.Empty;

// The LinkedArray initially references all elements in order
// linkedArray[0] == 10, linkedArray[1] == 20, etc.
```

## Usage Examples

### Basic Array Operations

```csharp
using RapidStreamer.BuildingBlocks.Application.Collections;

// Original data array
var products = new Product[]
{
    new Product { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics" },
    new Product { Id = 2, Name = "Book", Price = 19.99m, Category = "Education" },
    new Product { Id = 3, Name = "Headphones", Price = 199.99m, Category = "Electronics" },
    new Product { Id = 4, Name = "Pen", Price = 2.99m, Category = "Office" },
    new Product { Id = 5, Name = "Monitor", Price = 299.99m, Category = "Electronics" }
};

// Create LinkedArray from products
var linkedProducts = new LinkedArray<Product>(products);

Console.WriteLine($"Total products: {linkedProducts.Count}");

// Access elements by index (same as original array initially)
Console.WriteLine($"First product: {linkedProducts[0].Name}");
Console.WriteLine($"Last product: {linkedProducts[linkedProducts.Count - 1].Name}");

// Convert back to regular array (creates copy)
Product[] copiedProducts = linkedProducts.ToArray();

// Enumerate all products
foreach (var product in linkedProducts)
{
    Console.WriteLine($"{product.Name}: ${product.Price}");
}
```

### Index-Based Element Selection

```csharp
var numbers = new[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
var linkedNumbers = new LinkedArray<int>(numbers);

// Add specific indices to create custom view
linkedNumbers.Add(2);    // Add element at index 2 (300)
linkedNumbers.Add(0);    // Add element at index 0 (100)
linkedNumbers.Add(4);    // Add element at index 4 (500)

// Now linkedNumbers contains: [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 300, 100, 500]
Console.WriteLine($"Enhanced count: {linkedNumbers.Count}"); // 13

// The LinkedArray now has duplicates of some original elements
var result = linkedNumbers.ToArray();
// result = [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 300, 100, 500]

// Remove specific index references
linkedNumbers.Remove(2); // Remove reference to index 2 (one occurrence of 300)
Console.WriteLine($"After removal: {linkedNumbers.Count}"); // 12

// Check if specific index is referenced
bool containsIndex4 = linkedNumbers.Contains(4); // Check if index 4 is referenced
bool containsValue500 = linkedNumbers.Contains(500); // Check if value 500 exists
```

### Functional Transformations with ForEach

```csharp
var scores = new[] { 85, 92, 78, 96, 88, 74, 91, 83, 97, 79 };
var linkedScores = new LinkedArray<int>(scores);

// Action-based ForEach (side effects)
int sum = 0;
linkedScores.ForEach(score => sum += score);
Console.WriteLine($"Total score: {sum}");

// Action with index
linkedScores.ForEach((index, score) => 
    Console.WriteLine($"Student {index + 1}: {score}"));

// Projection-based ForEach (transformations)
string[] grades = linkedScores.ForEach(score => score switch
{
    >= 90 => "A",
    >= 80 => "B", 
    >= 70 => "C",
    >= 60 => "D",
    _ => "F"
});

Console.WriteLine("Grades: " + string.Join(", ", grades));

// Complex transformation with index
var detailedResults = linkedScores.ForEach((index, score) => new
{
    StudentNumber = index + 1,
    Score = score,
    Grade = score >= 90 ? "A" : score >= 80 ? "B" : score >= 70 ? "C" : "D",
    Status = score >= 70 ? "Pass" : "Fail"
});

foreach (var result in detailedResults)
{
    Console.WriteLine($"Student {result.StudentNumber}: {result.Score} ({result.Grade}) - {result.Status}");
}
```

### Filtering with CollectionHelper Integration

```csharp
// LinkedArray is created by CollectionHelper.Filter extension
var inventory = new InventoryItem[]
{
    new() { Name = "Widget A", Quantity = 15, Price = 10.99m },
    new() { Name = "Widget B", Quantity = 0, Price = 15.99m },
    new() { Name = "Widget C", Quantity = 25, Price = 8.50m },
    new() { Name = "Widget D", Quantity = 3, Price = 22.00m },
    new() { Name = "Widget E", Quantity = 0, Price = 12.75m },
};

// Filter creates a LinkedArray pointing to matching elements
LinkedArray<InventoryItem> inStockItems = inventory.Filter(item => item.Quantity > 0);

Console.WriteLine("In-stock items:");
inStockItems.ForEach(item => Console.WriteLine($"{item.Name}: {item.Quantity} units"));

// Filter by price range
LinkedArray<InventoryItem> midPriceItems = inventory.Filter(item => 
    item.Price >= 10.00m && item.Price <= 20.00m);

// Transform filtered results
var stockReport = midPriceItems.ForEach(item => new
{
    Product = item.Name,
    Status = item.Quantity > 10 ? "Well Stocked" : 
             item.Quantity > 0 ? "Low Stock" : "Out of Stock",
    Value = item.Quantity * item.Price
});

foreach (var report in stockReport)
{
    Console.WriteLine($"{report.Product}: {report.Status} (Value: ${report.Value:F2})");
}

public class InventoryItem
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

### Performance-Critical Data Processing

```csharp
// Large dataset processing without copying
var salesData = GenerateSalesData(100_000); // Large array
var linkedSales = new LinkedArray<SaleRecord>(salesData);

// Process in batches using LinkedArray (no memory copying)
var processingResults = linkedSales.ForEach((index, sale) => new ProcessingResult
{
    BatchNumber = index / 1000,
    SaleId = sale.Id,
    ProcessedValue = CalculateCommission(sale.Amount),
    Timestamp = DateTime.UtcNow
});

// Efficient aggregation
decimal totalCommission = 0;
linkedSales.ForEach(sale => totalCommission += CalculateCommission(sale.Amount));

Console.WriteLine($"Processed {linkedSales.Count} records");
Console.WriteLine($"Total commission: ${totalCommission:F2}");

// Memory-efficient batch processing
const int batchSize = 1000;
for (int batch = 0; batch < linkedSales.Count; batch += batchSize)
{
    ProcessBatch(linkedSales, batch, Math.Min(batchSize, linkedSales.Count - batch));
}

void ProcessBatch(LinkedArray<SaleRecord> sales, int startIndex, int count)
{
    // Process a batch without creating sub-arrays
    for (int i = startIndex; i < startIndex + count && i < sales.Count; i++)
    {
        var sale = sales[i];
        // Process individual sale record
        Console.WriteLine($"Processing sale {sale.Id}: ${sale.Amount}");
    }
}

SaleRecord[] GenerateSalesData(int count)
{
    return Enumerable.Range(1, count)
        .Select(i => new SaleRecord 
        { 
            Id = i, 
            Amount = Random.Shared.Next(100, 10000), 
            Date = DateTime.UtcNow.AddDays(-Random.Shared.Next(365))
        })
        .ToArray();
}

decimal CalculateCommission(decimal amount) => amount * 0.05m;

public class SaleRecord
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class ProcessingResult
{
    public int BatchNumber { get; set; }
    public int SaleId { get; set; }
    public decimal ProcessedValue { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Time Series Data Analysis

```csharp
// Time series data where we need various views without copying
var timeSeriesData = GenerateTimeSeriesData(1000);
var linkedSeries = new LinkedArray<DataPoint>(timeSeriesData);

// Analyze different aspects of the data
AnalyzeTimeSeries(linkedSeries);

void AnalyzeTimeSeries(LinkedArray<DataPoint> data)
{
    Console.WriteLine($"Analyzing {data.Count} data points...\n");
    
    // Calculate moving averages without copying data
    var movingAverages = data.ForEach((index, point) =>
    {
        if (index < 4) return point.Value; // First 5 points use actual value
        
        // Calculate 5-point moving average
        decimal sum = 0;
        for (int i = Math.Max(0, index - 4); i <= index; i++)
        {
            sum += data[i].Value;
        }
        return sum / 5;
    });
    
    // Find peaks and valleys
    var extremePoints = data.ForEach((index, point) => new
    {
        Index = index,
        Timestamp = point.Timestamp,
        Value = point.Value,
        Type = DeterminePointType(data, index)
    }).Where(p => p.Type != "Normal").ToArray();
    
    Console.WriteLine("Extreme Points Found:");
    foreach (var point in extremePoints.Take(10))
    {
        Console.WriteLine($"{point.Timestamp:yyyy-MM-dd HH:mm}: {point.Value:F2} ({point.Type})");
    }
    
    // Statistical analysis
    var stats = CalculateStatistics(data);
    Console.WriteLine($"\nStatistics:");
    Console.WriteLine($"Mean: {stats.Mean:F2}");
    Console.WriteLine($"Min: {stats.Min:F2}");
    Console.WriteLine($"Max: {stats.Max:F2}");
    Console.WriteLine($"Standard Deviation: {stats.StdDev:F2}");
}

string DeterminePointType(LinkedArray<DataPoint> data, int index)
{
    if (index == 0 || index == data.Count - 1) return "Normal";
    
    var current = data[index].Value;
    var prev = data[index - 1].Value;
    var next = data[index + 1].Value;
    
    if (current > prev && current > next) return "Peak";
    if (current < prev && current < next) return "Valley";
    return "Normal";
}

(decimal Mean, decimal Min, decimal Max, decimal StdDev) CalculateStatistics(LinkedArray<DataPoint> data)
{
    decimal sum = 0;
    decimal min = decimal.MaxValue;
    decimal max = decimal.MinValue;
    
    data.ForEach(point =>
    {
        sum += point.Value;
        if (point.Value < min) min = point.Value;
        if (point.Value > max) max = point.Value;
    });
    
    decimal mean = sum / data.Count;
    
    decimal sumSquares = 0;
    data.ForEach(point => sumSquares += (point.Value - mean) * (point.Value - mean));
    decimal stdDev = (decimal)Math.Sqrt((double)(sumSquares / data.Count));
    
    return (mean, min, max, stdDev);
}

DataPoint[] GenerateTimeSeriesData(int count)
{
    var baseTime = DateTime.UtcNow.AddHours(-count);
    return Enumerable.Range(0, count)
        .Select(i => new DataPoint
        {
            Timestamp = baseTime.AddMinutes(i),
            Value = 100 + (decimal)(Math.Sin(i * 0.1) * 20 + Random.Shared.NextDouble() * 10)
        })
        .ToArray();
}

public class DataPoint
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
}
```

### Game Development Scenarios

```csharp
// Game entities where we need different views for rendering, collision, AI, etc.
var gameEntities = CreateGameEntities(1000);
var linkedEntities = new LinkedArray<GameEntity>(gameEntities);

// Different processing passes without copying entity data
ProcessGameFrame(linkedEntities);

void ProcessGameFrame(LinkedArray<GameEntity> entities)
{
    Console.WriteLine($"Processing frame with {entities.Count} entities...\n");
    
    // Update AI for AI-enabled entities only (no copying, just processing)
    entities.ForEach(entity =>
    {
        if (entity.HasAI)
        {
            UpdateAI(entity);
        }
    });
    
    // Physics update for moving entities
    entities.ForEach(entity =>
    {
        if (entity.IsMoving)
        {
            UpdatePhysics(entity);
        }
    });
    
    // Render visible entities in distance order
    var renderList = entities.ForEach((index, entity) => new
    {
        Entity = entity,
        DistanceSquared = CalculateDistanceSquared(entity.Position),
        Index = index
    })
    .Where(item => item.Entity.IsVisible)
    .OrderBy(item => item.DistanceSquared)
    .ToArray();
    
    Console.WriteLine($"Rendering {renderList.Length} visible entities");
    
    // Collision detection for collidable entities
    var collidableEntities = entities.ForEach(entity => entity)
        .Where(e => e.HasCollision)
        .ToArray();
        
    CheckCollisions(collidableEntities);
    
    // Performance statistics
    var stats = entities.ForEach(entity => new
    {
        Type = entity.Type,
        HasAI = entity.HasAI,
        IsVisible = entity.IsVisible,
        IsMoving = entity.IsMoving
    })
    .GroupBy(e => e.Type)
    .Select(g => new
    {
        Type = g.Key,
        Count = g.Count(),
        AIEnabled = g.Count(e => e.HasAI),
        Visible = g.Count(e => e.IsVisible),
        Moving = g.Count(e => e.IsMoving)
    });
    
    Console.WriteLine("\nEntity Statistics:");
    foreach (var stat in stats)
    {
        Console.WriteLine($"{stat.Type}: {stat.Count} total, {stat.AIEnabled} AI, {stat.Visible} visible, {stat.Moving} moving");
    }
}

void UpdateAI(GameEntity entity)
{
    // AI processing logic
    entity.Position = new Vector3(
        entity.Position.X + Random.Shared.Next(-1, 2),
        entity.Position.Y,
        entity.Position.Z + Random.Shared.Next(-1, 2)
    );
}

void UpdatePhysics(GameEntity entity)
{
    // Physics processing logic
    entity.Position = new Vector3(
        entity.Position.X + entity.Velocity.X,
        entity.Position.Y + entity.Velocity.Y,
        entity.Position.Z + entity.Velocity.Z
    );
}

float CalculateDistanceSquared(Vector3 position)
{
    // Distance from camera at origin
    return position.X * position.X + position.Y * position.Y + position.Z * position.Z;
}

void CheckCollisions(GameEntity[] entities)
{
    Console.WriteLine($"Checking collisions for {entities.Length} entities");
    // Collision detection logic
}

GameEntity[] CreateGameEntities(int count)
{
    var types = new[] { "Player", "Enemy", "NPC", "Projectile", "Pickup", "Environment" };
    return Enumerable.Range(0, count)
        .Select(i => new GameEntity
        {
            Id = i,
            Type = types[i % types.Length],
            Position = new Vector3(Random.Shared.Next(-100, 100), 0, Random.Shared.Next(-100, 100)),
            Velocity = new Vector3((float)(Random.Shared.NextDouble() - 0.5), 0, (float)(Random.Shared.NextDouble() - 0.5)),
            HasAI = Random.Shared.NextDouble() > 0.7,
            IsVisible = Random.Shared.NextDouble() > 0.1,
            IsMoving = Random.Shared.NextDouble() > 0.5,
            HasCollision = Random.Shared.NextDouble() > 0.3
        })
        .ToArray();
}

public class GameEntity
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public bool HasAI { get; set; }
    public bool IsVisible { get; set; }
    public bool IsMoving { get; set; }
    public bool HasCollision { get; set; }
}

public struct Vector3
{
    public float X, Y, Z;
    public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
}
```

### Data Pipeline Processing

```csharp
// ETL pipeline where we process large datasets through multiple stages
var rawData = LoadRawData(50000);
var linkedData = new LinkedArray<RawDataRecord>(rawData);

// Multi-stage processing pipeline
var processedData = ProcessDataPipeline(linkedData);

ProcessedData[] ProcessDataPipeline(LinkedArray<RawDataRecord> rawRecords)
{
    Console.WriteLine($"Starting data pipeline with {rawRecords.Count} records...\n");
    
    // Stage 1: Data cleaning and validation
    Console.WriteLine("Stage 1: Data Cleaning");
    var cleaningResults = rawRecords.ForEach((index, record) => new
    {
        Index = index,
        Original = record,
        IsValid = ValidateRecord(record),
        CleanedData = CleanRecord(record)
    });
    
    var validRecords = cleaningResults.Where(r => r.IsValid).ToArray();
    Console.WriteLine($"  Cleaned {validRecords.Length} valid records from {rawRecords.Count}");
    
    // Stage 2: Data transformation
    Console.WriteLine("Stage 2: Data Transformation");
    var transformedRecords = validRecords
        .AsParallel() // Can parallelize because LinkedArray is thread-safe for reads
        .Select(r => TransformRecord(r.CleanedData))
        .ToArray();
    
    Console.WriteLine($"  Transformed {transformedRecords.Length} records");
    
    // Stage 3: Data enrichment
    Console.WriteLine("Stage 3: Data Enrichment");
    var enrichedRecords = transformedRecords
        .Select(r => EnrichRecord(r))
        .ToArray();
    
    Console.WriteLine($"  Enriched {enrichedRecords.Length} records");
    
    // Stage 4: Final processing and aggregation
    Console.WriteLine("Stage 4: Final Processing");
    var finalResults = enrichedRecords
        .GroupBy(r => r.Category)
        .Select(g => new ProcessedData
        {
            Category = g.Key,
            RecordCount = g.Count(),
            TotalValue = g.Sum(r => r.Value),
            AverageValue = g.Average(r => r.Value),
            ProcessedAt = DateTime.UtcNow
        })
        .ToArray();
    
    Console.WriteLine($"  Generated {finalResults.Length} summary records");
    return finalResults;
}

bool ValidateRecord(RawDataRecord record)
{
    return !string.IsNullOrEmpty(record.Id) && 
           record.Value >= 0 && 
           !string.IsNullOrEmpty(record.Category);
}

RawDataRecord CleanRecord(RawDataRecord record)
{
    return new RawDataRecord
    {
        Id = record.Id?.Trim() ?? "",
        Value = Math.Max(0, record.Value),
        Category = record.Category?.Trim().ToUpperInvariant() ?? "UNKNOWN",
        Timestamp = record.Timestamp == default ? DateTime.UtcNow : record.Timestamp
    };
}

TransformedRecord TransformRecord(RawDataRecord raw)
{
    return new TransformedRecord
    {
        Id = raw.Id,
        NormalizedValue = raw.Value / 100m, // Normalize to 0-1 scale
        Category = raw.Category,
        ProcessingScore = CalculateProcessingScore(raw),
        Timestamp = raw.Timestamp
    };
}

EnrichedRecord EnrichRecord(TransformedRecord transformed)
{
    return new EnrichedRecord
    {
        Id = transformed.Id,
        Value = transformed.NormalizedValue,
        Category = transformed.Category,
        Score = transformed.ProcessingScore,
        Grade = DetermineGrade(transformed.ProcessingScore),
        Metadata = GenerateMetadata(transformed),
        Timestamp = transformed.Timestamp
    };
}

decimal CalculateProcessingScore(RawDataRecord record)
{
    // Complex scoring algorithm
    return record.Value * 0.1m + (record.Category?.Length ?? 0) * 0.5m;
}

string DetermineGrade(decimal score)
{
    return score switch
    {
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F"
    };
}

Dictionary<string, object> GenerateMetadata(TransformedRecord record)
{
    return new Dictionary<string, object>
    {
        ["processed_at"] = DateTime.UtcNow,
        ["source_system"] = "RapidStreamer Pipeline",
        ["quality_score"] = Random.Shared.NextDouble(),
        ["category_rank"] = Random.Shared.Next(1, 100)
    };
}

RawDataRecord[] LoadRawData(int count)
{
    var categories = new[] { "ELECTRONICS", "BOOKS", "CLOTHING", "HOME", "SPORTS" };
    return Enumerable.Range(1, count)
        .Select(i => new RawDataRecord
        {
            Id = $"REC_{i:D6}",
            Value = Random.Shared.Next(1, 10000),
            Category = categories[i % categories.Length],
            Timestamp = DateTime.UtcNow.AddMinutes(-Random.Shared.Next(10080)) // Last week
        })
        .ToArray();
}

public class RawDataRecord
{
    public string Id { get; set; } = "";
    public decimal Value { get; set; }
    public string Category { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class TransformedRecord
{
    public string Id { get; set; } = "";
    public decimal NormalizedValue { get; set; }
    public string Category { get; set; } = "";
    public decimal ProcessingScore { get; set; }
    public DateTime Timestamp { get; set; }
}

public class EnrichedRecord
{
    public string Id { get; set; } = "";
    public decimal Value { get; set; }
    public string Category { get; set; } = "";
    public decimal Score { get; set; }
    public string Grade { get; set; } = "";
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class ProcessedData
{
    public string Category { get; set; } = "";
    public int RecordCount { get; set; }
    public decimal TotalValue { get; set; }
    public decimal AverageValue { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

## Advanced Features

### Integration with Collection Operations

```csharp
// LinkedArray works seamlessly with LINQ and other collection operations
var sourceData = Enumerable.Range(1, 100).ToArray();
var linkedData = new LinkedArray<int>(sourceData);

// LINQ operations work directly
var evenNumbers = linkedData.Where(x => x % 2 == 0).ToArray();
var doubled = linkedData.Select(x => x * 2).ToArray();
var sum = linkedData.Sum();

// Custom aggregations
var statistics = linkedData.Aggregate(
    new { Sum = 0, Count = 0, Min = int.MaxValue, Max = int.MinValue },
    (acc, value) => new
    {
        Sum = acc.Sum + value,
        Count = acc.Count + 1,
        Min = Math.Min(acc.Min, value),
        Max = Math.Max(acc.Max, value)
    });

Console.WriteLine($"Statistics: Sum={statistics.Sum}, Count={statistics.Count}, Min={statistics.Min}, Max={statistics.Max}");
```

### Memory and Performance Optimization

```csharp
// LinkedArray provides optimal memory usage for large datasets
var largeDataset = new decimal[1_000_000];
// Fill with data...

// Create LinkedArray - no memory copying
var linkedLargeDataset = new LinkedArray<decimal>(largeDataset);

// Efficient processing with spans (internal optimization)
var processingResults = linkedLargeDataset.ForEach(value => value * 1.05m); // 5% increase

// Memory-efficient batch processing
ProcessInBatches(linkedLargeDataset, batchSize: 10000);

void ProcessInBatches<T>(LinkedArray<T> data, int batchSize)
{
    for (int i = 0; i < data.Count; i += batchSize)
    {
        int currentBatchSize = Math.Min(batchSize, data.Count - i);
        Console.WriteLine($"Processing batch {i / batchSize + 1}: items {i} to {i + currentBatchSize - 1}");
        
        // Process current batch without creating sub-arrays
        for (int j = i; j < i + currentBatchSize; j++)
        {
            var item = data[j];
            // Process individual item
        }
    }
}
```

### Error Handling and Validation

```csharp
public static class LinkedArrayExtensions
{
    public static bool TryGetElement<T>(this LinkedArray<T> array, int index, out T? element)
    {
        element = default;
        
        if (index < 0 || index >= array.Count)
            return false;
            
        try
        {
            element = array[index];
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static LinkedArray<T> SafeFilter<T>(this T[] source, Func<T, bool> predicate)
    {
        try
        {
            return source.Filter(predicate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Filter operation failed: {ex.Message}");
            return LinkedArray<T>.Empty;
        }
    }
    
    public static TR[] SafeForEach<T, TR>(this LinkedArray<T> array, Func<T, TR> selector, TR defaultValue)
    {
        try
        {
            return array.ForEach(selector);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ForEach operation failed: {ex.Message}");
            return new TR[array.Count].Select(_ => defaultValue).ToArray();
        }
    }
}

// Safe usage examples
var testArray = new[] { 1, 2, 3, 4, 5 };
var linkedTest = new LinkedArray<int>(testArray);

// Safe element access
if (linkedTest.TryGetElement(10, out var element))
{
    Console.WriteLine($"Element at index 10: {element}");
}
else
{
    Console.WriteLine("Index 10 is out of range");
}

// Safe filtering
var safeFiltered = testArray.SafeFilter(x => x > 3);
Console.WriteLine($"Safe filtered count: {safeFiltered.Count}");

// Safe transformation with fallback
var safeTransformed = linkedTest.SafeForEach(x => x.ToString(), "ERROR");
Console.WriteLine($"Safe transformed: {string.Join(", ", safeTransformed)}");
```

## Performance Characteristics

### Time Complexity
- **Element Access**: O(1) - direct array indexing
- **Enumeration**: O(n) - optimized with spans and unsafe operations
- **Add/Remove Index**: O(1) amortized - operates on index list
- **Contains/IndexOf**: O(n) - linear search through referenced elements
- **ToArray**: O(n) - creates copy of referenced elements

### Space Complexity
- **Storage**: O(k) where k is the number of referenced indices (not the source array size)
- **Memory Overhead**: Minimal - only stores list of integers plus array reference

### Performance Tips

```csharp
// Prefer ForEach for transformations (optimized internally)
var results = linkedArray.ForEach(item => ProcessItem(item)); // Fast

// Avoid repeated indexer access in loops
// Slower:
for (int i = 0; i < linkedArray.Count; i++)
{
    ProcessItem(linkedArray[i]);
}

// Faster:
linkedArray.ForEach(item => ProcessItem(item));

// Use Count property instead of calling Count() extension
int count = linkedArray.Count; // Fast - property access
// int count = linkedArray.Count(); // Slower - LINQ extension method

// Pre-size result arrays when possible
var knownSize = linkedArray.Count;
var results = new ProcessedItem[knownSize]; // Better than List<T> with unknown size
```

## Best Practices

1. **Use for Large Datasets**: LinkedArray is most beneficial when working with large source arrays where copying would be expensive.

2. **Leverage ForEach Operations**: The ForEach methods are highly optimized and should be preferred over manual enumeration.

3. **Immutable After Construction**: Treat LinkedArray as immutable once created for thread safety and predictable behavior.

4. **Combine with Filter**: Use `CollectionHelper.Filter` to create LinkedArrays from filtered datasets efficiently.

5. **Index Management**: Be careful when adding/removing indices - ensure they're valid for the source array.

6. **Memory Considerations**: Remember that LinkedArray holds a reference to the original array, preventing garbage collection.

## Thread Safety

```csharp
// LinkedArray is thread-safe for concurrent reads
var sharedData = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var linkedSharedData = new LinkedArray<int>(sharedData);

// Safe concurrent reading
Parallel.For(0, Environment.ProcessorCount, threadId =>
{
    // Each thread can safely read from the LinkedArray
    var results = linkedSharedData.ForEach(value => value * threadId);
    Console.WriteLine($"Thread {threadId} processed {results.Length} items");
});

// Note: Modifications to the underlying array affect all LinkedArray instances
// This should be avoided or properly synchronized
```

## Integration Patterns

### Factory Patterns

```csharp
public static class LinkedArrayFactory
{
    public static LinkedArray<T> CreateFromSource<T>(T[] source)
    {
        return new LinkedArray<T>(source ?? throw new ArgumentNullException(nameof(source)));
    }
    
    public static LinkedArray<T> CreateFiltered<T>(T[] source, Func<T, bool> predicate)
    {
        return source.Filter(predicate);
    }
    
    public static LinkedArray<T> CreateFromIndices<T>(T[] source, params int[] indices)
    {
        var result = new LinkedArray<T>(source);
        result.Clear(); // Remove all default indices
        
        foreach (var index in indices)
        {
            if (index >= 0 && index < source.Length)
                result.Add(index);
        }
        
        return result;
    }
}

// Usage
var data = new[] { 10, 20, 30, 40, 50 };
var filtered = LinkedArrayFactory.CreateFiltered(data, x => x > 25);
var specific = LinkedArrayFactory.CreateFromIndices(data, 0, 2, 4);
```

### Dependency Injection

```csharp
// Register LinkedArray factory in DI container
services.AddSingleton<Func<int[], LinkedArray<int>>>(provider => 
    source => new LinkedArray<int>(source));

services.AddScoped<IDataProcessor, LinkedArrayDataProcessor>();

public interface IDataProcessor
{
    ProcessingResult ProcessData(int[] source);
}

public class LinkedArrayDataProcessor : IDataProcessor
{
    private readonly Func<int[], LinkedArray<int>> _linkedArrayFactory;
    
    public LinkedArrayDataProcessor(Func<int[], LinkedArray<int>> linkedArrayFactory)
    {
        _linkedArrayFactory = linkedArrayFactory;
    }
    
    public ProcessingResult ProcessData(int[] source)
    {
        var linkedData = _linkedArrayFactory(source);
        
        var sum = linkedData.ForEach(x => x).Sum();
        var processedValues = linkedData.ForEach(x => x * 2);
        
        return new ProcessingResult
        {
            OriginalCount = source.Length,
            ProcessedCount = processedValues.Length,
            Sum = sum,
            ProcessedSum = processedValues.Sum()
        };
    }
}

public class ProcessingResult
{
    public int OriginalCount { get; set; }
    public int ProcessedCount { get; set; }
    public int Sum { get; set; }
    public int ProcessedSum { get; set; }
}
```

## Related Components

- **[CollectionHelper](../Helpers/CollectionHelper.md)**: Provides the `Filter` extension method that creates LinkedArrays
- **[BindingDictionary](BindingDictionary.md)**: For observable dictionary operations with change notifications
- **[GenericOrderedDictionary](GenericOrderedDictionary.md)**: For type-safe ordered dictionary operations
- **Collections System**: Part of the broader Collections utilities in RapidStreamer BuildingBlocks

The `LinkedArray<T>` provides an efficient, memory-friendly way to work with array subsets and transformations, making it ideal for high-performance data processing scenarios where minimizing memory allocation and copying is crucial.