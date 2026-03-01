using System.Text.Json.Serialization;

namespace Verendar.Notification.Application.Dtos.ESms
{
    public class ZaloZnsRequest
    {
        [JsonPropertyName("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("SecretKey")]
        public string SecretKey { get; set; } = string.Empty;

        [JsonPropertyName("Phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("TempId")]
        public string TemplateId { get; set; } = string.Empty; // Zalo template ID

        [JsonPropertyName("Params")]
        public List<string> Params { get; set; } = new(); // Template parameters

        [JsonPropertyName("RequestId")]
        public string? RequestId { get; set; }
        [JsonPropertyName("Sandbox")]
        public int Sandbox { get; set; }
    }
}
