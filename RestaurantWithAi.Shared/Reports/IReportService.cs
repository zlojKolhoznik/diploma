namespace RestaurantWithAi.Shared.Reports;

public interface IReportService
{
    Task<ReportResponse> GenerateReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
}

