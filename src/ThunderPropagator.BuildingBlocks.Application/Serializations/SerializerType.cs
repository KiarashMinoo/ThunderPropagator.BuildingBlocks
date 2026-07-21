namespace ThunderPropagator.BuildingBlocks.Application.Serializations;

/// <summary>
/// Identifies the serialization library to use.
/// </summary>
/// <param name="Value">The numeric identifier of the serialization format.</param>
public readonly record struct SerializerType(int Value) : IComparable<SerializerType>, IComparable
{
    public static implicit operator int(SerializerType type) => type.Value;
    public static implicit operator SerializerType(int value) => new(value);

    public static bool operator <(SerializerType left, SerializerType right) => left.Value < right.Value;
    public static bool operator >(SerializerType left, SerializerType right) => left.Value > right.Value;
    public static bool operator <=(SerializerType left, SerializerType right) => left.Value <= right.Value;
    public static bool operator >=(SerializerType left, SerializerType right) => left.Value >= right.Value;

    /// <inheritdoc/>
    public int CompareTo(SerializerType other) => Value.CompareTo(other.Value);

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (obj is not SerializerType other)
        {
            throw new ArgumentException($"Object must be of type {nameof(SerializerType)}.", nameof(obj));
        }

        return CompareTo(other);
    }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}
