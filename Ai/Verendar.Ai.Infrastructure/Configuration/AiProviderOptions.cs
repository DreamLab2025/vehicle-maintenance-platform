namespace Verendar.Ai.Infrastructure.Configuration
{
    public class AiProviderOptions
    {
        public const string ConfigKey = "AiProvider";
        public string Provider { get; set; } = "Gemini";
    }
}
