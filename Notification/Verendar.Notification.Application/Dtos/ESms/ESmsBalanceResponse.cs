using System.Text.Json.Serialization;

namespace Verendar.Notification.Application.Dtos.ESms;

public class ESmsBalanceResponse
{
    [JsonPropertyName("CodeResult")]
    public string CodeResult { get; set; } = string.Empty;

    [JsonPropertyName("Balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("UserName")]
    public string? UserName { get; set; }
}
