using System.Reflection;
using RapidStreamer.BuildingBlocks.Application.Attributes;

namespace RapidStreamer.BuildingBlocks.Application.Objects
{
    public abstract class EquatableObject : IEquatable<EquatableObject>
    {
        protected virtual IEnumerable<object?> GetAtomicValues()
        {
            var type = GetType();

            var fieldsValues = type
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
                .Select(field => field.GetValue(this));

            var propertiesValues = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead && property.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
                .Select(property => property.GetValue(this));

            return fieldsValues.Union(propertiesValues);
        }

        /// <summary>
        ///     Returns true if LoginRequestDto instances are equal
        /// </summary>
        /// <param name="obj">Instance of ValueObject to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EquatableObject? obj) => obj is not null && GetAtomicValues().SequenceEqual(obj.GetAtomicValues());

        /// <summary>
        ///     Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object? obj) => Equals(obj as EquatableObject);

        /// <summary>
        ///     Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() => GetAtomicValues().Aggregate(0, HashCode.Combine);

        public static bool operator ==(EquatableObject obj1, EquatableObject obj2)
            => obj1 switch
            {
                null when Equals(obj2, null) => true,
                null => false,
                _ => obj1.Equals(obj2)
            };

        public static bool operator !=(EquatableObject obj1, EquatableObject obj2)
        {
            return !(obj1 == obj2);
        }
    }

    public abstract class EquatableObject<TEquatableObject> : EquatableObject,
        IEquatable<TEquatableObject>
        where TEquatableObject : EquatableObject<TEquatableObject>
    {
        /// <summary>
        ///     Returns true if LoginRequestDto instances are equal
        /// </summary>
        /// <param name="obj">Instance of ValueObject to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TEquatableObject? obj)
        {
            return Equals(obj as object);
        }
    }
}