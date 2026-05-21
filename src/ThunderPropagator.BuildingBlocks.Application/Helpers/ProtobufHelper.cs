using ProtoBuf;
using System.Diagnostics;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class ProtobufHelper
    {
        public static Stream ToProtobuf<T>(this T instance)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(ToProtobuf)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            MemoryStream memoryStream = new();
            Serializer.Serialize(memoryStream, instance);
            return memoryStream;
        }

        public static byte[] ToProtobufBytes<T>(this T instance)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(ToProtobufBytes)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using var stream = ToProtobuf(instance);
            return stream.ToByteArray();
        }

        public static string ToProtobufBase64<T>(this T instance)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(ToProtobufBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = instance.ToProtobufBytes();
            return Convert.ToBase64String(bytes);
        }

        public static T FromProtobuf<T>(this Stream stream)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(FromProtobuf)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            return Serializer.Deserialize<T>(stream);
        }

        public static T FromProtobuf<T>(this byte[] bytes)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(FromProtobuf)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            using MemoryStream memoryStream = new(bytes);
            return FromProtobuf<T>(memoryStream);
        }

        public static T FromProtobufBase64<T>(this string base64String)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(FromProtobufBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var bytes = Convert.FromBase64String(base64String);
            return bytes.FromProtobuf<T>();
        }
    }
}
