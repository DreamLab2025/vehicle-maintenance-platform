using Verendar.Garage.Application.Dtos;

namespace Verendar.Garage.Application.Mappings;

public static class GarageBundleMappings
{
    public static GarageBundle ToEntity(this CreateGarageBundleRequest request, Guid branchId) =>
        new()
        {
            GarageBranchId = branchId,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            DiscountAmount = request.DiscountAmount.HasValue
                ? new Money { Amount = request.DiscountAmount.Value, Currency = "VND" }
                : null,
            DiscountPercent = request.DiscountPercent,
            Items = request.Items.Select((i, idx) => i.ToItemEntity(idx)).ToList()
        };

    public static void UpdateFromRequest(this GarageBundle entity, UpdateGarageBundleRequest request)
    {
        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.ImageUrl = request.ImageUrl;
        entity.DiscountAmount = request.DiscountAmount.HasValue
            ? new Money { Amount = request.DiscountAmount.Value, Currency = "VND" }
            : null;
        entity.DiscountPercent = request.DiscountPercent;
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public static GarageBundleItem ToItemEntity(this BundleItemRequest request, int fallbackSortOrder = 0) =>
        new()
        {
            ProductId = request.ProductId,
            ServiceId = request.ServiceId,
            IncludeInstallation = request.IncludeInstallation,
            SortOrder = request.SortOrder > 0 ? request.SortOrder : fallbackSortOrder
        };

    public static GarageBundleListItemResponse ToListItemResponse(this GarageBundle entity)
    {
        var subTotal = CalculateSubTotal(entity);
        var finalPrice = CalculateFinalPrice(entity, subTotal);

        return new GarageBundleListItemResponse
        {
            Id = entity.Id,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            DiscountAmount = entity.DiscountAmount?.Amount,
            DiscountPercent = entity.DiscountPercent,
            SubTotal = subTotal,
            FinalPrice = finalPrice,
            Currency = "VND",
            ItemCount = entity.Items.Count,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt
        };
    }

    public static GarageBundleResponse ToResponse(this GarageBundle entity)
    {
        var subTotal = CalculateSubTotal(entity);
        var finalPrice = CalculateFinalPrice(entity, subTotal);

        return new GarageBundleResponse
        {
            Id = entity.Id,
            GarageBranchId = entity.GarageBranchId,
            Name = entity.Name,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            DiscountAmount = entity.DiscountAmount?.Amount,
            DiscountPercent = entity.DiscountPercent,
            SubTotal = subTotal,
            FinalPrice = finalPrice,
            Currency = "VND",
            Status = entity.Status,
            Items = entity.Items.Select(i => i.ToItemResponse()).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private static BundleItemResponse ToItemResponse(this GarageBundleItem item)
    {
        MoneyDto? materialPrice = null;
        MoneyDto? laborPrice = null;

        if (item.Product is not null)
        {
            materialPrice = new MoneyDto { Amount = item.Product.MaterialPrice.Amount, Currency = item.Product.MaterialPrice.Currency };
            if (item.IncludeInstallation && item.Product.InstallationService is not null)
                laborPrice = new MoneyDto { Amount = item.Product.InstallationService.LaborPrice.Amount, Currency = item.Product.InstallationService.LaborPrice.Currency };
        }
        else if (item.Service is not null)
        {
            laborPrice = new MoneyDto { Amount = item.Service.LaborPrice.Amount, Currency = item.Service.LaborPrice.Currency };
        }

        return new BundleItemResponse
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ServiceId = item.ServiceId,
            IncludeInstallation = item.IncludeInstallation,
            SortOrder = item.SortOrder,
            ItemName = item.Product?.Name ?? item.Service?.Name ?? string.Empty,
            MaterialPrice = materialPrice,
            LaborPrice = laborPrice
        };
    }

    internal static decimal CalculateSubTotal(GarageBundle entity)
    {
        decimal total = 0;
        foreach (var item in entity.Items)
        {
            if (item.Product is not null)
            {
                total += item.Product.MaterialPrice.Amount;
                if (item.IncludeInstallation && item.Product.InstallationService is not null)
                    total += item.Product.InstallationService.LaborPrice.Amount;
            }
            else if (item.Service is not null)
            {
                total += item.Service.LaborPrice.Amount;
            }
        }
        return total;
    }

    internal static decimal CalculateFinalPrice(GarageBundle entity, decimal subTotal)
    {
        if (entity.DiscountAmount is not null)
            return Math.Max(0, subTotal - entity.DiscountAmount.Amount);

        if (entity.DiscountPercent.HasValue)
            return Math.Max(0, subTotal * (1 - entity.DiscountPercent.Value / 100));

        return subTotal;
    }
}
