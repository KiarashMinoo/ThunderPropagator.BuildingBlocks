# DateTimeHelper

The `DateTimeHelper` class is a static utility class in the RapidStreamer BuildingBlocks that provides essential DateTime manipulation and validation methods. It offers convenient extension methods for DateTime objects to perform common date and time operations with improved readability and functionality.

## Purpose

This helper serves as:
- A DateTime utility provider for common date/time operations
- A validation tool for specific time conditions like midnight detection
- An extension provider that enhances DateTime functionality
- A performance-optimized utility for time-based queries and validations
- A foundation for time-sensitive business logic implementations

## Key Features

- **Midnight Detection**: Precise identification of midnight time (00:00:00)
- **TimeOfDay Optimization**: Efficient time component checking without full DateTime comparison
- **Extension Method Pattern**: Fluent, readable syntax for DateTime operations
- **Performance Optimized**: Uses pattern matching for efficient time validation
- **Null Safety**: Robust handling of DateTime operations

## Method

### IsMidnight
Determines whether a DateTime represents exactly midnight (00:00:00).

```csharp
public static bool IsMidnight(this DateTime dateTime)
```

**Implementation:**
```csharp
public static bool IsMidnight(this DateTime dateTime) => 
    dateTime.TimeOfDay is { Hours: 0, Minutes: 0, Seconds: 0 };
```

**Key Features:**
- Uses pattern matching for optimal performance
- Checks TimeOfDay property to avoid date component comparison
- Returns true only for exact midnight (00:00:00.000)
- Ignores milliseconds component for practical midnight detection

## Usage Examples

### Basic Midnight Detection

```csharp
using RapidStreamer.BuildingBlocks.Application.Helpers;

// Test various DateTime values
var midnightToday = DateTime.Today; // Today at 00:00:00
var noonToday = DateTime.Today.AddHours(12); // Today at 12:00:00
var almostMidnight = DateTime.Today.AddMilliseconds(500); // Today at 00:00:00.500

Console.WriteLine($"Midnight today: {midnightToday.IsMidnight()}"); // True
Console.WriteLine($"Noon today: {noonToday.IsMidnight()}"); // False
Console.WriteLine($"Almost midnight: {almostMidnight.IsMidnight()}"); // True (ignores milliseconds)

// Constructing midnight explicitly
var explicitMidnight = new DateTime(2024, 1, 1, 0, 0, 0);
Console.WriteLine($"Explicit midnight: {explicitMidnight.IsMidnight()}"); // True
```

### Business Hours Validation

```csharp
public class BusinessHoursValidator
{
    public bool IsOutsideBusinessHours(DateTime timestamp)
    {
        // Check if it's midnight (start of new business day)
        if (timestamp.IsMidnight())
        {
            return true; // Business typically doesn't start at midnight
        }
        
        var timeOfDay = timestamp.TimeOfDay;
        
        // Business hours: 9 AM to 6 PM
        var businessStart = new TimeSpan(9, 0, 0);   // 09:00:00
        var businessEnd = new TimeSpan(18, 0, 0);    // 18:00:00
        
        return timeOfDay < businessStart || timeOfDay >= businessEnd;
    }
    
    public string GetBusinessDayStatus(DateTime timestamp)
    {
        if (timestamp.IsMidnight())
        {
            return "Day Start - Business Closed";
        }
        
        return IsOutsideBusinessHours(timestamp) ? "Outside Business Hours" : "Business Hours";
    }
}

// Usage
var validator = new BusinessHoursValidator();

var timestamps = new[]
{
    DateTime.Today, // Midnight
    DateTime.Today.AddHours(8),  // 8 AM
    DateTime.Today.AddHours(12), // Noon
    DateTime.Today.AddHours(19), // 7 PM
};

foreach (var timestamp in timestamps)
{
    Console.WriteLine($"{timestamp:HH:mm:ss} - {validator.GetBusinessDayStatus(timestamp)}");
}
```

### Daily Processing System

```csharp
public class DailyProcessor
{
    private DateTime? _lastProcessingDate;
    
    public bool ShouldRunDailyProcess(DateTime currentTime)
    {
        // Run daily process only at midnight or if never run
        if (_lastProcessingDate == null)
        {
            return currentTime.IsMidnight();
        }
        
        // Check if it's a new day and it's midnight
        return currentTime.Date > _lastProcessingDate.Value.Date && currentTime.IsMidnight();
    }
    
    public async Task ProcessDailyTasks(DateTime timestamp)
    {
        if (!ShouldRunDailyProcess(timestamp))
        {
            Console.WriteLine($"Daily processing skipped at {timestamp:yyyy-MM-dd HH:mm:ss}");
            return;
        }
        
        Console.WriteLine($"Starting daily processing at {timestamp:yyyy-MM-dd HH:mm:ss}");
        
        // Perform daily tasks
        await GenerateDailyReports(timestamp.Date);
        await CleanupOldData(timestamp.Date);
        await SendDailySummary(timestamp.Date);
        
        _lastProcessingDate = timestamp;
        Console.WriteLine("Daily processing completed");
    }
    
    private async Task GenerateDailyReports(DateTime date)
    {
        // Simulate report generation
        await Task.Delay(100);
        Console.WriteLine($"Daily reports generated for {date:yyyy-MM-dd}");
    }
    
    private async Task CleanupOldData(DateTime date)
    {
        // Simulate data cleanup
        await Task.Delay(50);
        Console.WriteLine($"Old data cleaned up for {date:yyyy-MM-dd}");
    }
    
    private async Task SendDailySummary(DateTime date)
    {
        // Simulate sending summary
        await Task.Delay(75);
        Console.WriteLine($"Daily summary sent for {date:yyyy-MM-dd}");
    }
}
```

### Scheduling and Timing Systems

```csharp
public class ScheduleManager
{
    public class ScheduledTask
    {
        public string Name { get; set; } = string.Empty;
        public TimeSpan ScheduledTime { get; set; }
        public bool RunOnlyAtMidnight { get; set; }
        public Action<DateTime> Action { get; set; } = _ => { };
    }
    
    private readonly List<ScheduledTask> _tasks = new();
    
    public void AddTask(string name, TimeSpan scheduledTime, Action<DateTime> action, bool runOnlyAtMidnight = false)
    {
        _tasks.Add(new ScheduledTask
        {
            Name = name,
            ScheduledTime = scheduledTime,
            Action = action,
            RunOnlyAtMidnight = runOnlyAtMidnight
        });
    }
    
    public void ProcessTasks(DateTime currentTime)
    {
        foreach (var task in _tasks)
        {
            if (ShouldRunTask(task, currentTime))
            {
                Console.WriteLine($"Executing task: {task.Name} at {currentTime:HH:mm:ss}");
                task.Action(currentTime);
            }
        }
    }
    
    private bool ShouldRunTask(ScheduledTask task, DateTime currentTime)
    {
        // Check if task requires midnight execution
        if (task.RunOnlyAtMidnight && !currentTime.IsMidnight())
        {
            return false;
        }
        
        // Check if current time matches scheduled time
        var timeDifference = Math.Abs((currentTime.TimeOfDay - task.ScheduledTime).TotalSeconds);
        return timeDifference < 1; // Allow 1-second tolerance
    }
}

// Usage example
var scheduler = new ScheduleManager();

// Daily backup at midnight only
scheduler.AddTask("Daily Backup", TimeSpan.Zero, 
    timestamp => Console.WriteLine($"Running daily backup at {timestamp}"), 
    runOnlyAtMidnight: true);

// Regular report every 6 hours
scheduler.AddTask("6-Hour Report", new TimeSpan(6, 0, 0), 
    timestamp => Console.WriteLine($"Generating 6-hour report at {timestamp}"));

// Test scheduling
var testTimes = new[]
{
    DateTime.Today, // Midnight
    DateTime.Today.AddHours(6), // 6 AM
    DateTime.Today.AddHours(12), // Noon
    DateTime.Today.AddHours(18), // 6 PM
};

foreach (var time in testTimes)
{
    Console.WriteLine($"\n--- Processing at {time:yyyy-MM-dd HH:mm:ss} ---");
    scheduler.ProcessTasks(time);
}
```

## Real-World Applications

### Database Maintenance Scheduler

```csharp
public class DatabaseMaintenanceService
{
    private readonly string _connectionString;
    private DateTime? _lastMaintenanceRun;
    
    public DatabaseMaintenanceService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<bool> TryRunMaintenanceAsync(DateTime currentTime)
    {
        // Only run maintenance at midnight
        if (!currentTime.IsMidnight())
        {
            return false;
        }
        
        // Don't run if already executed today
        if (_lastMaintenanceRun?.Date == currentTime.Date)
        {
            return false;
        }
        
        Console.WriteLine($"Starting database maintenance at {currentTime:yyyy-MM-dd HH:mm:ss}");
        
        try
        {
            await RunMaintenanceTasks(currentTime);
            _lastMaintenanceRun = currentTime;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database maintenance failed: {ex.Message}");
            return false;
        }
    }
    
    private async Task RunMaintenanceTasks(DateTime maintenanceTime)
    {
        // Reindex tables
        await ReindexTables();
        
        // Update statistics
        await UpdateStatistics();
        
        // Clean up old logs
        await CleanupOldLogs(maintenanceTime.AddDays(-30));
        
        // Backup database
        await BackupDatabase(maintenanceTime);
    }
    
    private async Task ReindexTables()
    {
        await Task.Delay(1000); // Simulate reindexing
        Console.WriteLine("Database tables reindexed");
    }
    
    private async Task UpdateStatistics()
    {
        await Task.Delay(500); // Simulate statistics update
        Console.WriteLine("Database statistics updated");
    }
    
    private async Task CleanupOldLogs(DateTime cutoffDate)
    {
        await Task.Delay(750); // Simulate log cleanup
        Console.WriteLine($"Cleaned up logs older than {cutoffDate:yyyy-MM-dd}");
    }
    
    private async Task BackupDatabase(DateTime backupTime)
    {
        await Task.Delay(2000); // Simulate backup
        Console.WriteLine($"Database backup completed for {backupTime:yyyy-MM-dd}");
    }
}
```

### Event Processing System

```csharp
public class EventProcessor
{
    public class Event
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;
        public object Data { get; set; } = null!;
    }
    
    public void ProcessEvents(IEnumerable<Event> events)
    {
        var midnightEvents = events.Where(e => e.Timestamp.IsMidnight()).ToList();
        var regularEvents = events.Where(e => !e.Timestamp.IsMidnight()).ToList();
        
        // Process midnight events with special handling
        if (midnightEvents.Any())
        {
            Console.WriteLine($"Processing {midnightEvents.Count} midnight events with special handling");
            ProcessMidnightEvents(midnightEvents);
        }
        
        // Process regular events
        if (regularEvents.Any())
        {
            Console.WriteLine($"Processing {regularEvents.Count} regular events");
            ProcessRegularEvents(regularEvents);
        }
    }
    
    private void ProcessMidnightEvents(List<Event> midnightEvents)
    {
        // Midnight events might trigger day rollover processing
        foreach (var evt in midnightEvents)
        {
            Console.WriteLine($"Midnight event: {evt.Type} at {evt.Timestamp:yyyy-MM-dd HH:mm:ss}");
            
            // Trigger day change processing
            if (evt.Type == "DayChange")
            {
                ProcessDayChange(evt.Timestamp);
            }
        }
    }
    
    private void ProcessRegularEvents(List<Event> regularEvents)
    {
        foreach (var evt in regularEvents)
        {
            Console.WriteLine($"Regular event: {evt.Type} at {evt.Timestamp:HH:mm:ss}");
        }
    }
    
    private void ProcessDayChange(DateTime timestamp)
    {
        Console.WriteLine($"Day change detected at {timestamp:yyyy-MM-dd}");
        // Implement day change logic
    }
}
```

### Time-Based Filtering

```csharp
public static class TimeFilterExtensions
{
    public static IEnumerable<T> FilterMidnightEvents<T>(this IEnumerable<T> items, Func<T, DateTime> timestampSelector)
    {
        return items.Where(item => timestampSelector(item).IsMidnight());
    }
    
    public static IEnumerable<T> FilterNonMidnightEvents<T>(this IEnumerable<T> items, Func<T, DateTime> timestampSelector)
    {
        return items.Where(item => !timestampSelector(item).IsMidnight());
    }
}

// Usage
var logEntries = new[]
{
    new { Message = "System started", Timestamp = DateTime.Today },
    new { Message = "User login", Timestamp = DateTime.Today.AddHours(9) },
    new { Message = "Data processed", Timestamp = DateTime.Today.AddHours(12) },
    new { Message = "Daily backup", Timestamp = DateTime.Today.AddDays(1) }, // Next day midnight
};

var midnightLogs = logEntries.FilterMidnightEvents(log => log.Timestamp);
var businessHourLogs = logEntries.FilterNonMidnightEvents(log => log.Timestamp);

Console.WriteLine("Midnight Events:");
foreach (var log in midnightLogs)
{
    Console.WriteLine($"  {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.Message}");
}

Console.WriteLine("Business Hour Events:");
foreach (var log in businessHourLogs)
{
    Console.WriteLine($"  {log.Timestamp:yyyy-MM-dd HH:mm:ss} - {log.Message}");
}
```

## Integration with DataType System

The DateTimeHelper works seamlessly with the DataType enumeration system:

```csharp
public class DateTimeValidator
{
    public bool ValidateDateTime(object value, DataType expectedType)
    {
        if (value is not DateTime dateTime)
        {
            return false;
        }
        
        return expectedType switch
        {
            DataType.DateTime => true, // Any DateTime is valid
            DataType.Date when dateTime.IsMidnight() => true, // Date-only should be at midnight
            DataType.Time when !dateTime.IsMidnight() => true, // Time-only should not be midnight for time validation
            _ => false
        };
    }
    
    public string FormatByType(DateTime dateTime, DataType dataType)
    {
        return dataType switch
        {
            DataType.DateTime => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            DataType.Date => dateTime.ToString("yyyy-MM-dd"),
            DataType.Time when dateTime.IsMidnight() => "00:00:00",
            DataType.Time => dateTime.ToString("HH:mm:ss"),
            _ => dateTime.ToString()
        };
    }
}
```

## Performance Considerations

### Pattern Matching Optimization
The `IsMidnight` method uses C# pattern matching for optimal performance:

```csharp
// Optimized pattern matching - single property access
dateTime.TimeOfDay is { Hours: 0, Minutes: 0, Seconds: 0 }

// Equivalent but less efficient approach:
// dateTime.TimeOfDay.Hours == 0 && 
// dateTime.TimeOfDay.Minutes == 0 && 
// dateTime.TimeOfDay.Seconds == 0
```

### Memory Efficiency
- Uses `TimeOfDay` property to avoid creating new DateTime instances
- Pattern matching compiles to efficient IL code
- No additional object allocations during validation

### Benchmarking Example

```csharp
public class DateTimeHelperBenchmark
{
    private readonly DateTime[] _testDates;
    
    public DateTimeHelperBenchmark()
    {
        _testDates = GenerateTestDates(10000);
    }
    
    [Benchmark]
    public int CountMidnightDatesOptimized()
    {
        return _testDates.Count(date => date.IsMidnight());
    }
    
    [Benchmark]
    public int CountMidnightDatesTraditional()
    {
        return _testDates.Count(date => 
            date.Hour == 0 && date.Minute == 0 && date.Second == 0);
    }
    
    private DateTime[] GenerateTestDates(int count)
    {
        var random = new Random(42);
        return Enumerable.Range(0, count)
            .Select(_ => DateTime.Today.AddSeconds(random.Next(0, 86400)))
            .ToArray();
    }
}
```

## Thread Safety

- **Static Method**: Thread-safe as it's a stateless static extension method
- **Read-Only Operations**: Only reads DateTime properties without modification
- **No Shared State**: Each method call is independent with no shared mutable state

## Testing Strategies

```csharp
[Test]
public void IsMidnight_WithMidnightTime_ReturnsTrue()
{
    // Arrange
    var midnight = new DateTime(2024, 1, 1, 0, 0, 0);
    
    // Act
    bool result = midnight.IsMidnight();
    
    // Assert
    Assert.True(result);
}

[Test]
public void IsMidnight_WithNonMidnightTime_ReturnsFalse()
{
    // Arrange
    var noon = new DateTime(2024, 1, 1, 12, 0, 0);
    
    // Act
    bool result = noon.IsMidnight();
    
    // Assert
    Assert.False(result);
}

[Test]
public void IsMidnight_WithMilliseconds_IgnoresMilliseconds()
{
    // Arrange
    var midnightWithMilliseconds = new DateTime(2024, 1, 1, 0, 0, 0, 500);
    
    // Act
    bool result = midnightWithMilliseconds.IsMidnight();
    
    // Assert
    Assert.True(result); // Should ignore milliseconds
}

[Test]
public void IsMidnight_WithDateTimeToday_ReturnsTrue()
{
    // Arrange
    var today = DateTime.Today; // Always midnight
    
    // Act
    bool result = today.IsMidnight();
    
    // Assert
    Assert.True(result);
}
```

## Best Practices

1. **Use for Scheduling**: Leverage `IsMidnight()` for day-boundary processing and scheduling
2. **Combine with TimeSpan**: Use alongside TimeSpan for comprehensive time validation
3. **Business Logic**: Ideal for business rules that depend on specific times
4. **Event Processing**: Filter and categorize events based on timing characteristics
5. **Performance**: Use pattern matching approach for optimal performance in time validation

## Error Handling

```csharp
public static class SafeDateTimeHelper
{
    public static bool TryIsMidnight(DateTime? dateTime, out bool isMidnight)
    {
        isMidnight = false;
        
        if (!dateTime.HasValue)
        {
            return false;
        }
        
        try
        {
            isMidnight = dateTime.Value.IsMidnight();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public static string GetTimeDescription(DateTime dateTime)
    {
        try
        {
            return dateTime.IsMidnight() ? "Midnight" : $"Time: {dateTime:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            return $"Invalid time: {ex.Message}";
        }
    }
}
```

## Related Components

- **[DataType](../Enums/DataType.md)**: Provides DateTime-related data type classifications
- **[DateTimeOffset Handling](../Helpers/README.md)**: Part of the broader time and date processing utilities
- **[GuardClauseHelper](GuardClauseHelper.md)**: Used in time-based validation scenarios

The `DateTimeHelper` provides essential DateTime utilities with a focus on performance and practicality, making it a valuable tool for time-sensitive applications and business logic implementations.