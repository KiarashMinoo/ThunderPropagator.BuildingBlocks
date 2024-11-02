namespace RapidStreamer.BuildingBlocks.Infrastructure.System.Network
{
    public
#if !DEBUG
        sealed
#endif
        class NetworkPerformanceData
    {
        public long TcpBytesReceived { get; set; }
        public long TcpBytesSent { get; set; }
        public long TcpBytesTotal => TcpBytesReceived + TcpBytesSent;

        public long UdpBytesReceived { get; set; }
        public long UdpBytesSent { get; set; }
        public long UdpBytesTotal => UdpBytesReceived + UdpBytesSent;

        public long BytesReceived => TcpBytesReceived + UdpBytesReceived;
        public long BytesSent => TcpBytesSent + UdpBytesSent;
        public long BytesTotal => BytesReceived + BytesSent;
    }
}