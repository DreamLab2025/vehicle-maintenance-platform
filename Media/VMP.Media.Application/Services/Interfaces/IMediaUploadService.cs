using VMP.Common.Shared;
using VMP.Media.Application.Dtos;

namespace VMP.Media.Application.Services.Interfaces
{
    public interface IMediaUploadService
    {
        Task<ApiResponse<bool>> ConfirmUploadFileAsync(Guid id, Guid userId);
        Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId);
    }
}
