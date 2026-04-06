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
    public Guid OwnerUserId { get; set; }
    public List<Guid> ManagerUserIds { get; set; } = [];
    public int? CurrentOdometer { get; set; }
    public DateTime CompletedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BookingCompletedLineItem> LineItems { get; set; } = [];
}

public class BookingCompletedLineItem
{
    public Guid? GarageProductId { get; set; }

    public Guid? GarageServiceId { get; set; }

    public Guid? PartCategoryId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public bool UpdatesTracking { get; set; }

    public decimal Price { get; set; }

    public int? RecommendedKmInterval { get; set; }

    public int? RecommendedMonthsInterval { get; set; }
}
