using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml
{
    public
#if !DEBUG
        sealed
#endif
        class YamlSerializerSettings
    {
        public ScalarStyle? Style { get; set; }
        public bool JsonCompatible { get; set; } = false;
        public bool IgnoreFields { get; set; }
        public bool IncludeNonPublicProperties { get; set; }
        public bool EnablePrivateConstructors { get; set; }
        public INamingConvention? NamingConvention { get; set; }
        public INamingConvention? EnumNamingConvention { get; set; }
        public ITypeResolver? TypeResolver { get; set; }

        public IEnumerable<IYamlTypeConverter>? TypeConverters { get; set; }
        public IEnumerable<INodeDeserializer>? NodeDeserializers { get; set; }
    }
}