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

    /// <summary>Query parameters for GET /api/v1/maintenance-records/export</summary>
    public class ExportMaintenanceQueryRequest
    {
        public Guid UserVehicleId { get; set; }
        /// <summary>pdf or csv</summary>
        public string Format { get; set; } = "pdf";
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        /// <summary>Comma-separated column names. Null = all columns.</summary>
        public string? Columns { get; set; }
    }
}
