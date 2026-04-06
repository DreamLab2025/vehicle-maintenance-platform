namespace Verendar.Location.Domain.Repositories.Interfaces;

using Verendar.Location.Domain.Entities;

public interface IAdministrativeUnitRepository
{
    Task<List<AdministrativeUnit>> GetAllAsync();
    Task<AdministrativeUnit?> GetByIdAsync(int id);
}
