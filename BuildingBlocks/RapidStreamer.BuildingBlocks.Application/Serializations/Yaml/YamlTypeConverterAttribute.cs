namespace RapidStreamer.BuildingBlocks.Application.Serializations.Yaml
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
    public
#if !DEBUG
        sealed
#endif
        class YamlTypeConverterAttribute : Attribute
    {
        public Type ConverterType { get; }

        public YamlTypeConverterAttribute(Type converterType) => ConverterType = converterType;
    }
}