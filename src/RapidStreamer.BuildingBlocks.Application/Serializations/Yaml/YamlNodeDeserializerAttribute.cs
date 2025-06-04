namespace RapidStreamer.BuildingBlocks.Application.Serializations.Yaml;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
public
#if !DEBUG
        sealed
#endif
    class YamlNodeDeserializerAttribute : Attribute
{
    public Type NodeDeserializer { get; }

    public YamlNodeDeserializerAttribute(Type nodeDeserializer) => NodeDeserializer = nodeDeserializer;
}