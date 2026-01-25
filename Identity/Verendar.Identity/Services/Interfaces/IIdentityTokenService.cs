using Verendar.Identity.Dtos;

namespace Verendar.Identity.Services.Interfaces
{
    public interface IIdentityTokenService
    {
        public TokenResponse GenerateTokens(TokenClaims claims);
        public string GenerateRefreshToken();
    }
}
