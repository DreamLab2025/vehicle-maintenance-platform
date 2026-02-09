using Verendar.Identity.Application.Dtos;

namespace Verendar.Identity.Application.Services.Interfaces;

public interface IIdentityTokenService
{
    TokenResponse GenerateTokens(TokenClaims claims);
    string GenerateRefreshToken();
}
