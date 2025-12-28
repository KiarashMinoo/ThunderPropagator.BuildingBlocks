using ProtoBuf;
using System.Diagnostics;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class ProtobufHelper
    {
        public static Stream ToProtobuf(this object instance)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(ToProtobuf)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            MemoryStream memoryStream = new();
            Serializer.Serialize(memoryStream, instance);
            return memoryStream;
        }

        public static string ToProtobufBase64(this object instance)
        {
            using var stream = ToProtobuf(instance);
            return Convert.ToBase64String(stream.ToByteArray());
        }

        public static T FromProtobuf<T>(this Stream stream)
        {
            const string activityName = $"{nameof(ProtobufHelper)}_{nameof(FromProtobuf)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

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