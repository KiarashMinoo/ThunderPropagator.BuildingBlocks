# DisposableObject

The `DisposableObject` abstract class provides a comprehensive foundation for implementing proper resource management patterns in .NET. It extends `EquatableObject` and implements both `IDisposable` and `IAsyncDisposable` interfaces, offering complete lifecycle management with events, thread safety, and flexible disposal patterns.

## Overview

```csharp
public abstract class DisposableObject : EquatableObject, IDisposable, IAsyncDisposable
```

`DisposableObject` serves as a robust base class for objects that need deterministic resource cleanup, providing standardized disposal patterns, lifecycle events, and both synchronous and asynchronous disposal capabilities.

## Key Features

- **Dual Disposal Support**: Implements both `IDisposable` and `IAsyncDisposable` patterns
- **Lifecycle Events**: `Disposing` and `Disposed` events for cleanup coordination
- **Thread-Safe Implementation**: Proper synchronization for multi-threaded scenarios
- **Finalizer Protection**: Prevents resource leaks when disposal is missed
- **Anonymous Disposables**: Factory methods for creating disposables from actions
- **Flexible Override Points**: Virtual methods for different resource types
- **Inherited Equality**: Extends `EquatableObject` for value-based equality

## Disposal Lifecycle

### State Properties

#### IsDisposing
Indicates whether the object is currently in the disposing process.

```csharp
protected virtual bool IsDisposing { get; private set; }
```

**Usage:** Check during disposal operations to prevent recursive calls

#### IsDisposed
Indicates whether the object has been completely disposed.

```csharp
protected virtual bool IsDisposed { get; private set; }
```

**Usage:** Guard against using disposed objects and prevent double disposal

### Lifecycle Events

#### Disposing Event
Fired at the beginning of the disposal process, before any resources are released.

```csharp
public event EventHandler? Disposing;
```

**Timing:** Called before `DisposeManagedResources()` and other disposal methods
**Use Case:** Cancel ongoing operations, notify dependent objects

#### Disposed Event
Fired after the disposal process is complete and all resources have been released.

```csharp
public event EventHandler? Disposed;
```

**Timing:** Called after all disposal methods complete and `IsDisposed` is set to true
**Use Case:** Cleanup references, update UI, log disposal completion

## Virtual Disposal Methods

### DisposeManagedResources()
Override to dispose of managed resources (objects implementing IDisposable).

```csharp
protected virtual void DisposeManagedResources()
{
    // TODO: dispose managed state(managed objects)    
}
```

**Called When:** Only during explicit disposal (not from finalizer)
**Thread Safety:** Called within disposal lock

### ReleaseUnmanagedResources()
Override to release unmanaged resources like file handles, native memory, etc.

```csharp
protected virtual void ReleaseUnmanagedResources()
{
    // TODO: free unmanaged resources(unmanaged objects)
}
```

**Called When:** Both explicit disposal and finalization
**Thread Safety:** Must be thread-safe as can be called from finalizer

### SetLargeFieldsAsNull()
Override to set large object references to null, helping garbage collection.

```csharp
protected virtual void SetLargeFieldsAsNull()
{
    // TODO: set large fields to null
}
```

**Called When:** Both explicit disposal and finalization
**Purpose:** Aid garbage collector with large object collection

## Async Disposal Methods

### DisposeManagedResourcesAsync()
Override for asynchronous disposal of managed resources.

```csharp
protected virtual ValueTask DisposeManagedResourcesAsync()
{
    // TODO: dispose managed state(managed objects)
    return ValueTask.CompletedTask;
}
```

**Called When:** Only during explicit async disposal
**Pattern:** Return `ValueTask` for efficiency

### ReleaseUnmanagedResourcesAsync()
Override for asynchronous release of unmanaged resources.

```csharp
protected virtual ValueTask ReleaseUnmanagedResourcesAsync()
{
    // TODO: free unmanaged resources(unmanaged objects)
    return ValueTask.CompletedTask;
}
```

### SetLargeFieldsAsNullAsync()
Override for asynchronous cleanup of large field references.

```csharp
protected virtual ValueTask SetLargeFieldsAsNullAsync()
{
    // TODO: set large fields to null
    return ValueTask.CompletedTask;
}
```

## Factory Methods

### Empty Disposable
Returns a singleton empty disposable that does nothing when disposed.

```csharp
public static IDisposable Empty => EmptyDisposable.Instance;
```

**Use Case:** Null object pattern, placeholder disposables

### Action-Based Disposables
Create disposables from action delegates for lightweight cleanup scenarios.

```csharp
public static IDisposable Create(Action disposeAction);
public static IDisposable Create<TState>(TState state, Action<TState> disposeAction);
```

## Usage Examples

### Basic Resource Management

```csharp
public class DatabaseConnection : DisposableObject
{
    private SqlConnection? _connection;
    private readonly string _connectionString;
    
    public DatabaseConnection(string connectionString)
    {
        _connectionString = connectionString;
        _connection = new SqlConnection(connectionString);
    }
    
    public async Task OpenAsync()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(DatabaseConnection));
            
        if (_connection?.State != ConnectionState.Open)
        {
            await _connection!.OpenAsync();
        }
    }
    
    public async Task<DataTable> ExecuteQueryAsync(string sql)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(DatabaseConnection));
            
        using var command = new SqlCommand(sql, _connection);
        using var adapter = new SqlDataAdapter(command);
        
        var dataTable = new DataTable();
        await Task.Run(() => adapter.Fill(dataTable));
        return dataTable;
    }
    
    protected override void DisposeManagedResources()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        
        base.DisposeManagedResources();
    }
    
    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
        
        await base.DisposeManagedResourcesAsync();
    }
}
```

### File Processing with Events

```csharp
public class FileProcessor : DisposableObject
{
    private FileStream? _fileStream;
    private readonly List<IDisposable> _processors = new();
    private CancellationTokenSource? _cancellationTokenSource;
    
    public event EventHandler<FileProcessedEventArgs>? FileProcessed;
    public event EventHandler<ProcessingErrorEventArgs>? ProcessingError;
    
    public FileProcessor()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        
        // Subscribe to disposal events
        Disposing += OnDisposing;
        Disposed += OnDisposed;
    }
    
    public async Task ProcessFileAsync(string filePath)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(FileProcessor));
            
        try
        {
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            
            // Process file content
            await ProcessFileContentAsync(_fileStream, _cancellationTokenSource.Token);
            
            FileProcessed?.Invoke(this, new FileProcessedEventArgs(filePath));
        }
        catch (Exception ex) when (!IsDisposed)
        {
            ProcessingError?.Invoke(this, new ProcessingErrorEventArgs(filePath, ex));
            throw;
        }
    }
    
    public void AddProcessor(IDisposable processor)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(FileProcessor));
            
        _processors.Add(processor);
    }
    
    private async Task ProcessFileContentAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Process buffer content
            await ProcessBufferAsync(buffer.AsSpan(0, bytesRead), cancellationToken);
        }
    }
    
    private async Task ProcessBufferAsync(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken)
    {
        // Simulate processing
        await Task.Delay(10, cancellationToken);
    }
    
    private void OnDisposing(object? sender, EventArgs e)
    {
        Console.WriteLine("FileProcessor is starting disposal...");
        
        // Cancel any ongoing operations
        _cancellationTokenSource?.Cancel();
    }
    
    private void OnDisposed(object? sender, EventArgs e)
    {
        Console.WriteLine("FileProcessor disposal completed.");
    }
    
    protected override void DisposeManagedResources()
    {
        // Dispose all registered processors
        foreach (var processor in _processors)
        {
            processor?.Dispose();
        }
        _processors.Clear();
        
        _fileStream?.Dispose();
        _fileStream = null;
        
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        
        base.DisposeManagedResources();
    }
    
    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        // Dispose processors that support async disposal
        foreach (var processor in _processors)
        {
            if (processor is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                processor?.Dispose();
        }
        _processors.Clear();
        
        if (_fileStream != null)
        {
            await _fileStream.DisposeAsync();
            _fileStream = null;
        }
        
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        
        await base.DisposeManagedResourcesAsync();
    }
}

public class FileProcessedEventArgs : EventArgs
{
    public string FilePath { get; }
    public DateTime ProcessedAt { get; }
    
    public FileProcessedEventArgs(string filePath)
    {
        FilePath = filePath;
        ProcessedAt = DateTime.UtcNow;
    }
}

public class ProcessingErrorEventArgs : EventArgs
{
    public string FilePath { get; }
    public Exception Exception { get; }
    public DateTime ErrorOccurredAt { get; }
    
    public ProcessingErrorEventArgs(string filePath, Exception exception)
    {
        FilePath = filePath;
        Exception = exception;
        ErrorOccurredAt = DateTime.UtcNow;
    }
}
```

### Anonymous Disposables

```csharp
public class DisposableFactoryExamples
{
    public void DemonstrateAnonymousDisposables()
    {
        // Simple action-based disposable
        using var simpleDisposable = DisposableObject.Create(() => 
        {
            Console.WriteLine("Simple cleanup executed");
        });
        
        // State-based disposable
        var resource = new ExpensiveResource();
        using var stateDisposable = DisposableObject.Create(resource, r => 
        {
            Console.WriteLine($"Cleaning up resource: {r.Name}");
            r.Cleanup();
        });
        
        // Empty disposable for null object pattern
        IDisposable GetOptionalDisposable(bool createReal)
        {
            return createReal 
                ? new RealDisposable() 
                : DisposableObject.Empty;
        }
        
        using var optional1 = GetOptionalDisposable(true);  // Real disposal
        using var optional2 = GetOptionalDisposable(false); // No-op disposal
        
        Console.WriteLine("All disposables will be disposed at end of scope");
    }
    
    public IDisposable CreateTimerDisposable(TimeSpan interval, Action callback)
    {
        var timer = new Timer(_ => callback(), null, interval, interval);
        return DisposableObject.Create(timer, t => t.Dispose());
    }
    
    public IDisposable CreateEventSubscription<T>(IObservable<T> observable, Action<T> handler)
    {
        var subscription = observable.Subscribe(handler);
        return DisposableObject.Create(subscription, s => s.Dispose());
    }
}

public class ExpensiveResource
{
    public string Name { get; } = Guid.NewGuid().ToString();
    
    public void Cleanup()
    {
        Console.WriteLine($"ExpensiveResource {Name} cleaned up");
    }
}

public class RealDisposable : IDisposable
{
    public void Dispose()
    {
        Console.WriteLine("RealDisposable disposed");
    }
}
```

### Unmanaged Resource Management

```csharp
public class NativeMemoryBuffer : DisposableObject
{
    private IntPtr _nativeMemory;
    private readonly int _size;
    private bool _memoryAllocated;
    
    public int Size => IsDisposed ? 0 : _size;
    public IntPtr NativePointer => IsDisposed ? IntPtr.Zero : _nativeMemory;
    
    public NativeMemoryBuffer(int size)
    {
        if (size <= 0)
            throw new ArgumentException("Size must be positive", nameof(size));
            
        _size = size;
        _nativeMemory = Marshal.AllocHGlobal(size);
        _memoryAllocated = true;
        
        // Initialize memory to zero
        unsafe
        {
            byte* ptr = (byte*)_nativeMemory.ToPointer();
            for (int i = 0; i < size; i++)
            {
                ptr[i] = 0;
            }
        }
        
        Console.WriteLine($"Allocated {size} bytes of native memory at 0x{_nativeMemory:X}");
    }
    
    public void WriteBytes(int offset, ReadOnlySpan<byte> data)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(NativeMemoryBuffer));
            
        if (offset < 0 || offset + data.Length > _size)
            throw new ArgumentOutOfRangeException(nameof(offset));
        
        unsafe
        {
            byte* ptr = (byte*)_nativeMemory.ToPointer();
            data.CopyTo(new Span<byte>(ptr + offset, data.Length));
        }
    }
    
    public void ReadBytes(int offset, Span<byte> buffer)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(NativeMemoryBuffer));
            
        if (offset < 0 || offset + buffer.Length > _size)
            throw new ArgumentOutOfRangeException(nameof(offset));
        
        unsafe
        {
            byte* ptr = (byte*)_nativeMemory.ToPointer();
            new ReadOnlySpan<byte>(ptr + offset, buffer.Length).CopyTo(buffer);
        }
    }
    
    protected override void ReleaseUnmanagedResources()
    {
        if (_memoryAllocated && _nativeMemory != IntPtr.Zero)
        {
            Console.WriteLine($"Freeing native memory at 0x{_nativeMemory:X}");
            Marshal.FreeHGlobal(_nativeMemory);
            _nativeMemory = IntPtr.Zero;
            _memoryAllocated = false;
        }
        
        base.ReleaseUnmanagedResources();
    }
    
    protected override void SetLargeFieldsAsNull()
    {
        // No large managed fields to clear in this example
        base.SetLargeFieldsAsNull();
    }
}
```

### Composite Disposable Pattern

```csharp
public class CompositeService : DisposableObject
{
    private readonly List<IDisposable> _disposables = new();
    private readonly List<IAsyncDisposable> _asyncDisposables = new();
    private readonly SemaphoreSlim _operationSemaphore;
    private HttpClient? _httpClient;
    private ILogger? _logger;
    
    public CompositeService(ILogger logger)
    {
        _logger = logger;
        _operationSemaphore = new SemaphoreSlim(1, 1);
        _httpClient = new HttpClient();
        
        // Register disposables for automatic cleanup
        _disposables.Add(_operationSemaphore);
        _disposables.Add(_httpClient);
        
        _logger.LogInformation("CompositeService initialized");
    }
    
    public void AddDisposable(IDisposable disposable)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(CompositeService));
            
        _disposables.Add(disposable);
    }
    
    public void AddAsyncDisposable(IAsyncDisposable asyncDisposable)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(CompositeService));
            
        _asyncDisposables.Add(asyncDisposable);
    }
    
    public async Task<string> PerformOperationAsync(string url)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(CompositeService));
        
        await _operationSemaphore.WaitAsync();
        try
        {
            _logger?.LogInformation("Performing operation for URL: {Url}", url);
            var response = await _httpClient!.GetStringAsync(url);
            _logger?.LogInformation("Operation completed successfully");
            return response;
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }
    
    protected override void OnDisposing()
    {
        _logger?.LogInformation("CompositeService is disposing...");
        base.OnDisposing();
    }
    
    protected override void OnDisposed()
    {
        _logger?.LogInformation("CompositeService disposal completed");
        base.OnDisposed();
    }
    
    protected override void DisposeManagedResources()
    {
        // Dispose all registered disposables
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing resource");
            }
        }
        _disposables.Clear();
        
        _httpClient = null;
        _logger = null;
        
        base.DisposeManagedResources();
    }
    
    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        // Dispose async disposables first
        foreach (var asyncDisposable in _asyncDisposables)
        {
            try
            {
                await asyncDisposable.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing async resource");
            }
        }
        _asyncDisposables.Clear();
        
        // Then dispose regular disposables
        foreach (var disposable in _disposables)
        {
            try
            {
                if (disposable is IAsyncDisposable asyncDisp)
                    await asyncDisp.DisposeAsync();
                else
                    disposable?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing resource");
            }
        }
        _disposables.Clear();
        
        _httpClient = null;
        _logger = null;
        
        await base.DisposeManagedResourcesAsync();
    }
}
```

### Thread-Safe Disposal

```csharp
public class ThreadSafeService : DisposableObject
{
    private readonly object _lock = new();
    private readonly Dictionary<string, object> _resources = new();
    private volatile bool _operationsInProgress;
    private int _activeOperationCount;
    
    public async Task<T> PerformOperationAsync<T>(string operationId, Func<Task<T>> operation)
    {
        // Increment operation count atomically
        if (Interlocked.Increment(ref _activeOperationCount) == 1)
            _operationsInProgress = true;
            
        try
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ThreadSafeService));
            }
            
            return await operation();
        }
        finally
        {
            if (Interlocked.Decrement(ref _activeOperationCount) == 0)
                _operationsInProgress = false;
        }
    }
    
    public void AddResource(string key, object resource)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ThreadSafeService));
            
        lock (_lock)
        {
            if (IsDisposed) // Double-check after acquiring lock
                throw new ObjectDisposedException(nameof(ThreadSafeService));
                
            _resources[key] = resource;
        }
    }
    
    public T? GetResource<T>(string key) where T : class
    {
        if (IsDisposed)
            return null;
            
        lock (_lock)
        {
            return IsDisposed ? null : _resources.TryGetValue(key, out var resource) ? resource as T : null;
        }
    }
    
    protected override void DisposeManagedResources()
    {
        lock (_lock)
        {
            // Wait for operations to complete
            var timeout = TimeSpan.FromSeconds(30);
            var stopwatch = Stopwatch.StartNew();
            
            while (_operationsInProgress && stopwatch.Elapsed < timeout)
            {
                Monitor.Wait(_lock, 100);
            }
            
            if (_operationsInProgress)
            {
                Console.WriteLine("Warning: Disposing while operations are still in progress");
            }
            
            // Dispose all resources
            foreach (var kvp in _resources)
            {
                if (kvp.Value is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error disposing resource {kvp.Key}: {ex.Message}");
                    }
                }
            }
            
            _resources.Clear();
        }
        
        base.DisposeManagedResources();
    }
    
    protected override async ValueTask DisposeManagedResourcesAsync()
    {
        // For async disposal, we need to be more careful about the lock
        List<IAsyncDisposable> asyncDisposables;
        List<IDisposable> regularDisposables;
        
        lock (_lock)
        {
            // Wait for operations to complete (simplified for example)
            var timeout = TimeSpan.FromSeconds(30);
            var stopwatch = Stopwatch.StartNew();
            
            while (_operationsInProgress && stopwatch.Elapsed < timeout)
            {
                Monitor.Wait(_lock, 100);
            }
            
            // Separate async and regular disposables
            asyncDisposables = _resources.Values.OfType<IAsyncDisposable>().ToList();
            regularDisposables = _resources.Values.OfType<IDisposable>()
                .Where(d => d is not IAsyncDisposable).ToList();
            
            _resources.Clear();
        }
        
        // Dispose async disposables outside the lock
        foreach (var asyncDisposable in asyncDisposables)
        {
            try
            {
                await asyncDisposable.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing async resource: {ex.Message}");
            }
        }
        
        // Dispose regular disposables
        foreach (var disposable in regularDisposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing resource: {ex.Message}");
            }
        }
        
        await base.DisposeManagedResourcesAsync();
    }
}
```

## Testing Strategies

### Unit Tests

```csharp
[TestFixture]
public class DisposableObjectTests
{
    private class TestDisposable : DisposableObject
    {
        public bool ManagedResourcesDisposed { get; private set; }
        public bool UnmanagedResourcesReleased { get; private set; }
        public bool LargeFieldsCleared { get; private set; }
        public bool AsyncManagedResourcesDisposed { get; private set; }
        
        protected override void DisposeManagedResources()
        {
            ManagedResourcesDisposed = true;
            base.DisposeManagedResources();
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            UnmanagedResourcesReleased = true;
            base.ReleaseUnmanagedResources();
        }
        
        protected override void SetLargeFieldsAsNull()
        {
            LargeFieldsCleared = true;
            base.SetLargeFieldsAsNull();
        }
        
        protected override ValueTask DisposeManagedResourcesAsync()
        {
            AsyncManagedResourcesDisposed = true;
            return base.DisposeManagedResourcesAsync();
        }
    }
    
    [Test]
    public void Dispose_CallsAllDisposalMethods()
    {
        // Arrange
        var disposable = new TestDisposable();
        
        // Act
        disposable.Dispose();
        
        // Assert
        Assert.That(disposable.IsDisposed, Is.True);
        Assert.That(disposable.ManagedResourcesDisposed, Is.True);
        Assert.That(disposable.UnmanagedResourcesReleased, Is.True);
        Assert.That(disposable.LargeFieldsCleared, Is.True);
    }
    
    [Test]
    public async Task DisposeAsync_CallsAllDisposalMethods()
    {
        // Arrange
        var disposable = new TestDisposable();
        
        // Act
        await disposable.DisposeAsync();
        
        // Assert
        Assert.That(disposable.IsDisposed, Is.True);
        Assert.That(disposable.ManagedResourcesDisposed, Is.True);
        Assert.That(disposable.AsyncManagedResourcesDisposed, Is.True);
        Assert.That(disposable.UnmanagedResourcesReleased, Is.True);
        Assert.That(disposable.LargeFieldsCleared, Is.True);
    }
    
    [Test]
    public void Dispose_FiresEventsInCorrectOrder()
    {
        // Arrange
        var disposable = new TestDisposable();
        var events = new List<string>();
        
        disposable.Disposing += (_, _) => events.Add("Disposing");
        disposable.Disposed += (_, _) => events.Add("Disposed");
        
        // Act
        disposable.Dispose();
        
        // Assert
        Assert.That(events, Is.EqualTo(new[] { "Disposing", "Disposed" }));
    }
    
    [Test]
    public void DoubleDispose_DoesNotThrow()
    {
        // Arrange
        var disposable = new TestDisposable();
        
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            disposable.Dispose();
            disposable.Dispose(); // Second disposal should be safe
        });
        
        Assert.That(disposable.IsDisposed, Is.True);
    }
    
    [Test]
    public void EmptyDisposable_DoesNotThrow()
    {
        // Arrange & Act & Assert
        Assert.DoesNotThrow(() =>
        {
            var empty = DisposableObject.Empty;
            empty.Dispose();
        });
    }
    
    [Test]
    public void CreateWithAction_ExecutesActionOnDispose()
    {
        // Arrange
        bool actionExecuted = false;
        var disposable = DisposableObject.Create(() => actionExecuted = true);
        
        // Act
        disposable.Dispose();
        
        // Assert
        Assert.That(actionExecuted, Is.True);
    }
    
    [Test]
    public void CreateWithState_ExecutesActionWithStateOnDispose()
    {
        // Arrange
        var state = new { Value = 42 };
        object? receivedState = null;
        
        var disposable = DisposableObject.Create(state, s => receivedState = s);
        
        // Act
        disposable.Dispose();
        
        // Assert
        Assert.That(receivedState, Is.SameAs(state));
    }
}
```

### Integration Tests

```csharp
[TestFixture]
public class DisposableObjectIntegrationTests
{
    [Test]
    public async Task DatabaseConnection_ProperlyManagesResources()
    {
        // This would use a test database
        var connectionString = "Server=localhost;Database=TestDB;Integrated Security=true;";
        
        await using var connection = new DatabaseConnection(connectionString);
        
        // Use connection
        await connection.OpenAsync();
        var result = await connection.ExecuteQueryAsync("SELECT 1 as TestValue");
        
        Assert.That(result.Rows.Count, Is.EqualTo(1));
        
        // Disposal happens automatically via await using
    }
    
    [Test]
    public void FileProcessor_HandlesDisposalDuringProcessing()
    {
        var processor = new FileProcessor();
        var disposalCompleted = false;
        
        processor.Disposed += (_, _) => disposalCompleted = true;
        
        // Start processing (this would be async in real scenario)
        var processingTask = Task.Run(async () =>
        {
            try
            {
                await processor.ProcessFileAsync("test-file.txt");
            }
            catch (ObjectDisposedException)
            {
                // Expected when disposed during processing
            }
        });
        
        // Dispose while processing
        Thread.Sleep(100); // Allow processing to start
        processor.Dispose();
        
        // Wait for processing to complete
        Assert.DoesNotThrowAsync(async () => await processingTask);
        Assert.That(disposalCompleted, Is.True);
    }
}
```

## Best Practices

### 1. Always Call Base Methods
```csharp
protected override void DisposeManagedResources()
{
    // Your disposal logic here
    _myResource?.Dispose();
    
    // Always call base
    base.DisposeManagedResources();
}
```

### 2. Check IsDisposed in Public Methods
```csharp
public void DoWork()
{
    if (IsDisposed)
        throw new ObjectDisposedException(GetType().Name);
        
    // Perform work
}
```

### 3. Use Disposal Events for Coordination
```csharp
public MyService()
{
    Disposing += OnDisposing;
}

private void OnDisposing(object? sender, EventArgs e)
{
    // Cancel operations, notify dependents, etc.
    _cancellationTokenSource?.Cancel();
}
```

### 4. Handle Exceptions in Disposal
```csharp
protected override void DisposeManagedResources()
{
    try
    {
        _resource1?.Dispose();
    }
    catch (Exception ex)
    {
        // Log but don't rethrow
        _logger?.LogError(ex, "Error disposing resource1");
    }
    
    try
    {
        _resource2?.Dispose();
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "Error disposing resource2");
    }
    
    base.DisposeManagedResources();
}
```

### 5. Prefer ValueTask for Async Disposal
```csharp
protected override ValueTask DisposeManagedResourcesAsync()
{
    if (_asyncResource == null)
        return ValueTask.CompletedTask; // Efficient for no-op
        
    return _asyncResource.DisposeAsync();
}
```

## Error Handling

### Common Disposal Patterns

```csharp
public class RobustDisposableService : DisposableObject
{
    private readonly ILogger _logger;
    private readonly List<IDisposable> _resources = new();
    
    protected override void DisposeManagedResources()
    {
        var exceptions = new List<Exception>();
        
        foreach (var resource in _resources)
        {
            try
            {
                resource?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing resource of type {Type}", resource?.GetType().Name);
                exceptions.Add(ex);
            }
        }
        
        _resources.Clear();
        
        if (exceptions.Count > 0)
        {
            // Consider whether to throw AggregateException or just log
            _logger.LogWarning("Disposal completed with {ErrorCount} errors", exceptions.Count);
        }
        
        base.DisposeManagedResources();
    }
}
```

## Performance Considerations

### Memory Management

```csharp
public class PerformantDisposable : DisposableObject
{
    private byte[]? _largeBuffer;
    private List<object>? _largeCollection;
    
    protected override void SetLargeFieldsAsNull()
    {
        // Help GC by clearing large object references
        _largeBuffer = null;
        _largeCollection?.Clear();
        _largeCollection = null;
        
        base.SetLargeFieldsAsNull();
    }
    
    protected override void DisposeManagedResources()
    {
        // Dispose smaller objects first, large objects last
        _smallResource?.Dispose();
        
        base.DisposeManagedResources();
    }
}
```

## See Also

- [EquatableObject](EquatableObject.md) - Base class providing value-based equality
- [ImmutableObject](ImmutableObject.md) - Immutable object patterns with disposal
- [NotifiableObject](NotifiableObject.md) - Change notification with proper disposal
- [CompressedObject](CompressedObject.md) - Readonly struct for compressed data
- [ObjectHelper](../Helpers/ObjectHelper.md) - Object manipulation utilities

---

*Part of the RapidStreamer.BuildingBlocks.Application.Objects namespace - providing comprehensive resource management infrastructure for .NET applications.*