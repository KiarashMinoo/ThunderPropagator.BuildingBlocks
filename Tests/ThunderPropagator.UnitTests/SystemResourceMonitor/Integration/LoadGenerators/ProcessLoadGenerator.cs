namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;

/// <summary>
/// Generates process resource load (threads, handles) for testing process metrics.
/// </summary>
public sealed class ProcessLoadGenerator : IDisposable
{
    private readonly List<Thread> _threads = new();
    private readonly List<FileStream> _fileHandles = new();
    private readonly string _tempDirectory;
    private volatile bool _keepThreadsAlive;

    public ProcessLoadGenerator()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"ProcessLoadTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Creates and starts multiple threads that stay alive.
    /// </summary>
    /// <param name="threadCount">Number of threads to create</param>
    public void CreateThreads(int threadCount)
    {
        _keepThreadsAlive = true;

        for (var i = 0; i < threadCount; i++)
        {
            var thread = new Thread(() =>
            {
                while (_keepThreadsAlive)
                {
                    Thread.Sleep(100);
                }
            })
            {
                IsBackground = true,
                Name = $"LoadGenThread_{i}"
            };

            thread.Start();
            _threads.Add(thread);
        }
    }

    /// <summary>
    /// Opens multiple file handles and keeps them open.
    /// </summary>
    /// <param name="handleCount">Number of file handles to open</param>
    public void CreateFileHandles(int handleCount)
    {
        for (var i = 0; i < handleCount; i++)
        {
            var filePath = Path.Combine(_tempDirectory, $"handle_{i}.tmp");
            var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            fs.WriteByte(0); // Ensure file is created
            _fileHandles.Add(fs);
        }
    }

    /// <summary>
    /// Releases all threads.
    /// </summary>
    public void ReleaseThreads()
    {
        _keepThreadsAlive = false;

        foreach (var thread in _threads)
        {
            if (thread.IsAlive)
            {
                thread.Join(TimeSpan.FromSeconds(2));
            }
        }

        _threads.Clear();
    }

    /// <summary>
    /// Closes all file handles.
    /// </summary>
    public void ReleaseFileHandles()
    {
        foreach (var handle in _fileHandles)
        {
            try
            {
                handle.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _fileHandles.Clear();
    }

    public void Dispose()
    {
        ReleaseThreads();
        ReleaseFileHandles();

        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }
}

