using Verendar.Garage.Domain.ValueObjects;

namespace Verendar.Garage.Domain.Entities;

[Index(nameof(GarageBranchId))]
public class GarageProduct : BaseEntity
{
    public Guid GarageBranchId { get; set; }

    /// <summary>Liên kết với PartCategory trong Vehicle service để trigger PartTracking khi hoàn thành.</summary>
    public Guid? PartCategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public Money MaterialPrice { get; set; } = null!;

    public int? EstimatedDurationMinutes { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public string? CompatibleVehicleTypes { get; set; }

    public int? ManufacturerKmInterval { get; set; }

    public int? ManufacturerMonthInterval { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    /// <summary>
    /// Dịch vụ thay/lắp đặt kèm theo (optional). Khi có, user có thể chọn mua phụ tùng kèm thay hoặc chỉ mua phụ tùng.
    /// InstallationService phải thuộc cùng GarageBranchId.
    /// </summary>
    public Guid? InstallationServiceId { get; set; }

    // Navigation
    public GarageBranch GarageBranch { get; set; } = null!;
    public GarageService? InstallationService { get; set; }
}
