namespace Verendar.Media.Application.Mappings
{
    public static class MediaFileMappings
    {
        public static MediaFile ToEntity(this InitUploadRequest request, Guid userId, string fileKey, FileType fileType)
        {
            return new MediaFile
            {
                UserId = userId,
                Provider = request.Provider,
                FileType = fileType,
                FilePath = fileKey,
                OriginalFileName = request.FileName,
                ContentType = request.ContentType,
                Extension = Path.GetExtension(request.FileName),
                Size = request.Size,
                Status = FileStatus.Pending
            };
        }

        public static InitUploadResponse ToInitUploadResponse(this MediaFile entity, string presignedUrl)
        {
            return new InitUploadResponse
            {
                PresignedUrl = presignedUrl,
                FileId = entity.Id.ToString()
            };
        }
    }
}
