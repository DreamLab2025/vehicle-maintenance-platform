using Verendar.Common.Contracts;

namespace Verendar.Vehicle.Contracts.Events;

public class MaintenanceReminderEvent : BaseEvent
{
    public override string EventType => "vehicle.maintenance.reminder.v1";

    public Guid UserId { get; set; }

    public string? TargetValue { get; set; }

    /// <summary>Tên hiển thị người dùng (từ Identity), dùng trong email.</summary>
    public string? UserName { get; set; }

    public int Level { get; set; }

    public string LevelName { get; set; } = string.Empty;

    public List<MaintenanceReminderItemDto> Items { get; set; } = [];
}

public class MaintenanceReminderItemDto
{
    public string PartCategoryName { get; set; } = string.Empty;
    public Guid UserVehicleId { get; set; }
    public Guid ReminderId { get; set; }
    public int CurrentOdometer { get; set; }
    public int TargetOdometer { get; set; }
    /// <summary>Phần trăm còn lại trước khi đến hạn (0–100).</summary>
    public decimal PercentageRemaining { get; set; }
    /// <summary>Ví dụ: "Wave Alpha - 59-TEST-01".</summary>
    public string? VehicleDisplayName { get; set; }
}
