using System.Text;
using ToonNetSerializer;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers;

public static class ToonHelper
{
    private static ToonOptions BuildDefaultToonOptions() => new()
    {
        SerializerOptions = JsonHelper.BuildDefaultSerializerOptions()
    };

    private static ToonOptions ToonOptions<T>(ToonOptions? toonOptions = null)
        => ToonOptions(typeof(T), toonOptions);

    private static ToonOptions ToonOptions(Type type, ToonOptions? toonOptions = null)
    {
        toonOptions ??= BuildDefaultToonOptions();

        toonOptions.SerializerOptions = JsonHelper.JsonSerializerOptions(type, toonOptions.SerializerOptions);

        return toonOptions;
    }

    public static string ToToon<T>(this T instance, Func<ToonOptions, ToonOptions>? settings = null)
    {
        const string activityName = $"{nameof(ToonHelper)}_{nameof(ToToon)}";
        using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

        ToonOptions? serializerSettings = null;

        if (settings is not null)
        {
            serializerSettings = BuildDefaultToonOptions();
            settings(serializerSettings);
        }

        if (instance is Exception exception)
        {
            ExceptionInfo exceptionInfo = new(exception);
            return ToonNet.Encode(exceptionInfo, ToonOptions<T>(serializerSettings));
        }

        return ToonNet.Encode(instance, ToonOptions<T>(serializerSettings));
    }

    public static byte[] ToToonBytes<T>(this T instance, Func<ToonOptions, ToonOptions>? settings = null)
        where T : notnull
    {
        const string activityName = $"{nameof(ToonHelper)}_{nameof(ToToonBytes)}";
        using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

        var toon = ToToon(instance, settings);
        return Encoding.UTF8.GetBytes(toon);
    }

    public static string ToToonBase64<T>(this T instance, Func<ToonOptions, ToonOptions>? settings = null)
        where T : notnull
    {
        const string activityName = $"{nameof(ToonHelper)}_{nameof(ToToonBase64)}";
        using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

        var bytes = instance.ToToonBytes(settings);
        return Convert.ToBase64String(bytes);
    }

    public static T? FromToon<T>(this string toon, Func<ToonDecodeOptions, ToonDecodeOptions>? settings = null)
    {
        const string activityName = $"{nameof(ToonHelper)}_{nameof(FromToon)}";
        using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

        ToonDecodeOptions? serializerSettings = null;

        if (settings is not null)
        {
            serializerSettings = new ToonDecodeOptions();
            settings(serializerSettings);
        }

        return ToonNet.Decode<T>(toon, serializerSettings);
    }

    public static T? FromToonBytes<T>(this byte[] bytes, Func<ToonDecodeOptions, ToonDecodeOptions>? settings = null)
    {
        if (bytes.Length == 0)
        {
            return default;
        }

        var toon = Encoding.UTF8.GetString(bytes);
        return FromToon<T>(toon, settings);
    }

    public static T? FromToonBase64<T>(this string str, Func<ToonDecodeOptions, ToonDecodeOptions>? settings = null)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return default;
        }

        var bytes = Convert.FromBase64String(str);

        return bytes.FromToonBytes<T>(settings);
    }
}