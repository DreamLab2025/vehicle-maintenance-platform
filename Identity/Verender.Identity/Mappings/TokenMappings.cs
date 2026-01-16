using Verender.Identity.Dtos;
using Verender.Identity.Entities;

namespace Verender.Identity.Mappings
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
