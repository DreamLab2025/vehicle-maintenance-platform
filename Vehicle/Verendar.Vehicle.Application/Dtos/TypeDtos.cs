using System.ComponentModel.DataAnnotations;

namespace Verendar.Vehicle.Application.Dtos
{
    public class TypeRequest
    {
        [Required(ErrorMessage = "Tên loại xe không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên loại xe không được vượt quá 100 ký tự")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; } = null!;
    }

    public class TypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
