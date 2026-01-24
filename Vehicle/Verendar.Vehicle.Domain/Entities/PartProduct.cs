using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// <summary>
    /// Sản phẩm phụ tùng cụ thể (Castrol 10W-40, Michelin Pilot Street...)
    /// </summary>
    public class PartProduct : BaseEntity
    {
        [Required]
        public Guid PartCategoryId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Giá tham khảo (không phải giá thực tế), mặc định VND
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReferencePrice { get; set; }

        /// <summary>
        /// Chu kỳ khuyến nghị theo km của SẢN PHẨM này
        /// Ví dụ: Castrol thường 1500km, Motul cao cấp 2000km
        /// </summary>
        public int? RecommendedKmInterval { get; set; }

        /// <summary>
        /// Chu kỳ khuyến nghị theo tháng của SẢN PHẨM này
        /// </summary>
        public int? RecommendedMonthsInterval { get; set; }

        public EntityStatus Status { get; set; } = EntityStatus.Active;

        // Navigation properties
        public PartCategory Category { get; set; } = null!;

        public List<VehiclePartTracking> PartTrackings { get; set; } = [];
        public List<MaintenanceRecordItem> MaintenanceItems { get; set; } = [];
    }
}
