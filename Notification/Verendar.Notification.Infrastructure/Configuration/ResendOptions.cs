namespace Verendar.Notification.Infrastructure.Configuration
{
    public class ResendOptions
    {
        public const string SectionName = "Email:Resend";

        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Verendar";
        public string? ReplyTo { get; set; }
        public int Timeout { get; set; } = 30; // seconds
        public bool EnableTracking { get; set; } = true;
    
        // Template settings
        public string TemplateBasePath { get; set; } = "Templates/Email";
        public bool EnableTemplateCache { get; set; } = true;
        public int TemplateCacheExpirationMinutes { get; set; } = 60;
        public bool UseDatabaseTemplates { get; set; } = false; // If true, load from DB; if false, use file system
    }
}
