using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Data.Repositories;

public class ReservationRepository(RestaurantDbContext dbContext) : IReservationRepository
{
    public async Task<IEnumerable<Reservation>> GetReservationsByGuestAsync(string guestUserId)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.GuestUserId == guestUserId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByWaiterAsync(string waiterId)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.AssignedWaiterId == waiterId)
            .ToListAsync();
    }

    public async Task<Reservation> GetReservationByIdAsync(Guid id)
    {
        return await dbContext.Reservations
                   .AsNoTracking()
                   .FirstOrDefaultAsync(r => r.Id == id)
               ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");
    }

    public async Task<Reservation> CreateReservationAsync(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        var tableExists = await dbContext.Tables
            .AnyAsync(t => t.RestaurantId == reservation.RestaurantId && t.TableNumber == reservation.TableNumber);
        if (!tableExists)
            throw new KeyNotFoundException($"Table {reservation.TableNumber} for restaurant {reservation.RestaurantId} not found");

        if (await HasOverlapAsync(reservation.RestaurantId, reservation.TableNumber, reservation.StartTime, reservation.DurationMinutes))
            throw new ReservationConflictException($"Table {reservation.TableNumber} is not available at the requested time.");

        await dbContext.Reservations.AddAsync(reservation);
        await dbContext.SaveChangesAsync();
        return reservation;
    }

    public async Task DeleteReservationAsync(Guid id)
    {
        var reservation = await dbContext.Reservations.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        if (reservation.Status != ReservationStatus.Created)
            throw new InvalidOperationException("Cancellation is only possible while the reservation is in Created status.");

        dbContext.Reservations.Remove(reservation);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationTimeAsync(Guid id, DateTime startTime, int durationMinutes)
    {
        var reservation = await dbContext.Reservations.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        if (await HasOverlapAsync(reservation.RestaurantId, reservation.TableNumber, startTime, durationMinutes, id))
            throw new ReservationConflictException($"Table {reservation.TableNumber} is not available at the requested time.");

        reservation.StartTime = startTime;
        reservation.DurationMinutes = durationMinutes;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationTableAsync(Guid id, int tableNumber)
    {
        var reservation = await dbContext.Reservations.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        var tableExists = await dbContext.Tables
            .AnyAsync(t => t.RestaurantId == reservation.RestaurantId && t.TableNumber == tableNumber);
        if (!tableExists)
            throw new KeyNotFoundException($"Table {tableNumber} for restaurant {reservation.RestaurantId} not found");

        if (await HasOverlapAsync(reservation.RestaurantId, tableNumber, reservation.StartTime, reservation.DurationMinutes, id))
            throw new ReservationConflictException($"Table {tableNumber} is not available at the requested time.");

        reservation.TableNumber = tableNumber;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationWaiterAsync(Guid id, string? waiterId)
    {
        var reservation = await dbContext.Reservations.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        if (waiterId != null)
        {
            var waiterExists = await dbContext.Waiters.AnyAsync(w => w.UserId == waiterId);
            if (!waiterExists)
                throw new KeyNotFoundException($"Waiter with ID {waiterId} not found");
        }

        reservation.AssignedWaiterId = waiterId;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateReservationStatusAsync(Guid id, ReservationStatus status)
    {
        var reservation = await dbContext.Reservations.FindAsync(id)
                          ?? throw new KeyNotFoundException($"Reservation with ID {id} not found");

        ValidateStatusTransition(reservation.Status, status);

        reservation.Status = status;
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> HasOverlapAsync(Guid restaurantId, int tableNumber, DateTime startTime, int durationMinutes, Guid? excludeReservationId = null)
    {
        var windowStart = startTime.AddMinutes(-15);
        var windowEnd = startTime.AddMinutes(durationMinutes + 15);

        var query = dbContext.Reservations
            .Where(r => r.RestaurantId == restaurantId
                        && r.TableNumber == tableNumber
                        && r.Status != ReservationStatus.Closed);

        if (excludeReservationId.HasValue)
            query = query.Where(r => r.Id != excludeReservationId.Value);

        return await query.AnyAsync(r =>
            r.StartTime < windowEnd &&
            r.StartTime.AddMinutes(r.DurationMinutes) > windowStart);
    }

    private static void ValidateStatusTransition(ReservationStatus current, ReservationStatus next)
    {
        var validTransitions = new Dictionary<ReservationStatus, ReservationStatus>
        {
            [ReservationStatus.Created] = ReservationStatus.InProgress,
            [ReservationStatus.InProgress] = ReservationStatus.PendingPayment,
            [ReservationStatus.PendingPayment] = ReservationStatus.Closed
        };

        if (!validTransitions.TryGetValue(current, out var expected) || expected != next)
            throw new InvalidStatusTransitionException(
                $"Cannot transition from {current} to {next}. Valid transition from {current} is to {(validTransitions.ContainsKey(current) ? validTransitions[current] : "none")}.");
    }
}
