using Verendar.Common.Databases.UnitOfWork;
using Verendar.Media.Domain.Repositories.Interfaces;
using Verendar.Media.Infrastructure.Data;

namespace Verendar.Media.Infrastructure.Repositories.Implements
{
    public class UnitOfWork : BaseUnitOfWork<MediaDbContext>, IUnitOfWork
    {
        private IMediaFileRepository? _mediaFileRepository;
        public UnitOfWork(MediaDbContext context) : base(context)
        {
        }

        public IMediaFileRepository MediaFileRepository =>
            _mediaFileRepository ??= new MediaFileRepository(Context);
    }
}
