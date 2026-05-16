using System.Text.Json.Serialization;
using ThunderPropagator.BuildingBlocks.Application.Helpers;

namespace ThunderPropagator.BuildingBlocks.Application
{
    [Newtonsoft.Json.JsonConverter(typeof(ExceptionInfoNewtonsoftConverter))]
    public
#if !DEBUG
        sealed
#endif
        class ExceptionInfo
    {
        public string Type { get; init; } = null!;

        public string Message { get; init; } = null!;

        public string? Source { get; init; }

        public ExceptionInfo? InnerException { get; init; }

        [JsonConstructor]
        private ExceptionInfo()
        {
        }

        private const int MaxDepth = 10;

        private ExceptionInfo(Exception exception, int depth)
        {
            Type = exception.GetType().FullName!;
            Message = exception.Message;
            Source = exception.Source;

            if (depth < MaxDepth && exception.InnerException is not null)
                InnerException = new ExceptionInfo(exception.InnerException, depth + 1);
        }

        internal ExceptionInfo(Exception exception) : this(exception, 0)
        {
        }

        internal static ExceptionInfo Create(string type, string message, string? source, ExceptionInfo? innerException)
        {
            return new ExceptionInfo
            {
                Type = type,
                Message = message,
                Source = source,
                InnerException = innerException
            };
        }

        public static explicit operator ExceptionInfo(Exception exception) => new(exception);
    }
}
