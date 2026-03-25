using Verendar.Ai.Domain.Enums;

namespace Verendar.Ai.Application.Services.Interfaces
{
    /// <summary>
    /// Resolves <see cref="IGenerativeAiService"/> for Gemini or Bedrock with usage tracking.
    /// </summary>
    public interface IGenerativeAiServiceFactory
    {
        /// <summary>Provider from <c>AiProvider</c> config (default Gemini).</summary>
        IGenerativeAiService CreateDefault();

        /// <summary>Explicit provider (e.g. Bedrock for vision-only flows).</summary>
        IGenerativeAiService Create(AiProvider provider);
    }
}
