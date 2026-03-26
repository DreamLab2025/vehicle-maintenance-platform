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
        /// <summary>"Healthy" if the active provider is connected, otherwise "Unhealthy".</summary>
        public string Status { get; set; } = "Healthy";
        /// <summary>The currently configured default provider.</summary>
        public string ActiveProvider { get; set; } = string.Empty;
        /// <summary>Connectivity result for every registered AI provider.</summary>
        public List<ProviderHealthStatus> Providers { get; set; } = [];
    }
}
