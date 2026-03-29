namespace Verendar.Ai.Application.Dtos.Health
{
    public class ProviderHealthStatus
    {
        public string Provider { get; set; } = string.Empty;
        public bool Connected { get; set; }
        public string? Message { get; set; }
    }

    public class HealthCheckResponse
    {
        public string Status { get; set; } = "Healthy";
        public string ActiveProvider { get; set; } = string.Empty;
        public List<ProviderHealthStatus> Providers { get; set; } = [];
    }
}
