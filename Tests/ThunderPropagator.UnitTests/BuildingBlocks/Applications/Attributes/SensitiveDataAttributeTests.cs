using System.Reflection;
using ThunderPropagator.BuildingBlocks.Application.Attributes;
using ThunderPropagator.BuildingBlocks.Application.Identity;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Attributes
{
    public
#if !DEBUG
        sealed
#endif
        class SensitiveDataAttributeTests
    {
        [Fact]
        public void SensitiveDataAttribute_ValidTargets_IncludePropertyFieldAndParameter()
        {
            var usage = typeof(SensitiveDataAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            Assert.NotNull(usage);
            Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Property));
            Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Field));
            Assert.True(usage.ValidOn.HasFlag(AttributeTargets.Parameter));
        }

        [Fact]
        public void SensitiveDataAttribute_ValidTargets_DoNotIncludeClassOrMethod()
        {
            var usage = typeof(SensitiveDataAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            Assert.NotNull(usage);
            Assert.False(usage.ValidOn.HasFlag(AttributeTargets.Class));
            Assert.False(usage.ValidOn.HasFlag(AttributeTargets.Method));
        }

        // --- Enforcement: verify [SensitiveData] is applied to known credential properties ---

        [Fact]
        public void JwtConfiguration_IssuerSigningKey_HasSensitiveDataAttribute()
        {
            var property = typeof(JwtConfiguration).GetProperty(
                nameof(JwtConfiguration.IssuerSigningKey),
                BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(property?.GetCustomAttribute<SensitiveDataAttribute>());
        }

        [Fact]
        public void JwtConfiguration_NonSecretProperties_DoNotHaveSensitiveDataAttribute()
        {
            // ValidAudience and ValidIssuer are not key material — they must not carry the attribute.
            var audience = typeof(JwtConfiguration).GetProperty(
                nameof(JwtConfiguration.ValidAudience), BindingFlags.Public | BindingFlags.Instance);
            var issuer = typeof(JwtConfiguration).GetProperty(
                nameof(JwtConfiguration.ValidIssuer), BindingFlags.Public | BindingFlags.Instance);

            Assert.Null(audience?.GetCustomAttribute<SensitiveDataAttribute>());
            Assert.Null(issuer?.GetCustomAttribute<SensitiveDataAttribute>());
        }

        [Fact]
        public void BasicUserConfiguration_Password_HasSensitiveDataAttribute()
        {
            var property = typeof(BasicUserConfiguration).GetProperty(
                nameof(BasicUserConfiguration.Password),
                BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(property?.GetCustomAttribute<SensitiveDataAttribute>());
        }

        [Fact]
        public void BasicUserConfiguration_Username_DoesNotHaveSensitiveDataAttribute()
        {
            var property = typeof(BasicUserConfiguration).GetProperty(
                nameof(BasicUserConfiguration.Username),
                BindingFlags.Public | BindingFlags.Instance);

            Assert.Null(property?.GetCustomAttribute<SensitiveDataAttribute>());
        }
    }
}
