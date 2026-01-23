using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Domain.Entities
{
    /// Thông tin chi tiết cho nhớt động cơ (gắn 1-1 với VehiclePart thuộc category OIL)
    [Table("Oils")]
    [Index(nameof(VehiclePartId), IsUnique = true)]
    public class Oil : BaseEntity
    {
        /// Liên kết tới VehiclePart (Id của phụ tùng chung)
        [Required]
        public Guid VehiclePartId { get; set; }

        [ForeignKey(nameof(VehiclePartId))]
        public VehiclePart VehiclePart { get; set; } = null!;

        /// Cấp độ nhớt: ví dụ 5W-30, 10W-40
        [Required, MaxLength(20)]
        public string ViscosityGrade { get; set; } = null!;

        /// Chuẩn API: API SN, SM, …
        [MaxLength(20)]
        public string? ApiServiceClass { get; set; }

        /// Chuẩn JASO: MA, MA2 (thường cho xe số), MB (thường cho xe ga)
        [MaxLength(10)]
        public string? JasoRating { get; set; }

        /// Loại dầu: Khoáng / Bán tổng hợp / Tổng hợp
        [MaxLength(50)]
        public string? BaseOilType { get; set; }

        /// Dung tích khuyến nghị cho mỗi lần thay (lít)
        [Column(TypeName = "decimal(6,2)")]
        public decimal? RecommendedVolumeLiters { get; set; }

        /// Nhớt dành cho loại xe nào
        public OilVehicleUsage VehicleUsage { get; set; } = OilVehicleUsage.Both;

        /// Chu kỳ thay nhớt khuyến nghị cho xe ga (km)
        public int? RecommendedIntervalKmScooter { get; set; }

        /// Chu kỳ thay nhớt khuyến nghị cho xe số (km)
        public int? RecommendedIntervalKmManual { get; set; }

        /// Chu kỳ thay nhớt khuyến nghị theo thời gian (tháng)
        public int? RecommendedIntervalMonths { get; set; }
    }

    /// Phạm vi sử dụng của nhớt theo loại xe
    public enum OilVehicleUsage
    {
        /// Chỉ cho xe ga (CVT)
        Scooter = 1,

        /// Chỉ cho xe số (hộp số côn / hộp số thường)
        Manual = 2,

        /// Dùng được cho cả xe ga và xe số
        Both = 3
    }
}
