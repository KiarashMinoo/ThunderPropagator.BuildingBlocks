using RapidStreamer.BuildingBlocks.Application.Helpers;

namespace RapidStreamer.BuildingBlocks.Application.Objects
{
    public abstract class EquatableObject : IEquatable<EquatableObject>
    {
        /// <summary>
        ///     Returns true if LoginRequestDto instances are equal
        /// </summary>
        /// <param name="obj">Instance of ValueObject to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(EquatableObject? obj)
        {
            return Equals(obj as object);
        }

        /// <summary>
        ///     Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object? obj) => this.EquatableEqual(obj);

        /// <summary>
        ///     Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() => this.EquatableHashCode();

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