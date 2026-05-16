using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ThunderPropagator.BuildingBlocks.Application.Collections;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class CollectionHelper
    {
        public static LinkedArray<T> Filter<T>(this IEnumerable<T>? enumerable, Func<T, bool> func)
        {
            var array = enumerable as T[] ?? enumerable?.ToArray() ?? [];

            const string activityName = $"{nameof(CollectionHelper)}_{nameof(Filter)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(array.Length), array.Length);

            if (array.Length == 0)
                return LinkedArray<T>.Empty;

            var tempIndices = new List<int>();

            var arraySpan = array.AsSpan();
            ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
            for (var index = 0; index < arraySpan.Length; index++)
            {
                var source = Unsafe.Add(ref arraySpanReference, index);
                if (!func(source))
                {
                    continue;
                }

                tempIndices.Add(index);
            }

            return new LinkedArray<T>(array, tempIndices);
        }

        public static IEnumerable<T> Convert<T>(this IEnumerable<IConvertible<T>> enumerable) => enumerable.Select(item => item.Convert());

        public static TR[]? Convert<T, TR>(this T[]? array, Func<T, TR> func)
        {
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(Convert)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(Array.Length), array?.Length);

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
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            var array = enumerable as T[] ?? enumerable.ToArray();

            for (var i = 0; i < array.Length; i += count)
            {
                var length = Math.Min(count, array.Length - i);
                yield return new ArraySegment<T>(array, i, length);
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
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(ForEach)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            switch (collection)
            {
                case null or List<T> { Count: <= 0 }:
                    return;
                case List<T> list:
                {
                    activity?.SetTag(nameof(List<T>.Count), list.Count);

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
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(ForEach)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(Array.Length), array?.Length);

            if (array is not null && array.Length > 0)
            {
                var arraySpan = array.AsSpan();
                ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
                for (var index = 0; index < arraySpan.Length; index++)
                {
                    var source = Unsafe.Add(ref arraySpanReference, index);
                    execution(index, source);
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
            const string activityName = $"{nameof(CollectionHelper)}_{nameof(ForEach)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;
            activity?.SetTag(nameof(Array.Length), array.Count);

            if (array.Count > 0)
            {
                var arraySpan = array.AsSpan();
                ref var arraySpanReference = ref MemoryMarshal.GetReference(arraySpan);
                for (var index = 0; index < arraySpan.Length; index++)
                {
                    var source = Unsafe.Add(ref arraySpanReference, index);
                    execution(index, source);
                }
            }
        }

        #endregion
    }
}