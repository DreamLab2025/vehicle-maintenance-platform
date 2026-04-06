namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IGenerativeAiServiceFactory
    {
        IGenerativeAiService Create(AiProvider provider);
    }
}
