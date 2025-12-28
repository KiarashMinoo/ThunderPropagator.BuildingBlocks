using System.Net;
using System.Net.Sockets;

namespace ThunderPropagator.UnitTests.SystemResourceMonitor.LoadGenerators;

/// <summary>
/// Generates deterministic network I/O via loopback for testing network metrics.
/// </summary>
public sealed class NetworkIoGenerator : IDisposable
{
    private TcpListener? _listener;
    private readonly List<TcpClient> _clients = [];
    private readonly object _lock = new();
    private volatile bool _shouldStop;

    /// <summary>
    /// Starts a loopback server.
    /// </summary>
    /// <returns>Port number the server is listening on.</returns>
    public int StartServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        var port = ((IPEndPoint)_listener.LocalEndpoint).Port;

        // Start accepting connections in background
        _ = Task.Run(AcceptConnectionsAsync);

        return port;
    }

    private async Task AcceptConnectionsAsync()
    {
        if (_listener == null) return;

        while (!_shouldStop)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                lock (_lock)
                {
                    _clients.Add(client);
                }

                // Echo server
                _ = Task.Run(() => EchoClientAsync(client));
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch
            {
                // Ignore connection errors
            }
        }
    }

    private async Task EchoClientAsync(TcpClient client)
    {
        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[8192];

            while (!_shouldStop && client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0) break;

                await stream.WriteAsync(buffer.AsMemory(0, bytesRead));
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
    }

    /// <summary>
    /// Generates network traffic by sending data to loopback server.
    /// </summary>
    /// <param name="port">Port to connect to.</param>
    /// <param name="durationMs">Duration to maintain traffic.</param>
    /// <param name="throughputMbps">Target throughput in Mbps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task GenerateTrafficAsync(
        int port,
        int durationMs,
        int throughputMbps = 10,
        CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);

        await using var stream = client.GetStream();
        var endTime = DateTime.UtcNow.AddMilliseconds(durationMs);

        // Calculate bytes to send per interval
        var intervalMs = 100;
        var bytesPerInterval = (throughputMbps * 1024 * 1024 * intervalMs) / (8 * 1000);
        var sendBuffer = new byte[Math.Min(bytesPerInterval, 65536)];
        var receiveBuffer = new byte[65536];

        // Fill with data
        Random.Shared.NextBytes(sendBuffer);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            var intervalStart = DateTime.UtcNow;
            long bytesSentThisInterval = 0;

            // Send data for this interval
            while (bytesSentThisInterval < bytesPerInterval &&
                   DateTime.UtcNow < endTime &&
                   !cancellationToken.IsCancellationRequested)
            {
                var toSend = (int)Math.Min(sendBuffer.Length, bytesPerInterval - bytesSentThisInterval);
                await stream.WriteAsync(sendBuffer.AsMemory(0, toSend), cancellationToken);
                bytesSentThisInterval += toSend;

                // Read echo (to avoid buffer filling)
                if (stream.DataAvailable)
                {
                    _ = await stream.ReadAsync(receiveBuffer, cancellationToken);
                }
            }

            // Wait for rest of interval
            var elapsed = (DateTime.UtcNow - intervalStart).TotalMilliseconds;
            var remaining = intervalMs - (int)elapsed;
            if (remaining > 0)
            {
                await Task.Delay(remaining, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Stops the server and closes all connections.
    /// </summary>
    public void Stop()
    {
        _shouldStop = true;

        _listener?.Stop();
        _listener = null;

        lock (_lock)
        {
            foreach (var client in _clients)
            {
                try
                {
                    client.Close();
                }
                catch
                {
                    // Ignore
                }
            }
            _clients.Clear();
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
