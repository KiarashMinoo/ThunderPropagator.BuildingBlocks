using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class StreamHelper
    {
        public static byte[] ToByteArray(this Stream stream)
        {
#if DEBUG
            const string activityName = $"{nameof(StreamHelper)}_{nameof(ToByteArray)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(Stream.Length), stream.Length);
#endif

            if (stream.Position != 0)
            {
                stream.Position = 0;
                stream.Seek(0, SeekOrigin.Begin);
            }

            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();

            using var binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        public static Stream ToStream(this string str)
        {
#if DEBUG
            const string activityName = $"{nameof(StreamHelper)}_{nameof(ToStream)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(string.Length), str.Length);
#endif

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
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, null);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}