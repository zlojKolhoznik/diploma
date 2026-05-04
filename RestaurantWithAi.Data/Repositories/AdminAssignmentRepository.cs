using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class AdminAssignmentRepository(RestaurantDbContext dbContext) : IAdminAssignmentRepository
{
    public async Task<IEnumerable<AdminAssignment>> GetAppointedByAdminAsync(string appointerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointerId);
        return await dbContext.AdminAssignments
            .AsNoTracking()
            .Where(aa => aa.AppointedById == appointerId)
            .OrderByDescending(aa => aa.AssignedAtUtc)
            .ToListAsync();
    }

    public async Task<AdminAssignment?> GetAssignmentAsync(string appointedById, string appointedUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointedById);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointedUserId);

        return await dbContext.AdminAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(aa => aa.AppointedById == appointedById && aa.AppointedUserId == appointedUserId);
    }

    public async Task<AdminAssignment?> GetWhoAppointedAdminAsync(string adminUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminUserId);

        return await dbContext.AdminAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(aa => aa.AppointedUserId == adminUserId);
    }

    public async Task AddAssignmentAsync(AdminAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        await dbContext.AdminAssignments.AddAsync(assignment);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAssignmentAsync(Guid assignmentId)
    {
        var assignment = await dbContext.AdminAssignments.FindAsync(assignmentId)
            ?? throw new KeyNotFoundException($"Assignment with ID {assignmentId} not found");

        dbContext.AdminAssignments.Remove(assignment);
        await dbContext.SaveChangesAsync();
    }
}

