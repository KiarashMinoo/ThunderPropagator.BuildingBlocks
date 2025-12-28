namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    public enum KafkaSerializerType
    {
        /// <summary>
        /// System.Text.Json
        /// </summary>
        Json = SerializerType.Json,

        /// <summary>
        /// Newtonsoft.Json
        /// </summary>
        NJson = SerializerType.NJson,

        /// <summary>
        /// NetJSON 
        /// </summary>
        NetJson = SerializerType.NetJson,

        SchemaJson,
        Avro
    }
}