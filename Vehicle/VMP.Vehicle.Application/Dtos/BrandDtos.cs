using System.ComponentModel.DataAnnotations;

namespace VMP.Vehicle.Application.Dtos
{
    public class BrandRequest
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

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

    public class BrandResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? SupportPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
