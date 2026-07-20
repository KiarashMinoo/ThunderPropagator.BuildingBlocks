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

        /// <summary>
        /// System.Text.Json — application/json
        /// </summary>
        public static SerializerType Json { get; } = new(1);

        /// <summary>
        /// Newtonsoft.Json — application/json
        /// </summary>
        public static SerializerType NJson { get; } = new(2);
    }
}
