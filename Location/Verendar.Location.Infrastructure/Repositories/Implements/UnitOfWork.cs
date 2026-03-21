namespace Verendar.Location.Infrastructure.Repositories.Implements;

using Verendar.Common.Databases.UnitOfWork;
using Verendar.Location.Domain.Repositories.Interfaces;

public class UnitOfWork(LocationDbContext context) : BaseUnitOfWork<LocationDbContext>(context), IUnitOfWork
{
    private IProvinceRepository? _provinces;
    private IWardRepository? _wards;
    private IAdministrativeUnitRepository? _administrativeUnits;
    private IAdministrativeRegionRepository? _administrativeRegions;

    public IProvinceRepository Provinces => _provinces ??= new ProvinceRepository(Context);
    public IWardRepository Wards => _wards ??= new WardRepository(Context);
    public IAdministrativeUnitRepository AdministrativeUnits => _administrativeUnits ??= new AdministrativeUnitRepository(Context);
    public IAdministrativeRegionRepository AdministrativeRegions => _administrativeRegions ??= new AdministrativeRegionRepository(Context);
}
