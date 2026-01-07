using VMP.Identity.Dtos;

namespace VMP.Identity.Services.Interfaces
{
    public interface IIdentityTokenService
    {
        public TokenResponse GenerateTokens(TokenClaims claims);
        public string GenerateRefreshToken();
    }
}
