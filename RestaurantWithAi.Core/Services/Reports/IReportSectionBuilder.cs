using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Core.Services.Reports;

public interface IReportSectionBuilder
{
    Task<IReadOnlyCollection<ReportSectionResponse>> BuildSectionsAsync(GenerateReportRequest request, CancellationToken cancellationToken = default);
}


