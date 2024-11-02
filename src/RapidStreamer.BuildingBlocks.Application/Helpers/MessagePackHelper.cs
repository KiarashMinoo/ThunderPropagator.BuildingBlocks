using MessagePack;
using System.Diagnostics;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class MessagePackHelper
    {
        public static string ToMessagePackJson(this object instance, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
#if DEBUG
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(ToMessagePackJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif

            return MessagePackSerializer.SerializeToJson(instance, serializerOptions, cancellationToken);
        }

        public static Stream ToMessagePack(this object instance, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
#if DEBUG
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(ToMessagePack)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            MemoryStream memoryStream = new();
            MessagePackSerializer.Serialize(memoryStream, instance, serializerOptions, cancellationToken);
            return memoryStream;
        }

        public static T FromMessagePack<T>(this Stream stream, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
#if DEBUG
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(FromMessagePack)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            return MessagePackSerializer.Deserialize<T>(stream, serializerOptions, cancellationToken);
        }

        public static T FromMessagePack<T>(this byte[] bytes, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            using MemoryStream memoryStream = new(bytes);
            return FromMessagePack<T>(memoryStream, serializerOptions, cancellationToken);
        }

        public static T FromMessagePackJson<T>(this string json, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
#if DEBUG
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(FromMessagePackJson)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            var bytes = MessagePackSerializer.ConvertFromJson(json, serializerOptions, cancellationToken);
            using MemoryStream memoryStream = new(bytes);
            return FromMessagePack<T>(memoryStream, serializerOptions, cancellationToken);
        }
    }
}