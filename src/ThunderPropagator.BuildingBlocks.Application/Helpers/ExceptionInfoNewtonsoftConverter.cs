using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    internal sealed class ExceptionInfoNewtonsoftConverter : JsonConverter<ExceptionInfo>
    {
        public override ExceptionInfo? ReadJson(JsonReader reader, Type objectType, ExceptionInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var obj = JObject.Load(reader);

            return ExceptionInfo.Create(
                obj.Value<string>("type") ?? obj.Value<string>("Type") ?? string.Empty,
                obj.Value<string>("message") ?? obj.Value<string>("Message") ?? string.Empty,
                obj.Value<string>("source") ?? obj.Value<string>("Source"),
                ReadInnerException(obj, serializer));
        }

        private static ExceptionInfo? ReadInnerException(JObject obj, JsonSerializer serializer)
        {
            var token = obj["innerException"] ?? obj["InnerException"];
            if (token is JObject inner)
                return serializer.Deserialize<ExceptionInfo>(inner.CreateReader());
            return null;
        }

        public override void WriteJson(JsonWriter writer, ExceptionInfo? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type);
            writer.WritePropertyName("message");
            writer.WriteValue(value.Message);
            if (value.Source is not null)
            {
                writer.WritePropertyName("source");
                writer.WriteValue(value.Source);
            }
            if (value.InnerException is not null)
            {
                writer.WritePropertyName("innerException");
                serializer.Serialize(writer, value.InnerException);
            }
            writer.WriteEndObject();
        }
    }
}
