using RestaurantWithAi.Core.Services.Reports;
using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Core.Services;

public class ReportService(IReportSectionBuilder reportSectionBuilder) : IReportService
{
    public async Task<ReportResponse> GenerateReportAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var sections = await reportSectionBuilder.BuildSectionsAsync(request, cancellationToken);
        return new ReportResponse
        {
            Type = request.Type,
            Format = request.Format,
            RestaurantId = request.RestaurantId,
            GeneratedAtUtc = DateTime.UtcNow,
            Title = BuildTitle(request),
            Sections = sections
        };
    }

    private static void ValidateRequest(GenerateReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Report type is required.", nameof(request.Type));

        if (string.IsNullOrWhiteSpace(request.Format))
            throw new ArgumentException("Report format is required.", nameof(request.Format));

        if (request.FromUtc is not null && request.ToUtc is not null && request.FromUtc > request.ToUtc)
            throw new ArgumentException("Report start date must be earlier than or equal to the end date.", nameof(request.FromUtc));
    }

    private static string BuildTitle(GenerateReportRequest request)
    {
        var baseTitle = $"{request.Type} report";
        return request.RestaurantId is null
            ? baseTitle
            : $"{baseTitle} for restaurant {request.RestaurantId}";
    }
}

