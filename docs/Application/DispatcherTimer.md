# DispatcherTimer

The `DispatcherTimer` static class provides a simplified, thread-safe timer management system for executing periodic and one-time operations. It offers both synchronous and asynchronous execution patterns with built-in cancellation support and proper resource management through disposable handles.

## Overview

```csharp
public static class DispatcherTimer
{
    // Periodic execution methods
    public static IDisposable Run<TState>(Func<TState?, bool> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default);
    public static IDisposable Run(Func<bool> action, TimeSpan interval, CancellationToken cancellationToken = default);
    public static IDisposable Run<TState>(Func<TState?, CancellationToken, Task<bool>> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default);
    public static IDisposable Run(Func<CancellationToken, Task<bool>> action, TimeSpan interval, CancellationToken cancellationToken = default);
    
    // One-time execution methods
    public static IDisposable RunOnce<TState>(Action<TState?> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default);
    public static IDisposable RunOnce(Action action, TimeSpan interval, CancellationToken cancellationToken = default);
    public static IDisposable RunOnce<TState>(Func<TState?, CancellationToken, Task> action, TimeSpan interval, TState? state, CancellationToken cancellationToken = default);
    public static IDisposable RunOnce(Func<CancellationToken, Task> action, TimeSpan interval, CancellationToken cancellationToken = default);
}
```

The `DispatcherTimer` provides a clean, functional approach to timer management with automatic resource cleanup and cancellation support, making it ideal for background tasks, periodic monitoring, and delayed execution scenarios.

## Key Features

### Timer Patterns
- **Periodic Execution**: Repeating operations until stopped or action returns false
- **One-Time Execution**: Single delayed execution after specified interval
- **State Management**: Optional state parameter passing to timer callbacks
- **Cancellation Support**: Integrated `CancellationToken` support for graceful shutdown

### Execution Models
- **Synchronous Actions**: Simple delegate-based execution
- **Asynchronous Actions**: Task-based execution with proper async/await support
- **Return Value Control**: Boolean return values control timer continuation
- **Exception Handling**: Robust error handling with timer termination

### Resource Management
- **Disposable Handles**: All timer operations return `IDisposable` for cleanup
- **Automatic Cleanup**: Proper disposal of internal `CancellationTokenSource`
- **Thread Safety**: Safe concurrent access to timer operations
- **Memory Efficiency**: Minimal memory footprint with efficient resource usage

## Usage Examples

### Basic Periodic Operations

```csharp
public class PeriodicTaskManager
{
    private readonly List<IDisposable> _activeTimers;
    private readonly ILogger<PeriodicTaskManager> _logger;
    
    public PeriodicTaskManager(ILogger<PeriodicTaskManager> logger)
    {
        _logger = logger;
        _activeTimers = new List<IDisposable>();
    }
    
    public void StartPeriodicHealthCheck()
    {
        var healthCheckTimer = DispatcherTimer.Run(() =>
        {
            try
            {
                var isHealthy = CheckSystemHealth();
                _logger.LogInformation("Health check completed: {Status}", isHealthy ? "Healthy" : "Unhealthy");
                
                // Continue running the timer
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                
                // Stop the timer on critical errors
                return false;
            }
        }, TimeSpan.FromMinutes(5));
        
        _activeTimers.Add(healthCheckTimer);
    }
    
    public void StartPeriodicCleanup()
    {
        var cleanupState = new CleanupState { LastCleanup = DateTime.UtcNow };
        
        var cleanupTimer = DispatcherTimer.Run<CleanupState>(state =>
        {
            if (state == null) return false;
            
            var itemsRemoved = PerformCleanup(state.LastCleanup);
            state.LastCleanup = DateTime.UtcNow;
            
            _logger.LogInformation("Cleanup completed: {ItemsRemoved} items removed", itemsRemoved);
            
            // Continue running
            return true;
        }, TimeSpan.FromHours(1), cleanupState);
        
        _activeTimers.Add(cleanupTimer);
    }
    
    public void StartAsyncDataProcessing(CancellationToken cancellationToken)
    {
        var processingTimer = DispatcherTimer.Run(async (token) =>
        {
            try
            {
                var hasMoreData = await ProcessDataBatchAsync(token);
                _logger.LogInformation("Data processing batch completed: {HasMore}", hasMoreData);
                
                // Continue if there's more data to process
                return hasMoreData;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Data processing cancelled");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data processing failed");
                return false;
            }
        }, TimeSpan.FromSeconds(30), cancellationToken);
        
        _activeTimers.Add(processingTimer);
    }
    
    private bool CheckSystemHealth()
    {
        // Simulate health check logic
        var memoryUsage = GC.GetTotalMemory(false);
        var threadCount = Process.GetCurrentProcess().Threads.Count;
        
        return memoryUsage < 500_000_000 && threadCount < 100;
    }
    
    private int PerformCleanup(DateTime since)
    {
        // Simulate cleanup logic
        return Random.Shared.Next(10, 100);
    }
    
    private async Task<bool> ProcessDataBatchAsync(CancellationToken cancellationToken)
    {
        // Simulate async data processing
        await Task.Delay(Random.Shared.Next(100, 1000), cancellationToken);
        return Random.Shared.NextDouble() > 0.2; // 80% chance of having more data
    }
    
    public void StopAllTimers()
    {
        foreach (var timer in _activeTimers)
        {
            timer.Dispose();
        }
        _activeTimers.Clear();
        _logger.LogInformation("All timers stopped");
    }
}

public class CleanupState
{
    public DateTime LastCleanup { get; set; }
    public int TotalItemsProcessed { get; set; }
}
```

### One-Time Delayed Operations

```csharp
public class DelayedOperationsService
{
    private readonly ILogger<DelayedOperationsService> _logger;
    private readonly INotificationService _notificationService;
    
    public DelayedOperationsService(ILogger<DelayedOperationsService> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }
    
    public IDisposable SchedulePasswordReset(string userId, TimeSpan delay)
    {
        var resetData = new PasswordResetData { UserId = userId, ScheduledAt = DateTime.UtcNow };
        
        return DispatcherTimer.RunOnce<PasswordResetData>(data =>
        {
            if (data == null) return;
            
            try
            {
                SendPasswordResetEmail(data.UserId);
                _logger.LogInformation("Password reset email sent to user {UserId}", data.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to user {UserId}", data.UserId);
            }
        }, delay, resetData);
    }
    
    public IDisposable ScheduleAsyncNotification(string message, TimeSpan delay, CancellationToken cancellationToken)
    {
        return DispatcherTimer.RunOnce(async (token) =>
        {
            try
            {
                await _notificationService.SendNotificationAsync(message, token);
                _logger.LogInformation("Delayed notification sent: {Message}", message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Delayed notification cancelled: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send delayed notification: {Message}", message);
            }
        }, delay, cancellationToken);
    }
    
    public IDisposable ScheduleSessionTimeout(string sessionId, TimeSpan timeout)
    {
        return DispatcherTimer.RunOnce(() =>
        {
            try
            {
                InvalidateSession(sessionId);
                _logger.LogInformation("Session {SessionId} timed out and was invalidated", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invalidate session {SessionId}", sessionId);
            }
        }, timeout);
    }
    
    public async Task<string> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var result = default(T);
        var completed = false;
        var exception = default(Exception);
        
        // Start the operation
        var operationTask = Task.Run(async () =>
        {
            try
            {
                result = await operation();
                completed = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }, cts.Token);
        
        // Schedule timeout
        var timeoutTimer = DispatcherTimer.RunOnce(() =>
        {
            if (!completed)
            {
                cts.Cancel();
                _logger.LogWarning("Operation timed out after {Timeout}", timeout);
            }
        }, timeout);
        
        try
        {
            await operationTask;
            timeoutTimer.Dispose();
            
            if (exception != null)
                throw exception;
            
            return completed ? $"Completed: {result}" : "Operation was cancelled";
        }
        catch (OperationCanceledException)
        {
            return "Operation timed out";
        }
    }
    
    private void SendPasswordResetEmail(string userId)
    {
        // Simulate email sending
        Thread.Sleep(100);
    }
    
    private void InvalidateSession(string sessionId)
    {
        // Simulate session invalidation
        _logger.LogDebug("Invalidating session {SessionId}", sessionId);
    }
}

public class PasswordResetData
{
    public string UserId { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
}

// Mock notification service
public interface INotificationService
{
    Task SendNotificationAsync(string message, CancellationToken cancellationToken);
}
```

### Advanced Timer Coordination

```csharp
public class TimerCoordinationService : IDisposable
{
    private readonly ILogger<TimerCoordinationService> _logger;
    private readonly List<IDisposable> _timers;
    private readonly CancellationTokenSource _masterCancellation;
    
    public TimerCoordinationService(ILogger<TimerCoordinationService> logger)
    {
        _logger = logger;
        _timers = new List<IDisposable>();
        _masterCancellation = new CancellationTokenSource();
    }
    
    public void StartCoordinatedTimers()
    {
        var sharedState = new CoordinatedState();
        
        // Fast timer - every 5 seconds
        var fastTimer = DispatcherTimer.Run<CoordinatedState>(state =>
        {
            if (state == null) return false;
            
            state.FastOperationCount++;
            _logger.LogDebug("Fast operation #{Count}", state.FastOperationCount);
            
            // Stop after 20 operations or if master cancelled
            return state.FastOperationCount < 20 && !_masterCancellation.Token.IsCancellationRequested;
        }, TimeSpan.FromSeconds(5), sharedState, _masterCancellation.Token);
        
        // Medium timer - every 30 seconds
        var mediumTimer = DispatcherTimer.Run<CoordinatedState>(state =>
        {
            if (state == null) return false;
            
            state.MediumOperationCount++;
            _logger.LogInformation("Medium operation #{Count} (Fast: {Fast})", 
                state.MediumOperationCount, state.FastOperationCount);
            
            // Stop after 5 operations
            return state.MediumOperationCount < 5 && !_masterCancellation.Token.IsCancellationRequested;
        }, TimeSpan.FromSeconds(30), sharedState, _masterCancellation.Token);
        
        // Slow timer - every 2 minutes with async operation
        var slowTimer = DispatcherTimer.Run<CoordinatedState>(async (state, token) =>
        {
            if (state == null) return false;
            
            state.SlowOperationCount++;
            
            try
            {
                await PerformSlowOperationAsync(state, token);
                _logger.LogInformation("Slow operation #{Count} completed (Fast: {Fast}, Medium: {Medium})", 
                    state.SlowOperationCount, state.FastOperationCount, state.MediumOperationCount);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Slow operation #{Count} was cancelled", state.SlowOperationCount);
                return false;
            }
            
            // Stop after 3 operations
            return state.SlowOperationCount < 3 && !token.IsCancellationRequested;
        }, TimeSpan.FromMinutes(2), sharedState, _masterCancellation.Token);
        
        // Monitoring timer - check coordination state
        var monitorTimer = DispatcherTimer.Run<CoordinatedState>(state =>
        {
            if (state == null) return false;
            
            var totalOperations = state.FastOperationCount + state.MediumOperationCount + state.SlowOperationCount;
            _logger.LogInformation("Coordination status - Total: {Total}, Fast: {Fast}, Medium: {Medium}, Slow: {Slow}",
                totalOperations, state.FastOperationCount, state.MediumOperationCount, state.SlowOperationCount);
            
            // Stop monitoring when all operations are complete
            var allComplete = state.FastOperationCount >= 20 && 
                             state.MediumOperationCount >= 5 && 
                             state.SlowOperationCount >= 3;
            
            if (allComplete)
            {
                _logger.LogInformation("All coordinated operations completed successfully");
                _masterCancellation.Cancel(); // Signal completion
                return false;
            }
            
            return !_masterCancellation.Token.IsCancellationRequested;
        }, TimeSpan.FromSeconds(15), sharedState, _masterCancellation.Token);
        
        _timers.AddRange(new[] { fastTimer, mediumTimer, slowTimer, monitorTimer });
    }
    
    public void StartCascadingTimers()
    {
        // Timer 1: Starts immediately
        var timer1 = DispatcherTimer.RunOnce(() =>
        {
            _logger.LogInformation("Cascading Timer 1 executed");
            
            // Timer 2: Starts 10 seconds after Timer 1
            var timer2 = DispatcherTimer.RunOnce(() =>
            {
                _logger.LogInformation("Cascading Timer 2 executed");
                
                // Timer 3: Starts 5 seconds after Timer 2
                var timer3 = DispatcherTimer.RunOnce(async (token) =>
                {
                    _logger.LogInformation("Cascading Timer 3 (async) executed");
                    await Task.Delay(2000, token);
                    _logger.LogInformation("Cascading Timer 3 completed");
                }, TimeSpan.FromSeconds(5), _masterCancellation.Token);
                
                _timers.Add(timer3);
                
            }, TimeSpan.FromSeconds(10), _masterCancellation.Token);
            
            _timers.Add(timer2);
            
        }, TimeSpan.FromSeconds(1), _masterCancellation.Token);
        
        _timers.Add(timer1);
    }
    
    private async Task PerformSlowOperationAsync(CoordinatedState state, CancellationToken cancellationToken)
    {
        // Simulate slow async operation
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(1000, cancellationToken);
            _logger.LogDebug("Slow operation step {Step}/5", i + 1);
        }
    }
    
    public void CancelAllOperations()
    {
        _logger.LogInformation("Cancelling all timer operations");
        _masterCancellation.Cancel();
    }
    
    public void Dispose()
    {
        _masterCancellation.Cancel();
        
        foreach (var timer in _timers)
        {
            timer.Dispose();
        }
        
        _timers.Clear();
        _masterCancellation.Dispose();
        
        _logger.LogInformation("TimerCoordinationService disposed");
    }
}

public class CoordinatedState
{
    public int FastOperationCount { get; set; }
    public int MediumOperationCount { get; set; }
    public int SlowOperationCount { get; set; }
}
```

### Error Handling and Resilience

```csharp
public class ResilientTimerService
{
    private readonly ILogger<ResilientTimerService> _logger;
    private readonly List<IDisposable> _timers;
    
    public ResilientTimerService(ILogger<ResilientTimerService> logger)
    {
        _logger = logger;
        _timers = new List<IDisposable>();
    }
    
    public void StartResilientTimer()
    {
        var retryState = new RetryState { MaxRetries = 3, CurrentRetry = 0 };
        
        var resilientTimer = DispatcherTimer.Run<RetryState>(state =>
        {
            if (state == null) return false;
            
            try
            {
                // Simulate operation that might fail
                if (Random.Shared.NextDouble() < 0.3) // 30% chance of failure
                {
                    throw new InvalidOperationException("Simulated operation failure");
                }
                
                _logger.LogInformation("Resilient operation succeeded");
                state.CurrentRetry = 0; // Reset retry counter on success
                return true;
            }
            catch (Exception ex)
            {
                state.CurrentRetry++;
                _logger.LogWarning(ex, "Resilient operation failed (attempt {Attempt}/{Max})", 
                    state.CurrentRetry, state.MaxRetries);
                
                if (state.CurrentRetry >= state.MaxRetries)
                {
                    _logger.LogError("Resilient operation failed after {Max} attempts, stopping timer", state.MaxRetries);
                    return false; // Stop the timer
                }
                
                return true; // Continue trying
            }
        }, TimeSpan.FromSeconds(10), retryState);
        
        _timers.Add(resilientTimer);
    }
    
    public void StartCircuitBreakerTimer()
    {
        var circuitState = new CircuitBreakerState();
        
        var circuitTimer = DispatcherTimer.Run<CircuitBreakerState>(state =>
        {
            if (state == null) return false;
            
            switch (state.State)
            {
                case CircuitState.Closed:
                    return ExecuteWithCircuitBreaker(state);
                
                case CircuitState.Open:
                    if (DateTime.UtcNow - state.LastFailureTime > TimeSpan.FromMinutes(1))
                    {
                        _logger.LogInformation("Circuit breaker transitioning to half-open");
                        state.State = CircuitState.HalfOpen;
                    }
                    else
                    {
                        _logger.LogDebug("Circuit breaker is open, skipping operation");
                    }
                    return true;
                
                case CircuitState.HalfOpen:
                    return ExecuteWithCircuitBreaker(state);
                
                default:
                    return false;
            }
        }, TimeSpan.FromSeconds(5), circuitState);
        
        _timers.Add(circuitTimer);
    }
    
    private bool ExecuteWithCircuitBreaker(CircuitBreakerState state)
    {
        try
        {
            // Simulate operation that might fail
            if (Random.Shared.NextDouble() < 0.4) // 40% chance of failure
            {
                throw new InvalidOperationException("Simulated circuit breaker failure");
            }
            
            _logger.LogInformation("Circuit breaker operation succeeded");
            
            // Reset on success
            state.FailureCount = 0;
            if (state.State == CircuitState.HalfOpen)
            {
                state.State = CircuitState.Closed;
                _logger.LogInformation("Circuit breaker closed");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            state.FailureCount++;
            state.LastFailureTime = DateTime.UtcNow;
            
            _logger.LogWarning(ex, "Circuit breaker operation failed ({Count} failures)", state.FailureCount);
            
            if (state.FailureCount >= 3)
            {
                state.State = CircuitState.Open;
                _logger.LogWarning("Circuit breaker opened due to repeated failures");
            }
            
            return true; // Continue running the timer
        }
    }
    
    public void StartTimeoutTimer()
    {
        var timeoutTimer = DispatcherTimer.Run(async (cancellationToken) =>
        {
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // 5-second timeout
                
                await SimulateLongRunningOperationAsync(timeoutCts.Token);
                _logger.LogInformation("Timeout-controlled operation completed");
                return true;
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Timer was cancelled");
                    return false;
                }
                else
                {
                    _logger.LogWarning("Operation timed out, continuing timer");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in timeout timer");
                return true;
            }
        }, TimeSpan.FromSeconds(10));
        
        _timers.Add(timeoutTimer);
    }
    
    private async Task SimulateLongRunningOperationAsync(CancellationToken cancellationToken)
    {
        // Simulate operation that might take too long
        var delay = Random.Shared.Next(1000, 8000); // 1-8 seconds
        await Task.Delay(delay, cancellationToken);
    }
    
    public void StopAllTimers()
    {
        foreach (var timer in _timers)
        {
            timer.Dispose();
        }
        _timers.Clear();
    }
}

public class RetryState
{
    public int MaxRetries { get; set; }
    public int CurrentRetry { get; set; }
}

public class CircuitBreakerState
{
    public CircuitState State { get; set; } = CircuitState.Closed;
    public int FailureCount { get; set; }
    public DateTime LastFailureTime { get; set; }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
```

### Performance and Monitoring

```csharp
public class TimerPerformanceMonitor : IDisposable
{
    private readonly ILogger<TimerPerformanceMonitor> _logger;
    private readonly List<IDisposable> _timers;
    private readonly Dictionary<string, TimerMetrics> _metrics;
    
    public TimerPerformanceMonitor(ILogger<TimerPerformanceMonitor> logger)
    {
        _logger = logger;
        _timers = new List<IDisposable>();
        _metrics = new Dictionary<string, TimerMetrics>();
    }
    
    public void StartPerformanceMonitoring()
    {
        // Monitor CPU-intensive timer
        StartMonitoredTimer("CPU-Intensive", () =>
        {
            // Simulate CPU work
            var sum = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                sum += i;
            }
            return true;
        }, TimeSpan.FromSeconds(2));
        
        // Monitor memory-intensive timer
        StartMonitoredTimer("Memory-Intensive", () =>
        {
            // Simulate memory allocation
            var data = new byte[1024 * 1024]; // 1MB allocation
            Array.Fill(data, (byte)Random.Shared.Next(256));
            return true;
        }, TimeSpan.FromSeconds(3));
        
        // Monitor I/O-intensive timer
        StartMonitoredTimer("IO-Intensive", async (cancellationToken) =>
        {
            // Simulate I/O work
            var tempFile = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempFile, "Performance test data", cancellationToken);
                var content = await File.ReadAllTextAsync(tempFile, cancellationToken);
                return content.Length > 0;
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }, TimeSpan.FromSeconds(5));
        
        // Metrics reporting timer
        var metricsTimer = DispatcherTimer.Run(() =>
        {
            ReportMetrics();
            return true;
        }, TimeSpan.FromSeconds(10));
        
        _timers.Add(metricsTimer);
    }
    
    private void StartMonitoredTimer(string name, Func<bool> action, TimeSpan interval)
    {
        _metrics[name] = new TimerMetrics { Name = name };
        
        var timer = DispatcherTimer.Run(() =>
        {
            var metrics = _metrics[name];
            var stopwatch = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(false);
            
            try
            {
                var result = action();
                stopwatch.Stop();
                
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                UpdateMetrics(metrics, stopwatch.Elapsed, memoryUsed, true);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                UpdateMetrics(metrics, stopwatch.Elapsed, 0, false);
                _logger.LogError(ex, "Timer {Name} failed", name);
                return true; // Continue running
            }
        }, interval);
        
        _timers.Add(timer);
    }
    
    private void StartMonitoredTimer(string name, Func<CancellationToken, Task<bool>> action, TimeSpan interval)
    {
        _metrics[name] = new TimerMetrics { Name = name };
        
        var timer = DispatcherTimer.Run(async (cancellationToken) =>
        {
            var metrics = _metrics[name];
            var stopwatch = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(false);
            
            try
            {
                var result = await action(cancellationToken);
                stopwatch.Stop();
                
                var memoryAfter = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfter - memoryBefore;
                
                UpdateMetrics(metrics, stopwatch.Elapsed, memoryUsed, true);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                UpdateMetrics(metrics, stopwatch.Elapsed, 0, false);
                _logger.LogError(ex, "Async timer {Name} failed", name);
                return true; // Continue running
            }
        }, interval);
        
        _timers.Add(timer);
    }
    
    private void UpdateMetrics(TimerMetrics metrics, TimeSpan duration, long memoryUsed, bool success)
    {
        metrics.TotalExecutions++;
        if (success) metrics.SuccessfulExecutions++;
        
        metrics.TotalDuration += duration;
        metrics.TotalMemoryUsed += memoryUsed;
        
        if (duration > metrics.MaxDuration)
            metrics.MaxDuration = duration;
        
        if (metrics.MinDuration == TimeSpan.Zero || duration < metrics.MinDuration)
            metrics.MinDuration = duration;
    }
    
    private void ReportMetrics()
    {
        _logger.LogInformation("=== Timer Performance Report ===");
        
        foreach (var (name, metrics) in _metrics)
        {
            if (metrics.TotalExecutions == 0) continue;
            
            var avgDuration = TimeSpan.FromTicks(metrics.TotalDuration.Ticks / metrics.TotalExecutions);
            var successRate = (double)metrics.SuccessfulExecutions / metrics.TotalExecutions;
            var avgMemory = metrics.TotalMemoryUsed / metrics.TotalExecutions;
            
            _logger.LogInformation("{Name}: " +
                "Executions={Total}, Success Rate={SuccessRate:P2}, " +
                "Avg Duration={AvgDuration}ms, Min={MinDuration}ms, Max={MaxDuration}ms, " +
                "Avg Memory={AvgMemory:N0} bytes",
                name, metrics.TotalExecutions, successRate,
                avgDuration.TotalMilliseconds, metrics.MinDuration.TotalMilliseconds, metrics.MaxDuration.TotalMilliseconds,
                avgMemory);
        }
    }
    
    public void Dispose()
    {
        ReportMetrics(); // Final report
        
        foreach (var timer in _timers)
        {
            timer.Dispose();
        }
        
        _timers.Clear();
        _logger.LogInformation("TimerPerformanceMonitor disposed");
    }
}

public class TimerMetrics
{
    public string Name { get; set; } = "";
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public long TotalMemoryUsed { get; set; }
}
```

## Best Practices

### 1. **Resource Management**

```csharp
public class TimerResourceManager : IAsyncDisposable
{
    private readonly List<IDisposable> _timers = new();
    private readonly SemaphoreSlim _timerSemaphore = new(1, 1);
    
    public async Task<IDisposable> AddTimerAsync(Func<IDisposable> timerFactory)
    {
        await _timerSemaphore.WaitAsync();
        try
        {
            var timer = timerFactory();
            _timers.Add(timer);
            return timer;
        }
        finally
        {
            _timerSemaphore.Release();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await _timerSemaphore.WaitAsync();
        try
        {
            foreach (var timer in _timers)
            {
                timer.Dispose();
            }
            _timers.Clear();
        }
        finally
        {
            _timerSemaphore.Release();
            _timerSemaphore.Dispose();
        }
    }
}
```

### 2. **Error Handling**

```csharp
public static class SafeTimerExtensions
{
    public static IDisposable RunSafe(Func<bool> action, TimeSpan interval, 
        ILogger? logger = null, int maxFailures = 5)
    {
        var failureCount = 0;
        
        return DispatcherTimer.Run(() =>
        {
            try
            {
                var result = action();
                failureCount = 0; // Reset on success
                return result;
            }
            catch (Exception ex)
            {
                failureCount++;
                logger?.LogError(ex, "Timer action failed ({FailureCount}/{MaxFailures})", 
                    failureCount, maxFailures);
                
                return failureCount < maxFailures;
            }
        }, interval);
    }
}
```

### 3. **Performance Optimization**

```csharp
public class OptimizedTimerService
{
    public static IDisposable CreateHighFrequencyTimer(Action action, TimeSpan interval)
    {
        // For high-frequency timers, avoid Task.Delay overhead
        if (interval < TimeSpan.FromMilliseconds(100))
        {
            return CreateSpinWaitTimer(action, interval);
        }
        
        return DispatcherTimer.Run(() =>
        {
            action();
            return true;
        }, interval);
    }
    
    private static IDisposable CreateSpinWaitTimer(Action action, TimeSpan interval)
    {
        var cts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            var stopwatch = Stopwatch.StartNew();
            var nextExecution = interval;
            
            while (!cts.Token.IsCancellationRequested)
            {
                if (stopwatch.Elapsed >= nextExecution)
                {
                    action();
                    nextExecution += interval;
                }
                
                Thread.SpinWait(1000); // Minimal CPU usage
            }
        })
        {
            IsBackground = true,
            Name = "HighFrequencyTimer"
        };
        
        thread.Start();
        
        return new ActionDisposable(() =>
        {
            cts.Cancel();
            thread.Join(1000);
            cts.Dispose();
        });
    }
}

public class ActionDisposable : IDisposable
{
    private readonly Action _disposeAction;
    private bool _disposed;
    
    public ActionDisposable(Action disposeAction)
    {
        _disposeAction = disposeAction;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposeAction();
            _disposed = true;
        }
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class DispatcherTimerTests
{
    [Test]
    public async Task Run_SynchronousAction_ExecutesMultipleTimes()
    {
        // Arrange
        var executionCount = 0;
        var expectedExecutions = 3;
        
        // Act
        using var timer = DispatcherTimer.Run(() =>
        {
            executionCount++;
            return executionCount < expectedExecutions;
        }, TimeSpan.FromMilliseconds(50));
        
        // Wait for executions
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        
        // Assert
        Assert.That(executionCount, Is.EqualTo(expectedExecutions));
    }
    
    [Test]
    public async Task RunOnce_Action_ExecutesExactlyOnce()
    {
        // Arrange
        var executionCount = 0;
        
        // Act
        using var timer = DispatcherTimer.RunOnce(() => executionCount++, TimeSpan.FromMilliseconds(50));
        
        await Task.Delay(TimeSpan.FromMilliseconds(150));
        
        // Assert
        Assert.That(executionCount, Is.EqualTo(1));
    }
    
    [Test]
    public async Task Run_WithCancellation_StopsExecution()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var executionCount = 0;
        
        // Act
        using var timer = DispatcherTimer.Run(() =>
        {
            executionCount++;
            return true;
        }, TimeSpan.FromMilliseconds(50), cts.Token);
        
        await Task.Delay(TimeSpan.FromMilliseconds(125)); // Allow ~2 executions
        cts.Cancel();
        await Task.Delay(TimeSpan.FromMilliseconds(100)); // Wait after cancellation
        
        var countAfterCancellation = executionCount;
        await Task.Delay(TimeSpan.FromMilliseconds(100)); // Wait more
        
        // Assert
        Assert.That(executionCount, Is.EqualTo(countAfterCancellation), 
            "Timer should stop executing after cancellation");
    }
    
    [Test]
    public async Task Run_WithState_PassesStateCorrectly()
    {
        // Arrange
        var state = new { Value = 42, Name = "Test" };
        var receivedState = default(object);
        
        // Act
        using var timer = DispatcherTimer.RunOnce<object>(s =>
        {
            receivedState = s;
        }, TimeSpan.FromMilliseconds(50), state);
        
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        
        // Assert
        Assert.That(receivedState, Is.EqualTo(state));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class DispatcherTimerIntegrationTests
{
    [Test]
    public async Task MultipleTimers_RunConcurrently_DoNotInterfere()
    {
        // Arrange
        var timer1Count = 0;
        var timer2Count = 0;
        var timer3Count = 0;
        
        // Act
        using var timer1 = DispatcherTimer.Run(() => { timer1Count++; return timer1Count < 3; }, TimeSpan.FromMilliseconds(30));
        using var timer2 = DispatcherTimer.Run(() => { timer2Count++; return timer2Count < 2; }, TimeSpan.FromMilliseconds(70));
        using var timer3 = DispatcherTimer.Run(() => { timer3Count++; return timer3Count < 4; }, TimeSpan.FromMilliseconds(25));
        
        await Task.Delay(TimeSpan.FromMilliseconds(250));
        
        // Assert
        Assert.That(timer1Count, Is.EqualTo(3));
        Assert.That(timer2Count, Is.EqualTo(2));
        Assert.That(timer3Count, Is.EqualTo(4));
    }
}
```

## See Also

- [DisposableObject](Objects/DisposableObject.md) - Base class providing disposal pattern used in timer cleanup
- [CancellationToken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) - Cancellation support
- [Task](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) - Asynchronous programming model
- [Timer](https://learn.microsoft.com/en-us/dotnet/api/system.threading.timer) - Standard .NET Timer class

---

*Part of the RapidStreamer.BuildingBlocks.Application namespace - providing simplified, thread-safe timer management with proper resource cleanup and cancellation support.*