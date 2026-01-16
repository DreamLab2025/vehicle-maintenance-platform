using System.ComponentModel.DataAnnotations;

namespace Verender.Vehicle.Application.Dtos
{
    public class BrandRequest
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Phải chọn ít nhất 1 loại xe")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 loại xe")]
        public List<Guid> VehicleTypeIds { get; set; } = new();

        [MaxLength(500, ErrorMessage = "URL logo không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "URL logo không hợp lệ")]
        public string? LogoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "Website không được vượt quá 500 ký tự")]
        [Url(ErrorMessage = "Website không hợp lệ")]
        public string? Website { get; set; }

        [MaxLength(20, ErrorMessage = "Số điện thoại hỗ trợ không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại hỗ trợ không hợp lệ")]
        public string? SupportPhone { get; set; }
    }

    public class BulkBrandRequest
    {
        [Required(ErrorMessage = "Danh sách thương hiệu không được để trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 thương hiệu")]
        public List<BrandRequest> Brands { get; set; } = new();
    }

    public class BrandResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public List<string> VehicleTypeNames { get; set; } = new();
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? SupportPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BulkBrandResponse
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<BrandResponse> SuccessfulBrands { get; set; } = new();
        public List<BulkOperationError> Errors { get; set; } = new();
    }
}
