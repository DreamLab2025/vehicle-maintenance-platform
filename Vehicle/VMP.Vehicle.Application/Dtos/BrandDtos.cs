using System.ComponentModel.DataAnnotations;

namespace VMP.Vehicle.Application.Dtos
{
    public class BrandRequest
    {
        [Required(ErrorMessage = "Tên th??ng hi?u không ???c ?? tr?ng")]
        [MaxLength(100, ErrorMessage = "Tên th??ng hi?u không ???c v??t quá 100 ký t?")]
        public string Name { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "URL logo không ???c v??t quá 500 ký t?")]
        [Url(ErrorMessage = "URL logo không h?p l?")]
        public string? LogoUrl { get; set; }

        [MaxLength(500, ErrorMessage = "Website không ???c v??t quá 500 ký t?")]
        [Url(ErrorMessage = "Website không h?p l?")]
        public string? Website { get; set; }

        [MaxLength(20, ErrorMessage = "S? ?i?n tho?i h? tr? không ???c v??t quá 20 ký t?")]
        [Phone(ErrorMessage = "S? ?i?n tho?i h? tr? không h?p l?")]
        public string? SupportPhone { get; set; }
    }

    public class BulkBrandRequest
    {
        [Required(ErrorMessage = "Danh sách th??ng hi?u không ???c ?? tr?ng")]
        [MinLength(1, ErrorMessage = "Ph?i có ít nh?t 1 th??ng hi?u")]
        public List<BrandRequest> Brands { get; set; } = new();
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

    public class BulkBrandResponse
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<BrandResponse> SuccessfulBrands { get; set; } = new();
        public List<BulkOperationError> Errors { get; set; } = new();
    }
}
