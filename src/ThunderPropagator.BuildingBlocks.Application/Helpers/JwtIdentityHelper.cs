using Microsoft.IdentityModel.Tokens;
using ThunderPropagator.BuildingBlocks.Application.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class JwtIdentityHelper
    {
        /// <summary>
        /// Validates <paramref name="token"/> against <paramref name="jwtConfiguration"/> and
        /// returns a <see cref="Result{ClaimsPrincipal}"/> that carries the principal on success or
        /// the validation error message on failure.
        /// </summary>
        /// <remarks>
        /// Expected token-validation failures (<see cref="SecurityTokenException"/> and its
        /// subclasses) are captured in a failure result so callers are forced to inspect
        /// <see cref="Result{T}.IsSuccess"/>. All other exceptions propagate to the caller.
        /// The OpenTelemetry activity emitted by this method records only the outcome status and
        /// the exception type — never the token value, signing key, or any credential material.
        /// </remarks>
        public static Result<ClaimsPrincipal> GetPrincipalFromToken(string token, JwtConfiguration jwtConfiguration)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(token);
            ArgumentNullException.ThrowIfNull(jwtConfiguration);

            const string activityName = $"{nameof(JwtIdentityHelper)}_{nameof(GetPrincipalFromToken)}";
            using var activity = Telemetry.HasListeners() ? Telemetry.StartActivity(activityName, ActivityKind.Internal) : null;

            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.IssuerSigningKey)),
                ValidAudience = jwtConfiguration.ValidAudience,
                ValidIssuer = jwtConfiguration.ValidIssuer,
                ValidateLifetime = jwtConfiguration.ValidateLifetime,
                ValidateAudience = jwtConfiguration.ValidateAudience,
                ValidateIssuer = jwtConfiguration.ValidateIssuer,
                ValidateIssuerSigningKey = jwtConfiguration.ValidateIssuerSigningKey
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                activity?.SetTag(Telemetry.SuccessfulTag.Key, Telemetry.SuccessfulTag.Value);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Result<ClaimsPrincipal>.Success(principal);
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                // SecurityTokenException covers all standard JWT validation failures.
                // ArgumentException covers tokens that fail Base64Url decoding before
                // validation even begins (malformed/not-a-JWT input).
                // Tag only the exception type — never the message, which may reference
                // token structure detail, or any credential from jwtConfiguration.
                activity?.SetTag(Telemetry.UnsuccessfulTag.Key, Telemetry.UnsuccessfulTag.Value);
                activity?.SetTag("exception.type", ex.GetType().Name);
                activity?.SetStatus(ActivityStatusCode.Error);
                return Result<ClaimsPrincipal>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> and sets <paramref name="claimsPrincipal"/> when the
        /// token is valid; returns <see langword="false"/> and sets
        /// <paramref name="claimsPrincipal"/> to <see langword="null"/> when validation fails.
        /// </summary>
        public static bool IsTokenValid(string token, JwtConfiguration jwtConfiguration, out ClaimsPrincipal? claimsPrincipal)
        {
            var result = GetPrincipalFromToken(token, jwtConfiguration);
            claimsPrincipal = result.IsSuccess ? result.Value : null;
            return result.IsSuccess;
        }
    }
}
