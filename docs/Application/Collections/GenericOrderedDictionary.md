# GenericOrderedDictionary\<TKey, TValue>

`GenericOrderedDictionary<TKey, TValue>` is a type-safe, generic wrapper around the .NET Framework's `OrderedDictionary` that maintains insertion order while providing strongly-typed access to keys and values. This collection combines the benefits of dictionaries (fast key-based lookup) with the predictable ordering of insertion sequence, making it ideal for scenarios where both key-value access and element order matter.

## Overview

The `GenericOrderedDictionary` addresses the limitation of the standard `OrderedDictionary` class, which only provides object-based access and lacks generic type safety. This implementation provides:
- **Type safety** with generic constraints for both keys and values
- **Insertion order preservation** - elements maintain their order of insertion
- **Dual access patterns** - access by key (like Dictionary) or by index (like List)
- **Full interface compliance** with `IDictionary<TKey, TValue>`, `IOrderedDictionary`, and `IReadOnlyDictionary<TKey, TValue>`
- **Custom equality comparers** support for specialized key comparison logic
- **High-performance operations** with optimized enumeration and bulk operations

## Key Features

### 1. Order Preservation
Unlike standard dictionaries, insertion order is maintained throughout the collection's lifetime.

### 2. Dual Access Modes
- Key-based access: `dictionary[key]`
- Index-based access: `dictionary[index]` (through `IOrderedDictionary`)

### 3. Type Safety
Full generic type constraints prevent runtime casting errors and provide compile-time type checking.

### 4. Custom Equality Comparers
Support for `IOrderedEqualityComparer<TKey>` to customize key comparison logic.

### 5. Performance Optimization
Optimized for both sequential access and bulk operations using spans and unsafe memory operations where appropriate.

## Class Declaration

```csharp
public sealed class GenericOrderedDictionary<TKey, TValue> :
    IOrderedDictionary,
    IDictionary<TKey, TValue>,
    IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
```

## Constructor Options

```csharp
// Basic constructors
var dict1 = new GenericOrderedDictionary<string, int>();
var dict2 = new GenericOrderedDictionary<string, int>(capacity: 100);

// With custom comparer
var dict3 = new GenericOrderedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var dict4 = new GenericOrderedDictionary<string, int>(capacity: 50, StringComparer.OrdinalIgnoreCase);

// From existing collections
var existingDict = new Dictionary<string, int> { ["first"] = 1, ["second"] = 2 };
var dict5 = new GenericOrderedDictionary<string, int>(existingDict);
var dict6 = new GenericOrderedDictionary<string, int>(existingDict, StringComparer.OrdinalIgnoreCase);

// From IEnumerable<KeyValuePair>
var keyValuePairs = new[] 
{ 
    new KeyValuePair<string, int>("alpha", 10),
    new KeyValuePair<string, int>("beta", 20),
    new KeyValuePair<string, int>("gamma", 30)
};
var dict7 = new GenericOrderedDictionary<string, int>(keyValuePairs);
var dict8 = new GenericOrderedDictionary<string, int>(keyValuePairs, StringComparer.OrdinalIgnoreCase);
```

## Interface Support

### Custom Equality Comparer Interface

```csharp
public interface IOrderedEqualityComparer<in TKey> : IEqualityComparer, IEqualityComparer<TKey>
{
    // Combines both generic and non-generic equality comparison
    // Enables type-safe comparison while maintaining compatibility
}
```

## Usage Examples

### Basic Operations with Order Preservation

```csharp
using RapidStreamer.BuildingBlocks.Application.Collections;

// Create ordered dictionary
var processSteps = new GenericOrderedDictionary<string, ProcessStep>();

// Add items - order is preserved
processSteps.Add("initialize", new ProcessStep { Name = "Initialize", Duration = TimeSpan.FromSeconds(5) });
processSteps.Add("validate", new ProcessStep { Name = "Validate Input", Duration = TimeSpan.FromSeconds(2) });
processSteps.Add("process", new ProcessStep { Name = "Process Data", Duration = TimeSpan.FromMinutes(1) });
processSteps.Add("finalize", new ProcessStep { Name = "Finalize", Duration = TimeSpan.FromSeconds(3) });

// Enumerate in insertion order
Console.WriteLine("Process Steps (in order):");
foreach (var step in processSteps)
{
    Console.WriteLine($"{step.Key}: {step.Value.Name} ({step.Value.Duration})");
}
// Output:
// initialize: Initialize (00:00:05)
// validate: Validate Input (00:00:02)
// process: Process Data (00:01:00)
// finalize: Finalize (00:00:03)

// Key-based access
if (processSteps.TryGetValue("validate", out var validationStep))
{
    Console.WriteLine($"Validation step duration: {validationStep.Duration}");
}

// Check order is maintained after modifications
processSteps["process"] = new ProcessStep { Name = "Enhanced Processing", Duration = TimeSpan.FromMinutes(2) };

// Order remains the same, only value is updated
```

### Index-Based Access

```csharp
var menu = new GenericOrderedDictionary<string, MenuItem>();

// Add menu items in specific order
menu.Add("appetizer", new MenuItem("Caesar Salad", 8.99m));
menu.Add("soup", new MenuItem("Tomato Bisque", 6.50m));
menu.Add("main", new MenuItem("Grilled Salmon", 24.99m));
menu.Add("dessert", new MenuItem("Chocolate Cake", 7.99m));

// Access by index (through IOrderedDictionary interface)
var orderedMenu = (IOrderedDictionary)menu;

// Get first and last items
var firstCourse = orderedMenu[0]; // Caesar Salad
var lastCourse = orderedMenu[menu.Count - 1]; // Chocolate Cake

Console.WriteLine($"First course: {((MenuItem)firstCourse!).Name}");
Console.WriteLine($"Last course: {((MenuItem)lastCourse!).Name}");

// Insert at specific position
orderedMenu.Insert(2, "salad", new MenuItem("House Salad", 5.99m));

// Remove by index
orderedMenu.RemoveAt(0); // Removes appetizer

// Final order: soup, salad, main, dessert
foreach (DictionaryEntry entry in orderedMenu)
{
    var item = (MenuItem)entry.Value!;
    Console.WriteLine($"{entry.Key}: {item.Name} - ${item.Price}");
}
```

### Configuration Management with Ordered Settings

```csharp
// Configuration that needs to maintain order for processing
var serverConfig = new GenericOrderedDictionary<string, string>();

// Add configuration in processing order
serverConfig.Add("database_connection", "Server=localhost;Database=MyDB;Trusted_Connection=true;");
serverConfig.Add("cache_settings", "Redis:localhost:6379");
serverConfig.Add("logging_level", "Information");
serverConfig.Add("feature_flags", "EnableNewUI=true;EnableBetaFeature=false");
serverConfig.Add("security_settings", "JwtSecret=MySecretKey;TokenExpiry=3600");

// Process configuration in order
Console.WriteLine("Initializing server configuration:");
foreach (var config in serverConfig)
{
    Console.WriteLine($"Configuring {config.Key}...");
    ApplyConfiguration(config.Key, config.Value);
}

// Quick access to specific settings
if (serverConfig.TryGetValue("database_connection", out var dbConnection))
{
    Console.WriteLine($"Using database: {dbConnection}");
}

void ApplyConfiguration(string key, string value)
{
    // Simulate configuration application
    switch (key)
    {
        case "database_connection":
            Console.WriteLine("  ✓ Database connection established");
            break;
        case "cache_settings":
            Console.WriteLine("  ✓ Cache configured");
            break;
        case "logging_level":
            Console.WriteLine("  ✓ Logging level set");
            break;
        case "feature_flags":
            Console.WriteLine("  ✓ Feature flags applied");
            break;
        case "security_settings":
            Console.WriteLine("  ✓ Security settings configured");
            break;
    }
}
```

### HTTP Headers Management

```csharp
// HTTP headers where order matters (some servers are sensitive to header order)
var requestHeaders = new GenericOrderedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

// Add headers in specific order for compatibility
requestHeaders.Add("Host", "api.example.com");
requestHeaders.Add("User-Agent", "MyApp/1.0");
requestHeaders.Add("Accept", "application/json");
requestHeaders.Add("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
requestHeaders.Add("Content-Type", "application/json");
requestHeaders.Add("Content-Length", "256");

// Build HTTP request maintaining header order
var httpRequest = new StringBuilder();
httpRequest.AppendLine("POST /api/data HTTP/1.1");

foreach (var header in requestHeaders)
{
    httpRequest.AppendLine($"{header.Key}: {header.Value}");
}

Console.WriteLine("HTTP Request:");
Console.WriteLine(httpRequest.ToString());

// Case-insensitive access works due to comparer
if (requestHeaders.TryGetValue("AUTHORIZATION", out var authHeader))
{
    Console.WriteLine($"Found authorization header: {authHeader}");
}

// Update specific header while maintaining order
requestHeaders["Content-Length"] = "512"; // Order preserved, only value changes
```

### Form Fields Processing

```csharp
// Web form fields where order affects processing logic
var formFields = new GenericOrderedDictionary<string, FormField>();

// Add form fields in tab order
formFields.Add("firstName", new FormField { Label = "First Name", Required = true, Value = "" });
formFields.Add("lastName", new FormField { Label = "Last Name", Required = true, Value = "" });
formFields.Add("email", new FormField { Label = "Email", Required = true, Value = "" });
formFields.Add("phone", new FormField { Label = "Phone", Required = false, Value = "" });
formFields.Add("address", new FormField { Label = "Address", Required = false, Value = "" });
formFields.Add("city", new FormField { Label = "City", Required = false, Value = "" });
formFields.Add("zipCode", new FormField { Label = "ZIP Code", Required = false, Value = "" });

// Validate fields in order (early exit on first error)
bool isValid = ValidateFormInOrder(formFields);

// Generate form HTML in correct tab order
string formHtml = GenerateFormHtml(formFields);

bool ValidateFormInOrder(GenericOrderedDictionary<string, FormField> fields)
{
    foreach (var field in fields)
    {
        if (field.Value.Required && string.IsNullOrEmpty(field.Value.Value))
        {
            Console.WriteLine($"Validation failed: {field.Value.Label} is required");
            return false;
        }
        
        if (field.Key == "email" && !IsValidEmail(field.Value.Value))
        {
            Console.WriteLine($"Validation failed: Invalid email format");
            return false;
        }
    }
    return true;
}

string GenerateFormHtml(GenericOrderedDictionary<string, FormField> fields)
{
    var html = new StringBuilder("<form>");
    
    foreach (var field in fields)
    {
        var required = field.Value.Required ? " required" : "";
        html.AppendLine($"""
            <div class="form-group">
                <label for="{field.Key}">{field.Value.Label}</label>
                <input type="text" id="{field.Key}" name="{field.Key}" value="{field.Value.Value}"{required}>
            </div>
            """);
    }
    
    html.AppendLine("</form>");
    return html.ToString();
}

bool IsValidEmail(string? email) => !string.IsNullOrEmpty(email) && email.Contains("@");

public class FormField
{
    public string Label { get; set; } = "";
    public bool Required { get; set; }
    public string? Value { get; set; }
}
```

### Recipe Steps Management

```csharp
// Cooking recipe where step order is critical
var recipeSteps = new GenericOrderedDictionary<string, RecipeStep>();

// Add steps in cooking order
recipeSteps.Add("prep", new RecipeStep 
{ 
    Description = "Preheat oven to 350°F and prepare ingredients", 
    Duration = TimeSpan.FromMinutes(10),
    Temperature = 350
});

recipeSteps.Add("mix_dry", new RecipeStep 
{ 
    Description = "Mix flour, baking powder, and salt in large bowl", 
    Duration = TimeSpan.FromMinutes(2)
});

recipeSteps.Add("mix_wet", new RecipeStep 
{ 
    Description = "In separate bowl, whisk eggs, milk, and melted butter", 
    Duration = TimeSpan.FromMinutes(3)
});

recipeSteps.Add("combine", new RecipeStep 
{ 
    Description = "Gradually add wet ingredients to dry, mix until just combined", 
    Duration = TimeSpan.FromMinutes(2)
});

recipeSteps.Add("bake", new RecipeStep 
{ 
    Description = "Pour into greased pan and bake for 25-30 minutes", 
    Duration = TimeSpan.FromMinutes(30),
    Temperature = 350
});

recipeSteps.Add("cool", new RecipeStep 
{ 
    Description = "Cool completely before removing from pan", 
    Duration = TimeSpan.FromMinutes(15)
});

// Display recipe in correct order
Console.WriteLine("Recipe Instructions:");
int stepNumber = 1;
TimeSpan totalTime = TimeSpan.Zero;

foreach (var step in recipeSteps)
{
    var tempInfo = step.Value.Temperature.HasValue ? $" (at {step.Value.Temperature}°F)" : "";
    Console.WriteLine($"{stepNumber}. {step.Value.Description}{tempInfo}");
    Console.WriteLine($"   Time: {step.Value.Duration.TotalMinutes} minutes");
    totalTime = totalTime.Add(step.Value.Duration);
    stepNumber++;
}

Console.WriteLine($"\nTotal preparation time: {totalTime.TotalMinutes} minutes");

// Quick access to specific steps
if (recipeSteps.TryGetValue("bake", out var bakingStep))
{
    Console.WriteLine($"\nBaking temperature: {bakingStep.Temperature}°F");
    Console.WriteLine($"Baking time: {bakingStep.Duration.TotalMinutes} minutes");
}

public class RecipeStep
{
    public string Description { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public int? Temperature { get; set; }
}
```

### Build Pipeline Configuration

```csharp
// CI/CD pipeline steps where execution order is crucial
var buildPipeline = new GenericOrderedDictionary<string, PipelineStep>();

// Define pipeline steps in execution order
buildPipeline.Add("checkout", new PipelineStep
{
    Name = "Source Checkout",
    Command = "git checkout main",
    ContinueOnError = false,
    Timeout = TimeSpan.FromMinutes(5)
});

buildPipeline.Add("restore", new PipelineStep
{
    Name = "Restore Dependencies", 
    Command = "dotnet restore",
    ContinueOnError = false,
    Timeout = TimeSpan.FromMinutes(10)
});

buildPipeline.Add("build", new PipelineStep
{
    Name = "Build Solution",
    Command = "dotnet build --no-restore --configuration Release",
    ContinueOnError = false,
    Timeout = TimeSpan.FromMinutes(15)
});

buildPipeline.Add("test", new PipelineStep
{
    Name = "Run Tests",
    Command = "dotnet test --no-build --configuration Release --logger trx",
    ContinueOnError = true, // Continue even if tests fail
    Timeout = TimeSpan.FromMinutes(30)
});

buildPipeline.Add("publish", new PipelineStep
{
    Name = "Publish Artifacts",
    Command = "dotnet publish --no-build --configuration Release --output ./artifacts",
    ContinueOnError = false,
    Timeout = TimeSpan.FromMinutes(10)
});

buildPipeline.Add("deploy", new PipelineStep
{
    Name = "Deploy to Staging",
    Command = "azure webapp deploy --resource-group MyRG --name MyApp --src-path ./artifacts",
    ContinueOnError = false,
    Timeout = TimeSpan.FromMinutes(20)
});

// Execute pipeline in order
await ExecutePipelineAsync(buildPipeline);

async Task ExecutePipelineAsync(GenericOrderedDictionary<string, PipelineStep> pipeline)
{
    Console.WriteLine("Starting build pipeline...\n");
    
    foreach (var step in pipeline)
    {
        Console.WriteLine($"Executing: {step.Value.Name}");
        Console.WriteLine($"Command: {step.Value.Command}");
        
        try
        {
            // Simulate step execution
            await SimulateStepExecution(step.Value);
            Console.WriteLine($"✓ {step.Key} completed successfully\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ {step.Key} failed: {ex.Message}");
            
            if (!step.Value.ContinueOnError)
            {
                Console.WriteLine("Pipeline aborted due to critical failure");
                break;
            }
            
            Console.WriteLine("Continuing pipeline (non-critical failure)\n");
        }
    }
}

async Task SimulateStepExecution(PipelineStep step)
{
    // Simulate work
    await Task.Delay(1000);
    
    // Simulate occasional test failures
    if (step.Name == "Run Tests" && Random.Shared.NextDouble() < 0.3)
    {
        throw new Exception("Some tests failed");
    }
}

public class PipelineStep
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public bool ContinueOnError { get; set; }
    public TimeSpan Timeout { get; set; }
}
```

### Database Migration Scripts

```csharp
// Database migrations where execution order is critical
var migrations = new GenericOrderedDictionary<string, Migration>();

// Add migrations in chronological order
migrations.Add("001_initial_schema", new Migration
{
    Version = "001",
    Description = "Create initial database schema",
    UpScript = "CREATE TABLE Users (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100), Email NVARCHAR(255));",
    DownScript = "DROP TABLE Users;"
});

migrations.Add("002_add_timestamps", new Migration
{
    Version = "002", 
    Description = "Add CreatedAt and UpdatedAt columns",
    UpScript = "ALTER TABLE Users ADD CreatedAt DATETIME2 DEFAULT GETUTCDATE(), UpdatedAt DATETIME2;",
    DownScript = "ALTER TABLE Users DROP COLUMN CreatedAt, UpdatedAt;"
});

migrations.Add("003_add_indexes", new Migration
{
    Version = "003",
    Description = "Add indexes for performance",
    UpScript = "CREATE INDEX IX_Users_Email ON Users(Email); CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);",
    DownScript = "DROP INDEX IX_Users_Email ON Users; DROP INDEX IX_Users_CreatedAt ON Users;"
});

migrations.Add("004_add_user_roles", new Migration
{
    Version = "004",
    Description = "Add user roles table and foreign key",
    UpScript = """
        CREATE TABLE Roles (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(50));
        ALTER TABLE Users ADD RoleId INT;
        ALTER TABLE Users ADD CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id);
        """,
    DownScript = """
        ALTER TABLE Users DROP CONSTRAINT FK_Users_Roles;
        ALTER TABLE Users DROP COLUMN RoleId;
        DROP TABLE Roles;
        """
});

// Apply migrations in order
ApplyMigrations(migrations, "003"); // Apply up to version 003

// Rollback migrations in reverse order
RollbackMigrations(migrations, "002"); // Rollback to version 002

void ApplyMigrations(GenericOrderedDictionary<string, Migration> migrationList, string targetVersion)
{
    Console.WriteLine($"Applying migrations up to version {targetVersion}...\n");
    
    foreach (var migration in migrationList)
    {
        Console.WriteLine($"Applying migration {migration.Value.Version}: {migration.Value.Description}");
        Console.WriteLine($"SQL: {migration.Value.UpScript}");
        
        // Execute migration.Value.UpScript against database
        Console.WriteLine("✓ Migration applied successfully\n");
        
        if (migration.Value.Version == targetVersion)
            break;
    }
}

void RollbackMigrations(GenericOrderedDictionary<string, Migration> migrationList, string targetVersion)
{
    Console.WriteLine($"Rolling back migrations to version {targetVersion}...\n");
    
    // Convert to array and reverse for rollback
    var migrationsArray = migrationList.ToArray();
    Array.Reverse(migrationsArray);
    
    bool startRollback = false;
    foreach (var migration in migrationsArray)
    {
        // Start rolling back after we pass the target version
        if (!startRollback && string.Compare(migration.Value.Version, targetVersion) > 0)
        {
            startRollback = true;
        }
        
        if (startRollback && string.Compare(migration.Value.Version, targetVersion) > 0)
        {
            Console.WriteLine($"Rolling back migration {migration.Value.Version}: {migration.Value.Description}");
            Console.WriteLine($"SQL: {migration.Value.DownScript}");
            
            // Execute migration.Value.DownScript against database
            Console.WriteLine("✓ Migration rolled back successfully\n");
        }
    }
}

public class Migration
{
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string UpScript { get; set; } = "";
    public string DownScript { get; set; } = "";
}
```

## Advanced Features

### Custom Equality Comparer

```csharp
// Custom comparer for case-insensitive string keys with custom ordering
public class CustomStringComparer : IOrderedEqualityComparer<string>
{
    public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    public int GetHashCode(string obj) => obj.ToUpperInvariant().GetHashCode();
    
    // Non-generic IEqualityComparer implementation
    bool IEqualityComparer.Equals(object? x, object? y) => 
        x is string sx && y is string sy && Equals(sx, sy);
    int IEqualityComparer.GetHashCode(object obj) => 
        obj is string s ? GetHashCode(s) : 0;
}

// Usage with custom comparer
var customDict = new GenericOrderedDictionary<string, string>(new CustomStringComparer());
customDict.Add("Apple", "Red fruit");
customDict.Add("BANANA", "Yellow fruit");

// Case-insensitive access
bool found = customDict.TryGetValue("apple", out var appleDescription); // true
Console.WriteLine($"Apple: {appleDescription}"); // "Red fruit"
```

### Bulk Operations and Performance

```csharp
// Efficient bulk loading
var largeDataset = new GenericOrderedDictionary<string, DataRecord>(capacity: 10000);

// Bulk insert from database results (maintains order from query)
var databaseResults = GetOrderedResultsFromDatabase();
var bulkDict = new GenericOrderedDictionary<string, DataRecord>(databaseResults);

// Efficient enumeration using spans (internal optimization)
foreach (var record in bulkDict)
{
    ProcessRecord(record.Key, record.Value);
}

// Copy operations maintain order
var targetArray = new KeyValuePair<string, DataRecord>[bulkDict.Count];
bulkDict.CopyTo(targetArray, 0);

DataRecord[] GetOrderedResultsFromDatabase()
{
    // Simulate ordered database results
    return Enumerable.Range(1, 1000)
        .Select(i => new KeyValuePair<string, DataRecord>(
            $"record_{i:D6}", 
            new DataRecord { Id = i, Name = $"Record {i}", Timestamp = DateTime.UtcNow.AddMinutes(-i) }))
        .ToArray();
}

void ProcessRecord(string key, DataRecord record)
{
    // Process individual records
    Console.WriteLine($"Processing {key}: {record.Name}");
}

public class DataRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Timestamp { get; set; }
}
```

### Integration with LINQ

```csharp
var inventory = new GenericOrderedDictionary<string, InventoryItem>();

// Add items in priority order
inventory.Add("critical", new InventoryItem { Name = "Critical Component", Stock = 5, Priority = 1 });
inventory.Add("important", new InventoryItem { Name = "Important Part", Stock = 15, Priority = 2 });
inventory.Add("normal", new InventoryItem { Name = "Standard Item", Stock = 100, Priority = 3 });
inventory.Add("low", new InventoryItem { Name = "Low Priority", Stock = 500, Priority = 4 });

// LINQ operations maintain enumeration order
var lowStockItems = inventory
    .Where(kvp => kvp.Value.Stock < 20)
    .Select(kvp => new { Key = kvp.Key, Item = kvp.Value })
    .ToList();

Console.WriteLine("Low stock items (in priority order):");
foreach (var item in lowStockItems)
{
    Console.WriteLine($"{item.Key}: {item.Item.Name} (Stock: {item.Item.Stock})");
}

// Group by priority while maintaining order within groups
var priorityGroups = inventory
    .GroupBy(kvp => kvp.Value.Priority)
    .OrderBy(g => g.Key)
    .ToList();

foreach (var group in priorityGroups)
{
    Console.WriteLine($"\nPriority {group.Key} items:");
    foreach (var item in group)
    {
        Console.WriteLine($"  {item.Key}: {item.Value.Name}");
    }
}

public class InventoryItem
{
    public string Name { get; set; } = "";
    public int Stock { get; set; }
    public int Priority { get; set; }
}
```

## Performance Considerations

### Memory Usage

```csharp
// Pre-allocate capacity for known dataset sizes
var knownSizeDict = new GenericOrderedDictionary<string, object>(capacity: 1000);

// Use appropriate key types - avoid boxing
var intKeyDict = new GenericOrderedDictionary<int, string>(); // Good
var objectKeyDict = new GenericOrderedDictionary<object, string>(); // Avoid if possible
```

### Enumeration Performance

```csharp
// Most efficient enumeration pattern
foreach (var kvp in dictionary)
{
    // Direct access to KeyValuePair - no additional allocations
    ProcessItem(kvp.Key, kvp.Value);
}

// Avoid separate Keys/Values enumeration if you need both
// Less efficient:
foreach (var key in dictionary.Keys)
{
    var value = dictionary[key]; // Additional lookup
    ProcessItem(key, value);
}
```

### Index vs Key Access Trade-offs

```csharp
// Key-based access: O(1) average case
var value = dictionary["myKey"];

// Index-based access: O(n) - requires enumeration
var orderedDict = (IOrderedDictionary)dictionary;
var firstValue = orderedDict[0]; // Less efficient for large collections

// Use index access sparingly, prefer key access for performance
```

## Error Handling

```csharp
public static class OrderedDictionaryExtensions
{
    public static bool TryAdd<TKey, TValue>(
        this GenericOrderedDictionary<TKey, TValue> dict, 
        TKey key, 
        TValue value) where TKey : notnull
    {
        try
        {
            dict.Add(key, value);
            return true;
        }
        catch (ArgumentException)
        {
            // Key already exists
            return false;
        }
    }

    public static (bool Success, TValue? Value) TryGetSafe<TKey, TValue>(
        this GenericOrderedDictionary<TKey, TValue> dict,
        TKey key) where TKey : notnull
    {
        try
        {
            if (dict.TryGetValue(key, out var value))
            {
                return (true, value);
            }
            return (false, default);
        }
        catch (Exception)
        {
            return (false, default);
        }
    }
}

// Safe usage
var dict = new GenericOrderedDictionary<string, Customer>();
bool added = dict.TryAdd("CUST001", new Customer { Id = "CUST001" });
var (success, customer) = dict.TryGetSafe("CUST001");
```

## Best Practices

1. **Choose Appropriate Key Types**: Use value types or immutable reference types as keys to prevent modification issues.

2. **Pre-size Collections**: When the approximate size is known, specify capacity in the constructor to reduce internal resizing.

3. **Use Custom Comparers Wisely**: Only use custom equality comparers when necessary, as they can impact performance.

4. **Leverage Order Preservation**: Design APIs that take advantage of the guaranteed insertion order for cleaner code.

5. **Index Access Sparingly**: Use index-based access only when necessary, as it's less efficient than key-based access.

6. **Thread Safety**: This collection is not thread-safe. Use external synchronization for multi-threaded scenarios.

## Integration Patterns

### Factory Pattern for Configuration-Based Creation

```csharp
public static class OrderedDictionaryFactory
{
    public static GenericOrderedDictionary<string, T> CreateCaseInsensitive<T>(int capacity = 0)
    {
        return capacity > 0 
            ? new GenericOrderedDictionary<string, T>(capacity, StringComparer.OrdinalIgnoreCase)
            : new GenericOrderedDictionary<string, T>(StringComparer.OrdinalIgnoreCase);
    }

    public static GenericOrderedDictionary<TKey, TValue> CreateFromConfig<TKey, TValue>(
        IConfiguration config,
        string sectionName) where TKey : notnull
    {
        var section = config.GetSection(sectionName);
        var capacity = section.GetValue<int>("Capacity", 0);
        
        return capacity > 0 
            ? new GenericOrderedDictionary<TKey, TValue>(capacity)
            : new GenericOrderedDictionary<TKey, TValue>();
    }
}
```

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<GenericOrderedDictionary<string, ServiceConfiguration>>();
services.AddScoped(provider => 
    OrderedDictionaryFactory.CreateCaseInsensitive<ApplicationSetting>(100));
```

## Related Components

- **[BindingDictionary](BindingDictionary.md)**: For observable dictionary operations with change notifications
- **[LinkedArray](LinkedArray.md)**: For ordered array operations with linked access patterns
- **Collections System**: Part of the broader Collections utilities in RapidStreamer BuildingBlocks

The `GenericOrderedDictionary<TKey, TValue>` provides a type-safe, order-preserving dictionary implementation that combines the fast key-based access of dictionaries with the predictable ordering of lists, making it ideal for scenarios where both lookup performance and element order are important.