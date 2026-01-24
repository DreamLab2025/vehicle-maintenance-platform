using Verendar.Common.Databases.UnitOfWork;

namespace Verendar.Media.Domain.Repositories.Interfaces
{
    public interface IUnitOfWork : IBaseUnitOfWork
    {
        IMediaFileRepository MediaFileRepository { get; }
    }
}
