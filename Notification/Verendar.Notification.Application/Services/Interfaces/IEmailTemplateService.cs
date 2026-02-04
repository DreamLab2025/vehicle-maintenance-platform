namespace Verendar.Notification.Application.Services.Interfaces;


public interface IEmailTemplateService
{
    Task<string> RenderTemplateAsync<TModel>(
        string templateKey,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class;

   
    Task<string> RenderTemplateAsync(
        string templateKey,
        object? model = null,
        CancellationToken cancellationToken = default);

   
    Task<bool> TemplateExistsAsync(string templateKey, CancellationToken cancellationToken = default);

    void ClearCache();
}
