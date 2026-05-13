using System.Diagnostics.CodeAnalysis;
using RestaurantWithAi.Core.Services.Reports;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReportBinaryRendererTests
{
    [Fact]
    public async Task ExcelRenderer_WhenRenderingReport_ProducesBytes()
    {
        var renderer = new ExcelReportRenderer();
        var data = new ReportStructuredData
        {
            Type = "summary",
            Title = "Test Report",
            GeneratedAtUtc = DateTime.UtcNow,
            Sections = [
                new ReportSectionData
                {
                    Title = "Overview",
                    Metrics = [new ReportMetricData { Name = "Total", Value = "10" }]
                }
            ]
        };

        var bytes = await renderer.RenderAsync(data, "excel");

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task PdfRenderer_WhenRenderingReport_ProducesBytes()
    {
        var renderer = new PdfReportRenderer();
        var data = new ReportStructuredData
        {
            Type = "summary",
            Title = "Test Report",
            GeneratedAtUtc = DateTime.UtcNow,
            Sections = []
        };

        var bytes = await renderer.RenderAsync(data, "pdf");

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Theory]
    [InlineData("excel")]
    [InlineData("xlsx")]
    [InlineData("pdf")]
    public void ReportRendererFactory_WhenFormatIsSupported_ReturnsRenderer(string format)
    {
        var factory = new ReportRendererFactory();

        var renderer = factory.GetRenderer(format);

        Assert.NotNull(renderer);
        Assert.IsNotType<object>(renderer);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("docx")]
    [InlineData("")]
    public void ReportRendererFactory_WhenFormatIsUnsupported_ThrowsInvalidOperationException(string format)
    {
        var factory = new ReportRendererFactory();

        Assert.Throws<InvalidOperationException>(() => factory.GetRenderer(format));
    }
}

