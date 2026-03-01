using System.Text.Json.Serialization;

namespace Verendar.Notification.Application.Dtos.ESms
{
    public class ESmsRequest
    {
        [JsonPropertyName("ApiKey")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("SecretKey")]
        public string SecretKey { get; set; } = string.Empty;

        [JsonPropertyName("Phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("Content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("SmsType")]
        public int SmsType { get; set; }

        [JsonPropertyName("Brandname")]
        public string Brandname { get; set; } = string.Empty;
        [JsonPropertyName("RequestId")]
        public string? RequestId { get; set; }
        [JsonPropertyName("Sandbox")]
        public int Sandbox { get; set; }
    }
}
