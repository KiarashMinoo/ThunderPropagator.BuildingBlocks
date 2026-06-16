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
        partial class NetworkPerformanceReporter : DisposableObject
    {
        private long _tcpReceived;
        private long _tcpSent;
        private long _udpReceived;
        private long _udpSent;

        private readonly int _processId;
        private readonly string _sessionName;
        private readonly bool _enableUdp;
        private readonly ILogger<NetworkPerformanceReporter>? _logger;
        private DateTime _etwStartTime;
        private TraceEventSession? _etwSession;

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

            var networkData = new NetworkPerformanceData
            {
                TcpBytesReceived = Convert.ToInt64(Interlocked.Read(ref _tcpReceived) / timeDifferenceInSeconds),
                TcpBytesSent = Convert.ToInt64(Interlocked.Read(ref _tcpSent) / timeDifferenceInSeconds),
                UdpBytesReceived = Convert.ToInt64(Interlocked.Read(ref _udpReceived) / timeDifferenceInSeconds),
                UdpBytesSent = Convert.ToInt64(Interlocked.Read(ref _udpSent) / timeDifferenceInSeconds)
            };

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
                if (_logger is not null)
                    Log.EtwSessionFailed(_logger, exception, _sessionName);
                readyTcs?.TrySetException(exception);
            }

            return;

            void HandleTcpReceive(TcpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    Interlocked.Add(ref _tcpReceived, data.size);
                }
            }

            void HandleTcpSend(TcpIpSendTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    Interlocked.Add(ref _tcpSent, data.size);
                }
            }

            void HandleUdpReceive(UdpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    Interlocked.Add(ref _udpReceived, data.size);
                }
            }

            void HandleUdpSend(UdpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    Interlocked.Add(ref _udpSent, data.size);
                }
            }
        }

        private void ResetCounters()
        {
            Interlocked.Exchange(ref _tcpReceived, 0);
            Interlocked.Exchange(ref _tcpSent, 0);
            Interlocked.Exchange(ref _udpReceived, 0);
            Interlocked.Exchange(ref _udpSent, 0);

            _etwStartTime = DateTime.UtcNow;
        }

        protected override void DisposeManagedResources()
        {
            _etwSession?.Dispose();
        }

        /// <summary>Source-generated high-performance logging methods for <see cref="NetworkPerformanceReporter"/>.</summary>
        private static partial class Log
        {
            /// <summary>Logs an ETW session failure at <see cref="LogLevel.Error"/> level.</summary>
            [LoggerMessage(EventId = 2001, Level = LogLevel.Error,
                Message = "ETW session {Session} failed.")]
            public static partial void EtwSessionFailed(ILogger logger, Exception exception, string session);
        }
    }
}
