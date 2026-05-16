using System.Collections.Concurrent;
using System.Text;

namespace ThunderPropagator.BuildingBlocks.Application.CorrelationId
{
    public static class CorrelationIdProvider
    {
        private static readonly ConcurrentDictionary<Type, string> _typeSegmentCache = new();

        private static string GetTypeSegment(Type type)
        {
            return _typeSegmentCache.GetOrAdd(type, static t =>
            {
                var bytes = Encoding.UTF32.GetBytes(t.Name);
                return Convert.ToBase64String(bytes)[..^2];
            });
        }

        public static string GenerateCorrelationId<T>(this T input)
            where T : notnull
        {
            const string activityName = $"{nameof(CorrelationIdProvider)}_{nameof(GenerateCorrelationId)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var unixNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var hashCode = input switch
            {
                FeederMessage feederMessage => feederMessage.HashKey switch
                {
                    > 0 => feederMessage.HashKey.Value,
                    _ => input.GetHashCode()
                },
                _ => input.GetHashCode()
            };

            var typeSegment = GetTypeSegment(typeof(T));

            // Stack-allocate a buffer large enough for long (19) + '-' + int (11) + '-'
            Span<char> prefix = stackalloc char[32];
            unixNow.TryFormat(prefix, out var unixLen);
            prefix[unixLen] = '-';
            hashCode.TryFormat(prefix[(unixLen + 1)..], out var hashLen);
            prefix[unixLen + 1 + hashLen] = '-';
            var prefixLen = unixLen + 1 + hashLen + 1;

            return string.Concat(prefix[..prefixLen], typeSegment);
        }
    }
}
