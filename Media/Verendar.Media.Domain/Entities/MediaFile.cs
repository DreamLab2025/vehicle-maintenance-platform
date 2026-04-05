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

        // Vehicle
        MaintenanceInvoice = 6,

        // Garage
        GarageLogo = 10,
        GarageBranchCover = 11,
        GarageServiceImage = 12,
        GarageProductImage = 13,
        GarageBundleImage = 14,
        ServiceCategoryIcon = 15,

        Other = 99
    }
}