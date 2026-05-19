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
        Json = 0,

        /// <summary>
        /// Newtonsoft.Json — application/json
        /// </summary>
        NJson = 1,

        /// <summary>
        /// NetJSON — application/json
        /// </summary>
        NetJson = 2,

        /// <summary>
        /// protobuf-net — application/x-protobuf
        /// </summary>
        Protobuf = 3,

        /// <summary>
        /// MessagePack-CSharp — application/x-msgpack
        /// </summary>
        MessagePack = 4,

        /// <summary>
        /// System.Xml — application/xml
        /// </summary>
        Xml = 5,

        /// <summary>
        /// YamlDotNet — application/yaml
        /// </summary>
        Yaml = 6,
    }
}