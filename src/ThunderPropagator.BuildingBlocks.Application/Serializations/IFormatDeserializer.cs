namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// Deserializes objects from string and byte representations for a specific format.
    /// </summary>
    public interface IFormatDeserializer
    {
        /// <summary>Gets the deserializer format identifier.</summary>
        SerializerType SerializerType { get; }

        /// <summary>Gets the MIME type accepted by this deserializer.</summary>
        string MediaType { get; }

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from a string.
        /// Binary formats (Protobuf, MessagePack) expect a Base64-encoded string.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="data">The serialized string.</param>
        /// <returns>The deserialized instance, or <see langword="null"/> if the input is empty.</returns>
        T? Deserialize<T>(string data);

        /// <summary>
        /// Deserializes <typeparamref name="T"/> from a byte array.
        /// </summary>
        /// <typeparam name="T">The type to deserialize.</typeparam>
        /// <param name="bytes">The serialized bytes.</param>
        /// <returns>The deserialized instance, or <see langword="null"/> if the input is empty.</returns>
        T? Deserialize<T>(byte[] bytes);
    }
}
