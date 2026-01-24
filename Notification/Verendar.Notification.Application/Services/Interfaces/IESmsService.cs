using Verendar.Notification.Application.Dtos.ESms;

namespace Verendar.Notification.Application.Services.Interfaces;

public interface IESmsService
{
    Task<ESmsResponse> SendSmsAsync(string phoneNumber, string content, string? requestId = null);
    Task<ESmsResponse> SendOtpAsync(string phoneNumber, string otpCode, string? requestId = null);
    Task<ESmsResponse> SendZaloZnsAsync(string phoneNumber, string templateId, Dictionary<string, string> parameters, string? requestId = null);
    Task<ESmsBalanceResponse> GetBalanceAsync();
}
