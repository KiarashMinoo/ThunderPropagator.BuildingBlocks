using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class StreamHelper
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            const string activityName = $"{nameof(StreamHelper)}_{nameof(ToByteArray)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(Stream.Length), stream.Length);

            if (stream.Position != 0)
            {
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();

            using var outputStream = new MemoryStream();
            stream.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        public static Stream ToStream(this string str)
        {
            const string activityName = $"{nameof(StreamHelper)}_{nameof(ToStream)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(string.Length), str.Length);

            var bytes = Encoding.UTF8.GetBytes(str);
            MemoryStream memoryStream = new(bytes);
            return memoryStream;
        }

        public static Stream DecompressStream(this CompressedObject compressedObject,
            CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
        {
            using var memoryStream = new MemoryStream(compressedObject);
            var outputStream = new MemoryStream();

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

            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}