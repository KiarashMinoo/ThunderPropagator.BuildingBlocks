using System.Reflection;

namespace RapidStreamer.BuildingBlocks.Application.Objects;

public abstract class ImmutableObject<TImmutableObject> : EquatableObject<TImmutableObject>
    where TImmutableObject : ImmutableObject<TImmutableObject>
{
    private IEnumerable<object?>? _atomicValues;

    public ImmutableObject()
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
    }

    protected override IEnumerable<object?> GetAtomicValues() => _atomicValues ??= base.GetAtomicValues();
}

public abstract class ImmutableObject : EquatableObject<ImmutableObject>;