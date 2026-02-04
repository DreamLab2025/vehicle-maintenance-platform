using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RazorLight;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Infrastructure.Configuration;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend;

public class RazorEmailTemplateService : IEmailTemplateService
{
    private readonly ILogger<RazorEmailTemplateService> _logger;
    private readonly ResendOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly RazorLightEngine _razorEngine;

    public RazorEmailTemplateService(
        ILogger<RazorEmailTemplateService> logger,
        IOptions<ResendOptions> options,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _options = options.Value;
        _environment = environment;

        var templateDirectory = GetTemplateDirectory();

        // Ensure template directory exists
        if (!Directory.Exists(templateDirectory))
        {
            Directory.CreateDirectory(templateDirectory);
            _logger.LogWarning("Template directory created at {TemplateDirectory}", templateDirectory);
        }

        // Initialize RazorLight engine
        _razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templateDirectory)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderTemplateAsync<TModel>(
        string templateKey,
        TModel model,
        CancellationToken cancellationToken = default) where TModel : class
    {
        try
        {
            var templateFile = $"{templateKey}.cshtml";
            var templatePath = GetTemplatePath(templateKey);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            _logger.LogDebug("Rendering template {TemplateKey} with model type {ModelType}",
                templateKey, typeof(TModel).Name);

            return await _razorEngine.CompileRenderAsync(templateFile, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<string> RenderTemplateAsync(
        string templateKey,
        object? model = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var templateFile = $"{templateKey}.cshtml";
            var templatePath = GetTemplatePath(templateKey);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            return await _razorEngine.CompileRenderAsync(templateFile, model ?? new { });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateKey}", templateKey);
            throw;
        }
    }

    public Task<bool> TemplateExistsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var templatePath = GetTemplatePath(templateKey);
        return Task.FromResult(File.Exists(templatePath));
    }

    public void ClearCache()
    {
        // Không cache render email — không cần xóa cache.
    }

    private string GetTemplatePath(string templateKey)
    {
        var templateDirectory = GetTemplateDirectory();
        return Path.Combine(templateDirectory, $"{templateKey}.cshtml");
    }

    private string GetTemplateDirectory()
    {
        var basePath = _environment.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(basePath, _options.TemplateBasePath);
    }
}
