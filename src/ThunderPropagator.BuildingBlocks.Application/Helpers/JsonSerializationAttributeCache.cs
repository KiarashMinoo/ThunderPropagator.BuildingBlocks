using System.Collections.Concurrent;
using ThunderPropagator.BuildingBlocks.Application.Attributes;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    internal static class JsonSerializationAttributeCache
    {
        private static readonly ConcurrentDictionary<Type, JsonSerializationAttribute?> Cache = new();

        public static JsonSerializationAttribute? Get(Type type)
        {
            return Cache.GetOrAdd(type, t =>
                t.GetCustomAttributes(typeof(JsonSerializationAttribute), true)
                 .FirstOrDefault() as JsonSerializationAttribute);
        }
    }
}
