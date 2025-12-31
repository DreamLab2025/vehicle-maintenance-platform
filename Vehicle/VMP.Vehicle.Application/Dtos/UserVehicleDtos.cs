using System.ComponentModel.DataAnnotations;

namespace VMP.Vehicle.Application.Dtos
{
    public class UserVehicleRequest
    {
        [Required(ErrorMessage = "M?u xe không ???c ?? tr?ng")]
        public Guid VehicleModelId { get; set; }

        [Required(ErrorMessage = "Bi?n s? xe không ???c ?? tr?ng")]
        [MaxLength(20, ErrorMessage = "Bi?n s? xe không ???c v??t quá 20 ký t?")]
        public string LicensePlate { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "Tên g?i không ???c v??t quá 100 ký t?")]
        public string? Nickname { get; set; }

        [MaxLength(50, ErrorMessage = "S? VIN không ???c v??t quá 50 ký t?")]
        public string? VinNumber { get; set; }

        [Required(ErrorMessage = "Ngày mua không ???c ?? tr?ng")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "S? km hi?n t?i không ???c ?? tr?ng")]
        [Range(0, int.MaxValue, ErrorMessage = "S? km ph?i l?n h?n ho?c b?ng 0")]
        public int CurrentOdometer { get; set; }
    }

    public class UpdateOdometerRequest
    {
        [Required(ErrorMessage = "S? km hi?n t?i không ???c ?? tr?ng")]
        [Range(0, int.MaxValue, ErrorMessage = "S? km ph?i l?n h?n ho?c b?ng 0")]
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
