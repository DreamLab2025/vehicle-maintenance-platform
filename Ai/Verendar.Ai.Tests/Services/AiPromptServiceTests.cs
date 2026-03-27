namespace Verendar.Ai.Tests.Services;

using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Verendar.Ai.Application.Dtos.AiPrompt;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Domain.Entities;
using Verendar.Ai.Domain.Enums;
using Verendar.Ai.Domain.Repositories.Interfaces;
using Verendar.Common.Caching;
using Verendar.Common.Shared;

public class AiPromptServiceTests
{
    private const string ValidMaintenanceTemplate =
        "x [[TODAY]] [[VEHICLE_NAME]] [[CURRENT_ODO]] [[PURCHASE_DATE]] [[SCHEDULE_BLOCK]] [[ANSWER_BLOCK]] [[PART_CATEGORY_SLUG]]";

    private static AiPrompt MakeMaintenancePrompt(int version = 1) => new()
    {
        Id = Guid.NewGuid(),
        Operation = AiOperation.AnalyzeMaintenanceQuestionnaire,
        Provider = AiProvider.Gemini,
        Name = "Test",
        Content = ValidMaintenanceTemplate,
        VersionNumber = version,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = Guid.NewGuid(),
    };

    [Fact]
    public async Task UpdateAsync_WhenPlaceholderMissing_ReturnsFailure()
    {
        var entity = MakeMaintenancePrompt();
        var prompts = new Mock<IAiPromptRepository>(MockBehavior.Strict);
        prompts.Setup(p => p.GetByOperationAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var histories = new Mock<IAiPromptHistoryRepository>(MockBehavior.Strict);
        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uow.Setup(u => u.AiPrompts).Returns(prompts.Object);
        uow.Setup(u => u.AiPromptHistories).Returns(histories.Object);

        var sut = new AiPromptService(uow.Object, new Mock<ICacheService>(MockBehavior.Loose).Object, NullLogger<AiPromptService>.Instance);

        var result = await sut.UpdateAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, new UpdateAiPromptRequest
        {
            Content = "no placeholders",
            Provider = (int)AiProvider.Gemini,
        });

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("placeholder");
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenValid_AddsHistoryAndIncrementsVersion()
    {
        var entity = MakeMaintenancePrompt(1);
        var prompts = new Mock<IAiPromptRepository>(MockBehavior.Strict);
        prompts.Setup(p => p.GetByOperationAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var histories = new Mock<IAiPromptHistoryRepository>(MockBehavior.Strict);
        histories.Setup(h => h.AddAsync(It.IsAny<AiPromptHistory>()))
            .ReturnsAsync((AiPromptHistory h) => h);

        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uow.Setup(u => u.AiPrompts).Returns(prompts.Object);
        uow.Setup(u => u.AiPromptHistories).Returns(histories.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cache = new Mock<ICacheService>(MockBehavior.Strict);
        cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        var sut = new AiPromptService(uow.Object, cache.Object, NullLogger<AiPromptService>.Instance);

        var newContent = ValidMaintenanceTemplate + " updated";
        var result = await sut.UpdateAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, new UpdateAiPromptRequest
        {
            Content = newContent,
            Provider = (int)AiProvider.Bedrock,
            Note = "n",
        });

        result.IsSuccess.Should().BeTrue();
        entity.VersionNumber.Should().Be(2);
        entity.Content.Should().Be(newContent);
        entity.Provider.Should().Be(AiProvider.Bedrock);
        histories.Verify(h => h.AddAsync(It.Is<AiPromptHistory>(x =>
            x.VersionNumber == 1 && x.Content == ValidMaintenanceTemplate)), Times.Once);
    }

    [Fact]
    public async Task RollbackAsync_WhenHistoryContentMissingPlaceholders_ReturnsFailure()
    {
        var entity = MakeMaintenancePrompt(2);
        var badHistory = new AiPromptHistory
        {
            Id = Guid.NewGuid(),
            AiPromptId = entity.Id,
            VersionNumber = 1,
            Provider = AiProvider.Gemini,
            Content = "broken",
            CreatedAt = DateTime.UtcNow,
        };

        var prompts = new Mock<IAiPromptRepository>(MockBehavior.Strict);
        prompts.Setup(p => p.GetByOperationAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var histories = new Mock<IAiPromptHistoryRepository>(MockBehavior.Strict);
        histories.Setup(h => h.GetByVersionAsync(entity.Id, 1, It.IsAny<CancellationToken>())).ReturnsAsync(badHistory);

        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uow.Setup(u => u.AiPrompts).Returns(prompts.Object);
        uow.Setup(u => u.AiPromptHistories).Returns(histories.Object);

        var sut = new AiPromptService(uow.Object, new Mock<ICacheService>(MockBehavior.Loose).Object, NullLogger<AiPromptService>.Instance);

        var result = await sut.RollbackAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, new RollbackAiPromptRequest { VersionNumber = 1 });

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("placeholder");
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetVersionsAsync_IncludesCurrentRowSortedDescending()
    {
        var entity = MakeMaintenancePrompt(2);
        entity.UpdatedAt = new DateTime(2026, 3, 27, 12, 0, 0, DateTimeKind.Utc);
        var h1 = new AiPromptHistory
        {
            Id = Guid.NewGuid(),
            AiPromptId = entity.Id,
            VersionNumber = 1,
            Provider = AiProvider.Gemini,
            Content = "old",
            CreatedAt = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc),
        };

        var prompts = new Mock<IAiPromptRepository>(MockBehavior.Strict);
        prompts.Setup(p => p.GetByOperationAsync(AiOperation.AnalyzeMaintenanceQuestionnaire, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var histories = new Mock<IAiPromptHistoryRepository>(MockBehavior.Strict);
        histories.Setup(h => h.GetByPromptIdAsync(entity.Id, It.IsAny<CancellationToken>())).ReturnsAsync([h1]);

        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);
        uow.Setup(u => u.AiPrompts).Returns(prompts.Object);
        uow.Setup(u => u.AiPromptHistories).Returns(histories.Object);

        var sut = new AiPromptService(uow.Object, new Mock<ICacheService>(MockBehavior.Loose).Object, NullLogger<AiPromptService>.Instance);

        var result = await sut.GetVersionsAsync(AiOperation.AnalyzeMaintenanceQuestionnaire);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
        result.Data[0].VersionNumber.Should().Be(2);
        result.Data[0].IsCurrent.Should().BeTrue();
        result.Data[1].VersionNumber.Should().Be(1);
        result.Data[1].IsCurrent.Should().BeFalse();
    }
}
