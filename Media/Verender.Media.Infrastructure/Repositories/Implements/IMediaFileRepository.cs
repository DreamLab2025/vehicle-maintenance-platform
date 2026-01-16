using Verender.Common.Databases.Implements;
using Verender.Media.Domain.Entities;
using Verender.Media.Domain.Repositories.Interfaces;
using Verender.Media.Infrastructure.Data;

namespace Verender.Media.Infrastructure.Repositories.Implements
{
    public class MediaFileRepository : PostgresRepository<MediaFile>, IMediaFileRepository
    {
        public MediaFileRepository(MediaDbContext context) : base(context)
        {
        }
    }
}
