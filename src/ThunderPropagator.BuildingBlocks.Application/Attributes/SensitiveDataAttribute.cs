namespace ThunderPropagator.BuildingBlocks.Application.Attributes
{
    /// <summary>
    /// Marks a property, field, or parameter whose value must never appear in
    /// telemetry tags, log entries, or any other observable output channel.
    /// Apply to any member that holds secrets, credentials, key material, or
    /// personally-identifiable information that must not be emitted externally.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public
#if !DEBUG
        sealed
#endif
        class SensitiveDataAttribute : Attribute;
}
