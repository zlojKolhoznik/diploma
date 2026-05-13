namespace RestaurantWithAi.Shared.Reports;

public class ReportAnalysisRequest
{
    public string ReportContent { get; set; } = string.Empty;
    public string AnalysisType { get; set; } = string.Empty; // "menu" | "waiter" | "profitability"
    public Guid? RestaurantId { get; set; }
}

public class ReportAnalysisResponse
{
    public string Analysis { get; set; } = string.Empty;

    public DateTime AnalyzedAtUtc { get; set; }
}

public interface IReportAnalysisService
{
    Task<ReportAnalysisResponse> AnalyzeReportAsync(ReportAnalysisRequest request, CancellationToken cancellationToken = default);
}

