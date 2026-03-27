namespace Verendar.Ai.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Ai.Application.Clients;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Dtos.AiPrompt;
using Verendar.Ai.Application.Dtos.VehicleQuestionnaire;
using Verendar.Ai.Application.Dtos.VehicleService;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Application.Services.Interfaces;
using Verendar.Ai.Domain.Enums;
using Verendar.Common.Shared;

public class VehicleMaintenanceAnalysisServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid UserVehicleId = Guid.NewGuid();
    private static readonly Guid VehicleModelId = Guid.NewGuid();

    private static AiPromptResponse MakePromptResponse() => new()
    {
        Id = Guid.NewGuid(),
        Operation = (int)AiOperation.AnalyzeMaintenanceQuestionnaire,
        OperationName = nameof(AiOperation.AnalyzeMaintenanceQuestionnaire),
        Provider = (int)AiProvider.Gemini,
        ProviderName = nameof(AiProvider.Gemini),
        Name = "Vehicle Maintenance Analysis Prompt",
        Content = "Test template with [[TODAY]] [[VEHICLE_NAME]] [[CURRENT_ODO]] [[PURCHASE_DATE]] [[SCHEDULE_BLOCK]] [[ANSWER_BLOCK]] [[PART_CATEGORY_SLUG]]",
        VersionNumber = 1,
    };

    private static (VehicleMaintenanceAnalysisService Sut, Mock<IGenerativeAiService> AiService, Mock<IVehicleServiceClient> VehicleClient, Mock<IAiPromptService> PromptService) CreateSut()
    {
        var aiService = new Mock<IGenerativeAiService>(MockBehavior.Strict);
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(AiProvider.Gemini)).Returns(aiService.Object);

        var promptService = new Mock<IAiPromptService>(MockBehavior.Strict);
        promptService.Setup(p => p.GetPromptAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<AiPromptResponse>.SuccessResponse(MakePromptResponse()));

        var vehicleClient = new Mock<IVehicleServiceClient>(MockBehavior.Strict);
        var sut = new VehicleMaintenanceAnalysisService(promptService.Object, factory.Object, vehicleClient.Object, NullLogger<VehicleMaintenanceAnalysisService>.Instance);
        return (sut, aiService, vehicleClient, promptService);
    }

    private static VehicleQuestionnaireRequest MakeRequest() => new()
    {
        UserVehicleId = UserVehicleId,
        VehicleModelId = VehicleModelId,
        PartCategorySlug = "engine-oil",
        Answers =
        [
            new QuestionAnswerDto { Question = "Bạn thay nhớt lần cuối khi nào?", Value = "3 tháng trước" },
            new QuestionAnswerDto { Question = "Số km lúc thay?", Value = "40000" }
        ]
    };

    private static VehicleServiceUserVehicleResponse MakeVehicleResponse() => new()
    {
        CurrentOdometer = 45000,
        PurchaseDate = new DateOnly(2022, 6, 15),
        UserVehicleVariant = new VehicleServiceVariantResponse
        {
            Model = new VehicleServiceModelResponse { Name = "City", BrandName = "Honda" }
        }
    };

    private static VehicleServiceDefaultScheduleResponse MakeScheduleResponse() => new()
    {
        InitialKm = 1000,
        KmInterval = 5000,
        MonthsInterval = 6,
        RequiresOdometerTracking = true,
        RequiresTimeTracking = true
    };

    private static string MakeValidAiJson() =>
        """
        {
          "recommendations": [{
            "partCategorySlug": "engine-oil",
            "lastServiceOdometer": 40000,
            "lastServiceDate": "2025-12-01",
            "predictedNextOdometer": 45000,
            "predictedNextDate": "2026-06-01",
            "confidenceScore": 0.85,
            "reasoning": "Dựa trên quãng đường và thời gian",
            "needsImmediateAttention": false
          }],
          "warnings": []
        }
        """;

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenPromptNotFound_ReturnsFailure()
    {
        var promptService = new Mock<IAiPromptService>(MockBehavior.Strict);
        promptService.Setup(p => p.GetPromptAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<AiPromptResponse>.NotFoundResponse("Không tìm thấy prompt"));
        var factory = new Mock<IGenerativeAiServiceFactory>(MockBehavior.Strict);
        var vehicleClient = new Mock<IVehicleServiceClient>(MockBehavior.Strict);
        var sut = new VehicleMaintenanceAnalysisService(promptService.Object, factory.Object, vehicleClient.Object, NullLogger<VehicleMaintenanceAnalysisService>.Instance);

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không thể tải prompt AI");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenVehicleServiceThrows_ReturnsFailure()
    {
        var (sut, _, vehicleClient, _) = CreateSut();
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không thể lấy thông tin xe từ Vehicle Service");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenVehicleNotFound_ReturnsFailure()
    {
        var (sut, _, vehicleClient, _) = CreateSut();
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceUserVehicleResponse>.NotFoundResponse("Không tìm thấy xe"));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không tìm thấy xe");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenScheduleServiceThrows_ReturnsFailure()
    {
        var (sut, _, vehicleClient, _) = CreateSut();
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceUserVehicleResponse>.SuccessResponse(MakeVehicleResponse()));
        vehicleClient.Setup(c => c.GetDefaultScheduleAsync(VehicleModelId, "engine-oil", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("timeout"));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không thể lấy lịch bảo dưỡng chuẩn từ Vehicle Service");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenScheduleNotFound_ReturnsFailure()
    {
        var (sut, _, vehicleClient, _) = CreateSut();
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceUserVehicleResponse>.SuccessResponse(MakeVehicleResponse()));
        vehicleClient.Setup(c => c.GetDefaultScheduleAsync(VehicleModelId, "engine-oil", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceDefaultScheduleResponse>.FailureResponse("Không tìm thấy lịch bảo dưỡng"));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không tìm thấy lịch bảo dưỡng");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenAiServiceThrows_ReturnsFailure()
    {
        var (sut, aiService, vehicleClient, _) = CreateSut();
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceUserVehicleResponse>.SuccessResponse(MakeVehicleResponse()));
        vehicleClient.Setup(c => c.GetDefaultScheduleAsync(VehicleModelId, "engine-oil", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceDefaultScheduleResponse>.SuccessResponse(MakeScheduleResponse()));
        aiService.Setup(a => a.GenerateContentAsync(
                It.IsAny<string>(), AiOperation.AnalyzeMaintenanceQuestionnaire, UserId,
                null, null, null, 0.5m, null))
            .ThrowsAsync(new InvalidOperationException("AI unavailable"));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Không thể phân tích dữ liệu");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenAiServiceFails_ReturnsFailure()
    {
        var (sut, aiService, vehicleClient, _) = CreateSut();
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceUserVehicleResponse>.SuccessResponse(MakeVehicleResponse()));
        vehicleClient.Setup(c => c.GetDefaultScheduleAsync(VehicleModelId, "engine-oil", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceDefaultScheduleResponse>.SuccessResponse(MakeScheduleResponse()));
        aiService.Setup(a => a.GenerateContentAsync(
                It.IsAny<string>(), AiOperation.AnalyzeMaintenanceQuestionnaire, UserId,
                null, null, null, 0.5m, null))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.FailureResponse("Rate limited"));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "Rate limited");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenAiReturnsInvalidJson_ReturnsFailure()
    {
        var (sut, aiService, vehicleClient, _) = CreateSut();
        SetupHappyPathDependencies(vehicleClient);
        var aiResponse = new GenerativeAiResponse
        {
            Content = "This is not JSON",
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 50
        };
        aiService.Setup(a => a.GenerateContentAsync(
                It.IsAny<string>(), AiOperation.AnalyzeMaintenanceQuestionnaire, UserId,
                null, null, null, 0.5m, null))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Không thể phân tích kết quả từ AI");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenAiReturnsEmptyRecommendations_ReturnsFailure()
    {
        var (sut, aiService, vehicleClient, _) = CreateSut();
        SetupHappyPathDependencies(vehicleClient);
        var aiResponse = new GenerativeAiResponse
        {
            Content = """{"recommendations": [], "warnings": []}""",
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 50
        };
        aiService.Setup(a => a.GenerateContentAsync(
                It.IsAny<string>(), AiOperation.AnalyzeMaintenanceQuestionnaire, UserId,
                null, null, null, 0.5m, null))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertFailureEnvelope(result, "AI không thể đưa ra khuyến nghị");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenAiReturnsMultipleRecommendations_ReturnsFailure()
    {
        var (sut, aiService, vehicleClient, _) = CreateSut();
        SetupHappyPathDependencies(vehicleClient);
        var multipleRecJson = """
        {
          "recommendations": [
            {"partCategorySlug": "engine-oil", "confidenceScore": 0.8, "reasoning": "r1", "needsImmediateAttention": false},
            {"partCategorySlug": "brake-pad", "confidenceScore": 0.7, "reasoning": "r2", "needsImmediateAttention": true}
          ],
          "warnings": []
        }
        """;
        var aiResponse = new GenerativeAiResponse
        {
            Content = multipleRecJson,
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 200
        };
        aiService.Setup(a => a.GenerateContentAsync(
                It.IsAny<string>(), AiOperation.AnalyzeMaintenanceQuestionnaire, UserId,
                null, null, null, 0.5m, null))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("2 khuyến nghị thay vì 1");
    }

    [Fact]
    public async Task AnalyzeQuestionnaireAsync_WhenSuccess_ReturnsMappedResponse()
    {
        var (sut, aiService, vehicleClient, _) = CreateSut();
        SetupHappyPathDependencies(vehicleClient);
        var aiResponse = new GenerativeAiResponse
        {
            Content = MakeValidAiJson(),
            Model = "gemini-2.0-flash",
            Provider = AiProvider.Gemini,
            TotalTokens = 300,
            TotalCost = 0.001m,
            ResponseTimeMs = 1200
        };
        aiService.Setup(a => a.GenerateContentAsync(
                It.IsAny<string>(), AiOperation.AnalyzeMaintenanceQuestionnaire, UserId,
                null, null, null, 0.5m, null))
            .ReturnsAsync(ApiResponse<GenerativeAiResponse>.SuccessResponse(aiResponse));

        var result = await sut.AnalyzeQuestionnaireAsync(UserId, MakeRequest());

        AiServiceResponseAssert.AssertSuccessEnvelope(result);
        result.Data.Should().NotBeNull();
        result.Data!.Recommendations.Should().HaveCount(1);

        var rec = result.Data.Recommendations[0];
        rec.PartCategorySlug.Should().Be("engine-oil");
        rec.LastReplacementOdometer.Should().Be(40000);
        rec.LastReplacementDate.Should().Be(new DateOnly(2025, 12, 1));
        rec.PredictedNextOdometer.Should().Be(45000);
        rec.PredictedNextDate.Should().Be(new DateOnly(2026, 6, 1));
        rec.ConfidenceScore.Should().BeApproximately(0.85, 0.001);
        rec.Reasoning.Should().Be("Dựa trên quãng đường và thời gian");
        rec.NeedsImmediateAttention.Should().BeFalse();

        result.Data.Metadata.Should().NotBeNull();
        result.Data.Metadata.Model.Should().Be("gemini-2.0-flash");
        result.Data.Metadata.TotalTokens.Should().Be(300);
        result.Data.Metadata.TotalCost.Should().Be(0.001m);
        result.Data.Metadata.ResponseTimeMs.Should().Be(1200);
    }

    private static void SetupHappyPathDependencies(Mock<IVehicleServiceClient> vehicleClient)
    {
        vehicleClient.Setup(c => c.GetUserVehicleByIdAsync(UserVehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceUserVehicleResponse>.SuccessResponse(MakeVehicleResponse()));
        vehicleClient.Setup(c => c.GetDefaultScheduleAsync(VehicleModelId, "engine-oil", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResponse<VehicleServiceDefaultScheduleResponse>.SuccessResponse(MakeScheduleResponse()));
    }
}
