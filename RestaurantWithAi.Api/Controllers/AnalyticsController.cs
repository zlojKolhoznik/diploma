using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantWithAi.Core.Services.Reports;
using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/analytics")]
public class AnalyticsController(IReportService reportService, IReportRendererFactory rendererFactory, IReportAnalysisService analysisService, ILogger<AnalyticsController> logger) : ControllerBase
{
    [HttpPost("reports")]
    [ProducesResponseType(typeof(ReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
    {
        try
        {
            var report = await reportService.GenerateReportAsync(request);

            if (IsBinaryFormat(request.Format))
                return await RenderBinaryReport(report, request.Format);

            return Ok(report);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Generate report request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, "Generate report failed due to unsupported format or rendering issue.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while generating a report.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("reports/analyze")]
    [ProducesResponseType(typeof(ReportAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportAnalysisResponse>> AnalyzeReport([FromBody] ReportAnalysisRequest request)
    {
        try
        {
            var analysis = await analysisService.AnalyzeReportAsync(request);
            return Ok(analysis);
        }
        catch (ArgumentException ex)
        {
            logger.LogInformation(ex, "Analyze report request was invalid.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while analyzing a report.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
        }
    }

    private static bool IsBinaryFormat(string format)
    {
        return format switch
        {
            "excel" or "xlsx" or "pdf" => true,
            _ => false
        };
    }

    private async Task<IActionResult> RenderBinaryReport(ReportResponse report, string format)
    {
        var renderer = rendererFactory.GetRenderer(format);
        var data = new ReportStructuredData
        {
            Type = report.Type,
            Title = report.Title,
            GeneratedAtUtc = report.GeneratedAtUtc,
            Sections = report.Sections
                .Select(s => new ReportSectionData
                {
                    Title = s.Title,
                    Metrics = s.Metrics
                        .Select(m => new ReportMetricData { Name = m.Name, Value = m.Value })
                        .ToList()
                })
                .ToList()
        };

        var bytes = await renderer.RenderAsync(data, format);
        var mimeType = format switch
        {
            "excel" or "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
        var fileName = $"{report.Type}-report-{report.GeneratedAtUtc:yyyyMMdd-HHmmss}.{GetFileExtension(format)}";

        return File(bytes, mimeType, fileName);
    }

    private static string GetFileExtension(string format)
    {
        return format switch
        {
            "excel" or "xlsx" => "xlsx",
            "pdf" => "pdf",
            _ => "bin"
        };
    }
}

