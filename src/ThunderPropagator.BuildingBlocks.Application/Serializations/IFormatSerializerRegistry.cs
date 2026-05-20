namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// Provides lookup of format serializers and deserializers by <see cref="SerializerType"/> or MIME type.
    /// </summary>
    public interface IFormatSerializerRegistry
    {
        /// <summary>
        /// Returns the <see cref="IFormatSerializer"/> registered for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The serializer type.</param>
        /// <returns>The matching serializer.</returns>
        /// <exception cref="InvalidOperationException">No serializer is registered for <paramref name="type"/>.</exception>
        IFormatSerializer GetSerializer(SerializerType type);

        /// <summary>
        /// Returns the primary <see cref="IFormatSerializer"/> registered for <paramref name="mediaType"/>.
        /// When multiple serializers share a media type (e.g. <c>application/json</c>),
        /// the first registered wins.
        /// </summary>
        /// <param name="mediaType">The MIME type string.</param>
        /// <returns>The matching serializer.</returns>
        /// <exception cref="InvalidOperationException">No serializer is registered for <paramref name="mediaType"/>.</exception>
        IFormatSerializer GetSerializer(string mediaType);

        /// <summary>
        /// Returns the <see cref="IFormatDeserializer"/> registered for <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The serializer type.</param>
        /// <returns>The matching deserializer.</returns>
        /// <exception cref="InvalidOperationException">No deserializer is registered for <paramref name="type"/>.</exception>
        IFormatDeserializer GetDeserializer(SerializerType type);

        /// <summary>
        /// Returns the primary <see cref="IFormatDeserializer"/> registered for <paramref name="mediaType"/>.
        /// When multiple deserializers share a media type, the first registered wins.
        /// </summary>
        /// <param name="mediaType">The MIME type string.</param>
        /// <returns>The matching deserializer.</returns>
        /// <exception cref="InvalidOperationException">No deserializer is registered for <paramref name="mediaType"/>.</exception>
        IFormatDeserializer GetDeserializer(string mediaType);
    }
}
