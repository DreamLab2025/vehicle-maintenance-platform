using Verendar.Common.Databases.UnitOfWork;
using Verendar.Media.Domain.Repositories.Interfaces;
using Verendar.Media.Infrastructure.Data;

namespace Verendar.Media.Infrastructure.Repositories.Implements
{
    public class UnitOfWork(MediaDbContext context) : BaseUnitOfWork<MediaDbContext>(context), IUnitOfWork
    {
        private IMediaFileRepository? _mediaFileRepository;

        public IMediaFileRepository MediaFileRepository =>
            _mediaFileRepository ??= new MediaFileRepository(Context);
    }
}
