using System.Net;
using System.Net.Sockets;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.Integration.LoadGenerators;

/// <summary>
/// Generates network I/O load for testing network metrics.
/// </summary>
public sealed class NetworkIoGenerator : IDisposable
{
    private TcpListener? _listener;
    private readonly List<TcpClient> _clients = new();
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private volatile bool _isRunning;
    private int _port;

    /// <summary>
    /// Starts a loopback TCP server and begins generating network traffic.
    /// </summary>
    /// <param name="port">Port to listen on (0 for automatic)</param>
    /// <param name="clientCount">Number of concurrent clients</param>
    /// <param name="bytesPerSecond">Target bytes per second per client</param>
    public async Task StartAsync(int port = 0, int clientCount = 2, int bytesPerSecond = 1_000_000)
    {
        if (_isRunning)
            throw new InvalidOperationException("Network I/O generator is already running");

        _cts = new CancellationTokenSource();
        _isRunning = true;

        // Start server
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        _port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        _serverTask = Task.Run(async () => await RunServerAsync(_cts.Token), _cts.Token);

        // Wait a bit for server to start
        await Task.Delay(100);

        // Start clients
        for (var i = 0; i < clientCount; i++)
        {
            _ = Task.Run(async () => await RunClientAsync(bytesPerSecond, _cts.Token), _cts.Token);
        }
    }

    /// <summary>
    /// Performs a single large data transfer for testing throughput.
    /// </summary>
    /// <param name="sizeInMb">Amount of data to transfer in MB</param>
    public async Task TransferDataAsync(int sizeInMb = 10)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port);

        await using var stream = client.GetStream();
        var buffer = new byte[64 * 1024]; // 64 KB buffer
        var random = new Random();
        var totalBytes = sizeInMb * 1024 * 1024;
        var bytesTransferred = 0;

        while (bytesTransferred < totalBytes)
        {
            var bytesToSend = Math.Min(buffer.Length, totalBytes - bytesTransferred);
            random.NextBytes(buffer);
            await stream.WriteAsync(buffer.AsMemory(0, bytesToSend));
            bytesTransferred += bytesToSend;
        }

        await stream.FlushAsync();
    }

    /// <summary>
    /// Stops network I/O generation.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cts?.Cancel();

        // Close all clients
        foreach (var client in _clients)
        {
            try
            {
                client.Close();
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
        _clients.Clear();

        // Stop server
        _listener?.Stop();

        // Wait for tasks to complete
        if (_serverTask != null)
        {
            try
            {
                await _serverTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                // Ignore timeout
            }
        }
    }

    private async Task RunServerAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024]; // 64 KB buffer

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_listener == null || !_listener.Server.IsBound)
                    break;

                // Accept client with timeout
                var acceptTask = _listener.AcceptTcpClientAsync(cancellationToken);
                var client = await acceptTask;

                // Handle client in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await using var stream = client.GetStream();
                        
                        while (!cancellationToken.IsCancellationRequested && client.Connected)
                        {
                            var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                            if (bytesRead == 0)
                                break;

                            // Echo back
                            await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        }
                    }
                    catch
                    {
                        // Ignore client errors
                    }
                    finally
                    {
                        client.Close();
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore accept errors
            }
        }
    }

    private async Task RunClientAsync(int bytesPerSecond, CancellationToken cancellationToken)
    {
        try
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, _port, cancellationToken);
            _clients.Add(client);

            await using var stream = client.GetStream();
            var buffer = new byte[8192]; // 8 KB buffer
            var random = new Random();
            var delayMs = (buffer.Length * 1000) / bytesPerSecond;

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                try
                {
                    // Send data
                    random.NextBytes(buffer);
                    await stream.WriteAsync(buffer, cancellationToken);

                    // Read echo
                    var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                    if (bytesRead == 0)
                        break;

                    // Rate limiting
                    if (delayMs > 0)
                        await Task.Delay(delayMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Connection error, exit
                    break;
                }
            }
        }
        catch
        {
            // Ignore client startup errors
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _cts?.Dispose();
    }
}

