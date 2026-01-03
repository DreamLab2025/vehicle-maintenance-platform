using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VMP.Common.Shared;
using VMP.Media.Application.Configuration;
using VMP.Media.Application.Dtos;
using VMP.Media.Application.IStorage;
using VMP.Media.Application.Mappings;
using VMP.Media.Application.Services.Interfaces;
using VMP.Media.Domain.Entities;
using VMP.Media.Domain.Repositories.Interfaces;

namespace VMP.Media.Application.Services.Implements
{
    public class MediaUploadService : IMediaUploadService
    {
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FileUploadConfiguration _uploadConfig;
        private readonly ILogger<MediaUploadService> _logger;

        public MediaUploadService(
            IStorageService storageService,
            IUnitOfWork unitOfWork,
            IOptions<FileUploadConfiguration> uploadConfig,
            ILogger<MediaUploadService> logger)
        {
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _uploadConfig = uploadConfig.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId)
        {
            try
            {
                var mediaFile = await _unitOfWork.MediaFileRepository.GetByIdAsync(id);
                if (mediaFile == null)
                {
                    _logger.LogWarning("Media file with ID {FileId} not found for confirmation", id);
                    return ApiResponse<string>.FailureResponse("File không tồn tại");
                }
                if (mediaFile.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to confirm upload for file {FileId} they do not own", userId, id);
                    return ApiResponse<string>.FailureResponse("Bạn không có quyền xác nhận file này");
                }

                mediaFile.Status = FileStatus.Uploaded;
                await _unitOfWork.MediaFileRepository.UpdateAsync(id, mediaFile);
                await _unitOfWork.SaveChangesAsync();

                string finalUrl = _storageService.GetFilePath(mediaFile.FilePath);

                _logger.LogInformation("Upload confirmed for file {FileId} by user {UserId}", id, userId);
                return ApiResponse<string>.SuccessResponse(finalUrl, "Xác nhận upload thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming upload for file {FileId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xác nhận upload");
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileByKeyAsync(string url)
        {
            try
            {
                var fileKey = _storageService.ExtractKeyFromUrl(url);
                if (string.IsNullOrEmpty(fileKey))
                {
                    _logger.LogWarning("Invalid file url provided for deletion: {Url}", url);
                    return ApiResponse<bool>.FailureResponse("Đường dẫn file không hợp lệ");
                }

                var mediaFile = await _unitOfWork.MediaFileRepository.FindOneAsync(m => m.FilePath == fileKey);
                if (mediaFile == null)
                {
                    _logger.LogWarning("Media file with key {Key} not found for deletion", fileKey);
                    return ApiResponse<bool>.FailureResponse("File không tồn tại");
                }

                try
                {
                    await _storageService.DeleteFileAsync(fileKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Lỗi khi xóa trên S3 (nhưng vẫn tiếp tục xóa DB): {Key}", fileKey);
                }

                if (mediaFile != null)
                {
                    mediaFile.DeletedAt = DateTime.UtcNow;
                    mediaFile.FilePath = string.Empty;
                    mediaFile.Status = FileStatus.Deleted;

                    await _unitOfWork.MediaFileRepository.UpdateAsync(mediaFile.Id, mediaFile);
                    await _unitOfWork.SaveChangesAsync();
                }

                return ApiResponse<bool>.SuccessResponse(true, "Xóa file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file by url: {Url}", url);
                return ApiResponse<bool>.FailureResponse("Lỗi hệ thống khi xóa file");
            }
        }

        public async Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId)
        {
            try
            {
                if (!_uploadConfig.IsContentTypeAllowed(request.ContentType))
                {
                    return ApiResponse<InitUploadResponse>.FailureResponse(
                        $"Content type '{request.ContentType}' không được phép. Các loại cho phép: {string.Join(", ", _uploadConfig.AllowedContentTypes)}");
                }

                var maxFileSize = _uploadConfig.GetMaxFileSize(request.ContentType);
                if (request.Size > maxFileSize)
                {
                    var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                    return ApiResponse<InitUploadResponse>.FailureResponse(
                        $"Kích thước file vượt quá giới hạn {maxSizeMB:F2} MB cho loại '{request.ContentType}'");
                }

                if (request.Size <= 0)
                {
                    return ApiResponse<InitUploadResponse>.FailureResponse("Kích thước file phải lớn hơn 0");
                }

                string fileKey = GenerateFilePath(request.FileType, userId, request.FileName);

                var presignedUrl = await _storageService.GeneratePresignedUrlAsync(fileKey, request.ContentType);

                var mediaFile = request.ToEntity(userId, fileKey);

                await _unitOfWork.MediaFileRepository.AddAsync(mediaFile);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Initiated upload for file {FileName} (Type: {FileType}) with ID: {FileId}",
                    request.FileName, request.FileType, mediaFile.Id);

                return ApiResponse<InitUploadResponse>.SuccessResponse(
                    mediaFile.ToInitUploadResponse(presignedUrl),
                    "Khởi tạo upload thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating upload for file {FileName}", request.FileName);
                return ApiResponse<InitUploadResponse>.FailureResponse("Lỗi khi khởi tạo upload");
            }
        }

        private string GenerateFilePath(FileType fileType, Guid userId, string fileName)
        {
            string folder = fileType switch
            {
                FileType.Avatar => $"users/{userId}/avatar",
                FileType.VehicleBrand => "master/brands",
                FileType.VehicleModel => "master/models",
                FileType.ConsumableItem => "master/consumables",
                FileType.Other => "misc/general",
                _ => "misc/unknown"
            };

            return $"{folder}/{Guid.NewGuid()}{Path.GetExtension(fileName).ToLower()}";
        }
    }
}
