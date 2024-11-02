using System.Collections;
using System.Reflection;
using Ardalis.GuardClauses;
using RapidStreamer.BuildingBlocks.Application.Objects;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public unsafe class Size : DisposableObject
    {
        public static readonly int PointerSize = Environment.Is64BitOperatingSystem ? sizeof(long) : sizeof(int);

        private readonly object _obj;
        private readonly List<object> _references;

        private Size(object obj)
        {
            _obj = Guard.Against.Null(obj, nameof(obj));
            _references = [_obj];
        }

        private long Calculate()
        {
            return GetSizeInBytes(_obj);

            long GetSizeInBytes(object? obj)
            {
                if (obj is null)
                    return 0;

                var type = obj.GetType();
                var charSize = GetCharSize();
                return obj switch
                {
                    char => charSize,
                    Enum => sizeof(int),
                    Pointer => PointerSize,
                    decimal => sizeof(decimal),
                    DateTime => sizeof(DateTime),
                    IntPtr or UIntPtr => sizeof(nint),
                    string str => charSize * str.Length,
                    IEnumerable enumerable => enumerable.Cast<object?>().Sum(GetSizeInBytes),
                    _ => GetPrimitiveSize() ?? GetHierarchySize()
                };

                long? GetPrimitiveSize()
                    => type.IsPrimitive != true
                        ? null
                        : Type.GetTypeCode(type) switch
                        {
                            TypeCode.Boolean or TypeCode.Byte or TypeCode.SByte => sizeof(byte),
                            TypeCode.Single => sizeof(float),
                            TypeCode.Double => sizeof(double),
                            TypeCode.Decimal => sizeof(decimal),
                            TypeCode.Int16 or TypeCode.UInt16 => sizeof(short),
                            TypeCode.Int32 or TypeCode.UInt32 => sizeof(int),
                            TypeCode.Int64 or TypeCode.UInt64 => sizeof(long),
                            _ => sizeof(long)
                        };

                long GetCharSize() => GetFields(type).Length != 0 ? sizeof(char) * 2 : sizeof(char);

                FieldInfo[] GetFields(Type t) => t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                long GetHierarchySize()
                {
                    long size = 0;
                    var t = type;
                    while (t != null)
                    {
                        size += GetFields(t).Select(field => field.GetValue(obj)).OfType<object>().Where(field => !_references.Any(reference => ReferenceEquals(reference, field))).Sum(field =>
                        {
                            _references.Add(field);
                            return GetSizeInBytes(field);
                        });

                        t = t.BaseType;
                    }

                    return size;
                }
            }
        }

        protected override void DisposeManagedResources()
        {
            _references.Clear();
        }

        /// <summary>
        /// Calculate the optimistic size af any managed object.
        /// Get the minimal memory footprint of <paramref name="obj"/>.
        /// Counted are all <paramref name="obj"/> fields, including auto-generated, private and protected.
        /// Not counted: any static fields, any properties, functions, member methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Task<long> Calculate<T>(T obj)
            where T : notnull
            => Task.Run(() =>
            {
                var temp = new Size(obj);
                var tempSize = temp.Calculate();
                return tempSize;
            });

        public static Task<int> CalculateJsonify<T>(T obj)
            where T : notnull
            => Task.FromResult(obj.ToJsonBase64().Length);
    }
}