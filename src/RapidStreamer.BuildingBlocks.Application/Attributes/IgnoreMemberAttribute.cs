namespace RapidStreamer.BuildingBlocks.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public
#if !DEBUG
        sealed
#endif
        class IgnoreMemberAttribute : Attribute;
}