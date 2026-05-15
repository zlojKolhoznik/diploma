using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Core.Services;

public class WaiterScheduleService(IWaiterScheduleRepository scheduleRepository, IWaiterRepository waiterRepository, IMapper mapper) : IWaiterScheduleService
{
    private static readonly TimeOnly OpeningTime = new(9, 0);
    private static readonly TimeOnly ClosingTime = new(21, 0);

    public async Task<IEnumerable<WaiterScheduleResponse>> GetSchedulesForWaiterAsync(string waiterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);
        var schedules = await scheduleRepository.GetSchedulesForWaiterAsync(waiterId);
        return mapper.Map<IEnumerable<WaiterScheduleResponse>>(schedules);
    }

    public async Task<WaiterScheduleResponse?> GetScheduleAsync(string waiterId, DateOnly date)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);
        var schedule = await scheduleRepository.GetScheduleAsync(waiterId, date);
        return schedule == null ? null : mapper.Map<WaiterScheduleResponse>(schedule);
    }

    public async Task<IEnumerable<WaiterScheduleResponse>> GetSchedulesForDateAsync(DateOnly date)
    {
        var schedules = await scheduleRepository.GetSchedulesForDateAsync(date);
        return mapper.Map<IEnumerable<WaiterScheduleResponse>>(schedules);
    }

    public async Task<Guid> CreateScheduleAsync(CreateWaiterScheduleRequest request, string adminUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can create waiter schedules");

        // Validate waiter exists
        var waiter = await waiterRepository.GetWaiterByUserIdAsync(request.WaiterId);
        if (waiter == null)
            throw new KeyNotFoundException($"Waiter with ID '{request.WaiterId}' not found");

        // Validate shift times
        if (request.ShiftEnd <= request.ShiftStart)
            throw new ArgumentException("Shift end time must be after shift start time.");

        ValidateShiftWithinOpeningHours(request.ShiftStart, request.ShiftEnd);

        var schedule = mapper.Map<WaiterSchedule>(request);
        await scheduleRepository.AddScheduleAsync(schedule);
        
        return schedule.Id;
    }

    public async Task UpdateScheduleAsync(Guid scheduleId, UpdateWaiterScheduleRequest request, string adminUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can update waiter schedules");

        // Validate shift times
        if (request.ShiftEnd <= request.ShiftStart)
            throw new ArgumentException("Shift end time must be after shift start time.");

        ValidateShiftWithinOpeningHours(request.ShiftStart, request.ShiftEnd);

        var schedule = new WaiterSchedule { Id = scheduleId, ShiftStart = request.ShiftStart, ShiftEnd = request.ShiftEnd, WaiterId = string.Empty };
        await scheduleRepository.UpdateScheduleAsync(schedule);
    }

    public async Task DeleteScheduleAsync(Guid scheduleId, string adminUserId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only admins can delete waiter schedules");

        await scheduleRepository.DeleteScheduleAsync(scheduleId);
    }

    private static void ValidateShiftWithinOpeningHours(TimeOnly shiftStart, TimeOnly shiftEnd)
    {
        if (shiftStart < OpeningTime || shiftStart >= ClosingTime)
            throw new ArgumentException(
                $"Shift start time must be between {OpeningTime} and {ClosingTime}.");

        if (shiftEnd > ClosingTime)
            throw new ArgumentException(
                $"Shift end time cannot be later than closing time {ClosingTime}.");
    }
}

