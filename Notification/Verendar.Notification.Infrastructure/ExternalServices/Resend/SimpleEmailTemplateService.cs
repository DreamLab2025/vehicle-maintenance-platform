using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using Verendar.Notification.Application.Dtos.Email;
using Verendar.Notification.Application.Services.Interfaces;
using Verendar.Notification.Infrastructure.Configuration;

namespace Verendar.Notification.Infrastructure.ExternalServices.Resend
{
    public class SimpleEmailTemplateService : IEmailTemplateService
    {
        private readonly ILogger<SimpleEmailTemplateService> _logger;
        private readonly ResendOptions _options;
        private readonly IWebHostEnvironment _environment;
        private readonly Dictionary<string, string> _templateCache = new();
        private readonly object _cacheLock = new();

        public SimpleEmailTemplateService(
            ILogger<SimpleEmailTemplateService> logger,
            IOptions<ResendOptions> options,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _options = options.Value;
            _environment = environment;
        }

        public Task<string> RenderTemplateAsync<TModel>(
            string templateKey,
            TModel model,
            CancellationToken cancellationToken = default) where TModel : class
        {
            return RenderTemplateAsync(templateKey, model, cancellationToken);
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
                {
                    return templateHtml;
                }

                // Replace placeholders based on model type
                var result = model switch
                {
                    OtpEmailModel otpModel => RenderOtpTemplate(templateHtml, otpModel),
                    WelcomeEmailModel welcomeModel => RenderWelcomeTemplate(templateHtml, welcomeModel),
                    PasswordResetEmailModel resetModel => RenderPasswordResetTemplate(templateHtml, resetModel),
                    MaintenanceReminderEmailModel maintenanceModel => RenderMaintenanceReminderTemplate(templateHtml, maintenanceModel),
                    OdometerReminderEmailModel odometerModel => RenderOdometerReminderTemplate(templateHtml, odometerModel),
                    NotificationEmailModel notificationModel => RenderNotificationTemplate(templateHtml, notificationModel),
                    _ => ReplaceCommonPlaceholders(templateHtml, model)
                };

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
            lock (_cacheLock)
            {
                _templateCache.Clear();
            }
        }

        private async Task<string> GetTemplateContentAsync(string templateKey)
        {
            // Check cache first
            lock (_cacheLock)
            {
                if (_templateCache.TryGetValue(templateKey, out var cachedContent))
                {
                    return cachedContent;
                }
            }

            var templatePath = GetTemplatePath(templateKey);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template not found: {templatePath}");
            }

            var content = await File.ReadAllTextAsync(templatePath);

            // Cache the content
            lock (_cacheLock)
            {
                _templateCache[templateKey] = content;
            }

            return content;
        }

        private string RenderOtpTemplate(string template, OtpEmailModel model)
        {
            return template
                .Replace("{{UserName}}", model.UserName ?? "bạn")
                .Replace("{{OtpCode}}", model.OtpCode)
                .Replace("{{OtpType}}", model.OtpType)
                .Replace("{{ExpiryMinutes}}", model.ExpiryMinutes.ToString())
                .Replace("{{ExpiryTime}}", model.ExpiryTime.ToString("HH:mm dd/MM/yyyy"))
                .Replace("{{Year}}", DateTime.Now.Year.ToString());
        }

        private string RenderWelcomeTemplate(string template, WelcomeEmailModel model)
        {
            var welcomeMessage = !string.IsNullOrEmpty(model.WelcomeMessage)
                ? $"<p>{model.WelcomeMessage}</p>"
                : string.Empty;

            return template
                .Replace("{{FullName}}", model.FullName)
                .Replace("{{RegistrationDate}}", model.RegistrationDate.ToString("dd/MM/yyyy"))
                .Replace("{{WelcomeMessage}}", welcomeMessage)
                .Replace("{{Year}}", DateTime.Now.Year.ToString());
        }

        private string RenderPasswordResetTemplate(string template, PasswordResetEmailModel model)
        {
            return template
                .Replace("{{UserName}}", model.UserName ?? "bạn")
                .Replace("{{ResetUrl}}", model.ResetUrl)
                .Replace("{{ExpiryMinutes}}", model.ExpiryMinutes.ToString())
                .Replace("{{ExpiryTime}}", model.ExpiryTime.ToString("HH:mm dd/MM/yyyy"))
                .Replace("{{Year}}", DateTime.Now.Year.ToString());
        }

        private string RenderMaintenanceReminderTemplate(string template, MaintenanceReminderEmailModel model)
        {
            var criticalMessage = model.IsCritical
                ? "<p style=\"color: #dc3545; font-weight: 600;\">Xe của bạn có linh kiện đã đến mức <strong>khẩn cấp</strong> cần thay thế. Bạn sẽ nhận được email nhắc nhở hằng ngày cho đến khi bạn cập nhật đã thay linh kiện.</p>"
                : $"<p>Xe của bạn có linh kiện cần chú ý bảo dưỡng/thay thế (<strong>{model.LevelName}</strong>).</p>";

            var itemsHtml = new StringBuilder();
            foreach (var item in model.Items)
            {
                itemsHtml.AppendLine("<tr>");
                itemsHtml.AppendLine($"    <td>{item.PartCategoryName}</td>");
                itemsHtml.AppendLine($"    <td>{item.VehicleDisplayName ?? "—"}</td>");
                itemsHtml.AppendLine($"    <td class=\"text-right\">{item.CurrentOdometer.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} km</td>");
                itemsHtml.AppendLine($"    <td class=\"text-right\">{item.TargetOdometer.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} km</td>");
                itemsHtml.AppendLine($"    <td class=\"text-center\">{item.PercentageRemaining.ToString("F0")}%</td>");
                itemsHtml.AppendLine("</tr>");
            }

            var actionButton = !string.IsNullOrEmpty(model.ActionUrl) && !string.IsNullOrEmpty(model.ActionText)
                ? $"<p style=\"text-align: center; margin-top: 24px;\"><a href=\"{model.ActionUrl}\" class=\"button\">{model.ActionText}</a></p>"
                : string.Empty;

            return template
                .Replace("{{Title}}", model.Title)
                .Replace("{{UserName}}", string.IsNullOrEmpty(model.UserName) ? "bạn" : model.UserName)
                .Replace("{{CriticalMessage}}", criticalMessage)
                .Replace("{{MaintenanceItems}}", itemsHtml.ToString())
                .Replace("{{ActionButton}}", actionButton)
                .Replace("{{Year}}", DateTime.Now.Year.ToString());
        }

        private string RenderOdometerReminderTemplate(string template, OdometerReminderEmailModel model)
        {
            var itemsHtml = new StringBuilder();
            foreach (var vehicle in model.Vehicles)
            {
                itemsHtml.AppendLine("<tr>");
                itemsHtml.AppendLine($"    <td>{vehicle.VehicleDisplayName}</td>");
                itemsHtml.AppendLine($"    <td>{vehicle.LicensePlate ?? "—"}</td>");
                itemsHtml.AppendLine($"    <td class=\"text-right\">{vehicle.CurrentOdometer.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} km</td>");
                itemsHtml.AppendLine($"    <td>{vehicle.LastOdometerUpdateFormatted ?? "Chưa có"}</td>");
                itemsHtml.AppendLine($"    <td class=\"text-center\">{vehicle.DaysSinceUpdate} ngày</td>");
                itemsHtml.AppendLine("</tr>");
            }

            var actionButton = !string.IsNullOrEmpty(model.ActionUrl) && !string.IsNullOrEmpty(model.ActionText)
                ? $"<p style=\"text-align: center; margin-top: 24px;\"><a href=\"{model.ActionUrl}\" class=\"button\">{model.ActionText}</a></p>"
                : string.Empty;

            return template
                .Replace("{{Title}}", model.Title)
                .Replace("{{UserName}}", string.IsNullOrEmpty(model.UserName) ? "bạn" : model.UserName)
                .Replace("{{StaleOdometerDays}}", model.StaleOdometerDays.ToString())
                .Replace("{{VehicleItems}}", itemsHtml.ToString())
                .Replace("{{ActionButton}}", actionButton)
                .Replace("{{Year}}", DateTime.Now.Year.ToString());
        }

        private string RenderNotificationTemplate(string template, NotificationEmailModel model)
        {
            var actionButton = !string.IsNullOrEmpty(model.ActionUrl) && !string.IsNullOrEmpty(model.ActionText)
                ? $"<p style=\"text-align: center; margin-top: 24px;\"><a href=\"{model.ActionUrl}\" class=\"button\">{model.ActionText}</a></p>"
                : string.Empty;

            return template
                .Replace("{{Title}}", model.Title)
                .Replace("{{UserName}}", model.UserName ?? "bạn")
                .Replace("{{Message}}", model.Message)
                .Replace("{{ActionButton}}", actionButton)
                .Replace("{{Year}}", DateTime.Now.Year.ToString());
        }

        private string ReplaceCommonPlaceholders(string template, object model)
        {
            var result = template;
            var properties = model.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(model);
                var placeholder = $"{{{{{prop.Name}}}}}";
                var replacement = value?.ToString() ?? string.Empty;

                result = result.Replace(placeholder, replacement);
            }

            result = result.Replace("{{Year}}", DateTime.Now.Year.ToString());

            return result;
        }

        private string GetTemplatePath(string templateKey)
        {
            var templateDirectory = GetTemplateDirectory();
            return Path.Combine(templateDirectory, $"{templateKey}.html");
        }

        private string GetTemplateDirectory()
        {
            var basePath = _environment.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, _options.TemplateBasePath);
        }
    }
}
