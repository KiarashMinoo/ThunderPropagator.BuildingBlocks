using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ThunderPropagator.BuildingBlocks.Application;
using ThunderPropagator.BuildingBlocks.Application.Helpers;
using ThunderPropagator.BuildingBlocks.Application.Identity;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Helpers;

public class JwtIdentityHelperTests
{
    private const string SigningKey = "test-signing-key-that-is-at-least-256-bits-long-for-hmac!!";
    private const string Audience = "test-audience";
    private const string Issuer = "test-issuer";

    private static readonly TestJwtConfig Config = new()
    {
        IssuerSigningKey = SigningKey,
        ValidAudience = Audience,
        ValidIssuer = Issuer,
        ValidateLifetime = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true
    };

    // Flags are intentionally not set — relies on the secure defaults (all true).
    private static readonly TestJwtConfig DefaultConfig = new()
    {
        IssuerSigningKey = SigningKey,
        ValidAudience = Audience,
        ValidIssuer = Issuer
    };

    [Fact]
    public void GetPrincipalFromToken_ValidToken_ReturnsSuccessWithPrincipal()
    {
        var token = GenerateToken(Config, TimeSpan.FromMinutes(5));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, Config);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void GetPrincipalFromToken_TamperedSignature_ReturnsFailure()
    {
        var token = GenerateToken(Config, TimeSpan.FromMinutes(5));
        var tampered = token[..^4] + "XXXX";

        var result = JwtIdentityHelper.GetPrincipalFromToken(tampered, Config);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void GetPrincipalFromToken_ExpiredToken_ReturnsFailure()
    {
        // Use -10 minutes to exceed the default 5-minute ClockSkew in TokenValidationParameters.
        var token = GenerateToken(Config, TimeSpan.FromMinutes(-10));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, Config);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void GetPrincipalFromToken_WrongSigningKey_ReturnsFailure()
    {
        var otherConfig = new TestJwtConfig
        {
            IssuerSigningKey = "completely-different-key-that-is-long-enough-for-hmac!!",
            ValidAudience = Audience,
            ValidIssuer = Issuer,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true
        };
        var token = GenerateToken(otherConfig, TimeSpan.FromMinutes(5));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, Config);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void GetPrincipalFromToken_RandomString_ReturnsFailure()
    {
        var result = JwtIdentityHelper.GetPrincipalFromToken("not.a.jwt.at.all", Config);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void GetPrincipalFromToken_EmptyToken_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => JwtIdentityHelper.GetPrincipalFromToken("", Config));
    }

    [Fact]
    public void GetPrincipalFromToken_NullConfig_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JwtIdentityHelper.GetPrincipalFromToken("any.token.here", null!));
    }

    [Fact]
    public void IsTokenValid_ValidToken_ReturnsTrueAndSetsOutPrincipal()
    {
        var token = GenerateToken(Config, TimeSpan.FromMinutes(5));

        var isValid = JwtIdentityHelper.IsTokenValid(token, Config, out var principal);

        Assert.True(isValid);
        Assert.NotNull(principal);
    }

    [Fact]
    public void IsTokenValid_InvalidToken_ReturnsFalseAndSetsNullPrincipal()
    {
        var isValid = JwtIdentityHelper.IsTokenValid("invalid.token.value", Config, out var principal);

        Assert.False(isValid);
        Assert.Null(principal);
    }

    [Fact]
    public void Result_Success_ValueIsAccessible()
    {
        var result = Result<string>.Success("hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Result_Failure_ErrorIsSet()
    {
        var result = Result<string>.Failure("something went wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal("something went wrong", result.Error);
    }

    [Fact]
    public void Result_Value_OnFailure_ThrowsInvalidOperationException()
    {
        var result = Result<string>.Failure("error");

        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    // --- Default-flag security tests (acceptance criteria for issue #130) ---

    [Fact]
    public void DefaultConfig_ValidToken_ReturnsSuccess()
    {
        var token = GenerateToken(DefaultConfig, TimeSpan.FromMinutes(5));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, DefaultConfig);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public void DefaultConfig_ExpiredToken_ReturnsFailure()
    {
        var token = GenerateToken(DefaultConfig, TimeSpan.FromMinutes(-10));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, DefaultConfig);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void DefaultConfig_TamperedSignature_ReturnsFailure()
    {
        var token = GenerateToken(DefaultConfig, TimeSpan.FromMinutes(5));
        var tampered = token[..^4] + "XXXX";

        var result = JwtIdentityHelper.GetPrincipalFromToken(tampered, DefaultConfig);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void DefaultConfig_WrongSigningKey_ReturnsFailure()
    {
        var otherConfig = new TestJwtConfig
        {
            IssuerSigningKey = "completely-different-key-that-is-long-enough-for-hmac!!",
            ValidAudience = Audience,
            ValidIssuer = Issuer
        };
        var token = GenerateToken(otherConfig, TimeSpan.FromMinutes(5));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, DefaultConfig);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void DefaultConfig_WrongIssuer_ReturnsFailure()
    {
        var token = GenerateTokenWithOverrides(DefaultConfig, TimeSpan.FromMinutes(5), issuer: "wrong-issuer");

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, DefaultConfig);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void DefaultConfig_WrongAudience_ReturnsFailure()
    {
        var token = GenerateTokenWithOverrides(DefaultConfig, TimeSpan.FromMinutes(5), audience: "wrong-audience");

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, DefaultConfig);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    private static string GenerateToken(JwtConfiguration config, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.IssuerSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config.ValidIssuer,
            audience: config.ValidAudience,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // --- Issue #136 security tests: telemetry tag safety ---

    [Fact]
    public void GetPrincipalFromToken_FailureResult_DoesNotContainSigningKeyValue()
    {
        // The error message surfaced to callers on validation failure must never
        // expose the raw signing key.  If the JWT library ever changes to include
        // key material in exception messages this test will catch the regression.
        var expiredToken = GenerateToken(Config, TimeSpan.FromMinutes(-10));

        var result = JwtIdentityHelper.GetPrincipalFromToken(expiredToken, Config);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.DoesNotContain(SigningKey, result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetPrincipalFromToken_WrongKeyFailure_DoesNotContainSigningKeyValue()
    {
        var otherConfig = new TestJwtConfig
        {
            IssuerSigningKey = "completely-different-key-that-is-long-enough-for-hmac!!",
            ValidAudience = Audience,
            ValidIssuer = Issuer,
            ValidateIssuerSigningKey = true
        };
        var token = GenerateToken(otherConfig, TimeSpan.FromMinutes(5));

        var result = JwtIdentityHelper.GetPrincipalFromToken(token, Config);

        Assert.False(result.IsSuccess);
        Assert.DoesNotContain(SigningKey, result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateTokenWithOverrides(JwtConfiguration config, TimeSpan lifetime, string? issuer = null, string? audience = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.IssuerSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer ?? config.ValidIssuer,
            audience: audience ?? config.ValidAudience,
            expires: DateTime.UtcNow.Add(lifetime),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class TestJwtConfig : JwtConfiguration
    {
    }
}
