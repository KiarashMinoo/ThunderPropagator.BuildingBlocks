using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RapidStreamer.BuildingBlocks.Application.Objects;

public abstract class ImmutableObject<TImmutableObject> : EquatableObject<TImmutableObject>
    where TImmutableObject : ImmutableObject<TImmutableObject>
{
    private List<object?>? _atomicValues;
    private int? _hashCode;

    protected ImmutableObject()
    {
        ValidateFields();

        ValidateProperties();
    }

    private void ValidateFields()
    {
        var gotAnyPublicField = GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Length != 0;

        if (gotAnyPublicField)
            throw new InvalidOperationException("This object is immutable.");
    }

    private void ValidateProperties()
    {
        var gotAnyPublicSetter = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Any(property => property.CanWrite);

        if (gotAnyPublicSetter)
            throw new InvalidOperationException("This object is immutable.");

        _atomicValues = base.GetAtomicValues();
        _hashCode = _atomicValues.Aggregate(0, HashCode.Combine);
    }

    protected override List<object?> GetAtomicValues() => _atomicValues ??= base.GetAtomicValues();

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() => _hashCode ??= GetAtomicValues().Aggregate(0, HashCode.Combine);
}

public abstract class ImmutableObject : EquatableObject<ImmutableObject>;