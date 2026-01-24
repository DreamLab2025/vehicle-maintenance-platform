namespace Verendar.Media.Application.Configuration
{
    public class FileUploadConfiguration
    {
        public const string SectionName = "FileUpload";

        public Dictionary<string, long> MaxFileSizeByContentType { get; set; } = new()
        {
            // Images - 5MB 
            { "image/jpeg", 5 * 1024 * 1024 },
            { "image/jpg", 5 * 1024 * 1024 },
            { "image/png", 5 * 1024 * 1024 },
            { "image/gif", 5 * 1024 * 1024 },
            { "image/webp", 5 * 1024 * 1024 },
            { "image/bmp", 5 * 1024 * 1024 },
            { "image/svg+xml", 5 * 1024 * 1024 },
            
            // Videos - 20MB 
            { "video/mp4", 20 * 1024 * 1024 },
            { "video/mpeg", 20 * 1024 * 1024 },
            { "video/quicktime", 20 * 1024 * 1024 },
            { "video/x-msvideo", 20 * 1024 * 1024 },
            { "video/webm", 20 * 1024 * 1024 },
            { "video/x-flv", 20 * 1024 * 1024 },
            
            // Documents - 10MB
            { "application/pdf", 10 * 1024 * 1024 }
        };

        public long DefaultMaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

        public List<string> AllowedContentTypes { get; set; } = new()
        {
            // Images
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/bmp",
            "image/svg+xml",
            
            // Videos
            "video/mp4",
            "video/mpeg",
            "video/quicktime",
            "video/x-msvideo",
            "video/webm",
            "video/x-flv",
            
            // Documents
            "application/pdf"
        };

        public long GetMaxFileSize(string contentType)
        {
            if (MaxFileSizeByContentType.TryGetValue(contentType.ToLowerInvariant(), out var maxSize))
            {
                return maxSize;
            }
            return DefaultMaxFileSize;
        }

        public bool IsContentTypeAllowed(string contentType)
        {
            return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
        }
    }
}
