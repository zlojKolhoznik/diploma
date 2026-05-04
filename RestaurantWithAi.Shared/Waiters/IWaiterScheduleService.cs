using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Shared.Waiters;

public interface IWaiterScheduleService
{
    Task<IEnumerable<WaiterScheduleResponse>> GetSchedulesForWaiterAsync(string waiterId);
    Task<WaiterScheduleResponse?> GetScheduleAsync(string waiterId, DateOnly date);
    Task<IEnumerable<WaiterScheduleResponse>> GetSchedulesForDateAsync(DateOnly date);
    Task<Guid> CreateScheduleAsync(CreateWaiterScheduleRequest request, string adminUserId, bool isAdmin);
    Task UpdateScheduleAsync(Guid scheduleId, UpdateWaiterScheduleRequest request, string adminUserId, bool isAdmin);
    Task DeleteScheduleAsync(Guid scheduleId, string adminUserId, bool isAdmin);
}

