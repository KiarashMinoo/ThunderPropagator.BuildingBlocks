namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// MIME-type constants and bidirectional conversions for <see cref="SerializerType"/>.
    /// </summary>
    public static class SerializerMediaTypes
    {
        /// <summary>application/tpg</summary>
        public const string Proprietary = "application/tpg";

        /// <summary>application/json</summary>
        public const string Json = "application/json";

        /// <summary>application/x-protobuf</summary>
        public const string Protobuf = "application/x-protobuf";

        /// <summary>application/x-msgpack</summary>
        public const string MessagePack = "application/x-msgpack";

        /// <summary>application/xml</summary>
        public const string Xml = "application/xml";

        /// <summary>application/yaml</summary>
        public const string Yaml = "application/yaml";

        /// <summary>text/toon</summary>
        public const string Toon = "text/toon";

        /// <summary>
        /// Returns the canonical MIME type for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The serializer type.</param>
        /// <returns>The MIME type string.</returns>
        public static string ToMediaType(this SerializerType type)
        {
            return type switch
            {
                SerializerType.Proprietary => Proprietary,
                SerializerType.Protobuf    => Protobuf,
                SerializerType.MessagePack => MessagePack,
                SerializerType.Xml         => Xml,
                SerializerType.Yaml        => Yaml,
                SerializerType.Toon        => Toon,
                _                          => Json,
            };
        }

        /// <summary>
        /// Returns the <see cref="SerializerType"/> that corresponds to the given <paramref name="mediaType"/>.
        /// Unrecognised values fall back to <see cref="SerializerType.Json"/>.
        /// </summary>
        /// <param name="mediaType">The MIME type string.</param>
        /// <returns>The matching serializer type.</returns>
        public static SerializerType FromMediaType(string mediaType)
        {
            return mediaType switch
            {
                Proprietary => SerializerType.Proprietary,
                Protobuf    => SerializerType.Protobuf,
                MessagePack => SerializerType.MessagePack,
                Xml         => SerializerType.Xml,
                Yaml        => SerializerType.Yaml,
                Toon        => SerializerType.Toon,
                _           => SerializerType.Json,
            };
        }
    }
}
