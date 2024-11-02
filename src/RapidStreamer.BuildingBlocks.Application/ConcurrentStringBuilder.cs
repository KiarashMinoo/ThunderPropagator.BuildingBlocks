using RapidStreamer.BuildingBlocks.Application.Objects;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace RapidStreamer.BuildingBlocks.Application
{
    public
#if !DEBUG
        sealed
#endif
        class ConcurrentStringBuilder : DisposableObject,
        ICloneable,
        ICloneable<ConcurrentStringBuilder>,
        ICloneable<StringBuilder>
    {
        private readonly bool _concurrent;

        private readonly StringBuilder _stringBuilder;

        public ConcurrentStringBuilder() : this(true)
        {
        }

        public ConcurrentStringBuilder(bool concurrent)
        {
            _stringBuilder = new StringBuilder();
            _concurrent = concurrent;
        }

        public ConcurrentStringBuilder(int capacity, bool concurrent = true)
        {
            _stringBuilder = new StringBuilder(capacity);
            _concurrent = concurrent;
        }

        public ConcurrentStringBuilder(string? value, bool concurrent = true)
        {
            _stringBuilder = new StringBuilder(value);
            _concurrent = concurrent;
        }

        public ConcurrentStringBuilder(string? value, int capacity, bool concurrent = true)
        {
            _stringBuilder = new StringBuilder(value, capacity);
            _concurrent = concurrent;
        }

        public ConcurrentStringBuilder(string? value, int startIndex, int length, int capacity, bool concurrent = true)
        {
            _stringBuilder = new StringBuilder(value, startIndex, length, capacity);
            _concurrent = concurrent;
        }

        public ConcurrentStringBuilder(int capacity, int maxCapacity, bool concurrent = true)
        {
            _stringBuilder = new StringBuilder(capacity, maxCapacity);
            _concurrent = concurrent;
        }

        public int Capacity
        {
            get
            {
                lock (this)
                {
                    return _stringBuilder.Capacity;
                }
            }
            set
            {
                lock (this)
                {
                    _stringBuilder.Capacity = value;
                }
            }
        }

        public int MaxCapacity
        {
            get
            {
                lock (this)
                {
                    return _stringBuilder.MaxCapacity;
                }
            }
        }

        public int Length
        {
            get
            {
                lock (this)
                {
                    return _stringBuilder.Length;
                }
            }
        }

        [IndexerName("Chars")]
        public char this[int index]
        {
            get
            {
                try
                {
                    EnterLock();
                    return _stringBuilder[index];
                }
                finally
                {
                    ExitLock();
                }
            }
            set
            {
                try
                {
                    EnterLock();
                    _stringBuilder[index] = value;
                }
                finally
                {
                    ExitLock();
                }
            }
        }

        private void EnterLock()
        {
            if (_concurrent)
            {
                Monitor.Enter(this);
            }
        }

        private void ExitLock()
        {
            if (_concurrent)
            {
                Monitor.Exit(this);
            }
        }

        public int EnsureCapacity(int capacity)
        {
            try
            {
                EnterLock();
                return _stringBuilder.EnsureCapacity(capacity);
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(char value, int repeatCount)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value, repeatCount);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(char[]? value, int startIndex, int charCount)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value, startIndex, charCount);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(string? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(string? value, int startIndex, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value, startIndex, count);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(ConcurrentStringBuilder? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value?._stringBuilder);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(ConcurrentStringBuilder? value, int startIndex, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value?._stringBuilder, startIndex, count);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(StringBuilder? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(StringBuilder? value, int startIndex, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value, startIndex, count);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(bool value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(char value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(sbyte value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(byte value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(short value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(int value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(long value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(float value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(double value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(decimal value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(ushort value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(uint value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(ulong value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(object? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(char[]? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(ReadOnlySpan<char> value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Append(ReadOnlyMemory<char> value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public unsafe ConcurrentStringBuilder Append(char* value, int valueCount)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value, valueCount);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendLine()
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendLine();
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendLine(string? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Append(value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendJoin(string? separator, params object?[] values)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendJoin<T>(string? separator, IEnumerable<T> values)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendJoin(string? separator, params string?[] values)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendJoin(char separator, params object?[] values)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendJoin<T>(char separator, IEnumerable<T> values)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendJoin(char separator, params string?[] values)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, arg0);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, arg0, arg1);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, arg0, arg1, arg2);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, args);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, arg0);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, arg0, arg1);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1,
            object? arg2)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, arg0, arg1, arg2);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            try
            {
                EnterLock();
                _stringBuilder.AppendFormat(format, args);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, string? value, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value, count);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, string? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, bool value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, sbyte value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, byte value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, short value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, char value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, char[]? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, char[]? value, int startIndex, int charCount)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value, startIndex, charCount);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, int value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, long value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, float value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, double value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, decimal value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, ushort value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, uint value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, ulong value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, object? value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            try
            {
                EnterLock();
                _stringBuilder.Insert(index, value);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Replace(string oldValue, string? newValue)
        {
            try
            {
                EnterLock();
                _stringBuilder.Replace(oldValue, newValue);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Replace(string oldValue, string? newValue, int startIndex, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.Replace(oldValue, newValue, startIndex, count);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Replace(char oldChar, char newChar)
        {
            try
            {
                EnterLock();
                _stringBuilder.Replace(oldChar, newChar);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.Replace(oldChar, newChar, startIndex, count);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Remove(int startIndex, int length)
        {
            try
            {
                EnterLock();
                _stringBuilder.Remove(startIndex, length);
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public ConcurrentStringBuilder Clear()
        {
            try
            {
                EnterLock();
                _stringBuilder.Clear();
                return this;
            }
            finally
            {
                ExitLock();
            }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
            }
            finally
            {
                ExitLock();
            }
        }

        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            try
            {
                EnterLock();
                _stringBuilder.CopyTo(sourceIndex, destination, count);
            }
            finally
            {
                ExitLock();
            }
        }

        public bool Equals([NotNullWhen(true)] ConcurrentStringBuilder? sb)
        {
            try
            {
                EnterLock();
                return _stringBuilder.Equals(sb?._stringBuilder);
            }
            finally
            {
                ExitLock();
            }
        }

        public bool Equals([NotNullWhen(true)] StringBuilder? sb)
        {
            try
            {
                EnterLock();
                return _stringBuilder.Equals(sb);
            }
            finally
            {
                ExitLock();
            }
        }

        public bool Equals(ReadOnlySpan<char> span)
        {
            try
            {
                EnterLock();
                return _stringBuilder.Equals(span);
            }
            finally
            {
                ExitLock();
            }
        }

        public override string ToString()
        {
            try
            {
                EnterLock();
                return _stringBuilder.ToString();
            }
            finally
            {
                ExitLock();
            }
        }

        object ICloneable.Clone() => MemberwiseClone();
        public ConcurrentStringBuilder Clone() => new(ToString(), _concurrent);
        StringBuilder ICloneable<StringBuilder>.Clone() => new(ToString());

        public string ToString(int startIndex, int length)
        {
            try
            {
                EnterLock();
                return _stringBuilder.ToString(startIndex, length);
            }
            finally
            {
                ExitLock();
            }
        }

        public string ToString(bool removeLast)
        {
            try
            {
                EnterLock();
                var rtn = ToString();
                return removeLast ? rtn[..^1] : rtn;
            }
            finally
            {
                ExitLock();
            }
        }
    }
}