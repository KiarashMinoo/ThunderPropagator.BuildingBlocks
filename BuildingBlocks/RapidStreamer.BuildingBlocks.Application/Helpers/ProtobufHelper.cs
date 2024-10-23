using ProtoBuf;
using System.Diagnostics;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class ProtobufHelper
    {
        public static Stream ToProtobuf(this object instance)
        {
#if DEBUG
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(ToProtobuf)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            MemoryStream memoryStream = new();
            Serializer.Serialize(memoryStream, instance);
            return memoryStream;
        }

        public static string ToProtobufBase64(this object instance)
        {
            var stream = ToProtobuf(instance);
            return Convert.ToBase64String(stream.ToByteArray())[..^2];
        }

        public static T FromProtobuf<T>(this Stream stream)
        {
#if DEBUG
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(FromProtobuf)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif
            return Serializer.Deserialize<T>(stream);
        }

        public static T FromProtobuf<T>(this byte[] bytes)
        {
            using MemoryStream memoryStream = new(bytes);
            return FromProtobuf<T>(memoryStream);
        }

        public static T FromProtobufBase64<T>(this string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            return FromProtobuf<T>(bytes);
        }
    }
}