namespace Verendar.Notification.Application.Options;

public class NotificationAppOptions
{
    public const string SectionName = "NotificationApp";

    public string BaseUrl { get; set; } = "https://app.verendar.vn";

    public string LoginPath { get; set; } = "/login";

    public string BookingDetailPathFormat { get; set; } = "/bookings/{0}";

    public string UserVehicleMaintenancePathFormat { get; set; } = "/user-vehicles/{0}/maintenance-records";

    public string UserVehicleOdometerPathFormat { get; set; } = "/user-vehicles/{0}/odometer";

    public string UserVehiclesFallbackPath { get; set; } = "/user-vehicles";

    public string BookingDetailRelativeUrl(Guid bookingId) =>
        string.Format(BookingDetailPathFormat, bookingId);

    public string UserVehicleMaintenanceRelativeUrl(Guid userVehicleId) =>
        string.Format(UserVehicleMaintenancePathFormat, userVehicleId);

    public string UserVehicleOdometerRelativeUrl(Guid userVehicleId) =>
        string.Format(UserVehicleOdometerPathFormat, userVehicleId);

    public string ToAbsoluteUrl(string relativeOrAbsolutePath)
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
            return BaseUrl.TrimEnd('/');
        if (relativeOrAbsolutePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return relativeOrAbsolutePath;
        var b = BaseUrl.TrimEnd('/');
        var p = relativeOrAbsolutePath.StartsWith('/') ? relativeOrAbsolutePath : "/" + relativeOrAbsolutePath;
        return b + p;
    }

    public string LoginAbsoluteUrl() => ToAbsoluteUrl(LoginPath);
}
