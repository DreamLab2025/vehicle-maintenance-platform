using Verendar.Common.Databases.Base;

namespace Verendar.Vehicle.Application.Dtos
{
    public class PartCategoryRequest
    {
        public string Name { get; set; } = null!;

        public string Code { get; set; } = null!;

        public string? Description { get; set; }

        public string? IconUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool RequiresOdometerTracking { get; set; } = true;
        public bool RequiresTimeTracking { get; set; } = true;
        public bool AllowsMultipleInstances { get; set; } = false;

        public string? IdentificationSigns { get; set; }

        public string? ConsequencesIfNotHandled { get; set; }
    }

    public class PartCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public EntityStatus Status { get; set; }
        public bool RequiresOdometerTracking { get; set; }
        public bool RequiresTimeTracking { get; set; }
        public bool AllowsMultipleInstances { get; set; }
        public string? IdentificationSigns { get; set; }
        public string? ConsequencesIfNotHandled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PartCategorySummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public EntityStatus Status { get; set; }
    }

    public class PartProductRequest
    {
        public Guid PartCategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? Brand { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public decimal? ReferencePrice { get; set; }

        public int? RecommendedKmInterval { get; set; }

        public int? RecommendedMonthsInterval { get; set; }
    }

    public class PartProductSummary
    {
        public Guid Id { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? ReferencePrice { get; set; }
        public EntityStatus Status { get; set; }
    }

    public class PartProductResponse
    {
        public Guid Id { get; set; }
        public Guid PartCategoryId { get; set; }
        public string PartCategoryName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? ReferencePrice { get; set; }
        public int? RecommendedKmInterval { get; set; }
        public int? RecommendedMonthsInterval { get; set; }
        public EntityStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
