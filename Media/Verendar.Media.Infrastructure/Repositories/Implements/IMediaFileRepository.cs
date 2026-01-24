using Verendar.Common.Databases.Implements;
using Verendar.Media.Domain.Entities;
using Verendar.Media.Domain.Repositories.Interfaces;
using Verendar.Media.Infrastructure.Data;

namespace Verendar.Media.Infrastructure.Repositories.Implements
{
    public class MediaFileRepository : PostgresRepository<MediaFile>, IMediaFileRepository
    {
        public MediaFileRepository(MediaDbContext context) : base(context)
        {
        }
    }
}
