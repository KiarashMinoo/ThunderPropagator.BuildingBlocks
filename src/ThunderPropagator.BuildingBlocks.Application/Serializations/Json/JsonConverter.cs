using System.Text.Json;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Json
{
    public abstract class JsonConverter<T> : System.Text.Json.Serialization.JsonConverter<T>
    {
        public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");

            return ReadInternal(ref reader, typeToConvert, options);
        }

        protected abstract T? ReadInternal(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        protected void ThrowException(string message) => throw new JsonException(message);

        protected void WriteValue(Utf8JsonWriter writer, object value)
        {
            WriteValue(writer, null, value);
        }

        protected void WriteValue(Utf8JsonWriter writer, string? key, object objectValue)
        {
            if (!string.IsNullOrWhiteSpace(key))
                writer.WritePropertyName(key);

            switch (objectValue)
            {
                case Enum @enum:
                    writer.WriteNumberValue(Convert.ToInt32(@enum));
                    break;
                case string stringValue:
                    writer.WriteStringValue(stringValue);
                    break;
                case DateTime dateTime:
                    writer.WriteStringValue(dateTime);
                    break;
                case long longValue:
                    writer.WriteNumberValue(longValue);
                    break;
                case int intValue:
                    writer.WriteNumberValue(intValue);
                    break;
                case float floatValue:
                    writer.WriteNumberValue(floatValue);
                    break;
                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;
                case decimal decimalValue:
                    writer.WriteNumberValue(decimalValue);
                    break;
                case bool boolValue:
                    writer.WriteBooleanValue(boolValue);
                    break;
                case Dictionary<string, object> dict:
                    writer.WriteStartObject();
                    foreach (var item in dict)
                    {
                        WriteValue(writer, item.Key, item.Value);
                    }

                    writer.WriteEndObject();
                    break;
                case object[] array:
                    writer.WriteStartArray();
                    foreach (var item in array)
                    {
                        WriteValue(writer, item);
                    }

                    writer.WriteEndArray();
                    break;
                default:
                    writer.WriteNullValue();
                    break;
            }
        }

        protected object? ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.TryGetDateTime(out var date) ? date : reader.GetString();
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    return reader.TryGetInt64(out var result) ? result : reader.GetDecimal();
                case JsonTokenType.StartObject:
                    return Read(ref reader, null!, options);
                case JsonTokenType.StartArray:
                    var list = new List<object?>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        list.Add(ReadValue(ref reader, options));
                    return list;
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }
    }
}