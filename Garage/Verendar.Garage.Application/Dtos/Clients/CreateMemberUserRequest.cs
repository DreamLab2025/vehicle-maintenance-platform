namespace Verendar.Garage.Application.Dtos.Clients;

public record CreateMemberUserRequest(
    string FullName,
    string Email,
    string? PhoneNumber
);
