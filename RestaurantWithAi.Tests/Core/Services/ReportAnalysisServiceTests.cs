using System.Diagnostics.CodeAnalysis;
using Moq;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReportAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeReportAsync_WhenRequestIsValid_CallsClientAndReturnsAnalysis()
    {
        var client = new Mock<ITextGenerationClient>();
        var expectedAnalysis = "The report shows a 15% increase in bookings.";

        client.Setup(c => c.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalysis);

        var sut = new ReportAnalysisService(client.Object);
        var request = new ReportAnalysisRequest { ReportContent = "Bookings: 100, Revenue: $5000" };

        var result = await sut.AnalyzeReportAsync(request);

        Assert.Equal(expectedAnalysis, result.Analysis);
        Assert.True(result.AnalyzedAtUtc <= DateTime.UtcNow.AddSeconds(1));
        client.Verify(c => c.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AnalyzeReportAsync_WhenContentMissing_ThrowsArgumentException(string? content)
    {
        var client = new Mock<ITextGenerationClient>();
        var sut = new ReportAnalysisService(client.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.AnalyzeReportAsync(new ReportAnalysisRequest { ReportContent = content ?? "" }));
    }

    [Fact]
    public async Task AnalyzeReportAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        var client = new Mock<ITextGenerationClient>();
        var sut = new ReportAnalysisService(client.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AnalyzeReportAsync(null!));
    }
}

