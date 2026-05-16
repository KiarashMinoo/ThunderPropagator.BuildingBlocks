using System.Diagnostics.Metrics;

namespace ThunderPropagator.BuildingBlocks.Application
{
    public static class Telemetry
    {
        public const string MeterName = "thunderPropagator.meter";
        public const string ActivityName = "thunderPropagator.activity";

        private static readonly ActivitySource? ActivitySource;
        private static readonly Meter? Meter;
        public static string Version { get; set; } = "1.0.0";

        public static KeyValuePair<string, object?> SuccessfulTag => new("Status", "Success");
        public static KeyValuePair<string, object?> UnsuccessfulTag => new("Status", "Failed");


        static Telemetry()
        {
            var otelExporterEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            if (!string.IsNullOrEmpty(otelExporterEndpoint))
            {
                var activityName = Environment.GetEnvironmentVariable("ACTIVITY_NAME") ?? ActivityName;
                var version = Environment.GetEnvironmentVariable("VERSION") ?? Version;
                ActivitySource = new ActivitySource(activityName, version);
            }

            if (bool.TryParse(Environment.GetEnvironmentVariable("METER_ENABLED") ?? "true", out var meterEnabled) && meterEnabled)
            {
                var meterName = Environment.GetEnvironmentVariable("METER_NAME") ?? MeterName;
                Meter = new Meter(meterName);
            }
        }

        public static bool HasListeners() => ActivitySource?.HasListeners() ?? false;

        public static Activity? StartActivity(string name, ActivityKind kind) => ActivitySource?.StartActivity(kind, name: name);

        public static Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
            => ActivitySource?.StartActivity(kind, name: name, parentContext: parentContext);

        public static Counter<T>? CreateCounter<T>(string name, string? unit = null, string? description = null)
            where T : struct
            => Meter?.CreateCounter<T>(name, unit, description);

        public static UpDownCounter<T>? CreateUpDownCounter<T>(string name, string? unit = null, string? description = null)
            where T : struct
            => Meter?.CreateUpDownCounter<T>(name, unit, description);

        public static Histogram<T>? CreateHistogram<T>(string name, string? unit = null, string? description = null)
            where T : struct
            => Meter?.CreateHistogram<T>(name, unit, description);

        public static ObservableGauge<T>? CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
            where T : struct
            => Meter?.CreateObservableGauge(name, observeValue, unit, description);
    }
}