namespace ThunderPropagator.BuildingBlocks.Application.Serializations
{
    /// <summary>
    /// Identifies the serialization library to use.
    /// </summary>
    public class SerializerType
    {
        public int Value { get; }

        private SerializerType(int value) => Value = value;

        public static implicit operator int(SerializerType type) => type.Value;
        public static implicit operator SerializerType(int value) => new(value);
    }
}
