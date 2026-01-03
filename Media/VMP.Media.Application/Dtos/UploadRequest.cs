using VMP.Media.Domain.Entities;

namespace VMP.Media.Application.Dtos
{
    public class InitUploadRequest
    {
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Size { get; set; }
        public StorageProvider Provider { get; set; } = StorageProvider.AwsS3;
        public FileType FileType { get; set; } = FileType.Other;
    }

    public class InitUploadResponse
    {
        public string UploadUrl { get; set; } = null!;
        public string FileId { get; set; } = null!;
        public string FilePath { get; set; } = null!;
    }
}
