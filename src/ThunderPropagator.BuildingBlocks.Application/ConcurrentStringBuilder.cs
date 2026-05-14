using ThunderPropagator.BuildingBlocks.Application.Objects;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ThunderPropagator.BuildingBlocks.Application
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
#if NET9_0_OR_GREATER
        private readonly Lock _lock = new();
#else
        private readonly object _lock = new();
#endif

        private readonly StringBuilder _stringBuilder;

        public ConcurrentStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }

        public ConcurrentStringBuilder(int capacity)
        {
            _stringBuilder = new StringBuilder(capacity);
        }

        public ConcurrentStringBuilder(string? value)
        {
            _stringBuilder = new StringBuilder(value);
        }

        public ConcurrentStringBuilder(string? value, int capacity)
        {
            _stringBuilder = new StringBuilder(value, capacity);
        }

        public ConcurrentStringBuilder(string? value, int startIndex, int length, int capacity)
        {
            _stringBuilder = new StringBuilder(value, startIndex, length, capacity);
        }

        public ConcurrentStringBuilder(int capacity, int maxCapacity)
        {
            _stringBuilder = new StringBuilder(capacity, maxCapacity);
        }

        public int Capacity
        {
            get
            {
                lock (_lock)
                {
                    return _stringBuilder.Capacity;
                }
            }
            set
            {
                lock (_lock)
                {
                    _stringBuilder.Capacity = value;
                }
            }
        }

        public int MaxCapacity
        {
            get
            {
                lock (_lock)
                {
                    return _stringBuilder.MaxCapacity;
                }
            }
        }

        public int Length
        {
            get
            {
                lock (_lock)
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
                lock (_lock)
                    return _stringBuilder[index];
            }
            set
            {
                lock (_lock)
                    _stringBuilder[index] = value;
            }
        }

        public int EnsureCapacity(int capacity)
        {
            lock (_lock)
                return _stringBuilder.EnsureCapacity(capacity);
        }

        public ConcurrentStringBuilder Append(char value, int repeatCount)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value, repeatCount);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(char[]? value, int startIndex, int charCount)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value, startIndex, charCount);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(string? value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(string? value, int startIndex, int count)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value, startIndex, count);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(ConcurrentStringBuilder? value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value?._stringBuilder);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(ConcurrentStringBuilder? value, int startIndex, int count)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value?._stringBuilder, startIndex, count);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(StringBuilder? value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(StringBuilder? value, int startIndex, int count)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value, startIndex, count);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(bool value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(char value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(sbyte value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(byte value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(short value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(int value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(long value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(float value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(double value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(decimal value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(ushort value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(uint value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(ulong value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(object? value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(char[]? value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(ReadOnlySpan<char> value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public ConcurrentStringBuilder Append(ReadOnlyMemory<char> value)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value);
                return this;
            }
        }

        public unsafe ConcurrentStringBuilder Append(char* value, int valueCount)
        {
            lock (_lock)
            {
                _stringBuilder.Append(value, valueCount);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendLine()
        {
            lock (_lock)
            {
                _stringBuilder.AppendLine();
                return this;
            }
        }

        public ConcurrentStringBuilder AppendLine(string? value)
        {
            lock (_lock)
            {
                _stringBuilder.AppendLine(value);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendJoin(string? separator, params object?[] values)
        {
            lock (_lock)
            {
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendJoin<T>(string? separator, IEnumerable<T> values)
        {
            lock (_lock)
            {
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendJoin(string? separator, params string?[] values)
        {
            lock (_lock)
            {
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendJoin(char separator, params object?[] values)
        {
            lock (_lock)
            {
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendJoin<T>(char separator, IEnumerable<T> values)
        {
            lock (_lock)
            {
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendJoin(char separator, params string?[] values)
        {
            lock (_lock)
            {
                _stringBuilder.AppendJoin(separator, values);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(format, arg0);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(format, arg0, arg1);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(format, arg0, arg1, arg2);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(format, args);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(provider, format, arg0);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(provider, format, arg0, arg1);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1,
            object? arg2)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(provider, format, arg0, arg1, arg2);
                return this;
            }
        }

        public ConcurrentStringBuilder AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
        {
            lock (_lock)
            {
                _stringBuilder.AppendFormat(provider, format, args);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, string? value, int count)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value, count);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, string? value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, bool value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, sbyte value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, byte value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, short value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, char value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, char[]? value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, char[]? value, int startIndex, int charCount)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value, startIndex, charCount);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, int value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, long value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, float value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, double value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, decimal value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, ushort value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, uint value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, ulong value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, object? value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Insert(int index, ReadOnlySpan<char> value)
        {
            lock (_lock)
            {
                _stringBuilder.Insert(index, value);
                return this;
            }
        }

        public ConcurrentStringBuilder Replace(string oldValue, string? newValue)
        {
            lock (_lock)
            {
                _stringBuilder.Replace(oldValue, newValue);
                return this;
            }
        }

        public ConcurrentStringBuilder Replace(string oldValue, string? newValue, int startIndex, int count)
        {
            lock (_lock)
            {
                _stringBuilder.Replace(oldValue, newValue, startIndex, count);
                return this;
            }
        }

        public ConcurrentStringBuilder Replace(char oldChar, char newChar)
        {
            lock (_lock)
            {
                _stringBuilder.Replace(oldChar, newChar);
                return this;
            }
        }

        public ConcurrentStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            lock (_lock)
            {
                _stringBuilder.Replace(oldChar, newChar, startIndex, count);
                return this;
            }
        }

        public ConcurrentStringBuilder Remove(int startIndex, int length)
        {
            lock (_lock)
            {
                _stringBuilder.Remove(startIndex, length);
                return this;
            }
        }

        public ConcurrentStringBuilder Clear()
        {
            lock (_lock)
            {
                _stringBuilder.Clear();
                return this;
            }
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            lock (_lock)
            {
                _stringBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
            }
        }

        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            lock (_lock)
            {
                _stringBuilder.CopyTo(sourceIndex, destination, count);
            }
        }

        public bool Equals([NotNullWhen(true)] ConcurrentStringBuilder? sb)
        {
            lock (_lock)
            {
                return _stringBuilder.Equals(sb?._stringBuilder);
            }
        }

        public bool Equals([NotNullWhen(true)] StringBuilder? sb)
        {
            lock (_lock)
            {
                return _stringBuilder.Equals(sb);
            }
        }

        public bool Equals(ReadOnlySpan<char> span)
        {
            lock (_lock)
            {
                return _stringBuilder.Equals(span);
            }
        }

        public override string ToString()
        {
            lock (_lock)
            {
                return _stringBuilder.ToString();
            }
        }

        object ICloneable.Clone() => MemberwiseClone();
        public ConcurrentStringBuilder Clone() => new(ToString());
        StringBuilder ICloneable<StringBuilder>.Clone() => new(ToString());

        public string ToString(int startIndex, int length)
        {
            lock (_lock)
            {
                return _stringBuilder.ToString(startIndex, length);
            }
        }

        public string ToString(bool removeLast)
        {
            lock (_lock)
            {
                var rtn = ToString();
                return removeLast ? rtn[..^1] : rtn;
            }
        }
    }
}
