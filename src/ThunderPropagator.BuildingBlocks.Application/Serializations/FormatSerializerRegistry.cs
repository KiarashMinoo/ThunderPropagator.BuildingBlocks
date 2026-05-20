using System.Collections.Frozen;
using Ardalis.GuardClauses;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// Default registry that resolves <see cref="IFormatSerializer"/> and <see cref="IFormatDeserializer"/>
    /// instances by <see cref="SerializerType"/> or MIME type using frozen dictionaries for O(1) lookup.
    /// </summary>
    public sealed class FormatSerializerRegistry : IFormatSerializerRegistry
    {
        private readonly FrozenDictionary<SerializerType, IFormatSerializer> _serializersByType;
        private readonly FrozenDictionary<string, IFormatSerializer> _serializersByMediaType;
        private readonly FrozenDictionary<SerializerType, IFormatDeserializer> _deserializersByType;
        private readonly FrozenDictionary<string, IFormatDeserializer> _deserializersByMediaType;

        /// <summary>
        /// Initializes a new registry from the provided serializer and deserializer collections.
        /// When multiple implementations share a media type, the first one in the enumeration wins.
        /// </summary>
        /// <param name="serializers">All registered format serializers.</param>
        /// <param name="deserializers">All registered format deserializers.</param>
        public FormatSerializerRegistry(IEnumerable<IFormatSerializer> serializers, IEnumerable<IFormatDeserializer> deserializers)
        {
            Guard.Against.Null(serializers);
            Guard.Against.Null(deserializers);

            var serializerList = serializers.ToList();
            var deserializerList = deserializers.ToList();

            _serializersByType = serializerList.ToFrozenDictionary(s => s.SerializerType);

            var serializersByMedia = new Dictionary<string, IFormatSerializer>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in serializerList)
            {
                serializersByMedia.TryAdd(s.MediaType, s);
            }
            _serializersByMediaType = serializersByMedia.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

            _deserializersByType = deserializerList.ToFrozenDictionary(d => d.SerializerType);

            var deserializersByMedia = new Dictionary<string, IFormatDeserializer>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in deserializerList)
            {
                deserializersByMedia.TryAdd(d.MediaType, d);
            }
            _deserializersByMediaType = deserializersByMedia.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IFormatSerializer GetSerializer(SerializerType type)
        {
            if (_serializersByType.TryGetValue(type, out var serializer))
            {
                return serializer;
            }

            throw new InvalidOperationException($"No serializer is registered for SerializerType.{type}.");
        }

        /// <inheritdoc/>
        public IFormatSerializer GetSerializer(string mediaType)
        {
            Guard.Against.NullOrWhiteSpace(mediaType);

            if (_serializersByMediaType.TryGetValue(mediaType, out var serializer))
            {
                return serializer;
            }

            throw new InvalidOperationException($"No serializer is registered for media type '{mediaType}'.");
        }

        /// <inheritdoc/>
        public IFormatDeserializer GetDeserializer(SerializerType type)
        {
            if (_deserializersByType.TryGetValue(type, out var deserializer))
            {
                return deserializer;
            }

            throw new InvalidOperationException($"No deserializer is registered for SerializerType.{type}.");
        }

        /// <inheritdoc/>
        public IFormatDeserializer GetDeserializer(string mediaType)
        {
            Guard.Against.NullOrWhiteSpace(mediaType);

            if (_deserializersByMediaType.TryGetValue(mediaType, out var deserializer))
            {
                return deserializer;
            }

            throw new InvalidOperationException($"No deserializer is registered for media type '{mediaType}'.");
        }
    }
}
