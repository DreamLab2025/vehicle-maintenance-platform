using Verendar.Media.Domain.Repositories.Interfaces;
namespace Verendar.Media.Infrastructure.Repositories.Implements
{
    public class MediaFileRepository(MediaDbContext context) : PostgresRepository<MediaFile>(context), IMediaFileRepository
    {
    }
}
