namespace RestaurantWithAi.Core.Services.Reports;

public interface IReportBinaryRenderer
{
    Task<byte[]> RenderAsync(ReportStructuredData data, string format, CancellationToken cancellationToken = default);
}

public class ReportStructuredData
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
    public IReadOnlyCollection<ReportSectionData> Sections { get; set; } = [];
}

public class ReportSectionData
{
    public string Title { get; set; } = string.Empty;
    public IReadOnlyCollection<ReportMetricData> Metrics { get; set; } = [];
}

public class ReportMetricData
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

