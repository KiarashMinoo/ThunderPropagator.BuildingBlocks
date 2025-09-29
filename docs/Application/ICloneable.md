# ICloneable&lt;T&gt;

The `ICloneable<T>` interface provides a type-safe alternative to the standard .NET `ICloneable` interface, ensuring that cloning operations return strongly-typed objects instead of generic `object` references. This interface is essential for implementing the Clone pattern with full type safety and improved compile-time checking.

## Overview

```csharp
public interface ICloneable<out T>
{
    T Clone();
}
```

The generic `ICloneable<T>` interface addresses the limitations of the built-in `System.ICloneable` interface by providing:
- **Type Safety**: Returns `T` instead of `object`, eliminating the need for casting
- **Covariance**: Uses the `out` keyword to support covariant return types
- **Compile-Time Checking**: Ensures proper type relationships at compile time
- **IntelliSense Support**: Provides better IDE support with strongly-typed return values

## Key Features

### Type Safety
- **Strong Typing**: Returns the specific type `T` instead of `object`
- **No Casting Required**: Eliminates runtime casting and potential `InvalidCastException`
- **Compile-Time Validation**: Catches type mismatches during compilation
- **Generic Constraints**: Can be used with generic type constraints

### Covariance Support
- **Covariant Interface**: Supports inheritance hierarchies with `out T`
- **Flexible Assignments**: Allows assignment to base type references
- **Polymorphic Cloning**: Enables polymorphic cloning scenarios
- **Interface Composition**: Works well with other generic interfaces

### Performance Benefits
- **No Boxing**: Avoids boxing for value types when properly implemented
- **Reduced Allocations**: Eliminates temporary objects from casting operations
- **Optimized Implementations**: Enables compiler optimizations with known types
- **JIT Optimizations**: Better code generation with specific type information

## Usage Examples

### Basic Interface Implementation

```csharp
public class Person : ICloneable<Person>
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public Address Address { get; set; } = new();
    
    public Person Clone()
    {
        return new Person
        {
            FirstName = this.FirstName,
            LastName = this.LastName,
            DateOfBirth = this.DateOfBirth,
            Address = this.Address.Clone() // Deep copy of reference types
        };
    }
    
    // Optional: Also implement ICloneable for compatibility
    object ICloneable.Clone() => Clone();
}

public class Address : ICloneable<Address>
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public Country Country { get; set; } = Country.Unknown;
    
    public Address Clone()
    {
        return new Address
        {
            Street = this.Street,
            City = this.City,
            State = this.State,
            ZipCode = this.ZipCode,
            Country = this.Country // Enum values are copied by value
        };
    }
    
    object ICloneable.Clone() => Clone();
}

public enum Country
{
    Unknown,
    UnitedStates,
    Canada,
    UnitedKingdom,
    Germany,
    France,
    Australia
}
```

### Deep vs Shallow Cloning Implementations

```csharp
public class ShallowCloneExample : ICloneable<ShallowCloneExample>
{
    public string Name { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Shallow clone - shares reference type contents
    public ShallowCloneExample Clone()
    {
        return new ShallowCloneExample
        {
            Name = this.Name,           // String is immutable, safe to share
            Tags = this.Tags,           // Shallow copy - same List instance
            Metadata = this.Metadata    // Shallow copy - same Dictionary instance
        };
    }
    
    object ICloneable.Clone() => Clone();
}

public class DeepCloneExample : ICloneable<DeepCloneExample>
{
    public string Name { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<DeepCloneExample> Children { get; set; } = new();
    
    // Deep clone - creates independent copies
    public DeepCloneExample Clone()
    {
        var clone = new DeepCloneExample
        {
            Name = this.Name,
            Tags = new List<string>(this.Tags),                    // New List with copied elements
            Metadata = new Dictionary<string, object>(this.Metadata), // New Dictionary with copied elements
            Children = new List<DeepCloneExample>()
        };
        
        // Recursively clone children
        foreach (var child in this.Children)
        {
            clone.Children.Add(child.Clone());
        }
        
        return clone;
    }
    
    object ICloneable.Clone() => Clone();
}

public class SmartCloneExample : ICloneable<SmartCloneExample>
{
    public string Id { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public ImmutableList<string> ImmutableTags { get; set; } = ImmutableList<string>.Empty;
    public List<string> MutableTags { get; set; } = new();
    
    // Smart clone - shares immutable data, copies mutable data
    public SmartCloneExample Clone()
    {
        return new SmartCloneExample
        {
            Id = this.Id,
            CreatedAt = this.CreatedAt,
            ImmutableTags = this.ImmutableTags,           // Safe to share - immutable
            MutableTags = new List<string>(this.MutableTags) // Must copy - mutable
        };
    }
    
    object ICloneable.Clone() => Clone();
}
```

### Generic Cloning Utilities

```csharp
public static class CloneExtensions
{
    /// <summary>
    /// Creates a clone of an object if it implements ICloneable&lt;T&gt;
    /// </summary>
    public static T? CloneIfPossible<T>(this T? source) where T : class, ICloneable<T>
    {
        return source?.Clone();
    }
    
    /// <summary>
    /// Creates a clone or returns the original if cloning is not supported
    /// </summary>
    public static T CloneOrOriginal<T>(this T source) where T : ICloneable<T>
    {
        try
        {
            return source.Clone();
        }
        catch
        {
            return source; // Return original if cloning fails
        }
    }
    
    /// <summary>
    /// Clones a collection of cloneable objects
    /// </summary>
    public static List<T> CloneAll<T>(this IEnumerable<T> source) where T : ICloneable<T>
    {
        return source.Select(item => item.Clone()).ToList();
    }
    
    /// <summary>
    /// Creates a deep clone of a collection with cloneable elements
    /// </summary>
    public static ICollection<T> DeepCloneCollection<T>(this ICollection<T> source) where T : ICloneable<T>
    {
        var cloned = new List<T>(source.Count);
        foreach (var item in source)
        {
            cloned.Add(item.Clone());
        }
        return cloned;
    }
    
    /// <summary>
    /// Safely clones a nullable object
    /// </summary>
    public static T? SafeClone<T>(this T? source) where T : class, ICloneable<T>
    {
        return source?.Clone();
    }
}

public class CloneUtilityDemo
{
    public static void DemonstrateCloneExtensions()
    {
        // Create test data
        var person1 = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                Country = Country.UnitedStates
            }
        };
        
        var person2 = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1985, 5, 15),
            Address = new Address
            {
                Street = "456 Oak Ave",
                City = "Other City",
                State = "NY",
                ZipCode = "67890",
                Country = Country.UnitedStates
            }
        };
        
        var people = new List<Person> { person1, person2 };
        
        // Clone individual object
        var clonedPerson = person1.CloneIfPossible();
        Console.WriteLine($"Cloned: {clonedPerson?.FirstName} {clonedPerson?.LastName}");
        
        // Clone collection
        var clonedPeople = people.CloneAll();
        Console.WriteLine($"Cloned {clonedPeople.Count} people");
        
        // Verify independence
        clonedPerson!.FirstName = "Modified";
        Console.WriteLine($"Original: {person1.FirstName}, Clone: {clonedPerson.FirstName}");
        
        clonedPeople[0].LastName = "Changed";
        Console.WriteLine($"Original: {people[0].LastName}, Clone: {clonedPeople[0].LastName}");
    }
}
```

### Polymorphic Cloning with Covariance

```csharp
public abstract class Shape : ICloneable<Shape>
{
    public string Name { get; set; } = "";
    public Color Color { get; set; }
    
    // Abstract clone method - must be implemented by derived classes
    public abstract Shape Clone();
    
    object ICloneable.Clone() => Clone();
}

public class Circle : Shape, ICloneable<Circle>
{
    public double Radius { get; set; }
    
    // Return specific type Circle
    public new Circle Clone()
    {
        return new Circle
        {
            Name = this.Name,
            Color = this.Color,
            Radius = this.Radius
        };
    }
    
    // Implement base class abstract method
    public override Shape Clone() => this.Clone();
}

public class Rectangle : Shape, ICloneable<Rectangle>
{
    public double Width { get; set; }
    public double Height { get; set; }
    
    // Return specific type Rectangle
    public new Rectangle Clone()
    {
        return new Rectangle
        {
            Name = this.Name,
            Color = this.Color,
            Width = this.Width,
            Height = this.Height
        };
    }
    
    // Implement base class abstract method
    public override Shape Clone() => this.Clone();
}

public class Triangle : Shape, ICloneable<Triangle>
{
    public double Base { get; set; }
    public double Height { get; set; }
    
    // Return specific type Triangle
    public new Triangle Clone()
    {
        return new Triangle
        {
            Name = this.Name,
            Color = this.Color,
            Base = this.Base,
            Height = this.Height
        };
    }
    
    // Implement base class abstract method
    public override Shape Clone() => this.Clone();
}

public enum Color
{
    Red,
    Green,
    Blue,
    Yellow,
    Black,
    White
}

public class ShapeProcessor
{
    public List<Shape> ProcessShapes(IEnumerable<Shape> shapes)
    {
        var processedShapes = new List<Shape>();
        
        foreach (var shape in shapes)
        {
            // Clone the shape before processing to avoid modifying original
            var clonedShape = shape.Clone();
            
            // Process the clone
            clonedShape.Name = $"Processed_{clonedShape.Name}";
            
            // Type-specific processing
            switch (clonedShape)
            {
                case Circle circle:
                    ProcessCircle(circle);
                    break;
                case Rectangle rectangle:
                    ProcessRectangle(rectangle);
                    break;
                case Triangle triangle:
                    ProcessTriangle(triangle);
                    break;
            }
            
            processedShapes.Add(clonedShape);
        }
        
        return processedShapes;
    }
    
    private void ProcessCircle(Circle circle)
    {
        // Specific circle processing
        if (circle.Radius < 1.0)
        {
            circle.Radius = 1.0; // Minimum radius
        }
    }
    
    private void ProcessRectangle(Rectangle rectangle)
    {
        // Specific rectangle processing
        if (rectangle.Width <= 0 || rectangle.Height <= 0)
        {
            rectangle.Width = Math.Max(rectangle.Width, 1.0);
            rectangle.Height = Math.Max(rectangle.Height, 1.0);
        }
    }
    
    private void ProcessTriangle(Triangle triangle)
    {
        // Specific triangle processing
        if (triangle.Base <= 0 || triangle.Height <= 0)
        {
            triangle.Base = Math.Max(triangle.Base, 1.0);
            triangle.Height = Math.Max(triangle.Height, 1.0);
        }
    }
}

public class PolymorphicCloneDemo
{
    public static void DemonstratePolymorphicCloning()
    {
        var shapes = new List<Shape>
        {
            new Circle { Name = "Circle1", Color = Color.Red, Radius = 5.0 },
            new Rectangle { Name = "Rect1", Color = Color.Blue, Width = 10.0, Height = 8.0 },
            new Triangle { Name = "Triangle1", Color = Color.Green, Base = 6.0, Height = 4.0 }
        };
        
        var processor = new ShapeProcessor();
        
        // Process shapes (clones them internally)
        var processedShapes = processor.ProcessShapes(shapes);
        
        // Verify original shapes are unchanged
        Console.WriteLine("Original shapes:");
        foreach (var shape in shapes)
        {
            Console.WriteLine($"  {shape.Name} - {shape.Color}");
        }
        
        Console.WriteLine("\nProcessed shapes:");
        foreach (var shape in processedShapes)
        {
            Console.WriteLine($"  {shape.Name} - {shape.Color}");
        }
        
        // Demonstrate covariance
        ICloneable<Shape> shapeCloner = new Circle { Name = "CovariantCircle", Radius = 3.0 };
        Shape clonedShape = shapeCloner.Clone(); // Returns Shape, not Circle
        Console.WriteLine($"\nCovariant clone: {clonedShape.Name} (Type: {clonedShape.GetType().Name})");
    }
}
```

### Performance-Optimized Cloning

```csharp
public class PerformanceOptimizedClone : ICloneable<PerformanceOptimizedClone>
{
    private static readonly object _staticReference = new();
    
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public object SharedReference { get; set; } = _staticReference;
    
    // Optimized clone implementation
    public PerformanceOptimizedClone Clone()
    {
        var clone = new PerformanceOptimizedClone
        {
            Id = this.Id,                    // Value type - direct copy
            Name = this.Name,                // String is immutable - safe to share
            Timestamp = this.Timestamp,      // Value type - direct copy
            SharedReference = this.SharedReference // Shared reference - safe to share
        };
        
        // Only allocate new array if original has data
        if (this.Data.Length > 0)
        {
            clone.Data = new byte[this.Data.Length];
            Array.Copy(this.Data, clone.Data, this.Data.Length);
        }
        
        return clone;
    }
    
    object ICloneable.Clone() => Clone();
}

public class CopyOnWriteClone : ICloneable<CopyOnWriteClone>
{
    private List<string>? _items;
    private bool _isShared;
    
    public List<string> Items
    {
        get
        {
            _items ??= new List<string>();
            return _items;
        }
        private set => _items = value;
    }
    
    public void AddItem(string item)
    {
        if (_isShared && _items != null)
        {
            // Copy-on-write: create new list when modifying shared data
            _items = new List<string>(_items);
            _isShared = false;
        }
        
        Items.Add(item);
    }
    
    public CopyOnWriteClone Clone()
    {
        var clone = new CopyOnWriteClone
        {
            _items = this._items,  // Share the list initially
            _isShared = true       // Mark as shared
        };
        
        this._isShared = true; // Mark original as shared too
        
        return clone;
    }
    
    object ICloneable.Clone() => Clone();
}

public class ClonePerformanceTest
{
    public static async Task<PerformanceBenchmark> BenchmarkCloneMethodsAsync()
    {
        const int iterations = 100_000;
        var benchmark = new PerformanceBenchmark();
        
        // Test 1: Simple clone
        var simpleObject = new Person { FirstName = "Test", LastName = "User" };
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            var clone = simpleObject.Clone();
        }
        
        benchmark.SimpleCloneTime = stopwatch.Elapsed;
        
        // Test 2: Deep clone with collections
        var deepObject = new DeepCloneExample
        {
            Name = "Complex Object",
            Tags = new List<string> { "tag1", "tag2", "tag3" },
            Metadata = new Dictionary<string, object> { ["key1"] = "value1", ["key2"] = 42 }
        };
        
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var clone = deepObject.Clone();
        }
        
        benchmark.DeepCloneTime = stopwatch.Elapsed;
        
        // Test 3: Performance optimized clone
        var optimizedObject = new PerformanceOptimizedClone
        {
            Id = 12345,
            Name = "Optimized Object",
            Data = new byte[1024]
        };
        
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var clone = optimizedObject.Clone();
        }
        
        benchmark.OptimizedCloneTime = stopwatch.Elapsed;
        
        // Test 4: Copy-on-write clone
        var cowObject = new CopyOnWriteClone();
        cowObject.AddItem("item1");
        cowObject.AddItem("item2");
        
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var clone = cowObject.Clone();
        }
        
        benchmark.CopyOnWriteCloneTime = stopwatch.Elapsed;
        benchmark.Iterations = iterations;
        
        return await Task.FromResult(benchmark);
    }
}

public class PerformanceBenchmark
{
    public TimeSpan SimpleCloneTime { get; set; }
    public TimeSpan DeepCloneTime { get; set; }
    public TimeSpan OptimizedCloneTime { get; set; }
    public TimeSpan CopyOnWriteCloneTime { get; set; }
    public int Iterations { get; set; }
    
    public void PrintResults()
    {
        Console.WriteLine($"Clone Performance Benchmark ({Iterations:N0} iterations):");
        Console.WriteLine($"  Simple Clone:      {SimpleCloneTime.TotalMilliseconds:F2} ms ({SimpleCloneTime.Ticks / Iterations:F0} ticks/op)");
        Console.WriteLine($"  Deep Clone:        {DeepCloneTime.TotalMilliseconds:F2} ms ({DeepCloneTime.Ticks / Iterations:F0} ticks/op)");
        Console.WriteLine($"  Optimized Clone:   {OptimizedCloneTime.TotalMilliseconds:F2} ms ({OptimizedCloneTime.Ticks / Iterations:F0} ticks/op)");
        Console.WriteLine($"  Copy-on-Write:     {CopyOnWriteCloneTime.TotalMilliseconds:F2} ms ({CopyOnWriteCloneTime.Ticks / Iterations:F0} ticks/op)");
    }
}
```

### Thread-Safe Cloning

```csharp
public class ThreadSafeCloneable : ICloneable<ThreadSafeCloneable>
{
    private readonly object _lock = new();
    private readonly ConcurrentDictionary<string, object> _properties = new();
    
    public string Name
    {
        get => GetProperty<string>() ?? "";
        set => SetProperty(value);
    }
    
    public int Counter
    {
        get => GetProperty<int>();
        set => SetProperty(value);
    }
    
    private T GetProperty<T>([CallerMemberName] string propertyName = "")
    {
        return _properties.TryGetValue(propertyName, out var value) && value is T typedValue ? typedValue : default!;
    }
    
    private void SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
    {
        _properties[propertyName] = value!;
    }
    
    public ThreadSafeCloneable Clone()
    {
        lock (_lock)
        {
            var clone = new ThreadSafeCloneable();
            
            // Copy all properties atomically
            foreach (var kvp in _properties)
            {
                clone._properties[kvp.Key] = kvp.Value;
            }
            
            return clone;
        }
    }
    
    object ICloneable.Clone() => Clone();
    
    public void UpdateAtomically(string name, int counter)
    {
        lock (_lock)
        {
            Name = name;
            Counter = counter;
        }
    }
}

public class ThreadSafeCloneTest
{
    public static async Task DemonstrateThreadSafeCloning()
    {
        var original = new ThreadSafeCloneable();
        var clones = new ConcurrentBag<ThreadSafeCloneable>();
        var updateTasks = new List<Task>();
        var cloneTasks = new List<Task>();
        
        // Start multiple threads updating the original
        for (int i = 0; i < 5; i++)
        {
            var threadId = i;
            updateTasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    original.UpdateAtomically($"Thread{threadId}_Update{j}", threadId * 100 + j);
                    Thread.Sleep(1);
                }
            }));
        }
        
        // Start multiple threads cloning the original
        for (int i = 0; i < 10; i++)
        {
            cloneTasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 50; j++)
                {
                    var clone = original.Clone();
                    clones.Add(clone);
                    Thread.Sleep(2);
                }
            }));
        }
        
        // Wait for all operations to complete
        await Task.WhenAll(updateTasks.Concat(cloneTasks));
        
        Console.WriteLine($"Created {clones.Count} clones during concurrent updates");
        Console.WriteLine($"Final original state: Name='{original.Name}', Counter={original.Counter}");
        
        // Verify clones are independent
        var sampleClone = clones.FirstOrDefault();
        if (sampleClone != null)
        {
            sampleClone.UpdateAtomically("Modified Clone", 999);
            Console.WriteLine($"Sample clone after modification: Name='{sampleClone.Name}', Counter={sampleClone.Counter}");
            Console.WriteLine($"Original after clone modification: Name='{original.Name}', Counter={original.Counter}");
        }
    }
}
```

## Advanced Implementation Patterns

### Builder Pattern Integration

```csharp
public class ConfigurableObject : ICloneable<ConfigurableObject>
{
    public string Name { get; init; } = "";
    public int Value { get; init; }
    public List<string> Options { get; init; } = new();
    public Dictionary<string, object> Settings { get; init; } = new();
    
    public ConfigurableObject Clone()
    {
        return new ConfigurableObject
        {
            Name = this.Name,
            Value = this.Value,
            Options = new List<string>(this.Options),
            Settings = new Dictionary<string, object>(this.Settings)
        };
    }
    
    object ICloneable.Clone() => Clone();
}

public class ConfigurableObjectBuilder
{
    private readonly ConfigurableObject _template;
    
    public ConfigurableObjectBuilder(ConfigurableObject? template = null)
    {
        _template = template?.Clone() ?? new ConfigurableObject();
    }
    
    public ConfigurableObjectBuilder WithName(string name)
    {
        var clone = _template.Clone();
        return new ConfigurableObjectBuilder(clone with { Name = name });
    }
    
    public ConfigurableObjectBuilder WithValue(int value)
    {
        var clone = _template.Clone();
        return new ConfigurableObjectBuilder(clone with { Value = value });
    }
    
    public ConfigurableObjectBuilder AddOption(string option)
    {
        var clone = _template.Clone();
        clone.Options.Add(option);
        return new ConfigurableObjectBuilder(clone);
    }
    
    public ConfigurableObjectBuilder AddSetting(string key, object value)
    {
        var clone = _template.Clone();
        clone.Settings[key] = value;
        return new ConfigurableObjectBuilder(clone);
    }
    
    public ConfigurableObject Build() => _template.Clone();
}

public class BuilderPatternDemo
{
    public static void DemonstrateBuilderPattern()
    {
        // Create base template
        var baseConfig = new ConfigurableObject
        {
            Name = "BaseConfig",
            Value = 100,
            Options = new List<string> { "option1", "option2" },
            Settings = new Dictionary<string, object> { ["timeout"] = 30 }
        };
        
        // Use builder with template
        var builder = new ConfigurableObjectBuilder(baseConfig);
        
        var config1 = builder
            .WithName("Config1")
            .WithValue(200)
            .AddOption("option3")
            .AddSetting("retries", 3)
            .Build();
        
        var config2 = builder
            .WithName("Config2")
            .WithValue(300)
            .AddSetting("debug", true)
            .Build();
        
        Console.WriteLine($"Base: {baseConfig.Name}, Value: {baseConfig.Value}, Options: {baseConfig.Options.Count}");
        Console.WriteLine($"Config1: {config1.Name}, Value: {config1.Value}, Options: {config1.Options.Count}");
        Console.WriteLine($"Config2: {config2.Name}, Value: {config2.Value}, Options: {config2.Options.Count}");
    }
}
```

### Memento Pattern Integration

```csharp
public class StatefulObject : ICloneable<StatefulObject>
{
    public string State { get; set; } = "";
    public int Version { get; set; }
    public DateTime LastModified { get; set; }
    public List<string> History { get; set; } = new();
    
    public StatefulObject Clone()
    {
        return new StatefulObject
        {
            State = this.State,
            Version = this.Version,
            LastModified = this.LastModified,
            History = new List<string>(this.History)
        };
    }
    
    object ICloneable.Clone() => Clone();
    
    public void UpdateState(string newState)
    {
        History.Add($"v{Version}: {State} -> {newState} at {DateTime.Now:HH:mm:ss}");
        State = newState;
        Version++;
        LastModified = DateTime.UtcNow;
    }
}

public class MementoManager<T> where T : ICloneable<T>
{
    private readonly Stack<T> _snapshots = new();
    private readonly int _maxSnapshots;
    
    public MementoManager(int maxSnapshots = 10)
    {
        _maxSnapshots = maxSnapshots;
    }
    
    public void SaveSnapshot(T obj)
    {
        _snapshots.Push(obj.Clone());
        
        // Limit number of snapshots
        while (_snapshots.Count > _maxSnapshots)
        {
            _snapshots.Pop();
        }
    }
    
    public T? RestoreSnapshot()
    {
        return _snapshots.Count > 0 ? _snapshots.Pop() : default;
    }
    
    public T? PeekSnapshot()
    {
        return _snapshots.Count > 0 ? _snapshots.Peek() : default;
    }
    
    public int SnapshotCount => _snapshots.Count;
    
    public void ClearSnapshots()
    {
        _snapshots.Clear();
    }
}

public class MementoPatternDemo
{
    public static void DemonstrateMementoPattern()
    {
        var obj = new StatefulObject { State = "Initial" };
        var memento = new MementoManager<StatefulObject>();
        
        // Save initial state
        memento.SaveSnapshot(obj);
        Console.WriteLine($"Saved snapshot: {obj.State} (v{obj.Version})");
        
        // Make changes
        obj.UpdateState("Modified1");
        memento.SaveSnapshot(obj);
        Console.WriteLine($"Saved snapshot: {obj.State} (v{obj.Version})");
        
        obj.UpdateState("Modified2");
        memento.SaveSnapshot(obj);
        Console.WriteLine($"Saved snapshot: {obj.State} (v{obj.Version})");
        
        obj.UpdateState("Modified3");
        Console.WriteLine($"Current state: {obj.State} (v{obj.Version})");
        
        // Restore previous states
        Console.WriteLine($"\nSnapshots available: {memento.SnapshotCount}");
        
        var restored1 = memento.RestoreSnapshot();
        if (restored1 != null)
        {
            Console.WriteLine($"Restored: {restored1.State} (v{restored1.Version})");
            Console.WriteLine($"History entries: {restored1.History.Count}");
        }
        
        var restored2 = memento.RestoreSnapshot();
        if (restored2 != null)
        {
            Console.WriteLine($"Restored: {restored2.State} (v{restored2.Version})");
        }
    }
}
```

## Best Practices

### 1. **Deep vs Shallow Cloning Guidelines**

```csharp
public static class CloneBestPractices
{
    /// <summary>
    /// Guidelines for when to use shallow vs deep cloning
    /// </summary>
    public static class Guidelines
    {
        // Shallow cloning is appropriate for:
        // - Immutable reference types (strings, immutable collections)
        // - Shared configuration objects
        // - Objects with expensive-to-copy data that won't be modified
        
        // Deep cloning is required for:
        // - Mutable collections and arrays
        // - Objects that will be modified independently
        // - Objects containing sensitive data that shouldn't be shared
        
        public static void DemonstrateGuidelines()
        {
            var original = new ConfigurationObject
            {
                AppName = "MyApp",                           // Shallow clone OK - string is immutable
                Version = new Version(1, 0, 0),             // Shallow clone OK - Version is immutable
                ImmutableSettings = ImmutableDictionary<string, string>.Empty, // Shallow clone OK - immutable
                MutableSettings = new Dictionary<string, string>(),            // Deep clone required - mutable
                LazyExpensiveData = new Lazy<byte[]>(() => new byte[1000000]) // Shallow clone OK - expensive to recreate
            };
        }
    }
    
    /// <summary>
    /// Performance considerations for clone implementations
    /// </summary>
    public static class PerformanceGuidelines
    {
        public static T OptimizedClone<T>(T source) where T : ICloneable<T>, new()
        {
            // For simple objects, direct property copying is fastest
            // For complex objects, consider using serialization
            // For objects with many reference types, evaluate shallow vs deep cloning needs
            
            return source.Clone(); // Placeholder - actual implementation would vary
        }
        
        public static void AvoidCommonPitfalls()
        {
            // DON'T: Use reflection for every clone operation (slow)
            // DON'T: Clone large objects unnecessarily
            // DON'T: Create deep copies of immutable data
            // DO: Cache expensive computations in cloned objects
            // DO: Use copy-on-write for large collections when appropriate
            // DO: Consider pooling for frequently cloned objects
        }
    }
}

public class ConfigurationObject : ICloneable<ConfigurationObject>
{
    public string AppName { get; set; } = "";
    public Version Version { get; set; } = new(1, 0, 0);
    public ImmutableDictionary<string, string> ImmutableSettings { get; set; } = ImmutableDictionary<string, string>.Empty;
    public Dictionary<string, string> MutableSettings { get; set; } = new();
    public Lazy<byte[]> LazyExpensiveData { get; set; } = null!;
    
    public ConfigurationObject Clone()
    {
        return new ConfigurationObject
        {
            AppName = this.AppName,                                    // Shallow - string is immutable
            Version = this.Version,                                    // Shallow - Version is immutable
            ImmutableSettings = this.ImmutableSettings,               // Shallow - immutable collection
            MutableSettings = new Dictionary<string, string>(this.MutableSettings), // Deep - mutable collection
            LazyExpensiveData = this.LazyExpensiveData                 // Shallow - expensive to recreate
        };
    }
    
    object ICloneable.Clone() => Clone();
}
```

### 2. **Error Handling and Validation**

```csharp
public class RobustCloneable : ICloneable<RobustCloneable>
{
    public string? Name { get; set; }
    public List<string>? Items { get; set; }
    public Stream? DataStream { get; set; }
    
    public RobustCloneable Clone()
    {
        try
        {
            var clone = new RobustCloneable
            {
                Name = this.Name,
                Items = this.Items?.ToList() // Safe null handling
            };
            
            // Handle non-cloneable types gracefully
            if (this.DataStream != null)
            {
                if (this.DataStream.CanSeek)
                {
                    // Clone seekable streams
                    var originalPosition = this.DataStream.Position;
                    this.DataStream.Position = 0;
                    
                    var memoryStream = new MemoryStream();
                    this.DataStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;
                    
                    clone.DataStream = memoryStream;
                    this.DataStream.Position = originalPosition;
                }
                else
                {
                    // For non-seekable streams, create a placeholder or throw
                    throw new InvalidOperationException("Cannot clone non-seekable stream");
                }
            }
            
            return clone;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to clone {nameof(RobustCloneable)}: {ex.Message}", ex);
        }
    }
    
    object ICloneable.Clone() => Clone();
}
```

### 3. **Documentation and Testing**

```csharp
/// <summary>
/// Provides comprehensive documentation for clone implementations
/// </summary>
public class DocumentedCloneable : ICloneable<DocumentedCloneable>
{
    /// <summary>
    /// Creates a deep copy of this DocumentedCloneable instance.
    /// </summary>
    /// <returns>
    /// A new DocumentedCloneable instance with independent copies of all mutable properties.
    /// Immutable properties are shared between the original and clone for performance.
    /// </returns>
    /// <remarks>
    /// Clone behavior:
    /// - Name: Shallow copy (string is immutable)
    /// - Tags: Deep copy (new List with copied elements)
    /// - Metadata: Deep copy (new Dictionary with copied key-value pairs)
    /// - CreatedAt: Value copy (DateTime is a value type)
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the object is in an invalid state for cloning.
    /// </exception>
    public DocumentedCloneable Clone()
    {
        // Implementation with detailed comments...
        throw new NotImplementedException();
    }
    
    object ICloneable.Clone() => Clone();
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class ICloneableTests
{
    [Test]
    public void Clone_SimplePerson_CreatesIndependentCopy()
    {
        // Arrange
        var original = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        
        // Act
        var clone = original.Clone();
        
        // Assert
        Assert.That(clone, Is.Not.SameAs(original));
        Assert.That(clone.FirstName, Is.EqualTo(original.FirstName));
        Assert.That(clone.LastName, Is.EqualTo(original.LastName));
        Assert.That(clone.DateOfBirth, Is.EqualTo(original.DateOfBirth));
    }
    
    [Test]
    public void Clone_PersonWithAddress_CreatesDeepCopy()
    {
        // Arrange
        var original = new Person
        {
            FirstName = "Jane",
            LastName = "Smith",
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown"
            }
        };
        
        // Act
        var clone = original.Clone();
        
        // Modify clone's address
        clone.Address.Street = "456 Oak Ave";
        
        // Assert
        Assert.That(clone.Address, Is.Not.SameAs(original.Address));
        Assert.That(original.Address.Street, Is.EqualTo("123 Main St"));
        Assert.That(clone.Address.Street, Is.EqualTo("456 Oak Ave"));
    }
    
    [Test]
    public void Clone_PolymorphicShape_PreservesType()
    {
        // Arrange
        Shape original = new Circle { Name = "TestCircle", Radius = 5.0 };
        
        // Act
        var clone = original.Clone();
        
        // Assert
        Assert.That(clone, Is.TypeOf<Circle>());
        Assert.That(((Circle)clone).Radius, Is.EqualTo(5.0));
        Assert.That(clone, Is.Not.SameAs(original));
    }
    
    [Test]
    public void Clone_ThreadSafeCloneable_IsThreadSafe()
    {
        // Arrange
        var original = new ThreadSafeCloneable();
        var clones = new ConcurrentBag<ThreadSafeCloneable>();
        var errors = new ConcurrentBag<Exception>();
        
        // Act
        Parallel.For(0, 100, i =>
        {
            try
            {
                original.UpdateAtomically($"Update{i}", i);
                var clone = original.Clone();
                clones.Add(clone);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });
        
        // Assert
        Assert.That(errors, Is.Empty);
        Assert.That(clones.Count, Is.EqualTo(100));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class CloneIntegrationTests
{
    [Test]
    public async Task MementoPattern_WithCloneable_RestoresCorrectState()
    {
        // Arrange
        var obj = new StatefulObject { State = "Initial" };
        var memento = new MementoManager<StatefulObject>();
        
        // Act
        memento.SaveSnapshot(obj);
        obj.UpdateState("Modified");
        obj.UpdateState("MoreModified");
        
        var restored = memento.RestoreSnapshot();
        
        // Assert
        Assert.That(restored, Is.Not.Null);
        Assert.That(restored.State, Is.EqualTo("Initial"));
        Assert.That(restored, Is.Not.SameAs(obj));
    }
    
    [Test]
    public void BuilderPattern_WithCloneable_CreatesIndependentObjects()
    {
        // Arrange
        var baseConfig = new ConfigurableObject { Name = "Base" };
        var builder = new ConfigurableObjectBuilder(baseConfig);
        
        // Act
        var config1 = builder.WithName("Config1").AddOption("opt1").Build();
        var config2 = builder.WithName("Config2").AddOption("opt2").Build();
        
        // Assert
        Assert.That(config1.Name, Is.EqualTo("Config1"));
        Assert.That(config2.Name, Is.EqualTo("Config2"));
        Assert.That(config1.Options.Contains("opt1"), Is.True);
        Assert.That(config1.Options.Contains("opt2"), Is.False);
        Assert.That(config2.Options.Contains("opt2"), Is.True);
        Assert.That(config2.Options.Contains("opt1"), Is.False);
    }
}
```

## See Also

- [System.ICloneable](https://learn.microsoft.com/en-us/dotnet/api/system.icloneable) - Built-in .NET cloning interface
- [IConvertible<T>](IConvertible.md) - Type-safe conversion interface
- [FeederMessage](FeederMessage.md) - Dictionary-based message implementation that uses ICloneable<T>
- [Object.MemberwiseClone](https://learn.microsoft.com/en-us/dotnet/api/system.object.memberwiseclone) - Shallow cloning method
- [Copy Constructors](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/copy-constructors) - Alternative cloning approach

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing type-safe object cloning capabilities with full compile-time type checking and covariant support.*