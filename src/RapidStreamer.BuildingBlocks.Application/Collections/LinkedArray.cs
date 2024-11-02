using Ardalis.GuardClauses;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Application.Collections
{
    public readonly struct LinkedArray<T> : IList<T>,
        IReadOnlyList<T>,
        ICollection<T>,
        IReadOnlyCollection<T>
    {
        public static LinkedArray<T> Empty { get; } = new([]);

        private readonly T[] _array;
        private readonly List<int> _list = [];

        public int Count => _list.Count;
        public bool IsReadOnly => true;

        public T this[int index]
        {
            get => _array[_list[index]];
            set => throw new InvalidOperationException();
        }

        public LinkedArray(T[] array) => _array = Guard.Against.Null(array);

        public IEnumerator<T> GetEnumerator()
        {
            using var enumerator = _list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return _array[enumerator.Current];
            }
        }

        public void CopyTo(T[] destination, int destinationIndex = 0)
        {
            var array = ToArray();
            var size = array.Length;
            Array.Copy(array, destination, size);
        }

        public T[] ToArray()
        {
            return Count == 0 ? [] : ForEach(arrayItem => arrayItem);
        }

        public void ForEach(Action<T> execution)
        {
            ForEach((_, item) => execution(item));
        }

        public void ForEach(Action<int, T> execution)
        {
            ForEach((index, arrayItem) =>
            {
                execution.Invoke(index, arrayItem);
                return arrayItem;
            });
        }

        public TR[] ForEach<TR>(Func<T, TR> execution)
        {
            return ForEach((_, item) => execution(item));
        }

        public TR[] ForEach<TR>(Func<int, T, TR> execution)
        {
            if (Count <= 0)
            {
                return [];
            }

            var rtn = new TR[Count];

            var listSpan = CollectionsMarshal.AsSpan(_list);
            ref var listSpanReference = ref MemoryMarshal.GetReference(listSpan);
            for (var index = 0; index < listSpan.Length; index++)
            {
                var itemIndex = Unsafe.Add(ref listSpanReference, index);
                rtn[index] = execution(index, _array[itemIndex]);
            }

            return rtn;
        }

        #region IList<T>

        public int IndexOf(T item)
        {
            var sourceIndex = Array.IndexOf(_array, item);
            return sourceIndex >= 0 ? _list.IndexOf(sourceIndex) : -1;
        }

        public void Insert(int index, T item)
        {
            var sourceIndex = Array.IndexOf(_array, item);
            if (sourceIndex >= 0)
            {
                _list.Insert(index, sourceIndex);
            }
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        #endregion

        #region IReadOnlyList<T>

        T IReadOnlyList<T>.this[int index] => _array[_list[index]];

        #endregion IReadOnlyList<T>

        #region ICollection<T>

        public void Add(T item)
        {
            var sourceIndex = Array.IndexOf(_array, item);
            if (sourceIndex < 0)
            {
                throw new IndexOutOfRangeException();
            }

            Add(sourceIndex);
        }

        public bool Contains(T item)
        {
            var sourceIndex = Array.IndexOf(_array, item);
            return sourceIndex >= 0 && Contains(sourceIndex);
        }

        public bool Remove(T item)
        {
            var sourceIndex = Array.IndexOf(_array, item);
            return sourceIndex >= 0 && Remove(sourceIndex);
        }

        #endregion

        #region ICollection<int>

        public void Add(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            _list.Add(itemIndex);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(int itemIndex)
        {
            return _list.Contains(itemIndex);
        }

        public bool Remove(int itemIndex)
        {
            return _list.Remove(itemIndex);
        }

        #endregion

        #region IEnumerable<T>

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}