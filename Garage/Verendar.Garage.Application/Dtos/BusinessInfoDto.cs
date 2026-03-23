namespace Verendar.Garage.Application.Dtos;

public sealed record BusinessInfoDto(
    string TaxCode,
    string Name,
    string? InternationalName,
    string? ShortName,
    string? Address,
    string? Status);
