using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend;

public class DevLogEmailService(ILogger<DevLogEmailService> logger) : IResendEmailService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public Task<ResendEmailResponse> SendTemplatedEmailAsync<TModel>(
        string to,
        string templateKey,
        string subject,
        TModel model,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default) where TModel : class
    {
        LogEmail(to, templateKey, subject, JsonSerializer.Serialize(model, _jsonOptions));
        return Task.FromResult(new ResendEmailResponse { IsSuccess = true, MessageId = "dev-log" });
    }

    public Task<ResendEmailResponse> SendTemplatedEmailAsync(
        string to,
        string templateKey,
        string subject,
        object? model = null,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        LogEmail(to, templateKey, subject,
            model is null ? "null" : JsonSerializer.Serialize(model, _jsonOptions));
        return Task.FromResult(new ResendEmailResponse { IsSuccess = true, MessageId = "dev-log" });
    }

    private void LogEmail(string to, string templateKey, string subject, string modelJson)
    {
        logger.LogDebug(
            "[DEV EMAIL] To={To} | Template={Template} | Subject={Subject} | Model={Model}",
            to, templateKey, subject, modelJson);

        if (templateKey == "Otp")
            LogOtp(to, modelJson);
    }

    private void LogOtp(string to, string modelJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(modelJson);
            var root = doc.RootElement;
            var code = root.TryGetProperty("OtpCode", out var c) ? c.GetString() : "?";
            var expires = root.TryGetProperty("ExpiryMinutes", out var e) ? e.GetInt32() : 0;

            logger.LogDebug(">>> [DEV OTP] To={To} | Code={Code} | ExpiresIn={Expires} min <<<",
                to, code, expires);
        }
        catch
        {
            // ignore parse errors in dev helper
        }
    }
}
