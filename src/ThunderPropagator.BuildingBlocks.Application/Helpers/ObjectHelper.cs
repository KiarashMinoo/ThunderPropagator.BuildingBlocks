using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using ThunderPropagator.BuildingBlocks.Application.Collections;
using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class ObjectHelper
    {
        private static readonly ConcurrentDictionary<Type, List<FieldInfo>> ObjectFields = new();
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> ObjectProperties = new();

        public static IEnumerable<FieldInfo> GetFields(Type type)
            => ObjectFields.GetOrAdd(type,
                type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null).ToList());

        public static IEnumerable<PropertyInfo> GetProperties(Type type)
            => ObjectProperties.GetOrAdd(type,
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null).ToList());

        public static IEnumerable<FieldInfo> GetFields(this object input) => GetFields(input.GetType());

        public static IEnumerable<PropertyInfo> GetProperties(this object input) => GetProperties(input.GetType());

        public static bool EquatableEqual(this object obj, object? comparer)
        {
            return comparer != null &&
                   comparer.GetType() == obj.GetType() &&
                   obj.GetProperties().All(PropertiesAreEqual) &&
                   obj.GetFields().All(FieldsAreEqual);

            bool FieldsAreEqual(FieldInfo f) => Equals(f.GetValue(obj), f.GetValue(comparer));

            bool PropertiesAreEqual(PropertyInfo p) => Equals(p.GetValue(obj, null), p.GetValue(comparer, null));
        }

        public static int EquatableHashCode(this object obj)
        {
            var hash = obj.GetProperties().Select(propertyInfo => propertyInfo.GetValue(obj, null)).Aggregate(17, HashValue);

            return obj.GetFields().Select(fieldInfo => fieldInfo.GetValue(obj)).Aggregate(hash, HashValue);

            static int HashValue(int seed, object? value)
            {
                var currentHash = value?.GetHashCode() ?? 0;
                return seed * 23 + currentHash;
            }
        }

        public static T? As<T>(this object instance)
            where T : class
        {
            return instance as T;
        }

        public static T Clone<T>(this T instance)
            where T : class
        {
            if (instance is ICloneable cloneable)
                return (T)cloneable.Clone();

            var bytes = instance.ToNJsonBytes();
            return bytes.FromNJsonBytes<T>()!;
        }

        public static bool IsDisposed<T>(this T instance)
            where T : notnull
        {
            const string activityName = $"{nameof(ObjectHelper)}_{nameof(IsDisposed)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            try
            {
                _ = instance.GetHashCode();
            }
            catch (ObjectDisposedException)
            {
                return true;
            }

            return false;
        }

        public static CompressedObject Compress<T>(this T input,
            CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream,
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
            where T : notnull
        {
            var bytes = input.ToNJsonBytes();

            using var memoryStream = new MemoryStream();

            switch (compressionType)
            {
                case CompressedObject.CompressionType.GZipStream:
                    using (var gzipStream = new GZipStream(memoryStream, compressionLevel))
                        gzipStream.Write(bytes, 0, bytes.Length);
                    break;
                case CompressedObject.CompressionType.DeflateStream:
                    using (var gzipStream = new DeflateStream(memoryStream, compressionLevel))
                        gzipStream.Write(bytes, 0, bytes.Length);
                    break;
                case CompressedObject.CompressionType.BrotliStream:
                    using (var gzipStream = new BrotliStream(memoryStream, compressionLevel))
                        gzipStream.Write(bytes, 0, bytes.Length);
                    break;
                case CompressedObject.CompressionType.BZip2:
                {
                    var level = compressionLevel switch
                    {
                        CompressionLevel.Optimal => 5,
                        CompressionLevel.Fastest => 1,
                        CompressionLevel.NoCompression => 0,
                        CompressionLevel.SmallestSize => 9,
                        _ => throw new ArgumentOutOfRangeException(nameof(compressionLevel), compressionLevel, null)
                    };
                    using MemoryStream source = new(bytes);
                    BZip2.Compress(source, memoryStream, false, level);
                    break;
                }
                case CompressedObject.CompressionType.GZip:
                {
                    var level = compressionLevel switch
                    {
                        CompressionLevel.Optimal => 5,
                        CompressionLevel.Fastest => 1,
                        CompressionLevel.NoCompression => 0,
                        CompressionLevel.SmallestSize => 9,
                        _ => throw new ArgumentOutOfRangeException(nameof(compressionLevel), compressionLevel, null)
                    };
                    using MemoryStream source = new(bytes);
                    GZip.Compress(source, memoryStream, false, level);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(compressionType), compressionType, null);
            }

            return memoryStream.ToArray();
        }

        public static T Decompress<T>(this CompressedObject compressedObject,
            CompressedObject.CompressionType compressionType = CompressedObject.CompressionType.GZipStream)
            where T : notnull
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

            return outputStream.ToArray().FromNJsonBytes<T>()!;
        }

        public static string ToSafeString(this object? value, string? format = null, IFormatProvider? formatProvider = null) => value switch
        {
            null => string.Empty,
            string stringValue when !string.IsNullOrWhiteSpace(stringValue) => stringValue,
            IFormattable formattableValue => formattableValue.ToString(format, formatProvider),
            _ => value.ToString() ?? string.Empty
        };
    }
}