using Newtonsoft.Json;
using ThunderPropagator.BuildingBlocks.Application.Objects;
using System.Text.Json.Serialization;

namespace ThunderPropagator.BuildingBlocks.Application.Identity
{
    public abstract class BasicUserConfiguration : EquatableObject<BasicUserConfiguration>
    {
        [JsonProperty, JsonInclude] public string Username { get; protected set; } = null!;

        [JsonProperty, JsonInclude] public string Password { get; protected set; } = null!;

        [JsonProperty, JsonInclude] public string[]? Roles { get; protected set; }

        public override int GetHashCode() => Username.GetHashCode();
    }
}