namespace Verendar.Ai.Infrastructure.ExternalServices
{
    internal static class AiExternalServiceMessages
    {
        public const string ImageUrlDownloadFailed = "Không thể tải ảnh từ URL đã cung cấp.";
        public const string ImageUrlInvalidOrUnreachable = "Không thể tải ảnh. Vui lòng kiểm tra URL ảnh.";
        public const string UnexpectedAiError = "An unexpected error occurred while calling AI service";
        public const string EmptyAiResponse = "AI service returned empty response";
        public const string AiRequestTimeout = "AI service request timeout";
        public const string AiNotConfigured = "AI service is not properly configured";
    }
}
