namespace Verendar.Location.Domain.Repositories.Interfaces;

using Verendar.Location.Domain.Entities;

public interface IProvinceRepository
{
    Task<Province?> GetByCodeAsync(string code);
    Task<List<Province>> GetAllAsync();
    Task<List<Province>> GetAllByRegionIdAsync(int regionId);
    Task UpdateBoundaryUrlAsync(string code, string url);
}
