using Verendar.Common.Shared;
using Verendar.Media.Application.Dtos;

namespace Verendar.Media.Application.Services.Interfaces
{
    public interface IMediaUploadService
    {
        Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId);
        Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId);
        Task<ApiResponse<bool>> DeleteFileByUrlAsync(string url);
    }
}
