using System.Globalization;
using System.Numerics;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace ThunderPropagator.BuildingBlocks.Application.Serializations.Yaml
{
    public interface IYamlTypeConverter<T> : IYamlTypeConverter
    {
        T? ReadYaml(IParser parser, ObjectDeserializer rootDeserializer);
        void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer);
    }

    public abstract class BaseYamlTypeConverter
    {
        protected readonly char[] KeyCharactersThatRequireQuotes = [' ', '/', '\\', '~', ':', '$', '{', '}'];

        public abstract bool Accepts(Type type);

        protected IYamlTypeConverter? GetYamlTypeConverter(Type type)
        {
            var converterType = type.GetCustomAttribute<YamlTypeConverterAttribute>()?.ConverterType;
            return converterType is not null ? (IYamlTypeConverter)Activator.CreateInstance(converterType)! : null;
        }

        protected bool ShiftIf(Func<IParser, bool> check, IParser parser)
        {
            var result = check(parser);
            if (result) parser.MoveNext();
            return result;
        }

        protected bool IsMappingStart(IParser parser) => parser.Current is MappingStart;
        protected bool IsMappingEnd(IParser parser) => parser.Current is MappingEnd;
        protected bool IsSequenceStart(IParser parser) => parser.Current is SequenceStart;
        protected bool IsSequenceEnd(IParser parser) => parser.Current is SequenceEnd;

        protected bool IsMappingStartAndShift(IParser parser) => ShiftIf(IsMappingStart, parser);
        protected bool IsMappingEndAndShift(IParser parser) => ShiftIf(IsMappingEnd, parser);
        protected bool IsSequenceStartAndShift(IParser parser) => ShiftIf(IsSequenceStart, parser);
        protected bool IsSequenceEndAndShift(IParser parser) => ShiftIf(IsSequenceEnd, parser);

        protected void WriteMappingStart(IEmitter emitter) => emitter.Emit(new MappingStart(AnchorName.Empty, TagName.Empty, isImplicit: true, MappingStyle.Block));
        protected void WriteMappingEnd(IEmitter emitter) => emitter.Emit(new MappingEnd());
        protected void WriteSequenceStart(IEmitter emitter) => emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, isImplicit: true, SequenceStyle.Block));
        protected void WriteSequenceEnd(IEmitter emitter) => emitter.Emit(new SequenceEnd());


        protected void Serialize(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
            => GetYamlTypeConverter(value?.GetType() ?? type)?.WriteYaml(emitter, value, type, serializer);

        protected void Serialize<TAny>(IEmitter emitter, TAny? value, ObjectSerializer serializer)
            => (GetYamlTypeConverter(value?.GetType() ?? typeof(TAny)) as IYamlTypeConverter<TAny>)?.WriteYaml(emitter, value, typeof(TAny), serializer);

        protected object? Deserialize(IParser parser, Type type, ObjectDeserializer rootDeserializer)
            => GetYamlTypeConverter(type)?.ReadYaml(parser, type, rootDeserializer);

        protected TAny? Deserialize<TAny>(IParser parser, ObjectDeserializer rootDeserializer)
            => GetYamlTypeConverter(typeof(TAny)) is IYamlTypeConverter<TAny> converter ? converter.ReadYaml(parser, rootDeserializer) : default;

        protected object? DeserializeAndShift(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var result = Deserialize(parser, type, rootDeserializer);
            parser.MoveNext();
            return result;
        }

        protected TAny? DeserializeAndShift<TAny>(IParser parser, ObjectDeserializer rootDeserializer)
        {
            var result = Deserialize<TAny>(parser, rootDeserializer);
            parser.MoveNext();
            return result;
        }


        protected void WriteKey(IEmitter emitter, string key)
        {
            var style = key.IndexOfAny(KeyCharactersThatRequireQuotes) >= 0 ? ScalarStyle.DoubleQuoted : ScalarStyle.Plain;
            var keyScalar = new Scalar(null, null, key, style, isPlainImplicit: style == ScalarStyle.Plain, isQuotedImplicit: style != ScalarStyle.Plain);
            emitter.Emit(keyScalar);
        }

        protected string ReadKey(IParser parser)
        {
            var key = ReadValue(parser);
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidDataException("Invalid YAML content.");
            parser.MoveNext();
            return key;
        }

        protected void WriteValue(IEmitter emitter, string value)
            => emitter.Emit(new Scalar(null, null, value, ScalarStyle.DoubleQuoted, false, true));

        protected string? ReadValue(IParser parser) => (parser.Current as Scalar)?.Value;

        protected string? ReadValueAndShift(IParser parser)
        {
            var value = ReadValue(parser);
            parser.MoveNext();
            return value;
        }

        //Enum
        protected void WriteKeyValue(IEmitter emitter, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                WriteKey(emitter, key);
                WriteValue(emitter, value);
            }
        }

        protected void WriteEnum(IEmitter emitter, string key, Enum? value)
        {
            if (value is not null)
                WriteKeyValue(emitter, key, value.ToString()!);
        }

        protected Enum? ReadEnum(IParser parser, Type type)
        {
            var value = ReadValue(parser);
            return Enum.TryParse(type, value, true, out var result) ? result as Enum : null;
        }

        protected Enum? ReadEnumAndShift(IParser parser, Type type)
        {
            var result = ReadEnum(parser, type);
            parser.MoveNext();
            return result;
        }

        protected TEnum ReadEnum<TEnum>(IParser parser) where TEnum : struct
        {
            var value = ReadValue(parser);
            return Enum.TryParse(value, true, out TEnum result) ? result : default;
        }

        protected TEnum ReadEnumAndShift<TEnum>(IParser parser) where TEnum : struct
        {
            var result = ReadEnum<TEnum>(parser);
            parser.MoveNext();
            return result;
        }

        //Boolean
        protected void WriteBoolean(IEmitter emitter, string key, bool? value)
        {
            if (value is not null)
                WriteKeyValue(emitter, key, value == true ? "yes" : "no");
        }

        protected bool? ReadBoolean(IParser parser)
        {
            return (parser.Current as Scalar)?.Value?.ToLowerInvariant() switch
            {
                "1" or "true" or "yes" => true,
                "0" or "false" or "no" => false,
                _ => null
            };
        }

        protected bool? ReadBooleanAndShift(IParser parser)
        {
            var result = ReadBoolean(parser);
            parser.MoveNext();
            return result;
        }

        //Number
        protected void WriteNumber<TNumber>(IEmitter emitter, string key, TNumber? value) where TNumber : INumber<TNumber>
        {
            if (value is not null)
                WriteKeyValue(emitter, key, value.ToString()!);
        }

        protected TNumber? ReadNumber<TNumber>(IParser parser) where TNumber : INumber<TNumber>
        {
            var value = ReadValue(parser);
            return TNumber.TryParse(value, CultureInfo.CurrentCulture, out var result) ? result : default;
        }

        protected TNumber? ReadNumberAndShift<TNumber>(IParser parser) where TNumber : INumber<TNumber>
        {
            var result = ReadNumber<TNumber>(parser);
            parser.MoveNext();
            return result;
        }
    }

    public abstract class YamlTypeConverter : BaseYamlTypeConverter, IYamlTypeConverter
    {
        void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is not null)
                WriteYamlInternal(emitter, value, type, serializer);
        }

        protected abstract void WriteYamlInternal(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);

        object? IYamlTypeConverter.ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            if (!IsMappingStartAndShift(parser) && !IsSequenceStartAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
            return ReadYamlInternal(parser, type, rootDeserializer);
        }

        protected abstract object? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer);
    }

    public abstract class YamlTypeConverter<T> : BaseYamlTypeConverter, IYamlTypeConverter<T>
    {
        public override bool Accepts(Type type) => type == typeof(T);

        void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is T typed)
                WriteYamlInternal(emitter, typed, type, serializer);
        }

        void IYamlTypeConverter<T>.WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer)
            => WriteYamlInternal(emitter, value, typeof(T), serializer);

        object? IYamlTypeConverter.ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            if (!IsMappingStartAndShift(parser) && !IsSequenceStartAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
            return ReadYamlInternal(parser, type, rootDeserializer);
        }

        T? IYamlTypeConverter<T>.ReadYaml(IParser parser, ObjectDeserializer rootDeserializer)
        {
            if (!IsMappingStartAndShift(parser) && !IsSequenceStartAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
            return ReadYamlInternal(parser, typeof(T), rootDeserializer);
        }

        protected abstract void WriteYamlInternal(IEmitter emitter, T? value, Type type, ObjectSerializer serializer);
        protected abstract T? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer);
    }
}