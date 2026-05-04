using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IWaiterScheduleRepository
{
    Task<IEnumerable<WaiterSchedule>> GetSchedulesForWaiterAsync(string waiterId);
    Task<WaiterSchedule?> GetScheduleAsync(string waiterId, DateOnly date);
    Task<IEnumerable<WaiterSchedule>> GetSchedulesForDateAsync(DateOnly date);
    Task AddScheduleAsync(WaiterSchedule schedule);
    Task UpdateScheduleAsync(WaiterSchedule schedule);
    Task DeleteScheduleAsync(Guid scheduleId);
    Task<bool> IsWaiterScheduledAsync(string waiterId, DateOnly date);
}

