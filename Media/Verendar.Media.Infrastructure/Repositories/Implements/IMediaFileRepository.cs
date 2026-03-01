using Verendar.Common.Databases.Implements;
using Verendar.Media.Domain.Entities;
using Verendar.Media.Domain.Repositories.Interfaces;
using Verendar.Media.Infrastructure.Data;

namespace Verendar.Media.Infrastructure.Repositories.Implements
{
    public class MediaFileRepository(MediaDbContext context) : PostgresRepository<MediaFile>(context), IMediaFileRepository
    {
    }
}
