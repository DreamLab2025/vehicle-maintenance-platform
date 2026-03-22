namespace Verendar.Vehicle.Domain.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class VehicleType : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public List<Brand> Brands { get; set; } = [];
    }
}
