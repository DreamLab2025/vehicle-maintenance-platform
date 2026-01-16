using Verender.Common.Shared;
using Verender.Media.Application.Dtos;

namespace Verender.Media.Application.Services.Interfaces
{
    public interface IMediaUploadService
    {
        Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId);
        Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId);
        Task<ApiResponse<bool>> DeleteFileByUrlAsync(string url);
    }
}
