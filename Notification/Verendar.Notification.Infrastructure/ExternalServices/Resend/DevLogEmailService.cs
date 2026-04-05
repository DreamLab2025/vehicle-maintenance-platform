using System.Text.Json;
using Microsoft.Extensions.Logging;
using Verendar.Notification.Application.Services.Interfaces;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend;

public class DevLogEmailService(ILogger<DevLogEmailService> logger) : IResendEmailService
{
    public Task<ResendEmailResponse> SendTemplatedEmailAsync<TModel>(
        string to,
        string templateKey,
        string subject,
        TModel model,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default) where TModel : class
    {
        logger.LogDebug(
            "[DEV EMAIL] To={To} | Subject={Subject} | Template={Template} | Model={Model}",
            to, subject, templateKey,
            JsonSerializer.Serialize(model));

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
        logger.LogDebug(
            "[DEV EMAIL] To={To} | Subject={Subject} | Template={Template} | Model={Model}",
            to, subject, templateKey,
            model is null ? "null" : JsonSerializer.Serialize(model));

        return Task.FromResult(new ResendEmailResponse { IsSuccess = true, MessageId = "dev-log" });
    }
}
