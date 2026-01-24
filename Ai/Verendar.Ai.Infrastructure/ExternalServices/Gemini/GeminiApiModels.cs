using System.Text.Json.Serialization;

namespace Verendar.Ai.Infrastructure.ExternalServices.Gemini;

/// <summary>
/// Request model for Gemini API
/// https://ai.google.dev/api/rest/v1beta/models/generateContent
/// </summary>
public class GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new();

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig? GenerationConfig { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = new();

    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    [JsonPropertyName("topP")]
    public decimal? TopP { get; set; }

    [JsonPropertyName("topK")]
    public int? TopK { get; set; }

    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("responseMimeType")]
    public string? ResponseMimeType { get; set; }
}

/// <summary>
/// Response model from Gemini API
/// </summary>
public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate> Candidates { get; set; } = new();

    [JsonPropertyName("usageMetadata")]
    public GeminiUsageMetadata? UsageMetadata { get; set; }
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }
}

public class GeminiUsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; set; }

    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; set; }

    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; set; }
}

/// <summary>
/// Error response from Gemini API
/// </summary>
public class GeminiErrorResponse
{
    [JsonPropertyName("error")]
    public GeminiError? Error { get; set; }
}

public class GeminiError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
