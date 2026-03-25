namespace Verendar.Ai.Application.Clients
{
    public interface IMediaServiceClient
    {
        Task<string?> GetMediaFileUrlAsync(Guid mediaFileId, CancellationToken cancellationToken = default);
    }
}
