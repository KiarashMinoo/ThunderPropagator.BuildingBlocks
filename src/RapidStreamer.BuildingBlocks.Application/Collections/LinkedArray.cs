using Ardalis.GuardClauses;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RapidStreamer.BuildingBlocks.Application.Collections
{
    public class LinkedArray<T> : IList<T>,
        IReadOnlyList<T>,
        ICollection<T>,
        IReadOnlyCollection<T>
    {
        public static LinkedArray<T> Empty { get; } = new([]);

        private T[] _array;
        private List<int> _indices;

        public int Count => _indices.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _indices.Count)
                    throw new IndexOutOfRangeException();
                return _array[_indices[index]];
            }
            set
            {
                if (index < 0 || index >= _indices.Count)
                    throw new IndexOutOfRangeException();
                var sourceIndex = Array.IndexOf(_array, value);
                if (sourceIndex < 0)
                {
                    // Extend _array
                    var newArray = new T[_array.Length + 1];
                    Array.Copy(_array, newArray, _array.Length);
                    newArray[_array.Length] = value;
                    _array = newArray;
                    sourceIndex = _array.Length - 1;
                }
                _indices[index] = sourceIndex;
            }
        }

        public LinkedArray(T[] array)
        {
            _array = Guard.Against.Null(array);
            _indices = new List<int>(array.Length);
            for (int i = 0; i < array.Length; i++)
                _indices.Add(i);
        }

        public LinkedArray(T[] array, List<int> indices)
        {
            _array = Guard.Against.Null(array);
            _indices = Guard.Against.Null(indices);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _indices.Count; i++)
            {
                yield return _array[_indices[i]];
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
            if (_indices.Count <= 0)
            {
                return [];
            }

            var rtn = new TR[_indices.Count];

            var indicesSpan = CollectionsMarshal.AsSpan(_indices);
            for (var index = 0; index < indicesSpan.Length; index++)
            {
                var itemIndex = indicesSpan[index];
                rtn[index] = execution(index, _array[itemIndex]);
            }

            return rtn;
        }

        #region IList<T>

        public int IndexOf(T item)
        {
            for (int i = 0; i < _indices.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_array[_indices[i]], item))
                    return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _indices.Count)
                throw new IndexOutOfRangeException();
            var sourceIndex = Array.IndexOf(_array, item);
            if (sourceIndex < 0)
            {
                // Extend _array
                var newArray = new T[_array.Length + 1];
                Array.Copy(_array, newArray, _array.Length);
                newArray[_array.Length] = item;
                _array = newArray;
                sourceIndex = _array.Length - 1;
            }
            _indices.Insert(index, sourceIndex);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _indices.Count)
                throw new IndexOutOfRangeException();
            _indices.RemoveAt(index);
        }

        #endregion

        #region IReadOnlyList<T>

        T IReadOnlyList<T>.this[int index] => _array[_indices[index]];

        #endregion IReadOnlyList<T>

        #region ICollection<T>

        public void Add(T item)
        {
            var sourceIndex = Array.IndexOf(_array, item);
            if (sourceIndex < 0)
            {
                // Extend _array
                var newArray = new T[_array.Length + 1];
                Array.Copy(_array, newArray, _array.Length);
                newArray[_array.Length] = item;
                _array = newArray;
                sourceIndex = _array.Length - 1;
            }
            _indices.Add(sourceIndex);
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < _indices.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_array[_indices[i]], item))
                    return true;
            }
            return false;
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < _indices.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_array[_indices[i]], item))
                {
                    _indices.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region ICollection<int>

        public void Clear()
        {
            _indices.Clear();
        }

        internal bool Contains(int itemIndex)
        {
            return itemIndex >= 0 && itemIndex < _array.Length && _indices.Contains(itemIndex);
        }

        internal bool Remove(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _array.Length)
                return false;
            return _indices.Remove(itemIndex);
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