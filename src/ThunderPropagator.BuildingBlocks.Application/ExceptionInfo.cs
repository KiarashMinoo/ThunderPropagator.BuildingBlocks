using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ThunderPropagator.BuildingBlocks.Application
{
    public
#if !DEBUG
        sealed
#endif
        class ExceptionInfo
    {
        [JsonProperty, JsonInclude] public string Type { get; private set; } = null!;

        [JsonProperty, JsonInclude] public string Message { get; private set; } = null!;

        [JsonProperty, JsonInclude] public string? Source { get; private set; }

        [JsonProperty, JsonInclude] public ExceptionInfo? InnerException { get; set; }

        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        private ExceptionInfo()
        {
        }

        private ExceptionInfo(Exception exception, int level)
        {
            Type = exception.GetType().FullName!;
            Message = exception.Message;
            Source = exception.Source;

            if (level == 0 && exception.InnerException is not null)
                InnerException = new ExceptionInfo(exception.InnerException, 1);
        }

        internal ExceptionInfo(Exception exception) : this(exception, 0)
        {
        }

        public static explicit operator ExceptionInfo(Exception exception) => new(exception);
    }
}