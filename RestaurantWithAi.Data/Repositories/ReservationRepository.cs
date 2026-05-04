using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class ReservationRepository(RestaurantDbContext dbContext) : IReservationRepository
{
    private const int ReservationGapMinutes = 15;

    public async Task<IEnumerable<Reservation>> GetReservationsForGuestAsync(string guestId) =>
        await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.GuestId == guestId)
            .OrderBy(r => r.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetReservationsForWaiterAsync(string waiterId) =>
        await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.AssignedWaiterId == waiterId)
            .OrderBy(r => r.StartTime)
            .ToListAsync();

    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync() =>
        await dbContext.Reservations
            .AsNoTracking()
            .OrderBy(r => r.StartTime)
            .ToListAsync();

    public async Task<Reservation> GetReservationByIdAsync(Guid id) =>
        await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id)
        ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

    public async Task AddReservationAsync(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        await dbContext.Reservations.AddAsync(reservation);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteReservationAsync(Guid id)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        dbContext.Reservations.Remove(reservation);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationTimeAsync(Guid id, DateTime startTime, int durationMinutes)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        reservation.StartTime = startTime;
        reservation.ApproximateDurationMinutes = durationMinutes;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationTableAsync(Guid id, int? tableNumber)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        reservation.TableNumber = tableNumber;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationAssignedWaiterAsync(Guid id, string? waiterId)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        reservation.AssignedWaiterId = waiterId;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationStatusAsync(Guid id, string status)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        reservation.Status = status;
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> RestaurantExistsAsync(Guid restaurantId) =>
        dbContext.Restaurants.AnyAsync(r => r.Id == restaurantId);

    public Task<bool> TableExistsAsync(Guid restaurantId, int tableNumber) =>
        dbContext.Tables.AnyAsync(t => t.RestaurantId == restaurantId && t.TableNumber == tableNumber);

    public async Task<bool> HasTableConflictAsync(Guid restaurantId, int tableNumber, DateTime startTime, int durationMinutes, Guid? excludedReservationId = null)
    {
        var requestedEndTime = startTime.AddMinutes(durationMinutes);
        var openStatuses = new[] { ReservationStatuses.Created, ReservationStatuses.InProgress, ReservationStatuses.PendingPayment };

        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.RestaurantId == restaurantId &&
                        r.TableNumber == tableNumber &&
                        openStatuses.Contains(r.Status) &&
                        (!excludedReservationId.HasValue || r.Id != excludedReservationId.Value))
            .AnyAsync(r => startTime < r.StartTime.AddMinutes(r.ApproximateDurationMinutes + ReservationGapMinutes) &&
                           r.StartTime < requestedEndTime.AddMinutes(ReservationGapMinutes));
    }

    public async Task<IEnumerable<Table>> GetAvailableTablesAsync(Guid restaurantId, DateTime startTime, int durationMinutes, int? minimumSeats = null)
    {
        var requestedEndTime = startTime.AddMinutes(durationMinutes);
        var openStatuses = new[] { ReservationStatuses.Created, ReservationStatuses.InProgress, ReservationStatuses.PendingPayment };

        var tablesQuery = dbContext.Tables
            .AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId);

        if (minimumSeats.HasValue)
            tablesQuery = tablesQuery.Where(t => t.Seats >= minimumSeats.Value);

        var conflictingTableNumbers = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.RestaurantId == restaurantId &&
                        r.TableNumber.HasValue &&
                        openStatuses.Contains(r.Status) &&
                        startTime < r.StartTime.AddMinutes(r.ApproximateDurationMinutes + ReservationGapMinutes) &&
                        r.StartTime < requestedEndTime.AddMinutes(ReservationGapMinutes))
            .Select(r => r.TableNumber!.Value)
            .Distinct()
            .ToListAsync();

        return await tablesQuery
            .Where(t => !conflictingTableNumbers.Contains(t.TableNumber))
            .OrderBy(t => t.TableNumber)
            .ToListAsync();
    }

    public async Task<bool> HasAvailableTablesAsync(Guid restaurantId, DateTime startTime, int durationMinutes, int? minimumSeats = null)
    {
        var availableTables = await GetAvailableTablesAsync(restaurantId, startTime, durationMinutes, minimumSeats);
        return availableTables.Any();
    }

    public async Task<Guid?> GetWaiterRestaurantIdAsync(string waiterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(waiterId);

        return await dbContext.Waiters
            .AsNoTracking()
            .Where(w => w.UserId == waiterId)
            .Select(w => w.RestaurantId)
            .FirstOrDefaultAsync();
    }

    public async Task<Waiter?> GetLeastLoadedWaiterAsync(Guid restaurantId, DateOnly date)
    {
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        // Get all waiters for the restaurant
        var waiters = await dbContext.Waiters
            .AsNoTracking()
            .Where(w => w.RestaurantId == restaurantId)
            .ToListAsync();

        if (!waiters.Any())
            return null;

        // Count non-closed (open) reservations for each waiter on the given date
        var waiterLoadMap = new Dictionary<string, int>();
        foreach (var waiter in waiters)
        {
            var reservationCount = await dbContext.Reservations
                .AsNoTracking()
                .CountAsync(r => r.AssignedWaiterId == waiter.UserId &&
                                  r.StartTime >= startOfDay &&
                                  r.StartTime < endOfDay &&
                                  ReservationStatuses.OpenStatuses.Contains(r.Status));
            waiterLoadMap[waiter.UserId] = reservationCount;
        }

        // Return waiter with minimum load
        var leastLoadedWaiterId = waiterLoadMap.OrderBy(x => x.Value).First().Key;
        return await dbContext.Waiters
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == leastLoadedWaiterId);
    }
}


