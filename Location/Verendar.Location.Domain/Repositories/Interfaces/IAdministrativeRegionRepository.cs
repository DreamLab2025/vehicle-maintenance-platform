namespace Verendar.Location.Domain.Repositories.Interfaces;

using Verendar.Location.Domain.Entities;

public interface IAdministrativeRegionRepository
{
    Task<List<AdministrativeRegion>> GetAllAsync();
    Task<AdministrativeRegion?> GetByIdAsync(int id);
}
