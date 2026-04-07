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
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    public record CreateMechanicRequest(
        string FullName,
        string Email,
        string? PhoneNumber,
        string? Password = null
    );

    public record CreateMechanicResponse(Guid UserId, string ActualPassword);

    public record CreateManagerRequest(
        string FullName,
        string Email,
        string? PhoneNumber,
        string? Password = null
    );

    public record CreateManagerResponse(Guid UserId, string ActualPassword);

    public record GarageContactResponse(string FullName, string Email, string PhoneNumber);

    public record AssignRoleRequest(string Role);

    public record BulkDeactivateRequest(IEnumerable<Guid> UserIds);

    public class UserCreateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public List<UserRole> Roles { get; set; } = [];
    }

    public class UserUpdateRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public bool EmailVerified { get; set; }
        public bool PhoneNumberVerified { get; set; }
        public string? Password { get; set; }
        public List<UserRole> Roles { get; set; } = [];
    }

    /// <summary>
    /// Search/filter params for the admin "get all users" endpoint.
    /// SortBy: "name" | "email" | "phone" | "createdAt" (default).
    /// IsDescending: true (default) | false.
    /// </summary>
    public class UserFilterRequest : PaginationRequest
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public UserRole[]? Roles { get; set; }
        public string? SortBy { get; set; }
    }
}
