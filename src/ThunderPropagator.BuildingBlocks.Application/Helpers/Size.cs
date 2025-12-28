using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using ThunderPropagator.BuildingBlocks.Application.Objects;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public sealed unsafe class Size : DisposableObject
    {
        public static readonly int PointerSize = Environment.Is64BitOperatingSystem ? sizeof(long) : sizeof(int);

        private static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldCache = new();
        // cached compiled delegates to get Count quickly (avoids PropertyInfo.GetValue allocations)
        private static readonly ConcurrentDictionary<Type, Func<object, int>?> CountAccessorCache = new();
        // cache for element primitive size for arrays / generic collections: -1 = unknown/not-primitive
        private static readonly ConcurrentDictionary<Type, int> ElementPrimitiveSizeCache = new();
        // cache for List<T>._size field accessor
        private static readonly ConcurrentDictionary<Type, Func<object, int>?> ListSizeAccessorCache = new();
        private readonly object _obj;
        private readonly HashSet<object> _references = new HashSet<object>(ReferenceEqualityComparer.Instance);

        private Size(object obj)
        {
            _obj = Guard.Against.Null(obj);
            // do not pre-add root here; it should be measured like other objects
        }

        private static long GetPrimitiveSize(Type t)
        {
            return Type.GetTypeCode(t) switch
            {
                TypeCode.Boolean or TypeCode.Byte or TypeCode.SByte => sizeof(byte),
                TypeCode.Char => sizeof(char),
                TypeCode.Single => sizeof(float),
                TypeCode.Double => sizeof(double),
                TypeCode.Decimal => sizeof(decimal),
                TypeCode.Int16 or TypeCode.UInt16 => sizeof(short),
                TypeCode.Int32 or TypeCode.UInt32 => sizeof(int),
                // Int64/UInt64 are covered by default
                _ => sizeof(long)
                    };
        }

        private long Calculate()
        {
            const int charSize = sizeof(char);
            long total = 0;

            var stack = new Stack<object?>(8);
            var refs = _references; // local copy for faster access

            // mark root and push
            refs.Add(_obj);
            stack.Push(_obj);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is null) continue;

                var type = current.GetType();

                // fast-paths for known simple types
                if (type == typeof(string))
                {
                    total += (long)charSize * ((string)current).Length;
                    continue;
                }

                // fast-path for arrays: calculate size directly for primitive element types
                if (type.IsArray)
                {
                    var array = (Array)current;
                    Type? elementType = type.GetElementType();
                    // avoid repeated reflection for element type/size
                    Type? elementTypeForCache = elementType;
                    var elementSize = elementTypeForCache is null
                        ? -1
                        : ElementPrimitiveSizeCache.GetOrAdd(elementTypeForCache, et => (int)(et.IsPrimitive ? GetPrimitiveSize(et) : -1));

                    if (elementSize > 0)
                    {
                        total += (long)array.Length * elementSize;
                        continue;
                    }
                    // For non-primitive arrays (e.g., object[]), traverse elements; mark on push to avoid duplicates
                    foreach (var item in array)
                    {
                        if (item == null) continue;
                        if (refs.Add(item)) stack.Push(item);
                    }
                    continue;
                }

                if (type.IsEnum)
                {
                    total += sizeof(int);
                    continue;
                }

                // handle common structs that are not Type.IsPrimitive
                if (type == typeof(decimal))
                {
                    total += sizeof(decimal);
                    continue;
                }

                if (type == typeof(DateTime))
                {
                    total += sizeof(DateTime);
                    continue;
                }

                if (type == typeof(DateTimeOffset))
                {
                    total += sizeof(DateTimeOffset);
                    continue;
                }

                if (type == typeof(TimeSpan))
                {
                    total += sizeof(TimeSpan);
                    continue;
                }

                if (type == typeof(Guid))
                {
                    total += sizeof(Guid);
                    continue;
                }

                if (type == typeof(IntPtr) || type == typeof(UIntPtr))
                {
                    total += sizeof(nint);
                    continue;
                }

                // primitives (includes char, int, float, double, bool, etc.)
                if (type.IsPrimitive)
                {
                    total += GetPrimitiveSize(type);
                    continue;
                }

                // fast-path for generic collections of primitive element types (e.g., List<int>)
                if (type.IsGenericType)
                {
                    var genericArgs = type.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        var elemType = genericArgs[0];
                        if (elemType.IsPrimitive)
                        {
                            // check for ICollection<T> to get Count without iterating
                            var collIface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
                            if (collIface != null && collIface.GetGenericArguments()[0] == elemType)
                            {
                                int cnt = 0;
                                bool gotCount = false;
                                // special fast-path for List<T>: read _size field directly
                                if (type.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    var accessor = ListSizeAccessorCache.GetOrAdd(type, BuildListSizeAccessor);
                                    if (accessor != null)
                                    {
                                        try
                                        {
                                            cnt = accessor(current);
                                            gotCount = true;
                                        }
                                        catch
                                        {
                                            // fall through
                                        }
                                    }
                                }
                                if (!gotCount)
                                {
                                    // build/lookup a compiled accessor that returns int Count
                                    var accessor = CountAccessorCache.GetOrAdd(type, BuildCountAccessor);
                                    if (accessor != null)
                                    {
                                        try
                                        {
                                            cnt = accessor(current);
                                            gotCount = true;
                                        }
                                        catch
                                        {
                                            // fall through to normal enumeration
                                        }
                                    }
                                }
                                if (gotCount)
                                {
                                    var elemSize = ElementPrimitiveSizeCache.GetOrAdd(elemType, _ => (int)GetPrimitiveSize(elemType));
                                    total += (long)cnt * elemSize;
                                    continue;
                                }
                            }
                        }
                    }
                }

                // IEnumerable (but not string which was handled)
                if (current is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is null) continue;
                        if (refs.Add(item)) stack.Push(item);
                    }

                    continue;
                }

                // fallback: traverse fields
                var t = type;
                while (t != null)
                {
                    var fields = FieldCache.GetOrAdd(t, typ => typ.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                    for (var i = 0; i < fields.Length; i++)
                    {
                        var value = fields[i].GetValue(current);
                        if (value is null) continue;
                        if (refs.Add(value)) stack.Push(value);
                    }

                    t = t.BaseType;
                }

            }

            return total;

            }

            // Build a compiled fast accessor that, given any object, returns its ICollection<T>.Count as int
        private static Func<object, int>? BuildCountAccessor(Type t)
        {
            try
            {
                var iface = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
                if (iface == null) return null;
                var prop = iface.GetProperty("Count");
                if (prop == null) return null;

                var param = Expression.Parameter(typeof(object), "o");
                var cast = Expression.Convert(param, iface);
                var propAccess = Expression.Property(cast, prop);
                var convertToInt = Expression.Convert(propAccess, typeof(int));
                var lambda = Expression.Lambda<Func<object, int>>(convertToInt, param);
                return lambda.Compile();
            }
            catch
            {
                return null;
            }
        }

        // Build a compiled fast accessor for List<T>._size field
        private static Func<object, int>? BuildListSizeAccessor(Type t)
        {
            try
            {
                if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(List<>)) return null;
                var field = t.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null) return null;

                var param = Expression.Parameter(typeof(object), "o");
                var cast = Expression.Convert(param, t);
                var fieldAccess = Expression.Field(cast, field);
                var lambda = Expression.Lambda<Func<object, int>>(fieldAccess, param);
                return lambda.Compile();
            }
            catch
            {
                return null;
            }
        }

        protected override void DisposeManagedResources()
        {
            _references.Clear();
        }

        /// <summary>
        /// Calculate the optimistic size of any managed object.
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

        // Reference equality comparer to ensure object identity is used when tracking visited references
        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();

            bool IEqualityComparer<object>.Equals(object? x, object? y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}