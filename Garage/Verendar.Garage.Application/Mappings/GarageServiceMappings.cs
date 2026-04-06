using Verendar.Garage.Application.Dtos;
using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Application.Mappings;

public static class GarageServiceMappings
{
    public static GarageService ToEntity(this CreateGarageServiceRequest request, Guid branchId) =>
        new()
        {
            GarageBranchId = branchId,
            Name = request.Name,
            Description = request.Description,
            LaborPrice = new Money { Amount = request.LaborPrice.Amount, Currency = request.LaborPrice.Currency },
            ServiceCategoryId = request.ServiceCategoryId,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            ImageUrl = request.ImageUrl
        };

    public static void UpdateFromRequest(this GarageService entity, UpdateGarageServiceRequest request)
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.LaborPrice = new Money { Amount = request.LaborPrice.Amount, Currency = request.LaborPrice.Currency };
        entity.ServiceCategoryId = request.ServiceCategoryId;
        entity.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        entity.ImageUrl = request.ImageUrl;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public static GarageServiceListItemResponse ToListItemResponse(this GarageService entity) =>
        new()
        {
            Id = entity.Id,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            LaborPrice = new MoneyDto { Amount = entity.LaborPrice.Amount, Currency = entity.LaborPrice.Currency },
            ServiceCategoryId = entity.ServiceCategoryId,
            ServiceCategoryName = entity.ServiceCategory?.Name,
            EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
            ImageUrl = entity.ImageUrl,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };

    public static GarageServiceResponse ToResponse(this GarageService entity) =>
        new()
        {
            Id = entity.Id,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            LaborPrice = new MoneyDto { Amount = entity.LaborPrice.Amount, Currency = entity.LaborPrice.Currency },
            ServiceCategoryId = entity.ServiceCategoryId,
            ServiceCategoryName = entity.ServiceCategory?.Name,
            EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
            ImageUrl = entity.ImageUrl,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
}
