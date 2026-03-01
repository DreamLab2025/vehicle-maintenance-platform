namespace Verendar.Vehicle.Application.Services.Interfaces
{
    public interface IUserEmailProvider
    {
        Task<string?> GetUserEmailByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
