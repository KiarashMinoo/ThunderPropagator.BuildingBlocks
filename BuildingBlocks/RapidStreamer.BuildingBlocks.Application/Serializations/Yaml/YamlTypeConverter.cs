using System.Globalization;
using System.Numerics;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace RapidStreamer.BuildingBlocks.Application.Serializations.Yaml
{
    public interface IYamlTypeConverter<T> : IYamlTypeConverter
    {
        T? ReadYaml(IParser parser, ObjectDeserializer rootDeserializer);
        void WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer);
    }

    public abstract class BaseYamlTypeConverter
    {
        protected readonly Type MappingStartType = typeof(MappingStart);
        protected readonly Type MappingEndType = typeof(MappingEnd);

        protected readonly Type SequenceStartType = typeof(SequenceStart);
        protected readonly Type SequenceEndType = typeof(SequenceEnd);

        protected readonly char[] KeyCharactersThatRequireQuotes = [' ', '/', '\\', '~', ':', '$', '{', '}'];

        public abstract bool Accepts(Type type);

        protected IYamlTypeConverter? GetYamlTypeConverter(Type type)
        {
            var converterType = type.GetCustomAttribute<YamlTypeConverterAttribute>()?.ConverterType;
            return converterType is not null ? (IYamlTypeConverter)Activator.CreateInstance(converterType)! : null;
        }

        protected bool DoubleShift(IParser parser) => parser.MoveNext() && parser.MoveNext();

        protected void WriteMappingStart(IEmitter emitter) => emitter.Emit(new MappingStart(AnchorName.Empty, TagName.Empty, isImplicit: true, MappingStyle.Block));
        protected bool IsMappingStart(IParser parser) => parser.Current?.GetType() == MappingStartType;

        protected void WriteMappingEnd(IEmitter emitter) => emitter.Emit(new MappingEnd());
        protected bool IsMappingEnd(IParser parser) => parser.Current?.GetType() == MappingEndType;

        protected bool IsMappingEndAndShift(IParser parser)
        {
            var rtn = IsMappingEnd(parser);

            if (rtn)
                parser.MoveNext();

            return rtn;
        }

        protected void WriteSequenceStart(IEmitter emitter) => emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, isImplicit: true, SequenceStyle.Block));
        protected bool IsSequenceStart(IParser parser) => parser.Current?.GetType() == SequenceStartType;

        protected bool IsSequenceStartAndShift(IParser parser)
        {
            var rtn = IsSequenceStart(parser);

            parser.MoveNext();

            return rtn;
        }

        protected void WriteSequenceEnd(IEmitter emitter) => emitter.Emit(new SequenceEnd());
        protected bool IsSequenceEnd(IParser parser) => parser.Current?.GetType() == SequenceEndType;

        protected bool IsSequenceEndAndShift(IParser parser)
        {
            var rtn = IsSequenceEnd(parser);

            if (rtn)
                parser.MoveNext();

            return rtn;
        }

        //Serialization
        protected void Serialize(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
            => GetYamlTypeConverter(value?.GetType() ?? type)?.WriteYaml(emitter, value, type, serializer);

        protected object? Deserialize(IParser parser, Type type, ObjectDeserializer rootDeserializer) => GetYamlTypeConverter(type)?.ReadYaml(parser, type, rootDeserializer);

        protected object? DeserializeAndShift(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            var rtn = Deserialize(parser, type, rootDeserializer);

            parser.MoveNext();

            return rtn;
        }

        protected void Serialize<TAny>(IEmitter emitter, TAny? value, ObjectSerializer serializer)
            => (GetYamlTypeConverter(value?.GetType() ?? typeof(TAny)) as IYamlTypeConverter<TAny>)?.WriteYaml(emitter, value, typeof(TAny), serializer);

        protected TAny? Deserialize<TAny>(IParser parser, ObjectDeserializer rootDeserializer)
            => GetYamlTypeConverter(typeof(TAny)) is IYamlTypeConverter<TAny> converter ? converter.ReadYaml(parser, rootDeserializer) : default;

        protected TAny? DeserializeAndShift<TAny>(IParser parser, ObjectDeserializer rootDeserializer)
        {
            var rtn = Deserialize<TAny>(parser, rootDeserializer);

            parser.MoveNext();

            return rtn;
        }

        //Key Value
        protected void WriteKey(IEmitter emitter, string key)
        {
            var keyScalar = key.IndexOfAny(KeyCharactersThatRequireQuotes) >= 0
                ? new Scalar(AnchorName.Empty, TagName.Empty, key, ScalarStyle.DoubleQuoted, isPlainImplicit: false, isQuotedImplicit: true)
                : new Scalar(AnchorName.Empty, TagName.Empty, key, ScalarStyle.Plain, isPlainImplicit: true, isQuotedImplicit: false);

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
        {
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, value, ScalarStyle.DoubleQuoted, isPlainImplicit: false, isQuotedImplicit: true));
        }

        protected string? ReadValue(IParser parser) => (parser.Current as Scalar)?.Value;

        protected string? ReadValueAndShift(IParser parser)
        {
            var rtn = ReadValue(parser);

            parser.MoveNext();

            return rtn;
        }

        protected void WriteKeyValue(IEmitter emitter, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return;

            WriteKey(emitter, key);
            WriteValue(emitter, value);
        }

        //Enum
        protected void WriteEnum(IEmitter emitter, string key, Enum? value)
        {
            if (value is not null)
                WriteKeyValue(emitter, key, value.ToString());
        }

        protected Enum? ReadEnum(IParser parser, Type type)
        {
            var scalar = parser.Current as Scalar;
            return !string.IsNullOrWhiteSpace(scalar?.Value) && Enum.TryParse(type, scalar.Value, true, out var @enum) ? @enum as Enum : null;
        }

        protected Enum? ReadEnumAndShift(IParser parser, Type type)
        {
            var rtn = ReadEnum(parser, type);

            parser.MoveNext();

            return rtn;
        }

        protected TEnum ReadEnum<TEnum>(IParser parser)
            where TEnum : struct
        {
            var scalar = parser.Current as Scalar;
            return !string.IsNullOrWhiteSpace(scalar?.Value) && Enum.TryParse<TEnum>(scalar.Value, true, out var @enum) ? @enum : default;
        }

        protected TEnum ReadEnumAndShift<TEnum>(IParser parser)
            where TEnum : struct
        {
            var rtn = ReadEnum<TEnum>(parser);

            parser.MoveNext();

            return rtn;
        }

        //Boolean
        protected void WriteBoolean(IEmitter emitter, string key, bool? value)
        {
            if (value is not null)
                WriteKeyValue(emitter, key, value == true ? "yes" : "no");
        }

        protected bool? ReadBoolean(IParser parser)
        {
            var scalar = parser.Current as Scalar;

            return string.IsNullOrWhiteSpace(scalar?.Value)
                ? null
                : string.Equals(scalar.Value, "1") ||
                  string.Equals(scalar.Value, "true", StringComparison.InvariantCultureIgnoreCase) ||
                  string.Equals(scalar.Value, "yes", StringComparison.InvariantCultureIgnoreCase);
        }

        protected bool? ReadBooleanAndShift(IParser parser)
        {
            var rtn = ReadBoolean(parser);

            parser.MoveNext();

            return rtn;
        }

        //Number
        protected void WriteNumber<TNumber>(IEmitter emitter, string key, TNumber? value)
            where TNumber : INumber<TNumber>
        {
            if (value is not null)
                WriteKeyValue(emitter, key, value.ToString()!);
        }

        protected TNumber? ReadNumber<TNumber>(IParser parser)
            where TNumber : INumber<TNumber>
        {
            var rtn = ReadValue(parser);

            if (!string.IsNullOrEmpty(rtn) && TNumber.TryParse(rtn, CultureInfo.CurrentCulture, out var result))
                return result;

            return default;
        }

        protected TNumber? ReadNumberAndShift<TNumber>(IParser parser)
            where TNumber : INumber<TNumber>
        {
            var rtn = ReadNumber<TNumber>(parser);

            parser.MoveNext();

            return rtn;
        }


        //Throw Part
        protected bool IsMappingStartAndShift(IParser parser)
        {
            var rtn = IsMappingStart(parser);

            parser.MoveNext();

            return rtn;
        }

        protected void ThrowIfIsNotMappingEnd(IParser parser)
        {
            if (!IsMappingEnd(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotSequenceEnd(IParser parser)
        {
            if (!IsSequenceEnd(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotMappingStart(IParser parser)
        {
            if (!IsMappingStart(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotSequenceStart(IParser parser)
        {
            if (!IsSequenceStart(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotMappingEndAndShift(IParser parser)
        {
            if (!IsMappingEndAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotSequenceEndAndShift(IParser parser)
        {
            if (!IsSequenceEndAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotMappingStartAndShift(IParser parser)
        {
            if (!IsMappingStartAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }

        protected void ThrowIfIsNotSequenceStartAndShift(IParser parser)
        {
            if (!IsSequenceStartAndShift(parser))
                throw new InvalidDataException("Invalid YAML content.");
        }
    }

    public abstract class YamlTypeConverter : BaseYamlTypeConverter,
        IYamlTypeConverter
    {
        void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is not null)
                WriteYamlInternal(emitter, value, type, serializer);
        }

        protected abstract void WriteYamlInternal(IEmitter emitter, object? value, Type type, ObjectSerializer serializer);

        object? IYamlTypeConverter.ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            ThrowIfIsNotMappingStartAndShift(parser);
            return ReadYamlInternal(parser, type, rootDeserializer);
        }

        protected abstract object? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer);
    }

    public abstract class YamlTypeConverter<T> : BaseYamlTypeConverter,
        IYamlTypeConverter<T>
    {
        public override bool Accepts(Type type) => type == typeof(T);

        void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            if (value is T instance)
                WriteYamlInternal(emitter, instance, type, serializer);
        }

        void IYamlTypeConverter<T>.WriteYaml(IEmitter emitter, T? value, ObjectSerializer serializer) => WriteYamlInternal(emitter, value, typeof(T), serializer);
        protected abstract void WriteYamlInternal(IEmitter emitter, T? value, Type type, ObjectSerializer serializer);

        object? IYamlTypeConverter.ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            ThrowIfIsNotMappingStartAndShift(parser);
            return ReadYamlInternal(parser, type, rootDeserializer);
        }

        T? IYamlTypeConverter<T>.ReadYaml(IParser parser, ObjectDeserializer rootDeserializer)
        {
            ThrowIfIsNotMappingStartAndShift(parser);
            return ReadYamlInternal(parser, typeof(T), rootDeserializer);
        }

        protected abstract T? ReadYamlInternal(IParser parser, Type type, ObjectDeserializer rootDeserializer);
    }
}