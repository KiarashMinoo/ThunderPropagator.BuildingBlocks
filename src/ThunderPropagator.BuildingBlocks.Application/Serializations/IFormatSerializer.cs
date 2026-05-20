namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// Serializes objects to string and byte representations for a specific format.
    /// </summary>
    public interface IFormatSerializer
    {
        /// <summary>Gets the serializer format identifier.</summary>
        SerializerType SerializerType { get; }

        /// <summary>Gets the MIME type produced by this serializer.</summary>
        string MediaType { get; }

        /// <summary>
        /// Serializes <paramref name="instance"/> to a string representation.
        /// Binary formats (Protobuf, MessagePack) produce a Base64-encoded string.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="instance">The object to serialize.</param>
        /// <returns>The serialized string.</returns>
        string Serialize<T>(T instance);

        /// <summary>
        /// Serializes <paramref name="instance"/> to a byte array.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="instance">The object to serialize.</param>
        /// <returns>The serialized bytes.</returns>
        byte[] SerializeToBytes<T>(T instance);
    }
}
