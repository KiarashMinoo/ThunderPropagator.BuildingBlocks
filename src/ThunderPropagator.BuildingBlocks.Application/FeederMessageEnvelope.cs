using ThunderPropagator.BuildingBlocks.Application.Enums;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// The protocol-level header of a <see cref="FeederMessage"/>. Contains infrastructure-managed
    /// routing and correlation fields and never any user-defined payload.
    /// </summary>
    /// <remarks>
    /// Infrastructure code (routers, brokers, correlation trackers) may accept
    /// <see cref="FeederMessageEnvelope"/> directly instead of the full <see cref="FeederMessage"/>,
    /// reducing coupling to concrete message types.
    /// </remarks>
    public class FeederMessageEnvelope
    {
        /// <summary>Gets or sets the correlation identifier used for distributed tracing.</summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>Gets or sets the internal routing hash key. <see langword="null"/> when unassigned.</summary>
        public int? HashKey { get; set; }

        /// <summary>
        /// Gets or sets whether the message targets one subscriber or all.
        /// Defaults to <see cref="CastType.Multicast"/>.
        /// </summary>
        public CastType CastType { get; set; } = CastType.Multicast;

        /// <summary>Gets or sets the soft-delete flag.</summary>
        public bool IsDeleted { get; set; }
    }
}
