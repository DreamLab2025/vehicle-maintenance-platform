using Verendar.Notification.Application.Dtos.Email;

namespace Verendar.Notification.Application.Services.Interfaces;

public interface IResendEmailService
{
    /// <summary>
    /// Sends a templated email using Razor template
    /// </summary>
    Task<ResendEmailResponse> SendTemplatedEmailAsync<TModel>(
        string to,
        string templateKey,
        string subject,
        TModel model,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default) where TModel : class;

    /// <summary>
    /// Sends a templated email with dynamic model
    /// </summary>
    Task<ResendEmailResponse> SendTemplatedEmailAsync(
        string to,
        string templateKey,
        string subject,
        object? model = null,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default);
}

public class EmailAttachment
{
    public string Filename { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}

public class ResendEmailResponse
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
