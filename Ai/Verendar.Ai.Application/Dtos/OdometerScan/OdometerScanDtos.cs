namespace Verendar.Ai.Application.Dtos.OdometerScan
{
    public class OdometerScanRequest
    {
        public Guid MediaFileId { get; set; }
    }

    public class OdometerScanResponse
    {
        public int? DetectedOdometer { get; set; }
        public string? Confidence { get; set; }
        public string? Message { get; set; }
    }
}
