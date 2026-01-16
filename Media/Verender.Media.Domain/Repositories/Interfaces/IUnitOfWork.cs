using Verender.Common.Databases.UnitOfWork;

namespace Verender.Media.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IMediaFileRepository MediaFileRepository { get; }
    }
}
