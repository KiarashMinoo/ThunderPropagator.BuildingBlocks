using MessagePack;
using System.Diagnostics;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class MessagePackHelper
    {
        public static string ToMessagePackJson<T>(this T instance, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(ToMessagePackJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var originals = SensitiveDataEncryption.EncryptInPlace(instance);
            try
            {
                return MessagePackSerializer.SerializeToJson(instance, serializerOptions, cancellationToken);
            }
            finally
            {
                if (originals is not null)
                    SensitiveDataEncryption.RevertEncryption(instance, originals);
            }
        }

        public static Stream ToMessagePack<T>(this T instance, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(ToMessagePack)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            MemoryStream memoryStream = new();
            var originals = SensitiveDataEncryption.EncryptInPlace(instance);
            try
            {
                MessagePackSerializer.Serialize(memoryStream, instance, serializerOptions, cancellationToken);
            }
            finally
            {
                if (originals is not null)
                    SensitiveDataEncryption.RevertEncryption(instance, originals);
            }
            return memoryStream;
        }

        public static byte[] ToMessagePackBytes<T>(this T instance, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(ToMessagePackBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var originals = SensitiveDataEncryption.EncryptInPlace(instance);
            try
            {
                return MessagePackSerializer.Serialize(instance, serializerOptions, cancellationToken);
            }
            finally
            {
                if (originals is not null)
                    SensitiveDataEncryption.RevertEncryption(instance, originals);
            }
        }

        public static string ToMessagePackBase64<T>(this T instance, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(ToMessagePackBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = instance.ToMessagePackBytes(serializerOptions, cancellationToken);
            return Convert.ToBase64String(bytes);
        }

        public static T FromMessagePack<T>(this Stream stream, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(FromMessagePack)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var result = MessagePackSerializer.Deserialize<T>(stream, serializerOptions, cancellationToken);
            SensitiveDataEncryption.DecryptInPlace(result);
            return result;
        }

        public static T FromMessagePack<T>(this byte[] bytes, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(FromMessagePack)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using MemoryStream memoryStream = new(bytes);
            return FromMessagePack<T>(memoryStream, serializerOptions, cancellationToken);
        }

        public static T FromMessagePackJson<T>(this string json, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(FromMessagePackJson)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = MessagePackSerializer.ConvertFromJson(json, serializerOptions, cancellationToken);
            using MemoryStream memoryStream = new(bytes);
            return FromMessagePack<T>(memoryStream, serializerOptions, cancellationToken);
        }

        public static T FromMessagePackBase64<T>(this string base64String, MessagePackSerializerOptions? serializerOptions = null, CancellationToken cancellationToken = default)
        {
            const string activityName = $"{nameof(MessagePackHelper)}_{nameof(FromMessagePackBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = Convert.FromBase64String(base64String);
            return bytes.FromMessagePack<T>(serializerOptions, cancellationToken);
        }
    }
}
