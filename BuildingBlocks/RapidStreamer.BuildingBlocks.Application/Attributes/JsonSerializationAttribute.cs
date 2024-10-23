namespace RapidStreamer.BuildingBlocks.Application.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public
#if !DEBUG
        sealed
#endif
        class JsonSerializationAttribute : Attribute
    {
        public bool CamelCase { get; set; } = true;
    }
}