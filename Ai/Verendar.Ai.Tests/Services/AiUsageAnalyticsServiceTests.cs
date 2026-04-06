namespace Verendar.Ai.Tests.Services;

using Moq;
using Verendar.Ai.Application.Dtos.Ai;
using Verendar.Ai.Application.Services.Implements;
using Verendar.Ai.Domain.QueryResults;

public class AiUsageAnalyticsServiceTests
{
    [Fact]
    public async Task GetUsageByModelPagedAsync_WhenDataExists_ReturnsMappedPagedResponse()
    {
        var m = new AiUnitOfWorkMock();
        var summaries = new List<AiUsageByModelSummary>
        {
            new()
            {
                Model = "gemini-2.0-flash",
                RequestCount = 150,
                TotalInputTokens = 50000,
                TotalOutputTokens = 30000,
                TotalTokens = 80000,
                TotalCost = 1.25m,
                FailedRequestCount = 5,
                FirstUsedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                LastUsedAtUtc = new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Utc)
            }
        };
        m.AiUsages.Setup(r => r.GetAggregatedByModelPagedAsync(
                null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((summaries, 1));

        var sut = new AiUsageAnalyticsService(m.UnitOfWork.Object);
        var query = new AiUsageStatsQueryRequest { PageNumber = 1, PageSize = 10 };

        var result = await sut.GetUsageByModelPagedAsync(query);

        AiServiceResponseAssert.AssertSuccessPagedEnvelope(result, "Lấy thống kê usage theo model thành công");
        result.Data.Should().NotBeNull().And.HaveCount(1);

        var item = result.Data![0];
        item.Model.Should().Be("gemini-2.0-flash");
        item.RequestCount.Should().Be(150);
        item.TotalInputTokens.Should().Be(50000);
        item.TotalOutputTokens.Should().Be(30000);
        item.TotalTokens.Should().Be(80000);
        item.TotalCost.Should().Be(1.25m);
        item.FailedRequestCount.Should().Be(5);
        item.FirstUsedAtUtc.Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        item.LastUsedAtUtc.Should().Be(new DateTime(2026, 3, 25, 12, 0, 0, DateTimeKind.Utc));

        result.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUsageByModelPagedAsync_WhenNoData_ReturnsEmptyList()
    {
        var m = new AiUnitOfWorkMock();
        m.AiUsages.Setup(r => r.GetAggregatedByModelPagedAsync(
                null, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AiUsageByModelSummary>(), 0));

        var sut = new AiUsageAnalyticsService(m.UnitOfWork.Object);
        var query = new AiUsageStatsQueryRequest { PageNumber = 1, PageSize = 10 };

        var result = await sut.GetUsageByModelPagedAsync(query);

        AiServiceResponseAssert.AssertSuccessPagedEnvelope(result, "Lấy thống kê usage theo model thành công");
        result.Data.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetUsageByModelPagedAsync_WhenModelSearchProvided_NormalizesAndPassesToRepo()
    {
        var m = new AiUnitOfWorkMock();
        m.AiUsages.Setup(r => r.GetAggregatedByModelPagedAsync(
                "gemini", null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AiUsageByModelSummary>(), 0));

        var sut = new AiUsageAnalyticsService(m.UnitOfWork.Object);
        var query = new AiUsageStatsQueryRequest
        {
            PageNumber = 1,
            PageSize = 20,
            ModelSearch = "  gemini  "
        };

        var result = await sut.GetUsageByModelPagedAsync(query);

        AiServiceResponseAssert.AssertSuccessPagedEnvelope(result, "Lấy thống kê usage theo model thành công");
        m.AiUsages.Verify(r => r.GetAggregatedByModelPagedAsync(
            "gemini", null, null, 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsageByModelPagedAsync_WhenDateRangeProvided_PassesDatesToRepo()
    {
        var m = new AiUnitOfWorkMock();
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);

        m.AiUsages.Setup(r => r.GetAggregatedByModelPagedAsync(
                null, from, to, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<AiUsageByModelSummary>(), 0));

        var sut = new AiUsageAnalyticsService(m.UnitOfWork.Object);
        var query = new AiUsageStatsQueryRequest
        {
            PageNumber = 1,
            PageSize = 10,
            FromUtc = from,
            ToUtc = to
        };

        var result = await sut.GetUsageByModelPagedAsync(query);

        AiServiceResponseAssert.AssertSuccessPagedEnvelope(result, "Lấy thống kê usage theo model thành công");
        m.AiUsages.Verify(r => r.GetAggregatedByModelPagedAsync(
            null, from, to, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
