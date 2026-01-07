using VMP.Common.Shared;
using VMP.Media.Application.Dtos;

namespace VMP.Media.Application.Services.Interfaces
{
    public interface IMediaUploadService
    {
        Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId);
        Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId);
        Task<ApiResponse<bool>> DeleteFileByUrlAsync(string url);
    }
}
