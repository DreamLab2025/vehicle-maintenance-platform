using VMP.Common.Databases.UnitOfWork;
using VMP.Media.Domain.Repositories.Interfaces;
using VMP.Media.Infrastructure.Data;

namespace VMP.Media.Infrastructure.Repositories.Implements
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
