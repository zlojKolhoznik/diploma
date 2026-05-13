namespace RestaurantWithAi.Shared.Reports;

public class ReportResponse
{
    public string Type { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public Guid? RestaurantId { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public string Title { get; set; } = string.Empty;

    public IReadOnlyCollection<ReportSectionResponse> Sections { get; set; } = Array.Empty<ReportSectionResponse>();
}

