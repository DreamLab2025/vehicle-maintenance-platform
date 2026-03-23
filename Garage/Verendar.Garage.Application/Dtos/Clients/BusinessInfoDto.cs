namespace Verendar.Garage.Application.Dtos.Clients;

public sealed record BusinessInfoDto(
    string TaxCode,
    string Name,
    string? InternationalName,
    string? ShortName,
    string? Address,
    string? Status);
