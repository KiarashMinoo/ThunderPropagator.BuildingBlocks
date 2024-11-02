#if DEBUG
using System.Diagnostics;
#endif
using System.Diagnostics.Metrics;

namespace RapidStreamer.BuildingBlocks.Application
{
    public static class Telemetry
    {
        public const string MeterName = "rapidStreamer.meter";
        public const string ActivityName = "rapidStreamer.activity";

#if DEBUG
        private static readonly ActivitySource ActivitySource;
#endif
        private static readonly Meter Meter;

        public static string Version { get; set; } = "1";

        public static KeyValuePair<string, object?> SuccessfulTag => new("Status", "Success");
        public static KeyValuePair<string, object?> UnsuccessfulTag => new("Status", "Failed");


        static Telemetry()
        {
#if DEBUG
            ActivitySource = new ActivitySource(ActivityName, Version);
#endif
            Meter = new Meter(MeterName);
        }

#if DEBUG
        public static Activity? StartActivity(string name, ActivityKind kind) => ActivitySource.StartActivity(kind, name: name);
        public static Activity? StartActivity(string name, ActivityKind kind, ActivityContext parentContext)
            => ActivitySource.StartActivity(kind, name: name, parentContext: parentContext);
#endif

        public static Counter<T> CreateCounter<T>(string name, string? unit = null, string? description = null)
            where T : struct
            => Meter.CreateCounter<T>(name, unit, description);

        public static UpDownCounter<T> CreateUpDownCounter<T>(string name, string? unit = null, string? description = null)
            where T : struct
            => Meter.CreateUpDownCounter<T>(name, unit, description);

        public static Histogram<T> CreateHistogram<T>(string name, string? unit = null, string? description = null)
            where T : struct
            => Meter.CreateHistogram<T>(name, unit, description);

        public static ObservableGauge<T> CreateObservableGauge<T>(string name, Func<T> observeValue, string? unit = null, string? description = null)
            where T : struct
            => Meter.CreateObservableGauge(name, observeValue, unit, description);
    }
}