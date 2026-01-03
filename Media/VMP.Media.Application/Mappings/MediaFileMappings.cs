using VMP.Media.Application.Dtos;
using VMP.Media.Domain.Entities;

namespace VMP.Media.Application.Mappings
{
    public static class MediaFileMappings
    {
        public static MediaFile ToEntity(this InitUploadRequest request, Guid userId, string fileKey)
        {
            return new MediaFile
            {
                UserId = userId,
                Provider = request.Provider,
                FileType = request.FileType,
                FilePath = fileKey,
                OriginalFileName = request.FileName,
                ContentType = request.ContentType,
                Extension = Path.GetExtension(request.FileName),
                Size = request.Size,
                Status = FileStatus.Pending
            };
        }

        public static InitUploadResponse ToInitUploadResponse(this MediaFile entity, string uploadUrl)
        {
            return new InitUploadResponse
            {
                UploadUrl = uploadUrl,
                FileId = entity.Id.ToString()
            };
        }
    }
}
