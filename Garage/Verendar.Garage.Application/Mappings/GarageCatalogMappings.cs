using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class GarageCatalogMappings
{
    public static CatalogItemResponse ToCatalogItem(this GarageService entity) =>
        new()
        {
            Id = entity.Id,
            Type = CatalogItemType.Service,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Price = new MoneyDto { Amount = entity.LaborPrice.Amount, Currency = entity.LaborPrice.Currency },
            EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ServiceCategoryId = entity.ServiceCategoryId,
            ServiceCategoryName = entity.ServiceCategory?.Name
        };

    public static CatalogItemResponse ToCatalogItem(this GarageProduct entity) =>
        new()
        {
            Id = entity.Id,
            Type = CatalogItemType.Product,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Price = new MoneyDto { Amount = entity.MaterialPrice.Amount, Currency = entity.MaterialPrice.Currency },
            EstimatedDurationMinutes = entity.EstimatedDurationMinutes,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            PartCategoryId = entity.PartCategoryId,
            HasInstallationOption = entity.InstallationServiceId.HasValue
        };

    public static CatalogItemResponse ToCatalogItem(this GarageBundle entity)
    {
        var subTotal = GarageBundleMappings.CalculateSubTotal(entity);
        var finalPrice = GarageBundleMappings.CalculateFinalPrice(entity, subTotal);

        return new()
        {
            Id = entity.Id,
            Type = CatalogItemType.Bundle,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            Price = new MoneyDto { Amount = finalPrice, Currency = "VND" },
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ItemCount = entity.Items.Count,
            SubTotal = subTotal,
            FinalPrice = finalPrice,
            DiscountAmount = entity.DiscountAmount?.Amount,
            DiscountPercent = entity.DiscountPercent,
            Currency = "VND"
        };
    }
}
