using Microsoft.IdentityModel.Tokens;
using RapidStreamer.BuildingBlocks.Application.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class JwtIdentityHelper
    {
        public static ClaimsPrincipal? GetPrincipalFromToken(string token, JwtConfiguration jwtConfiguration)
        {
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

            ClaimsPrincipal? claimsPrincipal = null;

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                // ignored
            }

            return claimsPrincipal;
        }

        public static bool IsTokenValid(string token, JwtConfiguration jwtConfiguration, out ClaimsPrincipal? claimsPrincipal)
            => (claimsPrincipal = GetPrincipalFromToken(token, jwtConfiguration)) is not null;
    }
}