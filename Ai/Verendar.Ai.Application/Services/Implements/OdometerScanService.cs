using Verendar.Ai.Application.Dtos.OdometerScan;

namespace Verendar.Ai.Application.Services.Implements
{
    public class OdometerScanService(
        IAiPromptService promptService,
        IGenerativeAiServiceFactory generativeAiFactory,
        IMediaServiceClient mediaServiceClient,
        ILogger<OdometerScanService> logger) : IOdometerScanService
    {
        private readonly IAiPromptService _promptService = promptService;
        private readonly IGenerativeAiServiceFactory _generativeAiFactory = generativeAiFactory;
        private readonly IMediaServiceClient _mediaServiceClient = mediaServiceClient;
        private readonly ILogger<OdometerScanService> _logger = logger;

        public async Task<ApiResponse<OdometerScanResponse>> ScanOdometerAsync(
            Guid userId,
            OdometerScanRequest request,
            CancellationToken cancellationToken = default)
        {
            var promptResult = await _promptService.GetPromptAsync(AiOperation.ReadOdometerFromImage, cancellationToken);
            if (!promptResult.IsSuccess || promptResult.Data == null)
            {
                _logger.LogError("ScanOdometer: failed to load prompt for ReadOdometerFromImage");
                return ApiResponse<OdometerScanResponse>.FailureResponse("Không thể tải prompt AI");
            }

            var imageUrl = await _mediaServiceClient.GetMediaFileUrlAsync(request.MediaFileId, cancellationToken);
            if (string.IsNullOrEmpty(imageUrl))
            {
                _logger.LogWarning("ScanOdometer: media file {MediaFileId} not found for user {UserId}", request.MediaFileId, userId);
                return ApiResponse<OdometerScanResponse>.NotFoundResponse("Không tìm thấy ảnh. Vui lòng kiểm tra lại ID file.");
            }

            var aiService = _generativeAiFactory.Create((AiProvider)promptResult.Data.Provider);
            var aiResult = await aiService.GenerateContentFromImageAsync(
                imageUrl,
                promptResult.Data.Content,
                AiOperation.ReadOdometerFromImage,
                userId,
                cancellationToken: cancellationToken);

            if (!aiResult.IsSuccess || aiResult.Data == null)
            {
                _logger.LogWarning("ScanOdometer: AI call failed for user {UserId} mediaFile {MediaFileId}: {Message}",
                    userId, request.MediaFileId, aiResult.Message);
                return ApiResponse<OdometerScanResponse>.SuccessResponse(
                    new OdometerScanResponse
                    {
                        DetectedOdometer = null,
                        Confidence = "low",
                        Message = "Không thể phân tích ảnh. Vui lòng chụp lại ảnh rõ hơn."
                    });
            }

            var response = ParseAiResponse(aiResult.Data.Content);
            return ApiResponse<OdometerScanResponse>.SuccessResponse(response, "Quét số km thành công");
        }

        private static OdometerScanResponse ParseAiResponse(string content)
        {
            try
            {
                var json = content.Trim();
                if (json.StartsWith("```"))
                {
                    var start = json.IndexOf('{');
                    var end = json.LastIndexOf('}');
                    if (start >= 0 && end >= 0)
                        json = json[start..(end + 1)];
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                int? odometer = null;
                if (root.TryGetProperty("detectedOdometer", out var odoEl) && odoEl.ValueKind == JsonValueKind.Number)
                    odometer = odoEl.GetInt32();

                var confidence = root.TryGetProperty("confidence", out var confEl)
                    ? confEl.GetString()
                    : null;

                var message = root.TryGetProperty("message", out var msgEl)
                    ? msgEl.GetString()
                    : null;

                return new OdometerScanResponse
                {
                    DetectedOdometer = odometer,
                    Confidence = confidence,
                    Message = message
                };
            }
            catch
            {
                return new OdometerScanResponse
                {
                    DetectedOdometer = null,
                    Confidence = "low",
                    Message = "Không thể đọc được số km từ ảnh. Vui lòng thử lại với ảnh rõ hơn."
                };
            }
        }
    }
}
