using System.ComponentModel.DataAnnotations;
using Verendar.Vehicle.Domain.Entities;

namespace Verendar.Vehicle.Application.Dtos
{
    public class OilRequest
    {
        /// <summary>
        /// ID của VehiclePart (phải thuộc category OIL)
        /// </summary>
        [Required(ErrorMessage = "VehiclePartId không được để trống")]
        public Guid VehiclePartId { get; set; }

        /// <summary>
        /// Cấp độ nhớt: ví dụ 5W-30, 10W-40
        /// </summary>
        [Required(ErrorMessage = "Cấp độ nhớt không được để trống")]
        [MaxLength(20, ErrorMessage = "Cấp độ nhớt không được vượt quá 20 ký tự")]
        public string ViscosityGrade { get; set; } = null!;

        /// <summary>
        /// Chuẩn API: API SN, SM, …
        /// </summary>
        [MaxLength(20, ErrorMessage = "Chuẩn API không được vượt quá 20 ký tự")]
        public string? ApiServiceClass { get; set; }

        /// <summary>
        /// Chuẩn JASO: MA, MA2 (thường cho xe số), MB (thường cho xe ga)
        /// </summary>
        [MaxLength(10, ErrorMessage = "Chuẩn JASO không được vượt quá 10 ký tự")]
        public string? JasoRating { get; set; }

        /// <summary>
        /// Loại dầu: Khoáng / Bán tổng hợp / Tổng hợp
        /// </summary>
        [MaxLength(50, ErrorMessage = "Loại dầu không được vượt quá 50 ký tự")]
        public string? BaseOilType { get; set; }

        /// <summary>
        /// Dung tích khuyến nghị cho mỗi lần thay (lít)
        /// </summary>
        [Range(0.1, 100, ErrorMessage = "Dung tích phải từ 0.1 đến 100 lít")]
        public decimal? RecommendedVolumeLiters { get; set; }

        /// <summary>
        /// Nhớt dành cho loại xe nào
        /// </summary>
        [Required(ErrorMessage = "Loại xe sử dụng không được để trống")]
        public OilVehicleUsage VehicleUsage { get; set; } = OilVehicleUsage.Both;

        /// <summary>
        /// Chu kỳ thay nhớt khuyến nghị cho xe ga (km)
        /// </summary>
        [Range(500, 100000, ErrorMessage = "Chu kỳ xe ga phải từ 500 đến 100000 km")]
        public int? RecommendedIntervalKmScooter { get; set; }

        /// <summary>
        /// Chu kỳ thay nhớt khuyến nghị cho xe số (km)
        /// </summary>
        [Range(500, 100000, ErrorMessage = "Chu kỳ xe số phải từ 500 đến 100000 km")]
        public int? RecommendedIntervalKmManual { get; set; }

        /// <summary>
        /// Chu kỳ thay nhớt khuyến nghị theo thời gian (tháng)
        /// </summary>
        [Range(1, 24, ErrorMessage = "Chu kỳ theo tháng phải từ 1 đến 24 tháng")]
        public int? RecommendedIntervalMonths { get; set; }
    }

    public class OilResponse
    {
        public Guid Id { get; set; }
        public Guid VehiclePartId { get; set; }
        public string VehiclePartName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string ViscosityGrade { get; set; } = null!;
        public string? ApiServiceClass { get; set; }
        public string? JasoRating { get; set; }
        public string? BaseOilType { get; set; }
        public decimal? RecommendedVolumeLiters { get; set; }
        public OilVehicleUsage VehicleUsage { get; set; }
        public string VehicleUsageDisplay { get; set; } = null!;
        public int? RecommendedIntervalKmScooter { get; set; }
        public int? RecommendedIntervalKmManual { get; set; }
        public int? RecommendedIntervalMonths { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
