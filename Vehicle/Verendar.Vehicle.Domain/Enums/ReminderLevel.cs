using System.ComponentModel;

namespace Verendar.Vehicle.Domain.Enums
{
    public enum ReminderLevel
    {
        [Description("Thấp")]
        Low = 0,

        [Description("Trung bình")]
        Medium = 1,

        [Description("Cao")]
        High = 2,

        [Description("Khẩn cấp")]
        Urgent = 3
    }
}
