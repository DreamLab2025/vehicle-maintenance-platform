namespace Verendar.Ai.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Dtos.OdometerScan;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Common.Shared;

public class OdometerScanServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MediaFileId = Guid.NewGuid();

    private static (OdometerScanService Sut, Mock<IGenerativeAiService> AiService, Mock<IMediaServiceClient> MediaClient) CreateSut()
    {
        var aiService = new Mock<IGenerativeAiService>(MockBehavior.Strict);
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(AiProvider.Gemini)).Returns(aiService.Object);

        var mediaClient = new Mock<IMediaServiceClient>(MockBehavior.Strict);
        var sut = new OdometerScanService(factory.Object, mediaClient.Object, NullLogger<OdometerScanService>.Instance);
        return (sut, aiService, mediaClient);
    }

    private static OdometerScanRequest MakeRequest() => new() { MediaFileId = MediaFileId };

    [Fact]
    public async Task ScanOdometerAsync_WhenMediaFileNotFound_ReturnsNotFound()
    {
        var (sut, _, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy ảnh. Vui lòng kiểm tra lại ID file.");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenMediaReturnsEmpty_ReturnsNotFound()
    {
        var (sut, _, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy ảnh. Vui lòng kiểm tra lại ID file.");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenAiCallFails_ReturnsSuccessWithNullOdometer()
    {
        var (sut, aiService, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/image.jpg");
        aiService.Setup(a => a.GenerateContentFromImageAsync(
                "https://storage.example.com/image.jpg",
                It.IsAny<string>(),
                AiOperation.ReadOdometerFromImage,
                UserId,
                null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.FailureResponse("AI lỗi"));

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertSuccessEnvelope(result);
        result.Data.Should().NotBeNull();
        result.Data!.DetectedOdometer.Should().BeNull();
        result.Data.Confidence.Should().Be("low");
        result.Data.Message.Should().Be("Không thể phân tích ảnh. Vui lòng chụp lại ảnh rõ hơn.");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenAiReturnsValidJson_ReturnsParsedOdometer()
    {
        var (sut, aiService, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/image.jpg");

        var aiResponse = new GenerativeAiResponse
        {
            Content = """{"detectedOdometer": 45230, "confidence": "high", "message": "Đọc rõ số km"}""",
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 100
        };
        aiService.Setup(a => a.GenerateContentFromImageAsync(
                "https://storage.example.com/image.jpg",
                It.IsAny<string>(),
                AiOperation.ReadOdometerFromImage,
                UserId,
                null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertSuccessEnvelope(result, "Quét số km thành công");
        result.Data.Should().NotBeNull();
        result.Data!.DetectedOdometer.Should().Be(45230);
        result.Data.Confidence.Should().Be("high");
        result.Data.Message.Should().Be("Đọc rõ số km");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenAiReturnsMarkdownWrappedJson_StripsAndParsesCorrectly()
    {
        var (sut, aiService, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/image.jpg");

        var aiResponse = new GenerativeAiResponse
        {
            Content = "```json\n{\"detectedOdometer\": 12500, \"confidence\": \"medium\", \"message\": \"Ảnh hơi mờ\"}\n```",
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 120
        };
        aiService.Setup(a => a.GenerateContentFromImageAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                AiOperation.ReadOdometerFromImage, UserId,
                null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertSuccessEnvelope(result, "Quét số km thành công");
        result.Data!.DetectedOdometer.Should().Be(12500);
        result.Data.Confidence.Should().Be("medium");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenAiReturnsInvalidJson_ReturnsNullOdometerGracefully()
    {
        var (sut, aiService, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/image.jpg");

        var aiResponse = new GenerativeAiResponse
        {
            Content = "Tôi không thể đọc được ảnh này.",
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 50
        };
        aiService.Setup(a => a.GenerateContentFromImageAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                AiOperation.ReadOdometerFromImage, UserId,
                null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertSuccessEnvelope(result, "Quét số km thành công");
        result.Data.Should().NotBeNull();
        result.Data!.DetectedOdometer.Should().BeNull();
        result.Data.Confidence.Should().Be("low");
        result.Data.Message.Should().Be("Không thể đọc được số km từ ảnh. Vui lòng thử lại với ảnh rõ hơn.");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenAiReturnsNullOdometerInJson_ReturnsNullOdometer()
    {
        var (sut, aiService, mediaClient) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.example.com/image.jpg");

        var aiResponse = new GenerativeAiResponse
        {
            Content = """{"detectedOdometer": null, "confidence": "low", "message": "Không nhìn thấy đồng hồ"}""",
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 80
        };
        aiService.Setup(a => a.GenerateContentFromImageAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                AiOperation.ReadOdometerFromImage, UserId,
                null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertSuccessEnvelope(result, "Quét số km thành công");
        result.Data!.DetectedOdometer.Should().BeNull();
        result.Data.Confidence.Should().Be("low");
        result.Data.Message.Should().Be("Không nhìn thấy đồng hồ");
    }

    [Fact]
    public async Task ScanOdometerAsync_AlwaysUsesGeminiProvider_RegardlessOfConfig()
    {
        var aiService = new Mock<IGenerativeAiService>(MockBehavior.Strict);
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(AiProvider.Gemini)).Returns(aiService.Object);

        var mediaClient = new Mock<IMediaServiceClient>(MockBehavior.Strict);
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var sut = new OdometerScanService(factory.Object, mediaClient.Object, NullLogger<OdometerScanService>.Instance);

        await sut.ScanOdometerAsync(UserId, MakeRequest());

        factory.Verify(f => f.Create(AiProvider.Gemini), Times.Once);
        factory.Verify(f => f.CreateDefault(), Times.Never);
        factory.Verify(f => f.Create(AiProvider.Bedrock), Times.Never);
    }
}
