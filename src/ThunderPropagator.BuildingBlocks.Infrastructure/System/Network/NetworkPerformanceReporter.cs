using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Extensions.Logging;
using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.BuildingBlocks.Infrastructure.System.Network
{
    public
#if !DEBUG
        sealed
#endif
        class NetworkPerformanceReporter : DisposableObject
    {
#if NET9_0_OR_GREATER
        private readonly Lock _lock = new();
#else
        private readonly object _lock = new();
#endif

        private class Counters
        {
            public long TcpReceived;
            public long TcpSent;
            public long UdpReceived;
            public long UdpSent;
        }

        private readonly int _processId;
        private readonly string _sessionName;
        private readonly bool _enableUdp;
        private readonly ILogger<NetworkPerformanceReporter>? _logger;
        private DateTime _etwStartTime;
        private TraceEventSession? _etwSession;
        private readonly Counters _counters = new();

        private NetworkPerformanceReporter(int processId, string sessionName, bool enableUdp, ILogger<NetworkPerformanceReporter>? logger = null)
        {
            _processId = processId;
            _sessionName = sessionName;
            _enableUdp = enableUdp;
            _logger = logger;
        }

        public static async Task<NetworkPerformanceReporter> CreateAsync(int processId, string sessionName, bool enableUdp = false, ILogger<NetworkPerformanceReporter>? logger = null, CancellationToken cancellationToken = default)
        {
            var networkPerformanceReporter = new NetworkPerformanceReporter(processId, sessionName, enableUdp, logger);
            await networkPerformanceReporter.InitialiseAsync(cancellationToken);
            return networkPerformanceReporter;
        }

        public static NetworkPerformanceReporter Create(int processId, string sessionName, bool enableUdp = false, CancellationToken cancellationToken = default)
        {
            var networkPerformanceReporter = new NetworkPerformanceReporter(processId, sessionName, enableUdp);
            networkPerformanceReporter.Initialise(cancellationToken);
            return networkPerformanceReporter;
        }

        public NetworkPerformanceData GetNetworkPerformanceData()
        {
            var timeDifferenceInSeconds = (DateTime.UtcNow - _etwStartTime).TotalSeconds;

            NetworkPerformanceData networkData;

            lock (_lock)
            {
                networkData = new NetworkPerformanceData
                {
                    TcpBytesReceived = Convert.ToInt64(_counters.TcpReceived / timeDifferenceInSeconds),
                    TcpBytesSent = Convert.ToInt64(_counters.TcpSent / timeDifferenceInSeconds),

                    UdpBytesReceived = Convert.ToInt64(_counters.UdpReceived / timeDifferenceInSeconds),
                    UdpBytesSent = Convert.ToInt64(_counters.UdpSent / timeDifferenceInSeconds)
                };
            }

            // Reset the counters to get a fresh reading for next time this is called.
            ResetCounters();

            return networkData;
        }

        private void Initialise(CancellationToken cancellationToken = default)
        {
            var thread = new Thread(Start)
            {
                IsBackground = true,
                Name = $"ETW_{_sessionName}"
            };
            thread.Start(cancellationToken);
        }

        private async Task InitialiseAsync(CancellationToken cancellationToken = default)
        {
            var readyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(Start)
            {
                IsBackground = true,
                Name = $"ETW_{_sessionName}"
            };
            thread.Start((readyTcs, cancellationToken));
            await readyTcs.Task.ConfigureAwait(false);
        }

        private void Start(object? state)
        {
            if (!(TraceEventSession.IsElevated() ?? false))
                throw new InvalidOperationException("To turn on ETW events you need to be Administrator, please run from an Admin process.");

            CancellationToken cancellationToken = default;
            TaskCompletionSource? readyTcs = null;

            if (state is (TaskCompletionSource tcs, CancellationToken token))
            {
                readyTcs = tcs;
                cancellationToken = token;
            }
            else if (state is CancellationToken ct)
            {
                cancellationToken = ct;
            }

            try
            {
                ResetCounters();

                using (_etwSession = new TraceEventSession(_sessionName))
                {
                    _etwSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

                    _etwSession.Source.Kernel.TcpIpRecv += HandleTcpReceive;
                    _etwSession.Source.Kernel.TcpIpSend += HandleTcpSend;

                    if (_enableUdp)
                    {
                        _etwSession.Source.Kernel.UdpIpRecv += HandleUdpReceive;
                        _etwSession.Source.Kernel.UdpIpSend += HandleUdpSend;
                    }

                    cancellationToken.Register(() =>
                    {
                        _etwSession.Source.Kernel.TcpIpRecv -= HandleTcpReceive;
                        _etwSession.Source.Kernel.TcpIpSend -= HandleTcpSend;

                        if (_enableUdp)
                        {
                            _etwSession.Source.Kernel.UdpIpRecv -= HandleUdpReceive;
                            _etwSession.Source.Kernel.UdpIpSend -= HandleUdpSend;
                        }

                        _etwSession.Source.StopProcessing();
                    });

                    // Signal that the ETW session is ready before processing events
                    readyTcs?.TrySetResult();

                    _etwSession.Source.Process();
                }
            }
            catch (Exception exception)
            {
                ResetCounters(); // Stop reporting figures
                _logger?.LogError(exception, "ETW session {Session} failed.", _sessionName);
                readyTcs?.TrySetException(exception);
            }

            return;

            void HandleTcpReceive(TcpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_lock)
                    {
                        _counters.TcpReceived += data.size;
                    }
                }
            }

            void HandleTcpSend(TcpIpSendTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_lock)
                    {
                        _counters.TcpSent += data.size;
                    }
                }
            }

            void HandleUdpReceive(UdpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_lock)
                    {
                        _counters.UdpReceived += data.size;
                    }
                }
            }

            void HandleUdpSend(UdpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_lock)
                    {
                        _counters.UdpSent += data.size;
                    }
                }
            }
        }

        private void ResetCounters()
        {
            lock (_lock)
            {
                _counters.TcpSent = 0;
                _counters.TcpReceived = 0;
            }

            _etwStartTime = DateTime.UtcNow;
        }

        protected override void DisposeManagedResources()
        {
            _etwSession?.Dispose();
        }
    }
}
