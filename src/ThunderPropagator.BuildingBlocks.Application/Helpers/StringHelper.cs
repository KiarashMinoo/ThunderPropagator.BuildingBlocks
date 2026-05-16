using ThunderPropagator.BuildingBlocks.Application.Objects;
using System.Diagnostics;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using System.Text;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class StringHelper
    {
        public static byte[] ToByteArray(this string str)
        {
            const string activityName = $"{nameof(StringHelper)}_{nameof(ToByteArray)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(string.Length), str.Length);

            return Encoding.UTF8.GetBytes(str);
        }

        public static ReadOnlyMemory<byte> ToByteReadOnlyMemory(this string str)
        {
            const string activityName = $"{nameof(StringHelper)}_{nameof(ToByteReadOnlyMemory)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(string.Length), str.Length);

            return Encoding.UTF8.GetBytes(str);
        }

        public static string FromByteArray(this byte[] bytes)
        {
            const string activityName = $"{nameof(StringHelper)}_{nameof(FromByteArray)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(string.Length), bytes.Length);

            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToBase64(this string str)
        {
            const string activityName = $"{nameof(StringHelper)}_{nameof(ToBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(string.Length), str.Length);

            var bytes = ToByteArray(str);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string str)
        {
            const string activityName = $"{nameof(StringHelper)}_{nameof(FromBase64)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(string.Length), str.Length);

            var bytes = Convert.FromBase64String(str);
            return FromByteArray(bytes);
        }

        public static string DecompressString(this CompressedObject compressedObject,
            CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
        {
            using var memoryStream = new MemoryStream(compressedObject);
            using var outputStream = new MemoryStream();

            switch (compressionType)
            {
                case CompressedObject.CompressionType.GZipStream:
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        decompressStream.CopyTo(outputStream);
                    break;
                case CompressedObject.CompressionType.DeflateStream:
                    using (var decompressStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                        decompressStream.CopyTo(outputStream);
                    break;
                case CompressedObject.CompressionType.BrotliStream:
                    using (var decompressStream = new BrotliStream(memoryStream, CompressionMode.Decompress))
                        decompressStream.CopyTo(outputStream);
                    break;
                case CompressedObject.CompressionType.BZip2:
                    BZip2.Decompress(memoryStream, outputStream, false);
                    break;
                case CompressedObject.CompressionType.GZip:
                    GZip.Decompress(memoryStream, outputStream, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, null);
            }

            return Encoding.UTF8.GetString(outputStream.ToArray());
        }
    }
}