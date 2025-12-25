using VMP.Identity.Dtos;

namespace VMP.Identity.Services
{
    public interface IIdentityTokenService
    {
        public TokenResponse GenerateTokens(TokenClaims claims);
        public string GenerateRefreshToken();
    }
}
