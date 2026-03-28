namespace Verendar.Ai.Application.Services.Interfaces
{
    public interface IReAnalysisService
    {
        Task QueueReAnalysisForBaselinePartsAsync(Guid userVehicleId, Guid userId);
    }
}
