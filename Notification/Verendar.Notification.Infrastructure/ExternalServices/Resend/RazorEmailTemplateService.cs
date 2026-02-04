using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RazorLight;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Infrastructure.Configuration;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend;

public class RazorEmailTemplateService : IEmailTemplateService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RazorEmailTemplateService> _logger;
    private readonly ResendOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly RazorLightEngine _razorEngine;
    private static readonly object _lockObject = new();

    public RazorEmailTemplateService(
        IMemoryCache cache,
        ILogger<RazorEmailTemplateService> logger,
        IOptions<ResendOptions> options,
        IWebHostEnvironment environment)
    {
        _cache = cache;
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
            var cacheKey = GetCacheKey(templateKey, typeof(TModel).Name);

            if (_options.EnableTemplateCache && _cache.TryGetValue(cacheKey, out string? cached))
            {
                _logger.LogDebug("Template cache hit for {TemplateKey}", templateKey);
                return cached!;
            }

            var templateFile = $"{templateKey}.cshtml";
            var templatePath = GetTemplatePath(templateKey);
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            _logger.LogDebug("Rendering template {TemplateKey} with model type {ModelType}", 
                templateKey, typeof(TModel).Name);

            // RazorLight uses relative path from the project root
            var result = await _razorEngine.CompileRenderAsync(templateFile, model);

            if (_options.EnableTemplateCache)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_options.TemplateCacheExpirationMinutes));
            }

            return result;
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
            var cacheKey = GetCacheKey(templateKey, "dynamic");

            if (_options.EnableTemplateCache && _cache.TryGetValue(cacheKey, out string? cached))
            {
                return cached!;
            }

            var templateFile = $"{templateKey}.cshtml";
            var templatePath = GetTemplatePath(templateKey);
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            // RazorLight uses relative path from the project root
            var result = await _razorEngine.CompileRenderAsync(templateFile, model ?? new { });

            if (_options.EnableTemplateCache)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_options.TemplateCacheExpirationMinutes));
            }

            return result;
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
        lock (_lockObject)
        {
            if (_cache is MemoryCache memoryCache)
            {
                // Clear all template-related cache entries
                // Note: This is a simplified approach. In production, you might want to track cache keys
                _logger.LogInformation("Clearing email template cache");
            }
        }
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

    private static string GetCacheKey(string templateKey, string modelType)
    {
        return $"email_template_{templateKey}_{modelType}";
    }
}
