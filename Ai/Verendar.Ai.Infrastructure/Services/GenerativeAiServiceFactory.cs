using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Infrastructure.ExternalServices;

namespace Verendar.Ai.Infrastructure.Services
{
    public class GenerativeAiServiceFactory(
        GeminiService geminiService,
        BedrockService bedrockService,
        IAiUsageService usageService) : IGenerativeAiServiceFactory
    {
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
    }
}
