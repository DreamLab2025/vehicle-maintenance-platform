namespace Verendar.Vehicle.Application.Dtos
{
    public enum ExportFormat
    {
        Pdf,
        Csv
    }

    public class ExportMaintenanceRequest
    {
        public Guid UserVehicleId { get; set; }
        public ExportFormat Format { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public List<string>? Columns { get; set; }
    }

    public class ExportMaintenanceQueryRequest
    {
        public Guid UserVehicleId { get; set; }
        public string Format { get; set; } = "pdf";
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public string? Columns { get; set; }
    }
}
