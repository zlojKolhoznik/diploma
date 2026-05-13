using System.Diagnostics.CodeAnalysis;
using Moq;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Core.Services.Reports;
using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReportServiceTests
{
    [Fact]
    public async Task GenerateReportAsync_WhenRequestIsValid_BuildsStructuredReportAndInvokesRenderer()
    {
        var builder = new Mock<IReportSectionBuilder>();
        IReadOnlyCollection<ReportSectionResponse>? capturedRequestSections = null;

        builder.Setup(b => b.BuildSectionsAsync(It.IsAny<GenerateReportRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GenerateReportRequest, CancellationToken>((request, _) =>
            {
                capturedRequestSections = [
                    new ReportSectionResponse
                    {
                        Title = request.Type,
                        Metrics = [new ReportMetricResponse { Name = "Format", Value = request.Format }]
                    }
                ];
            })
            .ReturnsAsync([
                new ReportSectionResponse
                {
                    Title = "Overview",
                    Metrics = [new ReportMetricResponse { Name = "Total", Value = "10" }]
                }
            ]);

        var sut = new ReportService(builder.Object);
        var request = new GenerateReportRequest
        {
            Type = "summary",
            Format = "json",
            RestaurantId = Guid.NewGuid(),
            FromUtc = DateTime.UtcNow.AddDays(-1),
            ToUtc = DateTime.UtcNow
        };

        var result = await sut.GenerateReportAsync(request);

        Assert.NotNull(capturedRequestSections);
        builder.Verify(b => b.BuildSectionsAsync(It.IsAny<GenerateReportRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("summary", result.Type);
        Assert.Equal("json", result.Format);
        Assert.Equal(request.RestaurantId, result.RestaurantId);
        Assert.StartsWith("summary report for restaurant ", result.Title);
        Assert.Contains(request.RestaurantId.Value.ToString(), result.Title);
        Assert.Single(result.Sections);
        Assert.Equal("Overview", result.Sections.First().Title);
    }

    [Theory]
    [InlineData("", "json")]
    [InlineData("summary", "")]
    public async Task GenerateReportAsync_WhenTypeOrFormatMissing_ThrowsArgumentException(string type, string format)
    {
        var builder = new Mock<IReportSectionBuilder>();
        var sut = new ReportService(builder.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.GenerateReportAsync(new GenerateReportRequest
        {
            Type = type,
            Format = format
        }));
    }

    [Fact]
    public async Task GenerateReportAsync_WhenDateRangeInvalid_ThrowsArgumentException()
    {
        var builder = new Mock<IReportSectionBuilder>();
        var sut = new ReportService(builder.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.GenerateReportAsync(new GenerateReportRequest
        {
            Type = "summary",
            Format = "json",
            FromUtc = DateTime.UtcNow,
            ToUtc = DateTime.UtcNow.AddDays(-1)
        }));
    }
}


