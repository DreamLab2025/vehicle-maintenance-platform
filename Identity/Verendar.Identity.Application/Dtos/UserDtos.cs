namespace Verendar.Identity.Application.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public bool PhoneNumberVerified { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    public record CreateMechanicRequest(
        string FullName,
        string Email,
        string? PhoneNumber
    );

    public record CreateMechanicResponse(Guid UserId);

    public record GarageContactResponse(string FullName, string Email, string PhoneNumber);
}
