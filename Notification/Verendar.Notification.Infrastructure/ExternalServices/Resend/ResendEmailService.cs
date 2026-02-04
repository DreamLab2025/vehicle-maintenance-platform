using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Infrastructure.Configuration;
using ApplicationEmailAttachment = Verendar.Notification.Application.Services.Interfaces.EmailAttachment;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend;

public class ResendEmailService : IResendEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly ResendOptions _options;
    private readonly IEmailTemplateService _templateService;
    private const string ResendApiUrl = "https://api.resend.com/emails";

    public ResendEmailService(
        HttpClient httpClient,
        ILogger<ResendEmailService> logger,
        IOptions<ResendOptions> options,
        IEmailTemplateService templateService)
    {
        _logger = logger;
        _options = options.Value;
        _templateService = templateService;
        
        // Validate configuration
        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            throw new InvalidOperationException("Resend FromEmail is not configured. Please set it in appsettings.json");
        }
        
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Resend ApiKey is not configured. Please set it in appsettings.json");
        }
        
        // Log configuration on startup
        _logger.LogInformation("Resend Email Service initialized. FromEmail: {FromEmail}, ApiKey: {ApiKeyPrefix}...", 
            _options.FromEmail, 
            _options.ApiKey.Length > 10 ? _options.ApiKey.Substring(0, 10) + "..." : "***");
        
        // HttpClient is already configured via AddHttpClient in DI
        // Just set the base address and auth header
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.resend.com");
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.Timeout);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
    }

    public async Task<ResendEmailResponse> SendTemplatedEmailAsync<TModel>(
        string to,
        string templateKey,
        string subject,
        TModel model,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default) where TModel : class
    {
        try
        {
            _logger.LogInformation("Sending templated email to {To}, Template: {TemplateKey}, Subject: {Subject}", 
                to, templateKey, subject);
            
            var htmlContent = await _templateService.RenderTemplateAsync(templateKey, model, cancellationToken);
            return await SendEmailInternalAsync(to, subject, htmlContent, from, replyTo, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email. Template: {TemplateKey}, To: {To}", templateKey, to);
            return CreateErrorResponse($"Template rendering failed: {ex.Message}");
        }
    }

    public async Task<ResendEmailResponse> SendTemplatedEmailAsync(
        string to,
        string templateKey,
        string subject,
        object? model = null,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending templated email to {To}, Template: {TemplateKey}, Subject: {Subject}", 
                to, templateKey, subject);
            
            var htmlContent = await _templateService.RenderTemplateAsync(templateKey, model, cancellationToken);
            return await SendEmailInternalAsync(to, subject, htmlContent, from, replyTo, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email. Template: {TemplateKey}, To: {To}", templateKey, to);
            return CreateErrorResponse($"Template rendering failed: {ex.Message}");
        }
    }

    private async Task<ResendEmailResponse> SendEmailInternalAsync(
        string to,
        string subject,
        string htmlContent,
        string? from = null,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use only the email address (without FromName) to avoid unverified domain issues
            // Resend requires the exact verified domain format
            var fromEmail = from ?? _options.FromEmail;
            
            // Log the from email being used for debugging
            _logger.LogInformation("Sending email from: {FromEmail}, to: {To}", fromEmail, to);
            
            var replyToValue = replyTo ?? _options.ReplyTo;
            var requestBody = new Dictionary<string, object>
            {
                { "from", fromEmail }, // Use only email, not "Name <email>" format
                { "to", new[] { to } },
                { "subject", subject },
                { "html", htmlContent }
            };

            if (!string.IsNullOrEmpty(replyToValue))
            {
                requestBody["reply_to"] = replyToValue;
            }

            _logger.LogDebug("Resend API Request: From={FromEmail}, To={To}, Subject={Subject}", 
                fromEmail, to, subject);

            var response = await _httpClient.PostAsJsonAsync("/emails", requestBody, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            _logger.LogDebug("Resend API Response: Status={StatusCode}, Content={Content}", 
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ResendApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return CreateSuccessResponse(result?.Id);
            }

            var errorResult = JsonSerializer.Deserialize<ResendApiError>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return CreateErrorResponse(errorResult?.Message ?? $"HTTP {response.StatusCode}: {responseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {To}", to);
            return CreateErrorResponse(ex.Message);
        }
    }

    private class ResendApiResponse
    {
        public string? Id { get; set; }
    }

    private class ResendApiError
    {
        public string? Message { get; set; }
    }

    private static ResendEmailResponse CreateSuccessResponse(string? messageId)
    {
        return new ResendEmailResponse
        {
            IsSuccess = true,
            MessageId = messageId
        };
    }

    private static ResendEmailResponse CreateErrorResponse(string errorMessage)
    {
        return new ResendEmailResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
