using System.ComponentModel;

namespace Verendar.Vehicle.Domain.Enums
{
    public enum OdometerSource
    {
        [Description("Nhập thủ công")]
        ManualInput = 1,

        [Description("Từ GPS")]
        GPS = 2,

        [Description("Từ hồ sơ bảo dưỡng")]
        ServiceRecord = 3
    }
}
