using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Core.Services.Reports;

public class StructuredReportSectionBuilder : IReportSectionBuilder
{
    public Task<IReadOnlyCollection<ReportSectionResponse>> BuildSectionsAsync(GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        var metrics = new List<ReportMetricResponse>
        {
            new() { Name = "Type", Value = request.Type },
            new() { Name = "Format", Value = request.Format }
        };

        if (request.RestaurantId is not null)
            metrics.Add(new ReportMetricResponse { Name = "RestaurantId", Value = request.RestaurantId.Value.ToString() });

        if (request.FromUtc is not null)
            metrics.Add(new ReportMetricResponse { Name = "FromUtc", Value = request.FromUtc.Value.ToString("O") });

        if (request.ToUtc is not null)
            metrics.Add(new ReportMetricResponse { Name = "ToUtc", Value = request.ToUtc.Value.ToString("O") });

        IReadOnlyCollection<ReportSectionResponse> sections =
        [
            new ReportSectionResponse
            {
                Title = "Overview",
                Metrics = metrics
            }
        ];

        return Task.FromResult(sections);
    }
}

