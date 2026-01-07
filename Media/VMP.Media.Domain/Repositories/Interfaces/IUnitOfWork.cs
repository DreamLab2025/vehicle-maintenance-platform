using VMP.Common.Databases.UnitOfWork;

namespace VMP.Media.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IMediaFileRepository MediaFileRepository { get; }
    }
}
