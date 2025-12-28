namespace ThunderPropagator.BuildingBlocks.Infrastructure.HealthChecks
{
    public
#if !DEBUG
        sealed
#endif
        class ActiveMQHealthCheckOptions
    {
        public string BrokerUri { get; set; } = null!;
        public string? ClientId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string Queue { get; set; } = null!;
    }
}