namespace Verendar.Location.Domain.Repositories.Interfaces;

using Verendar.Common.Databases.UnitOfWork;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IProvinceRepository Provinces { get; }
    IWardRepository Wards { get; }
    IAdministrativeUnitRepository AdministrativeUnits { get; }
    IAdministrativeRegionRepository AdministrativeRegions { get; }
}
