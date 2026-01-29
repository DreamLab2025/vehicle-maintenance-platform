using Verendar.Common.Shared;
using Verendar.Media.Application.Dtos;

namespace Verendar.Media.Application.Services.Interfaces
{
    public interface IMediaUploadService
    {
        Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId);
        /// <param name="folderKey">avatar | vehicle-types | vehicle-brands | vehicle-models | consumables | misc</param>
        Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId, string folderKey);
        Task<ApiResponse<bool>> DeleteFileByUrlAsync(string url);
    }
}
