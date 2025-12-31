using System.ComponentModel.DataAnnotations;
using VMP.Vehicle.Domain.Entities;

namespace VMP.Vehicle.Application.Dtos
{
    public class ModelRequest
    {
        [Required(ErrorMessage = "Tên m?u xe không ???c ?? tr?ng")]
        [MaxLength(150, ErrorMessage = "Tên m?u xe không ???c v??t quá 150 ký t?")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Th??ng hi?u không ???c ?? tr?ng")]
        public Guid BrandId { get; set; }

        [Required(ErrorMessage = "Lo?i xe không ???c ?? tr?ng")]
        public Guid TypeId { get; set; }

        [Required(ErrorMessage = "N?m s?n xu?t không ???c ?? tr?ng")]
        [Range(1900, 2100, ErrorMessage = "N?m s?n xu?t không h?p l?")]
        public int ReleaseYear { get; set; }

        [Required(ErrorMessage = "Lo?i nhiên li?u không ???c ?? tr?ng")]
        public VehicleFuelType FuelType { get; set; }

        [MaxLength(500, ErrorMessage = "URL hình ?nh không ???c v??t quá 500 ký t?")]
        [Url(ErrorMessage = "URL hình ?nh không h?p l?")]
        public string? ImageUrl { get; set; }

        [Range(0, 100, ErrorMessage = "Dung tích d?u ph?i t? 0 ??n 100 lít")]
        public decimal? OilCapacity { get; set; }

        [MaxLength(50, ErrorMessage = "Kích th??c l?p tr??c không ???c v??t quá 50 ký t?")]
        public string? TireSizeFront { get; set; }

        [MaxLength(50, ErrorMessage = "Kích th??c l?p sau không ???c v??t quá 50 ký t?")]
        public string? TireSizeRear { get; set; }
    }

    public class BulkModelItem
    {
        [Required(ErrorMessage = "Tên m?u xe không ???c ?? tr?ng")]
        [MaxLength(150, ErrorMessage = "Tên m?u xe không ???c v??t quá 150 ký t?")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "N?m s?n xu?t không ???c ?? tr?ng")]
        [Range(1900, 2100, ErrorMessage = "N?m s?n xu?t không h?p l?")]
        public int ReleaseYear { get; set; }

        [Required(ErrorMessage = "Lo?i nhiên li?u không ???c ?? tr?ng")]
        public VehicleFuelType FuelType { get; set; }

        [MaxLength(500, ErrorMessage = "URL hình ?nh không ???c v??t quá 500 ký t?")]
        [Url(ErrorMessage = "URL hình ?nh không h?p l?")]
        public string? ImageUrl { get; set; }

        [Range(0, 100, ErrorMessage = "Dung tích d?u ph?i t? 0 ??n 100 lít")]
        public decimal? OilCapacity { get; set; }

        [MaxLength(50, ErrorMessage = "Kích th??c l?p tr??c không ???c v??t quá 50 ký t?")]
        public string? TireSizeFront { get; set; }

        [MaxLength(50, ErrorMessage = "Kích th??c l?p sau không ???c v??t quá 50 ký t?")]
        public string? TireSizeRear { get; set; }
    }

    public class BulkModelRequest
    {
        [Required(ErrorMessage = "Th??ng hi?u không ???c ?? tr?ng")]
        public Guid BrandId { get; set; }

        [Required(ErrorMessage = "Lo?i xe không ???c ?? tr?ng")]
        public Guid TypeId { get; set; }

        [Required(ErrorMessage = "Danh sách m?u xe không ???c ?? tr?ng")]
        [MinLength(1, ErrorMessage = "Ph?i có ít nh?t 1 m?u xe")]
        public List<BulkModelItem> Models { get; set; } = new();
    }

    public class ModelResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = null!;
        public Guid TypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public int ReleaseYear { get; set; }
        public VehicleFuelType FuelType { get; set; }
        public string FuelTypeName { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public decimal? OilCapacity { get; set; }
        public string? TireSizeFront { get; set; }
        public string? TireSizeRear { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BulkModelResponse
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<ModelResponse> SuccessfulModels { get; set; } = new();
        public List<BulkOperationError> Errors { get; set; } = new();
    }
}
