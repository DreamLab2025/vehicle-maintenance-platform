using System.ComponentModel.DataAnnotations;

namespace VMP.Vehicle.Application.Dtos
{
    public class ModelImageRequest
    {
        [Required(ErrorMessage = "Mã mẫu xe không được để trống")]
        public Guid VehicleModelId { get; set; }

        [Required(ErrorMessage = "Màu xe không được để trống")]
        [MaxLength(50, ErrorMessage = "Màu xe không được vượt quá 50 ký tự")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "Mã màu không được để trống")]
        [MaxLength(7, ErrorMessage = "Mã màu không được vượt quá 7 ký tự")]
        public string HexCode { get; set; } = null!;

        [Required(ErrorMessage = "URL hình ảnh không được để trống")]
        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string ImageUrl { get; set; } = null!;
    }

    public class ModelImageResponse
    {
        public Guid Id { get; set; }
        public Guid VehicleModelId { get; set; }
        public string Color { get; set; } = null!;
        public string HexCode { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelImageUpdateRequest
    {
        [Required(ErrorMessage = "Màu xe không được để trống")]
        [MaxLength(50, ErrorMessage = "Màu xe không được vượt quá 50 ký tự")]
        public string Color { get; set; } = null!;

        [Required(ErrorMessage = "Mã màu không được để trống")]
        [MaxLength(7, ErrorMessage = "Mã màu không được vượt quá 7 ký tự")]
        public string HexCode { get; set; } = null!;

        [Required(ErrorMessage = "URL hình ảnh không được để trống")]
        [MaxLength(500, ErrorMessage = "URL hình ảnh không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string ImageUrl { get; set; } = null!;
    }
}
