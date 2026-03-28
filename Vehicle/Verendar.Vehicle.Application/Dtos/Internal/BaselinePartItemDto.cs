namespace Verendar.Vehicle.Application.Dtos.Internal
{
    public class BaselinePartItemDto
    {
        public Guid PartTrackingId { get; set; }
        public string PartCategorySlug { get; set; } = string.Empty;
        public Guid VehicleModelId { get; set; }
    }
}
