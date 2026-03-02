namespace Verendar.Ai.Application.Dtos.Health
{
    public class HealthCheckResponse
    {
        public string Status { get; set; } = "Healthy";
        public string Provider { get; set; } = string.Empty;
        public bool ThirdPartyAiConnected { get; set; }
        public string? Message { get; set; }
    }
}
