namespace ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;

/// <summary>
/// Generates deterministic process resource usage (threads, handles) for testing process metrics.
/// </summary>
public sealed class ProcessLoadGenerator : IDisposable
{
    private readonly List<Thread> _threads = [];
    private readonly List<FileStream> _fileHandles = [];
    private readonly List<ManualResetEvent> _eventHandles = [];
    private readonly object _lock = new();
    private volatile bool _shouldStop;

    /// <summary>
    /// Creates additional threads.
    /// </summary>
    /// <param name="count">Number of threads to create.</param>
    public void CreateThreads(int count)
    {
        lock (_lock)
        {
            for (var i = 0; i < count; i++)
            {
                var thread = new Thread(() =>
                {
                    while (!_shouldStop)
                    {
                        Thread.Sleep(100);
                    }
                })
                {
                    IsBackground = true,
                    Name = $"TestThread-{i}"
                };

                _threads.Add(thread);
                thread.Start();
            }
        }
    }

    /// <summary>
    /// Creates file handles.
    /// </summary>
    /// <param name="count">Number of file handles to open.</param>
    public void CreateFileHandles(int count)
    {
        var tempPath = Path.GetTempPath();

        lock (_lock)
        {
            for (var i = 0; i < count; i++)
            {
                var tempFile = Path.Combine(tempPath, $"handle_test_{Guid.NewGuid():N}.tmp");
                var fileStream = new FileStream(
                    tempFile,
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.DeleteOnClose);

                _fileHandles.Add(fileStream);
            }
        }
    }

    /// <summary>
    /// Creates event handles.
    /// </summary>
    /// <param name="count">Number of event handles to create.</param>
    public void CreateEventHandles(int count)
    {
        lock (_lock)
        {
            for (var i = 0; i < count; i++)
            {
                _eventHandles.Add(new ManualResetEvent(false));
            }
        }
    }

    /// <summary>
    /// Gets current thread count created by this generator.
    /// </summary>
    public int ThreadCount
    {
        get
        {
            lock (_lock)
            {
                return _threads.Count(t => t.IsAlive);
            }
        }
    }

    /// <summary>
    /// Gets current handle count (file + event).
    /// </summary>
    public int HandleCount
    {
        get
        {
            lock (_lock)
            {
                return _fileHandles.Count + _eventHandles.Count;
            }
        }
    }

    /// <summary>
    /// Releases all created resources.
    /// </summary>
    public void ReleaseAll()
    {
        lock (_lock)
        {
            _shouldStop = true;

            // Stop threads
            foreach (var thread in _threads.Where(t => t.IsAlive))
            {
                thread.Join(TimeSpan.FromSeconds(1));
            }
            _threads.Clear();

            // Close file handles
            foreach (var handle in _fileHandles)
            {
                try
                {
                    handle.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
            _fileHandles.Clear();

            // Close event handles
            foreach (var handle in _eventHandles)
            {
                try
                {
                    handle.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
            _eventHandles.Clear();

            _shouldStop = false;
        }
    }

    public void Dispose()
    {
        ReleaseAll();
    }
}
