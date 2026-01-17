using Verendar.Identity.Dtos;
using Verendar.Identity.Entities;

namespace Verendar.Identity.Mappings
{
    public static class TokenMappings
    {
        public static TokenClaims ToTokenClaims(this User user)
        {
            return new TokenClaims
            {
                UserId = user.Id.ToString(),
                PhoneNumber = user.PhoneNumber,
                UserName = user.FullName,
                Roles = user.Roles.Select(r => r.ToString()).ToList()
            };
        }
    }
}
