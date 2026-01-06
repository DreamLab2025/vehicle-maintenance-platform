using System.ComponentModel.DataAnnotations;

namespace VMP.Vehicle.Application.Dtos
{
    public class ModelImageRequest
    {
        [Required(ErrorMessage = "Mã m?u xe không ???c ?? tr?ng")]
        public Guid VehicleModelId { get; set; }

        [Required(ErrorMessage = "Màu xe không ???c ?? tr?ng")]
        [MaxLength(50, ErrorMessage = "Màu xe không ???c v??t quá 50 ký t?")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "URL hình ?nh không ???c ?? tr?ng")]
        [MaxLength(500, ErrorMessage = "URL hình ?nh không ???c v??t quá 500 ký t?")]
        [Url(ErrorMessage = "URL hình ?nh không h?p l?")]
        public string ImageUrl { get; set; } = null!;
    }

    public class ModelImageResponse
    {
        public Guid Id { get; set; }
        public Guid VehicleModelId { get; set; }
        public string Color { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelImageUpdateRequest
    {
        [Required(ErrorMessage = "Màu xe không ???c ?? tr?ng")]
        [MaxLength(50, ErrorMessage = "Màu xe không ???c v??t quá 50 ký t?")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "URL hình ?nh không ???c ?? tr?ng")]
        [MaxLength(500, ErrorMessage = "URL hình ?nh không ???c v??t quá 500 ký t?")]
        [Url(ErrorMessage = "URL hình ?nh không h?p l?")]
        public string ImageUrl { get; set; } = null!;
    }
}
