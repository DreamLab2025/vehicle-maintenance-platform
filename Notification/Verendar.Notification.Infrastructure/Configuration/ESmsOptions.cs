namespace Verendar.Notification.Application.Dtos.ESms;

public class ESmsOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://rest.esms.vn/MainService.svc/json";
    public string BrandName { get; set; } = "Verendar"; // Tên thương hiệu hiển thị
    public int SmsType { get; set; } = 2; // 2: Brandname, 4: Brandname OTP, 8: Zalo ZNS
    public int Timeout { get; set; } = 30; // seconds
}
