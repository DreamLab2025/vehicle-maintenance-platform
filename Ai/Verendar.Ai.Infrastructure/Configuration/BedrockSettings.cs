namespace Verendar.Ai.Infrastructure.Configuration
{
    public class BedrockSettings
    {
        public const string SectionName = "Bedrock";
        public string Region { get; set; } = "us-east-1";
        public string? AccessKeyId { get; set; }
        public string? SecretAccessKey { get; set; }
        public string DefaultModel { get; set; } = "anthropic.claude-3-5-sonnet-20241022-v2:0";
        public int MaxRetries { get; set; } = 3;
        public int TimeoutSeconds { get; set; } = 60;
        public BedrockDefaultParameters DefaultParameters { get; set; } = new();
        public BedrockPricing Pricing { get; set; } = new();
    }

    public class BedrockDefaultParameters
    {
        public int MaxTokens { get; set; } = 8192;
        public decimal Temperature { get; set; } = 0.7m;
        public decimal TopP { get; set; } = 0.95m;
        public int TopK { get; set; } = 40;
    }

    public class BedrockPricing
    {
        public decimal InputCostPer1MTokens { get; set; } = 3.00m;
        public decimal OutputCostPer1MTokens { get; set; } = 15.00m;
    }
}
