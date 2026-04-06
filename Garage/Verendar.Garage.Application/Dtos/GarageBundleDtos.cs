namespace Verendar.Garage.Application.Dtos;

// ── Requests ──────────────────────────────────────────────────────────────────

public class GarageBundleQueryRequest : PaginationRequest
{
    public Guid BranchId { get; set; }
    public bool ActiveOnly { get; set; }
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

// ── Nested ────────────────────────────────────────────────────────────────────

public class BundleItemRequest
{
    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }
    public bool IncludeInstallation { get; set; }
    public int SortOrder { get; set; }
}

public class BundleItemResponse
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }
    public bool IncludeInstallation { get; set; }
    public int SortOrder { get; set; }
    public string ItemName { get; set; } = null!;
    public MoneyDto? MaterialPrice { get; set; }
    public MoneyDto? LaborPrice { get; set; }
}

// ── Requests ──────────────────────────────────────────────────────────────────

public class CreateGarageBundleRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public List<BundleItemRequest> Items { get; set; } = [];
}

public class UpdateGarageBundleRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public List<BundleItemRequest> Items { get; set; } = [];
}

public class UpdateGarageBundleStatusRequest
{
    public ProductStatus Status { get; set; }
}

// ── Response ──────────────────────────────────────────────────────────────────

public class GarageBundleListItemResponse
{
    public Guid Id { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal SubTotal { get; set; }
    public decimal FinalPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public int ItemCount { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GarageBundleResponse
{
    public Guid Id { get; set; }
    public Guid GarageBranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal SubTotal { get; set; }
    public decimal FinalPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public ProductStatus Status { get; set; }
    public List<BundleItemResponse> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
