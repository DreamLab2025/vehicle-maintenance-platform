namespace Verendar.Vehicle.Domain.Entities
{
    public class Variant : BaseEntity
    {
        [Required]
        public Guid VehicleModelId { get; set; }
        public Model VehicleModel { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Color { get; set; } = null!;

        [Required]
        [MaxLength(7)]
        public string HexCode { get; set; } = null!;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;

        public Guid? ImageMediaFileId { get; set; }
    }
}
