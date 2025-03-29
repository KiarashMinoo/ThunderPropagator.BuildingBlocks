using System.Diagnostics;

namespace RapidStreamer.BuildingBlocks.Application.CorrelationId
{
    public static class CorrelationIdSupportHelper
    {
        public static T GenerateCorrelationId<T>(this T input)
            where T : class, ICorrelationIdSupport
        {
            const string activityName = $"{nameof(CorrelationIdSupportHelper)}_{nameof(GenerateCorrelationId)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            var correlationId = CorrelationIdProvider.GenerateCorrelationId(input);

            input.CorrelationId = correlationId;

            return input;
        }

        public static T SetCorrelationId<T>(this T input, string correlationId)
            where T : class, ICorrelationIdSupport
        {
            const string activityName = $"{nameof(CorrelationIdSupportHelper)}_{nameof(GenerateCorrelationId)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);

            input.CorrelationId = correlationId;

            return input;
        }
    }
}