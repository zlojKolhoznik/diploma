using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);
    Task<IEnumerable<Report>> GetAllAsync(string? generatedById = null);
    Task AddAsync(Report report);
    Task UpdateAnalysisTextAsync(Guid id, string analysisText);
}

