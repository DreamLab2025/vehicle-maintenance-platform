namespace Verendar.Location.Domain.Repositories.Interfaces;

using Verendar.Location.Domain.Entities;

public interface IWardRepository
{
    Task<Ward?> GetByCodeAsync(string code);
    Task<List<Ward>> GetByProvinceCodeAsync(string provinceCode);
}
