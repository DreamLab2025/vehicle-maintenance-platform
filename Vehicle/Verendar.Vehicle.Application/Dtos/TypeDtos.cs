namespace Verendar.Vehicle.Application.Dtos
{
    public class TypeRequest
    {
        public string Name { get; set; } = null!;
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
