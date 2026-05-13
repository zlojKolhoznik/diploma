using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class ReportRepository(RestaurantDbContext dbContext) : IReportRepository
{
    public async Task<Report?> GetByIdAsync(Guid id)
    {
        return await dbContext.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Report>> GetAllAsync(string? generatedById = null)
    {
        var query = dbContext.Reports.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(generatedById))
            query = query.Where(r => r.GeneratedById == generatedById);

        return await query.OrderByDescending(r => r.GeneratedAtUtc).ToListAsync();
    }

    public async Task AddAsync(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);
        await dbContext.Reports.AddAsync(report);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAnalysisTextAsync(Guid id, string analysisText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(analysisText);

        var report = await dbContext.Reports.FirstOrDefaultAsync(r => r.Id == id)
                     ?? throw new KeyNotFoundException($"Report with ID {id} not found");

        report.AnalysisText = analysisText;
        await dbContext.SaveChangesAsync();
    }
}

