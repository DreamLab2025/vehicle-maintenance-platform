namespace Verendar.Media.Application.Services.Interfaces
{
    public interface IMediaUploadService
    {
        Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId);
        Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId);
        Task<ApiResponse<bool>> DeleteFileByUrlAsync(string url);
        Task ReleaseSupersededCatalogMediaAsync(
            Guid supersededMediaFileId,
            FileType expectedFileType,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<string>> GetMediaFileUrlAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
