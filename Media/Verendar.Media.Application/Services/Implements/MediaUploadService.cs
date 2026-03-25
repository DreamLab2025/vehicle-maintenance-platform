using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Verendar.Media.Application.Configuration;
using Verendar.Media.Application.Storage;
using Verendar.Media.Application.IStorage;
using Verendar.Media.Application.Mappings;
using Verendar.Media.Application.Services.Interfaces;
using Verendar.Media.Domain.Repositories.Interfaces;

namespace Verendar.Media.Application.Services.Implements
{
    public class MediaUploadService(
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        IOptions<FileUploadConfiguration> uploadConfig,
        IHostEnvironment hostEnvironment,
        ILogger<MediaUploadService> logger) : IMediaUploadService
    {
        private readonly IStorageService _storageService = storageService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly FileUploadConfiguration _uploadConfig = uploadConfig.Value;
        private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
        private readonly ILogger<MediaUploadService> _logger = logger;

        public async Task<ApiResponse<string>> ConfirmUploadFileAsync(Guid id, Guid userId)
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

            bool existsOnStorage;
            try
            {
                existsOnStorage = await _storageService.ExistsAsync(mediaFile.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage ExistsAsync failed for file {FileId} key {FileKey}", id, mediaFile.FilePath);
                return ApiResponse<string>.FailureResponse("Lỗi khi xác nhận upload");
            }

            if (!existsOnStorage)
            {
                _logger.LogWarning("File {FileId} (key: {FileKey}) chưa tồn tại trên S3", id, mediaFile.FilePath);
                return ApiResponse<string>.FailureResponse("File chưa được upload lên storage. Vui lòng upload file lên Presigned URL trước khi xác nhận.");
            }

            mediaFile.Status = FileStatus.Uploaded;
            await _unitOfWork.MediaFileRepository.UpdateAsync(id, mediaFile);
            await _unitOfWork.SaveChangesAsync();

            string finalUrl;
            try
            {
                finalUrl = _storageService.GetFilePath(mediaFile.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage GetFilePath failed for file {FileId}", id);
                return ApiResponse<string>.FailureResponse("Lỗi khi xác nhận upload");
            }

            return ApiResponse<string>.SuccessResponse(finalUrl, "Xác nhận upload thành công");
        }

        public async Task<ApiResponse<bool>> DeleteFileByUrlAsync(string url)
        {
            string fileKey;
            try
            {
                fileKey = _storageService.ExtractKeyFromUrl(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExtractKeyFromUrl failed for {Url}", url);
                return ApiResponse<bool>.FailureResponse("Lỗi hệ thống khi xóa file");
            }

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

            mediaFile.DeletedAt = DateTime.UtcNow;
            mediaFile.FilePath = string.Empty;
            mediaFile.Status = FileStatus.Deleted;

            await _unitOfWork.MediaFileRepository.UpdateAsync(mediaFile.Id, mediaFile);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Xóa file thành công");
        }

        public async Task ReleaseSupersededCatalogMediaAsync(
            Guid supersededMediaFileId,
            FileType expectedFileType,
            CancellationToken cancellationToken = default)
        {
            var mediaFile = await _unitOfWork.MediaFileRepository.GetByIdAsync(supersededMediaFileId);
            if (mediaFile == null)
            {
                _logger.LogInformation(
                    "ReleaseSupersededCatalogMedia: media file {MediaFileId} not found (already removed or never existed)",
                    supersededMediaFileId);
                return;
            }

            if (mediaFile.FileType != expectedFileType)
            {
                _logger.LogWarning(
                    "ReleaseSupersededCatalogMedia: skip {MediaFileId} — FileType is {Actual}, expected {Expected}",
                    supersededMediaFileId, mediaFile.FileType, expectedFileType);
                return;
            }

            var fileKey = mediaFile.FilePath;
            if (!string.IsNullOrWhiteSpace(fileKey))
            {
                try
                {
                    await _storageService.DeleteFileAsync(fileKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "S3 delete failed for superseded catalog media key {Key}", fileKey);
                    throw;
                }
            }

            mediaFile.DeletedAt = DateTime.UtcNow;
            mediaFile.FilePath = string.Empty;
            mediaFile.Status = FileStatus.Deleted;

            await _unitOfWork.MediaFileRepository.UpdateAsync(mediaFile.Id, mediaFile);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<string>> GetMediaFileUrlAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var mediaFile = await _unitOfWork.MediaFileRepository.GetByIdAsync(id);
            if (mediaFile == null || mediaFile.Status != FileStatus.Uploaded)
            {
                _logger.LogWarning("GetMediaFileUrl: file {FileId} not found or not uploaded", id);
                return ApiResponse<string>.NotFoundResponse("File không tồn tại hoặc chưa được upload");
            }

            var url = _storageService.GetFilePath(mediaFile.FilePath);
            return ApiResponse<string>.SuccessResponse(url);
        }

        public async Task<ApiResponse<InitUploadResponse>> InitiateUploadAsync(InitUploadRequest request, Guid userId)
        {
            if (!_uploadConfig.IsContentTypeAllowed(request.ContentType))
            {
                _logger.LogWarning("InitiateUpload: content type not allowed {ContentType}", request.ContentType);
                return ApiResponse<InitUploadResponse>.FailureResponse(
                    $"Content type '{request.ContentType}' không được phép. Các loại cho phép: {string.Join(", ", _uploadConfig.AllowedContentTypes)}");
            }

            var maxFileSize = _uploadConfig.GetMaxFileSize(request.ContentType);
            if (request.Size > maxFileSize)
            {
                var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                _logger.LogWarning("InitiateUpload: size exceeds limit {Size} for {ContentType}", request.Size, request.ContentType);
                return ApiResponse<InitUploadResponse>.FailureResponse(
                    $"Kích thước file vượt quá giới hạn {maxSizeMB:F2} MB cho loại '{request.ContentType}'");
            }

            string fileKey = S3KeyBuilder.BuildMediaUploadKey(
                _hostEnvironment.EnvironmentName,
                request.FileType,
                userId,
                request.FileName);
            string presignedUrl;
            try
            {
                presignedUrl = await _storageService.GeneratePresignedUrlAsync(fileKey, request.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GeneratePresignedUrlAsync failed for {FileName}", request.FileName);
                return ApiResponse<InitUploadResponse>.FailureResponse("Lỗi khi khởi tạo upload");
            }

            var mediaFile = request.ToEntity(userId, fileKey, request.FileType);

            await _unitOfWork.MediaFileRepository.AddAsync(mediaFile);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<InitUploadResponse>.SuccessResponse(
                mediaFile.ToInitUploadResponse(presignedUrl),
                "Khởi tạo upload thành công");
        }
    }
}
