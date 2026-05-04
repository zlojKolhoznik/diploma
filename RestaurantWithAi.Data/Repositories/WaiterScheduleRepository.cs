using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class WaiterScheduleRepository(RestaurantDbContext dbContext) : IWaiterScheduleRepository
{
    public async Task<IEnumerable<WaiterSchedule>> GetSchedulesForWaiterAsync(string waiterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);
        return await dbContext.WaiterSchedules
            .AsNoTracking()
            .Where(ws => ws.WaiterId == waiterId)
            .OrderBy(ws => ws.Date)
            .ToListAsync();
    }

    public async Task<WaiterSchedule?> GetScheduleAsync(string waiterId, DateOnly date)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);
        return await dbContext.WaiterSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(ws => ws.WaiterId == waiterId && ws.Date == date);
    }

    public async Task<IEnumerable<WaiterSchedule>> GetSchedulesForDateAsync(DateOnly date)
    {
        return await dbContext.WaiterSchedules
            .AsNoTracking()
            .Where(ws => ws.Date == date)
            .OrderBy(ws => ws.WaiterId)
            .ToListAsync();
    }

    public async Task AddScheduleAsync(WaiterSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        
        if (await GetScheduleAsync(schedule.WaiterId, schedule.Date) != null)
            throw new InvalidOperationException($"Schedule already exists for waiter {schedule.WaiterId} on {schedule.Date}");

        await dbContext.WaiterSchedules.AddAsync(schedule);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateScheduleAsync(WaiterSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        
        var existingSchedule = await dbContext.WaiterSchedules.FindAsync(schedule.Id)
            ?? throw new KeyNotFoundException($"Schedule with ID {schedule.Id} not found");

        existingSchedule.ShiftStart = schedule.ShiftStart;
        existingSchedule.ShiftEnd = schedule.ShiftEnd;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteScheduleAsync(Guid scheduleId)
    {
        var schedule = await dbContext.WaiterSchedules.FindAsync(scheduleId)
            ?? throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found");

        dbContext.WaiterSchedules.Remove(schedule);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsWaiterScheduledAsync(string waiterId, DateOnly date)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);
        return await dbContext.WaiterSchedules
            .AsNoTracking()
            .AnyAsync(ws => ws.WaiterId == waiterId && ws.Date == date);
    }
}

