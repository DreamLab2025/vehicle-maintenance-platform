using VMP.Common.Databases.Implements;
using VMP.Media.Domain.Entities;
using VMP.Media.Domain.Repositories.Interfaces;
using VMP.Media.Infrastructure.Data;

namespace VMP.Media.Infrastructure.Repositories.Implements
{
    public class MediaFileRepository : PostgresRepository<MediaFile>, IMediaFileRepository
    {
        public MediaFileRepository(MediaDbContext context) : base(context)
        {
        }
    }
}
