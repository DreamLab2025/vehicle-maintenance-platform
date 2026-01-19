using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VMP.Common.Databases.Base;

namespace VMP.Vehicle.Domain.Entities
{
    /// <summary>
    /// Thông tin chi tiết cho nhớt động cơ (gắn 1-1 với VehiclePart thuộc category OIL)
    /// </summary>
    public class Oil : BaseEntity
    {
        /// <summary>
        /// Liên kết tới VehiclePart (Id của phụ tùng chung)
        /// </summary>
        [Required]
        public Guid VehiclePartId { get; set; }

        [ForeignKey(nameof(VehiclePartId))]
        public VehiclePart VehiclePart { get; set; } = null!;

        /// <summary>
        /// Cấp độ nhớt: ví dụ 5W-30, 10W-40
        /// </summary>
        [Required, MaxLength(20)]
        public string ViscosityGrade { get; set; } = null!;

        /// <summary>
        /// Chuẩn API: API SN, SM, …
        /// </summary>
        [MaxLength(20)]
        public string? ApiServiceClass { get; set; }

        /// <summary>
        /// Chuẩn JASO: MA, MA2 (thường cho xe số), MB (thường cho xe ga)
        /// </summary>
        [MaxLength(10)]
        public string? JasoRating { get; set; }

        /// <summary>
        /// Loại dầu: Khoáng / Bán tổng hợp / Tổng hợp
        /// </summary>
        [MaxLength(50)]
        public string? BaseOilType { get; set; }

        /// <summary>
        /// Dung tích khuyến nghị cho mỗi lần thay (lít)
        /// </summary>
        [Column(TypeName = "decimal(6,2)")]
        public decimal? RecommendedVolumeLiters { get; set; }

        /// <summary>
        /// Nhớt dành cho loại xe nào
        /// </summary>
        public OilVehicleUsage VehicleUsage { get; set; } = OilVehicleUsage.Both;

        /// <summary>
        /// Chu kỳ thay nhớt khuyến nghị cho xe ga (km)
        /// </summary>
        public int? RecommendedIntervalKmScooter { get; set; }

        /// <summary>
        /// Chu kỳ thay nhớt khuyến nghị cho xe số (km)
        /// </summary>
        public int? RecommendedIntervalKmManual { get; set; }

        /// <summary>
        /// Chu kỳ thay nhớt khuyến nghị theo thời gian (tháng)
        /// </summary>
        public int? RecommendedIntervalMonths { get; set; }
    }

    /// <summary>
    /// Phạm vi sử dụng của nhớt theo loại xe
    /// </summary>
    public enum OilVehicleUsage
    {
        /// <summary>
        /// Chỉ cho xe ga (CVT)
        /// </summary>
        Scooter = 1,

        /// <summary>
        /// Chỉ cho xe số (hộp số côn / hộp số thường)
        /// </summary>
        Manual = 2,

        /// <summary>
        /// Dùng được cho cả xe ga và xe số
        /// </summary>
        Both = 3
    }
}
