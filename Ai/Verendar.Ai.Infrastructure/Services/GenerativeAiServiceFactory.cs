using Microsoft.Extensions.Options;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Infrastructure.Configuration;
using Verendar.Ai.Infrastructure.ExternalServices;

namespace Verendar.Ai.Infrastructure.Services
{
    public class GenerativeAiServiceFactory(
        IOptions<AiProviderOptions> options,
        GeminiService geminiService,
        BedrockService bedrockService,
        IAiUsageService usageService) : IGenerativeAiServiceFactory
    {
        public IGenerativeAiService CreateDefault()
        {
            var provider = ResolveProviderFromConfig(options.Value.Provider);
            return Create(provider);
        }

        public IGenerativeAiService Create(AiProvider provider)
        {
            IGenerativeAiService inner = provider switch
            {
                AiProvider.Gemini => geminiService,
                AiProvider.Bedrock => bedrockService,
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
            };

            return new AiUsageTrackingDecorator(inner, usageService, provider);
        }

        private static AiProvider ResolveProviderFromConfig(string? config)
        {
            if (string.IsNullOrWhiteSpace(config))
                return AiProvider.Gemini;

            var p = config.Trim();
            if (p.Equals("Bedrock", StringComparison.OrdinalIgnoreCase))
                return AiProvider.Bedrock;

            // Legacy: any other value falls back to Gemini (same as previous DI lambda).
            return AiProvider.Gemini;
        }
    }
}
