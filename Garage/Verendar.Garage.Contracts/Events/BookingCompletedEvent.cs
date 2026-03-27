using Verendar.Common.Contracts;

namespace Verendar.Garage.Contracts.Events;

public class BookingCompletedEvent : BaseEvent
{
    public override string EventType => "garage.booking.completed.v1";

    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid UserVehicleId { get; set; }
    public Guid GarageBranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int? CurrentOdometer { get; set; }
    public DateTime CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BookingCompletedLineItem> LineItems { get; set; } = [];
}

/// <summary>
/// Một dòng item đã được flatten từ booking tại thời điểm hoàn tất.
/// Bundle sẽ được expand thành nhiều items con để Vehicle service có thể cập nhật PartTracking.
/// </summary>
public class BookingCompletedLineItem
{
    /// <summary>ID của GarageProduct (phụ tùng). Null nếu item là GarageService thuần.</summary>
    public Guid? GarageProductId { get; set; }

    /// <summary>ID của GarageService (dịch vụ). Null nếu item là GarageProduct thuần.</summary>
    public Guid? GarageServiceId { get; set; }

    public Guid? PartCategoryId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    /// <summary>true nếu item này cần cập nhật PartTracking trong Vehicle service.</summary>
    public bool UpdatesTracking { get; set; }

    public decimal Price { get; set; }

    /// <summary>Chu kỳ thay thế theo km từ GarageProduct.ManufacturerKmInterval. Null nếu không có.</summary>
    public int? RecommendedKmInterval { get; set; }

    /// <summary>Chu kỳ thay thế theo tháng từ GarageProduct.ManufacturerMonthInterval. Null nếu không có.</summary>
    public int? RecommendedMonthsInterval { get; set; }
}
