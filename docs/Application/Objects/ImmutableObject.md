# ImmutableObject

The `ImmutableObject` abstract class enforces immutability constraints at runtime while providing value-based equality through inheritance from `EquatableObject`. It validates that derived classes have no public fields or settable properties, ensuring true immutability with performance optimizations for hash code and atomic value caching.

## Overview

```csharp
public abstract class ImmutableObject<TImmutableObject> : EquatableObject<TImmutableObject>
    where TImmutableObject : ImmutableObject<TImmutableObject>

public abstract class ImmutableObject : EquatableObject<ImmutableObject>
```

`ImmutableObject` provides compile-time and runtime guarantees of immutability by preventing public mutable state and optimizing equality operations through caching. It serves as a foundation for creating truly immutable value objects with built-in validation.

## Key Features

- **Runtime Immutability Validation**: Prevents public fields and settable properties
- **Performance Optimization**: Caches atomic values and hash codes after construction
- **Value-Based Equality**: Inherits comprehensive equality from `EquatableObject`
- **Thread Safety**: Immutable objects are inherently thread-safe
- **Generic and Non-Generic Versions**: Flexible inheritance patterns
- **Construction-Time Validation**: Early detection of immutability violations

## Immutability Enforcement

### Field Validation
Prevents any public instance fields, ensuring encapsulation.

```csharp
private void ValidateFields()
{
    var gotAnyPublicField = GetType()
        .GetFields(BindingFlags.Instance | BindingFlags.Public)
        .Length != 0;

    if (gotAnyPublicField)
        throw new InvalidOperationException("This object is immutable.");
}
```

**Validation Rules:**
- No public instance fields allowed
- Private fields are permitted for internal state
- Readonly fields are recommended

### Property Validation
Ensures all public properties are read-only.

```csharp
private void ValidateProperties()
{
    var gotAnyPublicSetter = GetType()
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Any(property => property.CanWrite);

    if (gotAnyPublicSetter)
        throw new InvalidOperationException("This object is immutable.");

    _atomicValues = base.GetAtomicValues();
    _hashCode = _atomicValues.Aggregate(0, HashCode.Combine);
}
```

**Validation Rules:**
- No public properties with setters allowed
- Get-only properties are required
- Init-only properties are supported
- Caches values after validation for performance

## Performance Optimizations

### Cached Atomic Values
Values used for equality comparison are computed once and cached.

```csharp
private List<object?>? _atomicValues;

protected override List<object?> GetAtomicValues() => _atomicValues ??= base.GetAtomicValues();
```

### Cached Hash Code
Hash code is computed once during construction and reused.

```csharp
private int? _hashCode;

public override int GetHashCode() => _hashCode ??= GetAtomicValues().Aggregate(0, HashCode.Combine);
```

## Usage Examples

### Basic Immutable Value Object

```csharp
public class Point : ImmutableObject<Point>
{
    public double X { get; }
    public double Y { get; }
    
    public Point(double x, double y)
    {
        X = x;
        Y = y;
        // Validation happens in base constructor
    }
    
    public double DistanceFromOrigin => Math.Sqrt(X * X + Y * Y);
    
    public Point Translate(double deltaX, double deltaY)
    {
        return new Point(X + deltaX, Y + deltaY);
    }
    
    public Point Scale(double factor)
    {
        return new Point(X * factor, Y * factor);
    }
    
    public override string ToString() => $"({X}, {Y})";
}

// Usage example
public void DemonstrateBasicImmutability()
{
    var point1 = new Point(3, 4);
    var point2 = new Point(3, 4);
    
    Console.WriteLine($"Distance from origin: {point1.DistanceFromOrigin}"); // 5
    Console.WriteLine($"Points equal: {point1 == point2}"); // True
    Console.WriteLine($"Hash codes equal: {point1.GetHashCode() == point2.GetHashCode()}"); // True
    
    // Immutable operations return new instances
    var translatedPoint = point1.Translate(1, 1);
    Console.WriteLine($"Original: {point1}"); // (3, 4)
    Console.WriteLine($"Translated: {translatedPoint}"); // (4, 5)
    
    var scaledPoint = point1.Scale(2);
    Console.WriteLine($"Scaled: {scaledPoint}"); // (6, 8)
}
```

### Complex Immutable Object

```csharp
public class PersonData : ImmutableObject<PersonData>
{
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime DateOfBirth { get; }
    public IReadOnlyList<string> EmailAddresses { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
    
    private readonly List<string> _emailList;
    private readonly Dictionary<string, string> _metadataDict;
    
    public PersonData(string firstName, 
                     string lastName, 
                     DateTime dateOfBirth,
                     IEnumerable<string>? emailAddresses = null,
                     IDictionary<string, string>? metadata = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        DateOfBirth = dateOfBirth;
        
        _emailList = emailAddresses?.ToList() ?? new List<string>();
        _metadataDict = metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>();
        
        EmailAddresses = _emailList.AsReadOnly();
        Metadata = _metadataDict.AsReadOnly();
        
        // Base constructor performs validation after our initialization
    }
    
    public string FullName => $"{FirstName} {LastName}";
    
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    
    public PersonData WithFirstName(string newFirstName)
    {
        return new PersonData(newFirstName, LastName, DateOfBirth, _emailList, _metadataDict);
    }
    
    public PersonData WithLastName(string newLastName)
    {
        return new PersonData(FirstName, newLastName, DateOfBirth, _emailList, _metadataDict);
    }
    
    public PersonData WithEmailAddress(string emailAddress)
    {
        var newEmails = new List<string>(_emailList) { emailAddress };
        return new PersonData(FirstName, LastName, DateOfBirth, newEmails, _metadataDict);
    }
    
    public PersonData WithMetadata(string key, string value)
    {
        var newMetadata = new Dictionary<string, string>(_metadataDict) { [key] = value };
        return new PersonData(FirstName, LastName, DateOfBirth, _emailList, newMetadata);
    }
    
    public PersonData RemoveEmailAddress(string emailAddress)
    {
        var newEmails = _emailList.Where(e => e != emailAddress).ToList();
        return new PersonData(FirstName, LastName, DateOfBirth, newEmails, _metadataDict);
    }
    
    public override string ToString() => $"{FullName} (Age: {Age})";
}

// Usage example
public void DemonstrateComplexImmutability()
{
    var person = new PersonData(
        "John", 
        "Doe", 
        new DateTime(1990, 5, 15),
        new[] { "john@example.com", "john.doe@work.com" },
        new Dictionary<string, string> { ["Department"] = "Engineering", ["Level"] = "Senior" }
    );
    
    Console.WriteLine($"Original: {person}");
    Console.WriteLine($"Emails: {string.Join(", ", person.EmailAddresses)}");
    Console.WriteLine($"Metadata: {string.Join(", ", person.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
    
    // Immutable updates
    var updatedPerson = person
        .WithFirstName("Jonathan")
        .WithEmailAddress("jonathan@personal.com")
        .WithMetadata("Location", "Remote");
    
    Console.WriteLine($"\nUpdated: {updatedPerson}");
    Console.WriteLine($"Emails: {string.Join(", ", updatedPerson.EmailAddresses)}");
    Console.WriteLine($"Metadata: {string.Join(", ", updatedPerson.Metadata.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
    
    // Original is unchanged
    Console.WriteLine($"\nOriginal unchanged: {person}");
    Console.WriteLine($"Objects equal: {person == updatedPerson}"); // False
}
```

### Immutable Configuration Object

```csharp
public class DatabaseConfiguration : ImmutableObject<DatabaseConfiguration>
{
    public string ConnectionString { get; }
    public int MaxConnections { get; }
    public TimeSpan ConnectionTimeout { get; }
    public bool EnableRetries { get; }
    public IReadOnlyList<string> AvailableSchemas { get; }
    public IReadOnlyDictionary<string, object> AdditionalSettings { get; }
    
    private readonly List<string> _schemas;
    private readonly Dictionary<string, object> _settings;
    
    public DatabaseConfiguration(string connectionString,
                                int maxConnections = 100,
                                TimeSpan? connectionTimeout = null,
                                bool enableRetries = true,
                                IEnumerable<string>? availableSchemas = null,
                                IDictionary<string, object>? additionalSettings = null)
    {
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        MaxConnections = maxConnections > 0 ? maxConnections : throw new ArgumentException("Must be positive", nameof(maxConnections));
        ConnectionTimeout = connectionTimeout ?? TimeSpan.FromSeconds(30);
        EnableRetries = enableRetries;
        
        _schemas = availableSchemas?.ToList() ?? new List<string>();
        _settings = additionalSettings?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
        
        AvailableSchemas = _schemas.AsReadOnly();
        AdditionalSettings = _settings.AsReadOnly();
    }
    
    public DatabaseConfiguration WithConnectionString(string newConnectionString)
    {
        return new DatabaseConfiguration(newConnectionString, MaxConnections, ConnectionTimeout, EnableRetries, _schemas, _settings);
    }
    
    public DatabaseConfiguration WithMaxConnections(int newMaxConnections)
    {
        return new DatabaseConfiguration(ConnectionString, newMaxConnections, ConnectionTimeout, EnableRetries, _schemas, _settings);
    }
    
    public DatabaseConfiguration WithConnectionTimeout(TimeSpan newTimeout)
    {
        return new DatabaseConfiguration(ConnectionString, MaxConnections, newTimeout, EnableRetries, _schemas, _settings);
    }
    
    public DatabaseConfiguration WithRetries(bool enableRetries)
    {
        return new DatabaseConfiguration(ConnectionString, MaxConnections, ConnectionTimeout, enableRetries, _schemas, _settings);
    }
    
    public DatabaseConfiguration WithSchema(string schema)
    {
        var newSchemas = new List<string>(_schemas) { schema };
        return new DatabaseConfiguration(ConnectionString, MaxConnections, ConnectionTimeout, EnableRetries, newSchemas, _settings);
    }
    
    public DatabaseConfiguration WithSetting(string key, object value)
    {
        var newSettings = new Dictionary<string, object>(_settings) { [key] = value };
        return new DatabaseConfiguration(ConnectionString, MaxConnections, ConnectionTimeout, EnableRetries, _schemas, newSettings);
    }
    
    public T? GetSetting<T>(string key)
    {
        return _settings.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
    }
    
    public override string ToString() => $"DB Config: {MaxConnections} max connections, {ConnectionTimeout} timeout";
}

// Usage in application configuration
public class ConfigurationService
{
    private DatabaseConfiguration _currentConfig;
    
    public ConfigurationService(DatabaseConfiguration initialConfig)
    {
        _currentConfig = initialConfig ?? throw new ArgumentNullException(nameof(initialConfig));
    }
    
    public DatabaseConfiguration CurrentConfiguration => _currentConfig;
    
    public void UpdateConfiguration(Func<DatabaseConfiguration, DatabaseConfiguration> updateFunc)
    {
        var newConfig = updateFunc(_currentConfig);
        _currentConfig = newConfig;
        
        Console.WriteLine($"Configuration updated: {newConfig}");
        OnConfigurationChanged?.Invoke(newConfig);
    }
    
    public event Action<DatabaseConfiguration>? OnConfigurationChanged;
    
    // Example usage methods
    public void IncreaseMaxConnections(int additionalConnections)
    {
        UpdateConfiguration(config => config.WithMaxConnections(config.MaxConnections + additionalConnections));
    }
    
    public void AddSchema(string schema)
    {
        UpdateConfiguration(config => config.WithSchema(schema));
    }
    
    public void EnableConnectionPooling(int poolSize)
    {
        UpdateConfiguration(config => config.WithSetting("PoolSize", poolSize).WithSetting("Pooling", true));
    }
}

// Usage example
public void DemonstrateConfigurationManagement()
{
    var initialConfig = new DatabaseConfiguration(
        "Server=localhost;Database=MyApp;Integrated Security=true;",
        maxConnections: 50,
        connectionTimeout: TimeSpan.FromSeconds(45),
        availableSchemas: new[] { "dbo", "reporting" }
    );
    
    var configService = new ConfigurationService(initialConfig);
    
    configService.OnConfigurationChanged += config => 
        Console.WriteLine($"Configuration changed: {config.MaxConnections} connections");
    
    // Configuration updates are immutable
    configService.IncreaseMaxConnections(25);
    configService.AddSchema("analytics");
    configService.EnableConnectionPooling(20);
    
    var finalConfig = configService.CurrentConfiguration;
    Console.WriteLine($"Final configuration has {finalConfig.AvailableSchemas.Count} schemas");
    Console.WriteLine($"Pool size: {finalConfig.GetSetting<int>("PoolSize")}");
    
    // Original config is unchanged
    Console.WriteLine($"Original config unchanged: {initialConfig.MaxConnections} connections");
}
```

### Immutable Command Pattern

```csharp
public abstract class ImmutableCommand : ImmutableObject<ImmutableCommand>
{
    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public string CommandType { get; }
    
    protected ImmutableCommand(string commandType)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        CommandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
    }
    
    public abstract Task<CommandResult> ExecuteAsync();
}

public class CreateUserCommand : ImmutableCommand
{
    public string Username { get; }
    public string Email { get; }
    public IReadOnlyList<string> Roles { get; }
    
    private readonly List<string> _rolesList;
    
    public CreateUserCommand(string username, string email, IEnumerable<string>? roles = null)
        : base(nameof(CreateUserCommand))
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        _rolesList = roles?.ToList() ?? new List<string>();
        Roles = _rolesList.AsReadOnly();
    }
    
    public CreateUserCommand WithRole(string role)
    {
        var newRoles = new List<string>(_rolesList) { role };
        return new CreateUserCommand(Username, Email, newRoles);
    }
    
    public CreateUserCommand WithRoles(IEnumerable<string> roles)
    {
        return new CreateUserCommand(Username, Email, roles);
    }
    
    public override async Task<CommandResult> ExecuteAsync()
    {
        // Simulate user creation
        await Task.Delay(100);
        
        return new CommandResult(
            Id,
            true,
            $"User '{Username}' created with email '{Email}' and {Roles.Count} roles"
        );
    }
    
    public override string ToString() => $"CreateUser: {Username} ({Email})";
}

public class CommandResult : ImmutableObject<CommandResult>
{
    public Guid CommandId { get; }
    public bool Success { get; }
    public string Message { get; }
    public DateTime CompletedAt { get; }
    public IReadOnlyDictionary<string, object> Data { get; }
    
    private readonly Dictionary<string, object> _dataDict;
    
    public CommandResult(Guid commandId, bool success, string message, IDictionary<string, object>? data = null)
    {
        CommandId = commandId;
        Success = success;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        CompletedAt = DateTime.UtcNow;
        _dataDict = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
        Data = _dataDict.AsReadOnly();
    }
    
    public CommandResult WithData(string key, object value)
    {
        var newData = new Dictionary<string, object>(_dataDict) { [key] = value };
        return new CommandResult(CommandId, Success, Message, newData);
    }
    
    public T? GetData<T>(string key)
    {
        return _dataDict.TryGetValue(key, out var value) && value is T typedValue ? typedValue : default;
    }
    
    public override string ToString() => $"{(Success ? "Success" : "Failed")}: {Message}";
}

// Command processor using immutable commands
public class ImmutableCommandProcessor
{
    private readonly List<ImmutableCommand> _commandHistory = new();
    private readonly Dictionary<Guid, CommandResult> _results = new();
    
    public IReadOnlyList<ImmutableCommand> CommandHistory => _commandHistory.AsReadOnly();
    public IReadOnlyDictionary<Guid, CommandResult> Results => _results.AsReadOnly();
    
    public async Task<CommandResult> ProcessAsync(ImmutableCommand command)
    {
        Console.WriteLine($"Processing command: {command}");
        
        // Commands are immutable, so we can safely store them
        _commandHistory.Add(command);
        
        try
        {
            var result = await command.ExecuteAsync();
            _results[command.Id] = result;
            
            Console.WriteLine($"Command completed: {result}");
            return result;
        }
        catch (Exception ex)
        {
            var errorResult = new CommandResult(command.Id, false, $"Error: {ex.Message}");
            _results[command.Id] = errorResult;
            return errorResult;
        }
    }
    
    public async Task<IEnumerable<CommandResult>> ProcessBatchAsync(IEnumerable<ImmutableCommand> commands)
    {
        var tasks = commands.Select(ProcessAsync);
        return await Task.WhenAll(tasks);
    }
    
    public void PrintHistory()
    {
        Console.WriteLine($"\nCommand History ({_commandHistory.Count} commands):");
        foreach (var cmd in _commandHistory)
        {
            var result = _results[cmd.Id];
            Console.WriteLine($"  {cmd.CreatedAt:HH:mm:ss} - {cmd} -> {result}");
        }
    }
}

// Usage example
public async Task DemonstrateImmutableCommands()
{
    var processor = new ImmutableCommandProcessor();
    
    // Create immutable commands
    var baseCommand = new CreateUserCommand("john.doe", "john@example.com");
    var adminCommand = baseCommand.WithRole("Admin");
    var superAdminCommand = adminCommand.WithRole("SuperAdmin");
    
    // Commands are immutable - original is unchanged
    Console.WriteLine($"Base command roles: {baseCommand.Roles.Count}");       // 0
    Console.WriteLine($"Admin command roles: {adminCommand.Roles.Count}");     // 1
    Console.WriteLine($"Super admin command roles: {superAdminCommand.Roles.Count}"); // 2
    
    // Process commands
    await processor.ProcessAsync(baseCommand);
    await processor.ProcessAsync(adminCommand);
    await processor.ProcessAsync(superAdminCommand);
    
    // Commands can be safely stored and referenced
    processor.PrintHistory();
    
    // Commands with same values are equal
    var duplicateCommand = new CreateUserCommand("john.doe", "john@example.com");
    Console.WriteLine($"Commands equal: {baseCommand == duplicateCommand}"); // True (same values)
    Console.WriteLine($"Commands same reference: {ReferenceEquals(baseCommand, duplicateCommand)}"); // False
}
```

### Thread-Safe Operations

```csharp
public class ImmutableCounter : ImmutableObject<ImmutableCounter>
{
    public int Value { get; }
    public DateTime LastModified { get; }
    public IReadOnlyList<DateTime> History { get; }
    
    private readonly List<DateTime> _historyList;
    
    public ImmutableCounter(int initialValue = 0, IEnumerable<DateTime>? history = null)
    {
        Value = initialValue;
        LastModified = DateTime.UtcNow;
        _historyList = history?.ToList() ?? new List<DateTime>();
        _historyList.Add(LastModified);
        History = _historyList.AsReadOnly();
    }
    
    public ImmutableCounter Increment()
    {
        return new ImmutableCounter(Value + 1, _historyList);
    }
    
    public ImmutableCounter Decrement()
    {
        return new ImmutableCounter(Value - 1, _historyList);
    }
    
    public ImmutableCounter Add(int amount)
    {
        return new ImmutableCounter(Value + amount, _historyList);
    }
    
    public ImmutableCounter Reset()
    {
        return new ImmutableCounter(0, _historyList);
    }
    
    public override string ToString() => $"Counter: {Value} (modified {LastModified:HH:mm:ss})";
}

// Thread-safe service using immutable objects
public class ThreadSafeCounterService
{
    private volatile ImmutableCounter _currentCounter;
    private readonly object _lock = new();
    
    public ThreadSafeCounterService(int initialValue = 0)
    {
        _currentCounter = new ImmutableCounter(initialValue);
    }
    
    public ImmutableCounter CurrentCounter => _currentCounter;
    
    public ImmutableCounter Increment()
    {
        lock (_lock)
        {
            _currentCounter = _currentCounter.Increment();
            return _currentCounter;
        }
    }
    
    public ImmutableCounter Decrement()
    {
        lock (_lock)
        {
            _currentCounter = _currentCounter.Decrement();
            return _currentCounter;
        }
    }
    
    public ImmutableCounter Add(int amount)
    {
        lock (_lock)
        {
            _currentCounter = _currentCounter.Add(amount);
            return _currentCounter;
        }
    }
    
    public ImmutableCounter Reset()
    {
        lock (_lock)
        {
            _currentCounter = _currentCounter.Reset();
            return _currentCounter;
        }
    }
    
    // Thread-safe read operations don't need locking due to immutability
    public int GetValue() => _currentCounter.Value;
    public DateTime GetLastModified() => _currentCounter.LastModified;
    public int GetHistoryCount() => _currentCounter.History.Count;
}

// Usage example with concurrent operations
public async Task DemonstrateThreadSafety()
{
    var counterService = new ThreadSafeCounterService();
    var tasks = new List<Task>();
    
    // Simulate concurrent operations
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(Task.Run(() =>
        {
            for (int j = 0; j < 100; j++)
            {
                var result = counterService.Increment();
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: {result}");
                Thread.Sleep(1); // Small delay to increase contention
            }
        }));
    }
    
    // Wait for all operations to complete
    await Task.WhenAll(tasks);
    
    var finalCounter = counterService.CurrentCounter;
    Console.WriteLine($"\nFinal result: {finalCounter}");
    Console.WriteLine($"History entries: {finalCounter.History.Count}");
    Console.WriteLine($"Expected value: 1000, Actual value: {finalCounter.Value}");
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class ImmutableObjectTests
{
    private class ValidImmutableObject : ImmutableObject<ValidImmutableObject>
    {
        public string Name { get; }
        public int Value { get; }
        
        public ValidImmutableObject(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }
    
    private class InvalidImmutableObject : ImmutableObject<InvalidImmutableObject>
    {
        public string Name { get; set; } // This should cause validation to fail
        
        public InvalidImmutableObject(string name)
        {
            Name = name;
        }
    }
    
    [Test]
    public void ValidImmutableObject_ShouldConstruct()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() => new ValidImmutableObject("Test", 42));
    }
    
    [Test]
    public void InvalidImmutableObject_ShouldThrowException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => new InvalidImmutableObject("Test"));
        Assert.That(ex.Message, Contains.Substring("immutable"));
    }
    
    [Test]
    public void ImmutableObject_ShouldCacheHashCode()
    {
        // Arrange
        var obj = new ValidImmutableObject("Test", 42);
        
        // Act
        int hash1 = obj.GetHashCode();
        int hash2 = obj.GetHashCode();
        
        // Assert
        Assert.That(hash1, Is.EqualTo(hash2));
    }
    
    [Test]
    public void ImmutableObject_ShouldCacheAtomicValues()
    {
        // Arrange
        var obj = new ValidImmutableObject("Test", 42);
        
        // Act
        var values1 = obj.GetAtomicValues();
        var values2 = obj.GetAtomicValues();
        
        // Assert
        Assert.That(ReferenceEquals(values1, values2), Is.True);
    }
    
    [Test]
    public void EqualImmutableObjects_ShouldHaveSameHashCode()
    {
        // Arrange
        var obj1 = new ValidImmutableObject("Test", 42);
        var obj2 = new ValidImmutableObject("Test", 42);
        
        // Act & Assert
        Assert.That(obj1, Is.EqualTo(obj2));
        Assert.That(obj1.GetHashCode(), Is.EqualTo(obj2.GetHashCode()));
    }
}
```

### Performance Tests

```csharp
[TestFixture]
public class ImmutableObjectPerformanceTests
{
    private class BenchmarkImmutableObject : ImmutableObject<BenchmarkImmutableObject>
    {
        public string StringValue { get; }
        public int IntValue { get; }
        public DateTime DateValue { get; }
        
        public BenchmarkImmutableObject(string stringValue, int intValue, DateTime dateValue)
        {
            StringValue = stringValue;
            IntValue = intValue;
            DateValue = dateValue;
        }
    }
    
    [Test]
    public void HashCode_Performance_ShouldBeFast()
    {
        // Arrange
        var objects = Enumerable.Range(0, 10000)
            .Select(i => new BenchmarkImmutableObject($"String{i}", i, DateTime.Now.AddDays(i)))
            .ToList();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        foreach (var obj in objects)
        {
            _ = obj.GetHashCode(); // Should use cached value after first call
        }
        
        stopwatch.Stop();
        
        // Assert
        Console.WriteLine($"Hash code operations: {stopwatch.ElapsedMilliseconds}ms for {objects.Count} objects");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100)); // Should be very fast due to caching
    }
    
    [Test]
    public void Equality_Performance_ShouldBeFast()
    {
        // Arrange
        var obj1 = new BenchmarkImmutableObject("Test", 42, DateTime.Now);
        var obj2 = new BenchmarkImmutableObject("Test", 42, DateTime.Now);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 100000; i++)
        {
            _ = obj1.Equals(obj2);
        }
        
        stopwatch.Stop();
        
        // Assert
        Console.WriteLine($"Equality operations: {stopwatch.ElapsedMilliseconds}ms for 100,000 comparisons");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));
    }
}
```

## Best Practices

### 1. Use Readonly Collections for Mutable Types
```csharp
public class ImmutableContainer : ImmutableObject<ImmutableContainer>
{
    private readonly List<string> _items;
    
    public IReadOnlyList<string> Items { get; }
    
    public ImmutableContainer(IEnumerable<string> items)
    {
        _items = items.ToList();
        Items = _items.AsReadOnly(); // Expose as readonly
    }
    
    public ImmutableContainer WithItem(string item)
    {
        var newItems = new List<string>(_items) { item };
        return new ImmutableContainer(newItems);
    }
}
```

### 2. Validate Invariants in Constructor
```csharp
public class ImmutableRange : ImmutableObject<ImmutableRange>
{
    public int Min { get; }
    public int Max { get; }
    
    public ImmutableRange(int min, int max)
    {
        if (min > max)
            throw new ArgumentException($"Min ({min}) cannot be greater than Max ({max})");
            
        Min = min;
        Max = max;
    }
    
    public bool Contains(int value) => value >= Min && value <= Max;
}
```

### 3. Provide Fluent Update Methods
```csharp
public class ImmutableSettings : ImmutableObject<ImmutableSettings>
{
    public string Name { get; }
    public bool Enabled { get; }
    public int Timeout { get; }
    
    public ImmutableSettings(string name, bool enabled = true, int timeout = 30)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Enabled = enabled;
        Timeout = timeout > 0 ? timeout : throw new ArgumentException("Must be positive", nameof(timeout));
    }
    
    public ImmutableSettings WithName(string name) => new(name, Enabled, Timeout);
    public ImmutableSettings Enable() => new(Name, true, Timeout);
    public ImmutableSettings Disable() => new(Name, false, Timeout);
    public ImmutableSettings WithTimeout(int timeout) => new(Name, Enabled, timeout);
}
```

### 4. Use Init-Only Properties When Appropriate
```csharp
public class ModernImmutableObject : ImmutableObject<ModernImmutableObject>
{
    public string Name { get; init; } = string.Empty;
    public int Value { get; init; }
    public DateTime Created { get; init; } = DateTime.UtcNow;
    
    // Note: Validation still occurs in constructor
    public ModernImmutableObject()
    {
        // Base constructor validates immutability
    }
}
```

## Error Handling

### Validation Errors

```csharp
public class ImmutableValidatedObject : ImmutableObject<ImmutableValidatedObject>
{
    public string Email { get; }
    public int Age { get; }
    
    public ImmutableValidatedObject(string email, int age)
    {
        Email = ValidateEmail(email);
        Age = ValidateAge(age);
    }
    
    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
            
        if (!email.Contains('@'))
            throw new ArgumentException("Email must contain @ symbol", nameof(email));
            
        return email.Trim().ToLowerInvariant();
    }
    
    private static int ValidateAge(int age)
    {
        if (age < 0 || age > 150)
            throw new ArgumentOutOfRangeException(nameof(age), "Age must be between 0 and 150");
            
        return age;
    }
}
```

## See Also

- [EquatableObject](EquatableObject.md) - Base class providing value-based equality
- [DisposableObject](DisposableObject.md) - Resource management patterns
- [NotifiableObject](NotifiableObject.md) - Change notification infrastructure
- [CompressedObject](CompressedObject.md) - Immutable compressed data container
- [ObjectHelper](../Helpers/ObjectHelper.md) - Object manipulation utilities

---

*Part of the RapidStreamer.BuildingBlocks.Application.Objects namespace - providing immutable object infrastructure with runtime validation for .NET applications.*