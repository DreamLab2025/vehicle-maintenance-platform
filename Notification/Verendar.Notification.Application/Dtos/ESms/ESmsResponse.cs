using System.Text.Json.Serialization;

namespace Verendar.Notification.Application.Dtos.ESms;

public class ESmsResponse
{
    [JsonPropertyName("CodeResult")]
    public string CodeResult { get; set; } = string.Empty;

    [JsonPropertyName("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("SMSID")]
    public string? SmsId { get; set; }

    [JsonPropertyName("Balance")]
    public decimal? Balance { get; set; }

    public bool IsSuccess => CodeResult == "100";
}
