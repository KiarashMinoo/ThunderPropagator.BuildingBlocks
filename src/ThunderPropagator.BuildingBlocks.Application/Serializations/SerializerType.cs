namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// Identifies the serialization library to use.
    /// </summary>
    public enum SerializerType
    {
        /// <summary>
        /// System.Text.Json — application/json
        /// </summary>
        Proprietary = 0,

        /// <summary>
        /// System.Text.Json — application/json
        /// </summary>
        Json = 1,

        /// <summary>
        /// Newtonsoft.Json — application/json
        /// </summary>
        NJson = 2,

        /// <summary>
        /// NetJSON — application/json
        /// </summary>
        NetJson = 3,

        /// <summary>
        /// protobuf-net — application/x-protobuf
        /// </summary>
        Protobuf = 4,

        /// <summary>
        /// MessagePack-CSharp — application/x-msgpack
        /// </summary>
        MessagePack = 5,

        /// <summary>
        /// System.Xml — application/xml
        /// </summary>
        Xml = 6,

        /// <summary>
        /// YamlDotNet — application/yaml
        /// </summary>
        Yaml = 7,

        /// <summary>
        /// YamlDotNet — text/toon
        /// </summary>
        Toon = 8,
    }
}
