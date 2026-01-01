using System.ComponentModel.DataAnnotations;

namespace VMP.Vehicle.Application.Dtos
{
    public class UserVehicleRequest
    {
        [Required(ErrorMessage = "Mẫu xe không được để trống")]
        public Guid VehicleModelId { get; set; }

        [Required(ErrorMessage = "Biển số xe không được để trống")]
        [MaxLength(20, ErrorMessage = "Biển số xe không được vượt quá 20 ký tự")]
        public string LicensePlate { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "Tên gọi không được vượt quá 100 ký tự")]
        public string? Nickname { get; set; }

        [MaxLength(50, ErrorMessage = "Số VIN không được vượt quá 50 ký tự")]
        public string? VinNumber { get; set; }

        [Required(ErrorMessage = "Ngày mua không được để trống")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Số km hiện tại không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số km phải lớn hơn hoặc bằng 0")]
        public int CurrentOdometer { get; set; }
    }

    public class UpdateOdometerRequest
    {
        [Required(ErrorMessage = "Số km hiện tại không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số km phải lớn hơn hoặc bằng 0")]
        public int CurrentOdometer { get; set; }
    }

    public class UserVehicleResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid VehicleModelId { get; set; }
        public string VehicleModelName { get; set; } = null!;
        public string BrandName { get; set; } = null!;
        public string TypeName { get; set; } = null!;
        public string LicensePlate { get; set; } = null!;
        public string? Nickname { get; set; }
        public string? VinNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int CurrentOdometer { get; set; }
        public DateTime LastOdometerUpdateAt { get; set; }
        public decimal AverageKmPerDay { get; set; }
        public DateTime? LastCalculatedDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserVehicleDetailResponse : UserVehicleResponse
    {
        public int TotalMaintenanceActivities { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public int DaysSincePurchase { get; set; }
        public int TotalKmDriven { get; set; }
    }
}
