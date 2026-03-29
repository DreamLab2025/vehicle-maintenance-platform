namespace Verendar.Garage.Domain.ValueObjects;

public class WorkingHours
{
    public Dictionary<DayOfWeek, DaySchedule> Schedule { get; set; } = [];
}

public class DaySchedule
{
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; }
}
