namespace Verendar.Garage.Application.Dtos;

// ── Requests ──────────────────────────────────────────────────────────────────

public class GarageServiceQueryRequest : PaginationRequest
{
    public Guid BranchId { get; set; }
    public bool ActiveOnly { get; set; }
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? CategoryId { get; set; }
}

public class CreateGarageServiceRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto LaborPrice { get; set; } = null!;
    public Guid? ServiceCategoryId { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateGarageServiceRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto LaborPrice { get; set; } = null!;
    public Guid? ServiceCategoryId { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateGarageServiceStatusRequest
{
    public ProductStatus Status { get; set; }
}

// ── Response ──────────────────────────────────────────────────────────────────

public class GarageServiceListItemResponse
{
    public Guid Id { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto LaborPrice { get; set; } = null!;
    public Guid? ServiceCategoryId { get; set; }
    public string? ServiceCategoryName { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GarageServiceResponse
{
    public Guid Id { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MoneyDto LaborPrice { get; set; } = null!;
    public Guid? ServiceCategoryId { get; set; }
    public string? ServiceCategoryName { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
