namespace RestaurantWithAi.Shared.Reports;

public class ReportSectionResponse
{
    public string Title { get; set; } = string.Empty;

    public IReadOnlyCollection<ReportMetricResponse> Metrics { get; set; } = Array.Empty<ReportMetricResponse>();
}

