using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Verender.Common.Jwt
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid UserId
        {
            get
            {
                var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdString, out var userId))
                {
                    return userId;
                }
                return Guid.Empty;
            }
        }
        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        public string? Claim(string claimType) => _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);
    }
}
