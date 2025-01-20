namespace RapidStreamer.BuildingBlocks.Application.Objects
{
    public readonly struct CompressedObject
    {
        public enum CompressionType
        {
            GZipStream,
            DeflateStream,
            BrotliStream,
            BZip2,
            GZip
        }

        private readonly byte[] _value;

        public int Length => _value.Length;

        internal CompressedObject(byte[] value) => _value = value;

        public override string ToString() => Convert.ToBase64String(_value);

        public static implicit operator CompressedObject(string value) => Convert.FromBase64String(value);
        public static implicit operator string(CompressedObject compressedObject) => compressedObject.ToString();

        public static implicit operator CompressedObject(byte[] bytes) => new(bytes);
        public static implicit operator byte[](CompressedObject compressedObject) => compressedObject._value;
    }
}