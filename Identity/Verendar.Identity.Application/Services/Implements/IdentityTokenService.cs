using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Verendar.Common.Jwt;
using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Application.Services.Interfaces;

namespace Verendar.Identity.Application.Services.Implements
{
    public class IdentityTokenService(IOptions<JwtBearerConfigurationOptions> jwtOptionsAccessor) : IIdentityTokenService
    {
        private readonly JwtBearerConfigurationOptions _jwtOptions = jwtOptionsAccessor.Value;
        private readonly SymmetricSecurityKey _signingKey = CreateSigningKey(jwtOptionsAccessor.Value);

        private static SymmetricSecurityKey CreateSigningKey(JwtBearerConfigurationOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.SecretKey))
                throw new InvalidOperationException("JWT SecretKey cannot be null or empty.");

            var keyBytes = Encoding.UTF8.GetBytes(options.SecretKey);
            if (keyBytes.Length < 32)
                throw new InvalidOperationException("JWT SecretKey must be at least 32 UTF-8 bytes for HMAC-SHA256.");

            return new SymmetricSecurityKey(keyBytes);
        }

        public TokenResponse GenerateTokens(TokenClaims tokenClaims)
        {
            var accessToken = GenerateAccessToken(tokenClaims);
            var refreshToken = GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                TokenType = "Bearer"
            };
        }

        private string GenerateAccessToken(TokenClaims tokenClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, tokenClaims.UserId),
                new Claim(JwtRegisteredClaimNames.Email, tokenClaims.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, tokenClaims.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("userId", tokenClaims.UserId),
                new Claim("userName", tokenClaims.UserName)
            };
            foreach (var role in tokenClaims.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
