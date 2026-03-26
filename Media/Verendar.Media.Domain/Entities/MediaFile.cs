namespace Verendar.Media.Domain.Entities
{
    [Index(nameof(UserId))]
    [Index(nameof(FileType))]
    public class MediaFile : BaseEntity
    {
        public Guid UserId { get; set; }

        public StorageProvider Provider { get; set; }

        public FileType FileType { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string OriginalFileName { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string ContentType { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Extension { get; set; } = null!;

        public long Size { get; set; }

        public FileStatus Status { get; set; } = FileStatus.Pending;

        /// <summary>Full public URL (CloudFront or S3). Populated on upload confirm; null while Pending or after deletion.</summary>
        [MaxLength(2000)]
        public string? PublicPath { get; set; }

        [Column(TypeName = "jsonb")]
        public string? Metadata { get; set; }
    }

    public enum FileStatus
    {
        Pending,
        Uploaded,
        Failed,
        Deleted
    }

    public enum StorageProvider
    {
        AwsS3 = 1,
        Firebase = 2
    }

    public enum FileType
    {
        Avatar = 1,
        VehicleType = 2,
        VehicleBrand = 3,
        VehicleVariant = 4,
        PartCategory = 5,
        Other = 99
    }
}