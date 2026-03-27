using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Application.Mappings;

public static class GarageProductMappings
{
    public static GarageProduct ToEntity(this CreateGarageProductRequest request, Guid branchId) =>
        new()
        {
            GarageBranchId = branchId,
            Name = request.Name,
            Description = request.Description,
            MaterialPrice = new Money { Amount = request.MaterialPrice.Amount, Currency = request.MaterialPrice.Currency },
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            ImageUrl = request.ImageUrl,
            CompatibleVehicleTypes = request.CompatibleVehicleTypes,
            ManufacturerKmInterval = request.ManufacturerKmInterval,
            ManufacturerMonthInterval = request.ManufacturerMonthInterval,
            PartCategoryId = request.PartCategoryId,
            InstallationServiceId = request.InstallationServiceId
        };

    public static void UpdateFromRequest(this GarageProduct entity, UpdateGarageProductRequest request)
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.MaterialPrice = new Money { Amount = request.MaterialPrice.Amount, Currency = request.MaterialPrice.Currency };
        entity.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        entity.ImageUrl = request.ImageUrl;
        entity.CompatibleVehicleTypes = request.CompatibleVehicleTypes;
        entity.ManufacturerKmInterval = request.ManufacturerKmInterval;
        entity.ManufacturerMonthInterval = request.ManufacturerMonthInterval;
        entity.PartCategoryId = request.PartCategoryId;
        entity.InstallationServiceId = request.InstallationServiceId;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public static GarageProductListItemResponse ToListItemResponse(this GarageProduct entity) =>
        new()
        {
            Id = entity.Id,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            MaterialPrice = new MoneyDto { Amount = entity.MaterialPrice.Amount, Currency = entity.MaterialPrice.Currency },
            EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
            ImageUrl = entity.ImageUrl,
            PartCategoryId = entity.PartCategoryId,
            HasInstallationOption = entity.InstallationServiceId.HasValue,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };

    public static GarageProductResponse ToResponse(this GarageProduct entity) =>
        new()
        {
            Id = entity.Id,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            MaterialPrice = new MoneyDto { Amount = entity.MaterialPrice.Amount, Currency = entity.MaterialPrice.Currency },
            EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
            ImageUrl = entity.ImageUrl,
            CompatibleVehicleTypes = entity.CompatibleVehicleTypes,
            ManufacturerKmInterval = entity.ManufacturerKmInterval,
            ManufacturerMonthInterval = entity.ManufacturerMonthInterval,
            PartCategoryId = entity.PartCategoryId,
            Status = entity.Status,
            InstallationService = entity.InstallationService is null ? null : new InstallationServiceSummary
            {
                Id = entity.InstallationService.Id,
                Name = entity.InstallationService.Name,
                LaborPrice = new MoneyDto { Amount = entity.InstallationService.LaborPrice.Amount, Currency = entity.InstallationService.LaborPrice.Currency },
                EstimatedDurationMinutes = entity.InstallationService.EstimatedDurationMinutes
            },
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
}
