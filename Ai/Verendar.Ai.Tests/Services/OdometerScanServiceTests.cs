namespace Verendar.Ai.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Dtos.AiPrompt;
using Verendar.Ai.Application.Dtos.OdometerScan;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Common.Shared;

public class OdometerScanServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid MediaFileId = Guid.NewGuid();

    private static AiPromptResponse MakePromptResponse() => new()
    {
        Id = Guid.NewGuid(),
        Operation = (int)AiOperation.ReadOdometerFromImage,
        OperationName = nameof(AiOperation.ReadOdometerFromImage),
        Provider = (int)AiProvider.Gemini,
        ProviderName = nameof(AiProvider.Gemini),
        Name = "Odometer Scan Prompt",
        Content = "Test prompt content",
        VersionNumber = 1,
    };

    private static (OdometerScanService Sut, Mock<IGenerativeAiService> AiService, Mock<IMediaServiceClient> MediaClient, Mock<IAiPromptService> PromptService) CreateSut()
    {
        var aiService = new Mock<IGenerativeAiService>(MockBehavior.Strict);
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(AiProvider.Gemini)).Returns(aiService.Object);

        var promptService = new Mock<IAiPromptService>(MockBehavior.Strict);
        promptService.Setup(p => p.GetPromptAsync(AiOperation.ReadOdometerFromImage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<AiPromptResponse>.SuccessResponse(MakePromptResponse()));

        var mediaClient = new Mock<IMediaServiceClient>(MockBehavior.Strict);
        var sut = new OdometerScanService(promptService.Object, factory.Object, mediaClient.Object, NullLogger<OdometerScanService>.Instance);
        return (sut, aiService, mediaClient, promptService);
    }

    private static OdometerScanRequest MakeRequest() => new() { MediaFileId = MediaFileId };

    [Fact]
    public async Task ScanOdometerAsync_WhenPromptNotFound_ReturnsFailure()
    {
        var promptService = new Mock<IAiPromptService>(MockBehavior.Strict);
        promptService.Setup(p => p.GetPromptAsync(AiOperation.ReadOdometerFromImage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<AiPromptResponse>.NotFoundResponse("Không tìm thấy prompt"));
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        var mediaClient = new Mock<IMediaServiceClient>(MockBehavior.Strict);
        var sut = new OdometerScanService(promptService.Object, factory.Object, mediaClient.Object, NullLogger<OdometerScanService>.Instance);

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không thể tải prompt AI");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenMediaFileNotFound_ReturnsNotFound()
    {
        var (sut, _, mediaClient, _) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy ảnh. Vui lòng kiểm tra lại ID file.");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenMediaReturnsEmpty_ReturnsNotFound()
    {
        var (sut, _, mediaClient, _) = CreateSut();
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var result = await sut.ScanOdometerAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, 404, "Không tìm thấy ảnh. Vui lòng kiểm tra lại ID file.");
    }

    [Fact]
    public async Task ScanOdometerAsync_WhenAiCallFails_ReturnsSuccessWithNullOdometer()
    {
        var (sut, aiService, mediaClient, _) = CreateSut();
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
        var (sut, aiService, mediaClient, _) = CreateSut();
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
        var (sut, aiService, mediaClient, _) = CreateSut();
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
        var (sut, aiService, mediaClient, _) = CreateSut();
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
        var (sut, aiService, mediaClient, _) = CreateSut();
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
    public async Task ScanOdometerAsync_UsesProviderFromPrompt_NotHardcoded()
    {
        var promptResponse = MakePromptResponse();
        promptResponse.Provider = (int)AiProvider.Bedrock;

        var aiService = new Mock<IGenerativeAiService>(MockBehavior.Strict);
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(AiProvider.Bedrock)).Returns(aiService.Object);

        var promptService = new Mock<IAiPromptService>(MockBehavior.Strict);
        promptService.Setup(p => p.GetPromptAsync(AiOperation.ReadOdometerFromImage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<AiPromptResponse>.SuccessResponse(promptResponse));

        var mediaClient = new Mock<IMediaServiceClient>(MockBehavior.Strict);
        mediaClient.Setup(c => c.GetMediaFileUrlAsync(MediaFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var sut = new OdometerScanService(promptService.Object, factory.Object, mediaClient.Object, NullLogger<OdometerScanService>.Instance);

        await sut.ScanOdometerAsync(UserId, MakeRequest());

    }
}
