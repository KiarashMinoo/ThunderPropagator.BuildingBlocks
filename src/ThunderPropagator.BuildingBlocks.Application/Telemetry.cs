using System.Diagnostics.Metrics;

namespace ThunderPropagator.BuildingBlocks.Application
{
    public static class Telemetry
    {
        public const string MeterName = "thunderPropagator.meter";
        public const string ActivityName = "thunderPropagator.activity";

        private static string _version = "1.0.0";
        private static int _configured;

        private static readonly Lazy<ActivitySource?> _activitySource = new(CreateActivitySource);
        private static readonly Lazy<Meter?> _meter = new(CreateMeter);

        /// <summary>Gets the version reported by the <see cref="ActivitySource"/>.</summary>
        public static string Version => _version;

        public static KeyValuePair<string, object?> SuccessfulTag => new("Status", "Success");
        public static KeyValuePair<string, object?> UnsuccessfulTag => new("Status", "Failed");

        /// <summary>
        /// Sets the version reported by the <see cref="ActivitySource"/>. Must be called once
        /// at application startup, before any telemetry activity is started.
        /// Subsequent calls are silently ignored.
        /// </summary>
        /// <param name="version">The version string to use for the <see cref="ActivitySource"/>.</param>
        public static void Configure(string version)
        {
            if (Interlocked.CompareExchange(ref _configured, 1, 0) == 0)
                _version = version;
        }

        private static ActivitySource? CreateActivitySource()
        {
            var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            if (!string.IsNullOrEmpty(endpoint))
            {
                var name = Environment.GetEnvironmentVariable("ACTIVITY_NAME") ?? ActivityName;
                return new ActivitySource(name, _version);
            }

            return null;
        }

        private static Meter? CreateMeter()
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable("METER_ENABLED") ?? "true", out var enabled) && enabled)
            {
                var name = Environment.GetEnvironmentVariable("METER_NAME") ?? MeterName;
                return new Meter(name);
            }

            return null;
        }

        public static bool HasListeners()
        {
            return _activitySource.Value?.HasListeners() ?? false;
        }

        public static Activity? StartActivity(string name, ActivityKind kind)
        {
            return _activitySource.Value?.StartActivity(kind, name: name);
        }

        public static Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
        {
            return _activitySource.Value?.StartActivity(kind, name: name, parentContext: parentContext);
        }

        public static Counter<T>? CreateCounter<T>(string name, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateCounter<T>(name, unit, description);
        }

        public static Counter<T>? CreateCounter<T>(string name, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateCounter<T>(name, unit, description, tags);
        }

        public static UpDownCounter<T>? CreateUpDownCounter<T>(string name, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateUpDownCounter<T>(name, unit, description);
        }

        public static UpDownCounter<T>? CreateUpDownCounter<T>(string name, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateUpDownCounter<T>(name, unit, description, tags);
        }

        public static ObservableUpDownCounter<T>? CreateObservableUpDownCounter<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableUpDownCounter(name, observeValue, unit, description);
        }

        public static ObservableUpDownCounter<T>? CreateObservableUpDownCounter<T>(string name, Func<T> observeValue, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableUpDownCounter(name, observeValue, unit, description, tags);
        }

        public static ObservableUpDownCounter<T>? CreateObservableUpDownCounter<T>(string name, Func<Measurement<T>> observeValue, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableUpDownCounter(name, observeValue, unit, description);
        }

        public static ObservableUpDownCounter<T>? CreateObservableUpDownCounter<T>(string name, Func<IEnumerable<Measurement<T>>> observeValues, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableUpDownCounter(name, observeValues, unit, description, tags);
        }

        public static ObservableCounter<T>? CreateObservableCounter<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableCounter(name, observeValue, unit, description);
        }

        public static ObservableCounter<T>? CreateObservableCounter<T>(string name, Func<T> observeValue, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableCounter(name, observeValue, unit, description, tags);
        }

        public static ObservableCounter<T>? CreateObservableCounter<T>(string name, Func<Measurement<T>> observeValue, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableCounter(name, observeValue, unit, description);
        }

        public static ObservableCounter<T>? CreateObservableCounter<T>(string name, Func<Measurement<T>> observeValue, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableCounter(name, observeValue, unit, description, tags);
        }

        public static ObservableCounter<T>? CreateObservableCounter<T>(string name, Func<IEnumerable<Measurement<T>>> observeValues, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableCounter(name, observeValues, unit, description);
        }

        public static ObservableCounter<T>? CreateObservableCounter<T>(string name, Func<IEnumerable<Measurement<T>>> observeValues, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableCounter(name, observeValues, unit, description, tags);
        }

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableGauge(name, observeValue, unit, description);
        }

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableGauge(name, observeValue, unit, description, tags);
        }

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<Measurement<T>> observeValue, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableGauge(name, observeValue, unit, description);
        }

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<Measurement<T>> observeValue, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableGauge(name, observeValue, unit, description, tags);
        }

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<IEnumerable<Measurement<T>>> observeValues, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateObservableGauge(name, observeValues, unit, description);
        }

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<IEnumerable<Measurement<T>>> observeValues, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateObservableGauge(name, observeValues, unit, description, tags);
        }
    }
}
