using System.Reflection;
using System.Text;

namespace RapidStreamer.BuildingBlocks.Application.CorrelationId
{
    public static class CorrelationIdProvider
    {
        public static string GenerateCorrelationId<T>(this T input)
            where T : notnull
        {
#if DEBUG
            const string activityName = $"{nameof(CorrelationIdProvider)}_{nameof(GenerateCorrelationId)}";
            using var activity = Telemetry.StartActivity(activityName, ActivityKind.Internal);
#endif

            StringBuilder stringBuilder = new();

            var unixNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            stringBuilder.Append($"{unixNow}-");

            var hashCode = input switch
            {
                FeederMessage feederMessage => feederMessage.HashKey switch
                {
                    not null or 0 => feederMessage.HashKey.Value,
                    _ => input.GetHashCode()
                },
                _ => input.GetHashCode()
            };

            stringBuilder.Append($"{hashCode}-");

            var type = typeof(T);
            var typeName = type.GetTypeInfo().Name;
            var typeNameBytes = Encoding.UTF32.GetBytes(typeName);
            var typeNameBase64 = Convert.ToBase64String(typeNameBytes)[..^2];

            stringBuilder.Append(typeNameBase64);

            return stringBuilder.ToString();
        }
    }
}