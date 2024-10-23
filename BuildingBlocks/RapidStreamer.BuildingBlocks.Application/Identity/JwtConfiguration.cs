using Newtonsoft.Json;
using RapidStreamer.BuildingBlocks.Application.Objects;
using System.Text.Json.Serialization;

namespace RapidStreamer.BuildingBlocks.Application.Identity
{
    public abstract class JwtConfiguration : EquatableObject<JwtConfiguration>
    {
        [JsonProperty, JsonInclude] public string IssuerSigningKey { get; set; } = null!;

        [JsonProperty, JsonInclude] public string ValidAudience { get; set; } = null!;

        [JsonProperty, JsonInclude] public string ValidIssuer { get; set; } = null!;

        [JsonProperty, JsonInclude] public bool ValidateLifetime { get; set; }

        [JsonProperty, JsonInclude] public bool ValidateAudience { get; set; }

        [JsonProperty, JsonInclude] public bool ValidateIssuer { get; set; }

        [JsonProperty, JsonInclude] public bool ValidateIssuerSigningKey { get; set; }
    }
}