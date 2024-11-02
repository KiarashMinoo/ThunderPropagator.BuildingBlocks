using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RapidStreamer.BuildingBlocks.Application.Collections;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class CollectionHelper
    {
        public static LinkedArray<T> Filter<T>(this IEnumerable<T>? enumerable, Func<T, bool> func)
        {
            var array = enumerable as T[] ?? enumerable?.ToArray() ?? [];

#if DEBUG
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(Filter)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(array.Length), array.Length);
#endif

            if (array.Length == 0)
                return LinkedArray<T>.Empty;

            var rtn = new LinkedArray<T>(array);

            var arraySpan = array.AsSpan();
            ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
            for (var index = 0; index < arraySpan.Length; index++)
            {
                var source = Unsafe.Add(ref arraySpanReference, index);
                if (!func.Invoke(source))
                {
                    continue;
                }

                rtn.Add(index);
            }

            return rtn;
        }

        public static IEnumerable<T> Convert<T>(this IEnumerable<IConvertible<T>> enumerable) => enumerable.Select(item => item.Convert());

        public static TR[]? Convert<T, TR>(this T[]? array, Func<T, TR> func)
        {
#if DEBUG
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(Convert)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(Array.Length), array?.Length);
#endif
            if (array is null || array.Length <= 0)
            {
                return null;
            }

            var rtn = new TR[array.Length];

            var arraySpan = array.AsSpan();
            ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
            for (var index = 0; index < arraySpan.Length; index++)
            {
                var source = Unsafe.Add(ref arraySpanReference, index);
                rtn[index] = func(source);
            }

            return rtn;
        }

        public static IEnumerable<ArraySegment<T>> Splice<T>(this IEnumerable<T> enumerable, int count)
        {
            var array = enumerable as T[] ?? enumerable.ToArray();

            if (array.Length < count)
                yield return array;

            var loopsCount = (int)Math.Ceiling(array.Length / (count * 1.0));

            for (var i = 0; i < loopsCount; i++)
            {
                if (i == 0)
                    yield return new ArraySegment<T>(array, 0, count);
                else if (i == loopsCount - 1)
                    yield return new ArraySegment<T>(array, i * count, array.Length % count);
                else
                    yield return new ArraySegment<T>(array, i * count, count);
            }
        }

        public static bool IsEquals<T>(this IEnumerable<T>? enumerable, IEnumerable<T>? other)
            => enumerable is not null && other is not null ? enumerable.SequenceEqual(other) : enumerable is null && other is null;

        #region "Enumerable"

        //Enumerable, Void

        public static void ForEach<T>(this IEnumerable<T>? collection, Action<T> action)
        {
            ForEach(collection, (_, item) => action(item));
        }

        public static void ForEach<T>(this IEnumerable<T>? collection, Action<int, T> action)
        {
#if DEBUG
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(ForEach)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif

            switch (collection)
            {
                case null or List<T> { Count: <= 0 }:
                    return;
                case List<T> list:
                {
#if DEBUG
                    activity?.SetTag(nameof(List<T>.Count), list.Count);
#endif

                    var listSpan = CollectionsMarshal.AsSpan(list);
                    ref var listSpanReference = ref MemoryMarshal.GetReference(listSpan);
                    for (var index = 0; index < listSpan.Length; index++)
                    {
                        var source = Unsafe.Add(ref listSpanReference, index);
                        action(index, source);
                    }

                    return;
                }
                default:
                {
                    var array = collection as T[] ?? collection.ToArray();
                    ForEach(array, action);
                    return;
                }
            }
        }

        #endregion

        #region "Array"

        public static void ForEach<T>(this T[]? array, Action<T> execution)
        {
            ForEach(array, (_, item) => execution(item));
        }

        public static void ForEach<T>(this T[]? array, Action<int, T> execution)
        {
#if DEBUG
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(ForEach)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(Array.Length), array?.Length);
#endif

            if (array is not null && array.Length > 0)
            {
                var arraySpan = array.AsSpan();
                ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
                for (var index = 0; index < arraySpan.Length; index++)
                {
                    var source = Unsafe.Add(ref arraySpanReference, index);
                    execution.Invoke(index, source);
                }
            }
        }

        #endregion

        #region "ArraySegment"

        public static void ForEach<T>(this ArraySegment<T> array, Action<T> execution)
        {
            ForEach(array, (_, item) => execution(item));
        }

        public static void ForEach<T>(this ArraySegment<T> array, Action<int, T> execution)
        {
#if DEBUG
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(ForEach)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal)?
                .SetTag(nameof(Array.Length), array.Count);
#endif

            if (array.Count > 0)
            {
                var arraySpan = array.AsSpan();
                ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
                for (var index = 0; index < arraySpan.Length; index++)
                {
                    var source = Unsafe.Add(ref arraySpanReference, index);
                    execution.Invoke(index, source);
                }
            }
        }

        #endregion
    }
}