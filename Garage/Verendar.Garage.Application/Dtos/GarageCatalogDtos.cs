namespace Verendar.Garage.Application.Dtos;

public enum CatalogItemType
{
    Service = 0,
    Product = 1,
    Bundle = 2
}

public class CatalogQueryRequest
{
    public CatalogItemType? Type { get; set; }
    public Guid? CategoryId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public void Normalize()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 50) PageSize = 50;
    }
}

public class CatalogItemResponse
{
    public Guid Id { get; set; }
    public CatalogItemType Type { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public MoneyDto Price { get; set; } = null!;
    public int? EstimatedDurationMinutes { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // dịch vụ
    public Guid? ServiceCategoryId { get; set; }
    public string? ServiceCategoryName { get; set; }

    // sản phẩm
    public Guid? PartCategoryId { get; set; }
    public bool HasInstallationOption { get; set; }

    // combo
    public int? ItemCount { get; set; }
    public decimal? SubTotal { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public string Currency { get; set; } = "VND";
}
