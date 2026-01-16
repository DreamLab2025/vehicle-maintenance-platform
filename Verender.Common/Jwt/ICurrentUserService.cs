namespace Verender.Common.Jwt
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        string? Email { get; }
        string? Claim(string claimType);
    }
}
