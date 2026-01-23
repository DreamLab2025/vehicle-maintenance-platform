using System.ComponentModel;

namespace Verendar.Vehicle.Domain.Enums
{
    /// <summary>
    /// Mức độ nhắc nhở bảo dưỡng dựa trên % còn lại đến hạn
    /// </summary>
    public enum ReminderLevel
    {
        [Description("Thấp")]
        Low = 0,        // >50% remaining

        [Description("Trung bình")]
        Medium = 1,     // 25-50% remaining

        [Description("Cao")]
        High = 2,       // 10-25% remaining

        [Description("Khẩn cấp")]
        Urgent = 3      // <10% remaining
    }
}
