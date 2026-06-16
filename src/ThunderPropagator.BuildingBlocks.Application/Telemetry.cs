using System.Diagnostics.Metrics;

namespace ThunderPropagator.BuildingBlocks.Application
{
    /// <summary>
    /// Central telemetry facade for the ThunderPropagator.BuildingBlocks library.
    /// Wraps <see cref="ActivitySource"/> and <see cref="System.Diagnostics.Metrics.Meter"/> so
    /// callers never reference the underlying instances directly.
    /// <para>
    /// <b>Naming conventions (OTel semantic conventions):</b><br/>
    /// Meter name: <c>thunderpropagator.{subsystem}</c> — e.g., <c>thunderpropagator.buildingblocks</c>.<br/>
    /// Metric name: <c>thunderpropagator.{subsystem}.{noun}.{verb}</c> — all lowercase, dot-separated.<br/>
    /// Unit strings: use OTel units — <c>{message}</c>, <c>{request}</c>, <c>ms</c>, <c>By</c>, <c>1</c>, etc.<br/>
    /// No <c>snake_case</c> or <c>PascalCase</c> in metric names.
    /// </para>
    /// </summary>
    public static class Telemetry
    {
        /// <summary>
        /// The default <see cref="System.Diagnostics.Metrics.Meter"/> name for this library.
        /// Override at startup via the <c>METER_NAME</c> environment variable.
        /// Follows the OTel <c>thunderpropagator.{subsystem}</c> convention.
        /// </summary>
        public const string MeterName = "thunderpropagator.buildingblocks";

        /// <summary>
        /// The default <see cref="ActivitySource"/> name for this library.
        /// Override at startup via the <c>ACTIVITY_NAME</c> environment variable.
        /// Follows the OTel <c>thunderpropagator.{subsystem}</c> convention.
        /// </summary>
        public const string ActivityName = "thunderpropagator.buildingblocks";

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

        public static Histogram<T>? CreateHistogram<T>(string name, string? unit = null, string? description = null)
            where T : struct
        {
            return _meter.Value?.CreateHistogram<T>(name, unit, description);
        }

        public static Histogram<T>? CreateHistogram<T>(string name, string? unit, string? description, IEnumerable<KeyValuePair<string, object?>> tags)
            where T : struct
        {
            return _meter.Value?.CreateHistogram<T>(name, unit, description, tags);
        }
    }
}
