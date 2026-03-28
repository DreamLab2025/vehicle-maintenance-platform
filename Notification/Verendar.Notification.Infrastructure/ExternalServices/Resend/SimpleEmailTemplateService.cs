using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Verendar.Notification.Application.Dtos.Email;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Infrastructure.Configuration;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend;

public class SimpleEmailTemplateService(
    ILogger<SimpleEmailTemplateService> logger,
    IOptions<ResendOptions> options,
    IWebHostEnvironment environment) : IEmailTemplateService
{
    private readonly ILogger<SimpleEmailTemplateService> _logger = logger;
    private readonly ResendOptions _options = options.Value;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly Dictionary<string, string> _templateCache = new();
    private readonly object _cacheLock = new();

    public Task<string> RenderTemplateAsync<TModel>(
        string templateKey,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class
    {
        return RenderTemplateAsync(templateKey, (object?)model, cancellationToken);
    }

    public async Task<string> RenderTemplateAsync(
        string templateKey,
        object? model = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templateHtml = await GetTemplateContentAsync(templateKey);

            if (model == null)
                return templateHtml;

            return model switch
            {
                OtpEmailModel otpModel => RenderOtpTemplate(templateHtml, otpModel),
                NotificationEmailModel notificationModel => RenderNotificationTemplate(templateHtml, notificationModel),
                MemberAccountCreatedEmailModel memberModel => RenderMemberAccountCreatedTemplate(templateHtml, memberModel),
                _ => ReplaceCommonPlaceholders(templateHtml, model)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateKey}", templateKey);
            throw;
        }
    }

    public Task<bool> TemplateExistsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var fileName = $"{templateKey}.html";
        foreach (var dir in GetTemplateSearchDirectories())
        {
            if (File.Exists(Path.Combine(dir, fileName)))
                return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _templateCache.Clear();
        }
    }

    private async Task<string> GetTemplateContentAsync(string templateKey)
    {
        lock (_cacheLock)
        {
            if (_templateCache.TryGetValue(templateKey, out var cachedContent))
                return cachedContent;
        }

        var templatePath = ResolveTemplatePath(templateKey);
        var content = await File.ReadAllTextAsync(templatePath);

        lock (_cacheLock)
        {
            _templateCache[templateKey] = content;
        }

        return content;
    }

    private static string RenderOtpTemplate(string template, OtpEmailModel model)
    {
        return template
            .Replace("{{UserName}}", model.UserName ?? "bạn")
            .Replace("{{OtpCode}}", model.OtpCode)
            .Replace("{{ExpiryMinutes}}", model.ExpiryMinutes.ToString())
            .Replace("{{ExpiryTime}}", model.ExpiryTime.ToString("HH:mm dd/MM/yyyy"))
            .Replace("{{Year}}", DateTime.Now.Year.ToString());
    }

    private static string RenderNotificationTemplate(string template, NotificationEmailModel model)
    {
        var actionButton = !string.IsNullOrEmpty(model.CtaUrl) && !string.IsNullOrEmpty(model.CtaText)
            ? $"<p style=\"text-align: center; margin-top: 24px;\"><a href=\"{model.CtaUrl}\" class=\"button\">{model.CtaText}</a></p>"
            : string.Empty;

        return template
            .Replace("{{Title}}", model.Title)
            .Replace("{{UserName}}", model.UserName ?? "bạn")
            .Replace("{{Message}}", model.Message)
            .Replace("{{ActionButton}}", actionButton)
            .Replace("{{Year}}", DateTime.Now.Year.ToString());
    }

    private static string RenderMemberAccountCreatedTemplate(string template, MemberAccountCreatedEmailModel model)
    {
        var actionButton = !string.IsNullOrEmpty(model.CtaUrl) && !string.IsNullOrEmpty(model.CtaText)
            ? $"<p style=\"text-align: center; margin-top: 24px;\"><a href=\"{WebUtility.HtmlEncode(model.CtaUrl)}\" class=\"button\">{WebUtility.HtmlEncode(model.CtaText)}</a></p>"
            : string.Empty;

        return template
            .Replace("{{Title}}", WebUtility.HtmlEncode(model.Title))
            .Replace("{{UserName}}", WebUtility.HtmlEncode(string.IsNullOrEmpty(model.UserName) ? "bạn" : model.UserName))
            .Replace("{{DisplayName}}", WebUtility.HtmlEncode(model.DisplayName))
            .Replace("{{Role}}", WebUtility.HtmlEncode(model.Role))
            .Replace("{{TempPassword}}", WebUtility.HtmlEncode(model.TempPassword))
            .Replace("{{ActionButton}}", actionButton)
            .Replace("{{Year}}", DateTime.Now.Year.ToString());
    }

    private static string ReplaceCommonPlaceholders(string template, object model)
    {
        var result = template;
        foreach (var prop in model.GetType().GetProperties())
        {
            var value = prop.GetValue(model);
            var placeholder = $"{{{{{prop.Name}}}}}";
            result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
        }

        return result.Replace("{{Year}}", DateTime.Now.Year.ToString());
    }

    private string ResolveTemplatePath(string templateKey)
    {
        var fileName = $"{templateKey}.html";
        foreach (var dir in GetTemplateSearchDirectories())
        {
            var path = Path.Combine(dir, fileName);
            if (File.Exists(path))
                return path;
        }

        var searched = string.Join(", ", GetTemplateSearchDirectories());
        throw new FileNotFoundException($"Email template not found: {fileName}. Searched directories: {searched}");
    }

    private IEnumerable<string> GetTemplateSearchDirectories()
    {
        var assemblyDir = GetInfrastructureAssemblyDirectory();
        if (!string.IsNullOrEmpty(assemblyDir))
        {
            var resendEmail = Path.Combine(assemblyDir, "ExternalServices", "Resend", "Templates", "Email");
            if (Directory.Exists(resendEmail))
                yield return resendEmail;
        }

        var basePath = _environment.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;
        yield return Path.Combine(basePath, _options.TemplateBasePath);
    }

    private static string? GetInfrastructureAssemblyDirectory()
    {
        var location = typeof(SimpleEmailTemplateService).Assembly.Location;
        return !string.IsNullOrEmpty(location) ? Path.GetDirectoryName(location) : AppContext.BaseDirectory;
    }
}
