using Verendar.Identity.Application.Dtos;
using Verendar.Identity.Domain.Entities;

namespace Verendar.Identity.Application.Mappings
{
    public static class TokenMappings
    {
        public static TokenClaims ToTokenClaims(this User user)
        {
            return new TokenClaims
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
                UserName = user.FullName,
                Roles = user.Roles.Select(r => r.ToString()).ToList()
            };
        }
    }
}
