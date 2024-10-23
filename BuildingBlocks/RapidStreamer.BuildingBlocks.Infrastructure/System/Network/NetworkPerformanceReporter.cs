using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.BuildingBlocks.Infrastructure.System.Network
{
    public
#if !DEBUG
        sealed
#endif
        class NetworkPerformanceReporter : DisposableObject
    {
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
        private DateTime _etwStartTime;
        private TraceEventSession? _etwSession;
        private readonly Counters _counters = new();

        private NetworkPerformanceReporter(int processId, string sessionName, bool enableUdp)
        {
            _processId = processId;
            _sessionName = sessionName;
            _enableUdp = enableUdp;
        }

        public static NetworkPerformanceReporter Create(int processId, string sessionName, bool enableUdp = false, CancellationToken cancellationToken = default)
        {
            var networkPerformancePresenter = new NetworkPerformanceReporter(processId, sessionName, enableUdp);
            networkPerformancePresenter.Initialise(cancellationToken);
            return networkPerformancePresenter;
        }

        public NetworkPerformanceData GetNetworkPerformanceData()
        {
            var timeDifferenceInSeconds = (DateTime.UtcNow - _etwStartTime).TotalSeconds;

            NetworkPerformanceData networkData;

            lock (_counters)
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
            new Thread(Start).Start(cancellationToken);
        }

        private void Start(object? state)
        {
            if (!(TraceEventSession.IsElevated() ?? false))
                throw new InvalidOperationException("To turn on ETW events you need to be Administrator, please run from an Admin process.");

            if (state is not CancellationToken cancellationToken)
                throw new InvalidOperationException();

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

                    _etwSession.Source.Process();
                }
            }
            catch
            {
                ResetCounters(); // Stop reporting figures
                // Probably should log the exception
            }

            return;

            void HandleTcpReceive(TcpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_counters)
                    {
                        _counters.TcpReceived += data.size;
                    }
                }
            }

            void HandleTcpSend(TcpIpSendTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_counters)
                    {
                        _counters.TcpSent += data.size;
                    }
                }
            }

            void HandleUdpReceive(UdpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_counters)
                    {
                        _counters.UdpReceived += data.size;
                    }
                }
            }

            void HandleUdpSend(UdpIpTraceData data)
            {
                if (data.ProcessID == _processId)
                {
                    lock (_counters)
                    {
                        _counters.UdpSent += data.size;
                    }
                }
            }
        }

        private void ResetCounters()
        {
            lock (_counters)
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