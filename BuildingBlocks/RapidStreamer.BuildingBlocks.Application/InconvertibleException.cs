namespace RapidStreamer.BuildingBlocks.Application
{
    public
#if !DEBUG
        sealed
#endif
        class InconvertibleException : Exception
    {
        public InconvertibleException(string message) : base(message)
        {
        }

        public InconvertibleException(Type sourceType, Type destinationType)
            : base($"value with type {sourceType} is not convertable to type {destinationType} ")
        {
        }

        public static void ThrowIfInconvertible(Func<bool> condition, string message)
        {
            ThrowIfInconvertible(condition.Invoke(), message);
        }

        public static void ThrowIfInconvertible(bool condition, string message)
        {
            if (!condition)
            {
                throw new InconvertibleException(message);
            }
        }
    }
}