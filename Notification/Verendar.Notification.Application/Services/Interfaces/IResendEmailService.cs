namespace Verendar.Notification.Application.Services.Interfaces
{
    public interface IResendEmailService
    {
        Task<ResendEmailResponse> SendTemplatedEmailAsync<TModel>(
        string to,
        string templateKey,
        string subject,
        TModel model,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default) where TModel : class;


        Task<ResendEmailResponse> SendTemplatedEmailAsync(
            string to,
            string templateKey,
            string subject,
            object? model = null,
            string? from = null,
            string? replyTo = null,
            CancellationToken cancellationToken = default);
    }

    public class ResendEmailResponse
    {
        public bool IsSuccess { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
