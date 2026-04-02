using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Data.Repositories;

public class ReservationRepository(RestaurantDbContext dbContext) : IReservationRepository
{
    private static readonly TimeSpan GapBuffer = TimeSpan.FromMinutes(15);

    public async Task<IEnumerable<Reservation>> GetReservationsByGuestIdAsync(string guestId)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.GuestId == guestId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByWaiterIdAsync(string waiterId)
    {
        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.WaiterId == waiterId)
            .ToListAsync();
    }

    public async Task<Reservation> GetReservationByIdAsync(Guid id)
    {
        return await dbContext.Reservations
                   .FirstOrDefaultAsync(r => r.Id == id)
               ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");
    }

    public async Task<Reservation> AddReservationAsync(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        await dbContext.Reservations.AddAsync(reservation);
        await dbContext.SaveChangesAsync();
        return reservation;
    }

    public async Task<bool> HasOverlappingReservationAsync(
        Guid restaurantId,
        int tableNumber,
        DateTimeOffset startTime,
        int durationMinutes,
        Guid? excludeReservationId = null)
    {
        var endTime = startTime.AddMinutes(durationMinutes);

        return await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.RestaurantId == restaurantId
                        && r.TableNumber == tableNumber
                        && r.Status != ReservationStatus.Cancelled
                        && (excludeReservationId == null || r.Id != excludeReservationId))
            .AnyAsync(r =>
                startTime < r.StartTime.AddMinutes(r.DurationMinutes).Add(GapBuffer) &&
                endTime > r.StartTime.Add(-GapBuffer));
    }

    public async Task<IEnumerable<int>> GetAvailableTableNumbersAsync(
        Guid restaurantId,
        DateTimeOffset startTime,
        int durationMinutes)
    {
        var endTime = startTime.AddMinutes(durationMinutes);

        var allTableNumbers = await dbContext.Tables
            .AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId)
            .Select(t => t.TableNumber)
            .ToListAsync();

        var occupiedTableNumbers = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.RestaurantId == restaurantId
                        && r.TableNumber != null
                        && r.Status != ReservationStatus.Cancelled
                        && startTime < r.StartTime.AddMinutes(r.DurationMinutes).Add(GapBuffer)
                        && endTime > r.StartTime.Add(-GapBuffer))
            .Select(r => r.TableNumber!.Value)
            .Distinct()
            .ToListAsync();

        return allTableNumbers.Except(occupiedTableNumbers).ToList();
    }

    public async Task<bool> HasAvailableTablesAsync(Guid restaurantId, DateTimeOffset startTime, int durationMinutes)
    {
        var availableNumbers = await GetAvailableTableNumbersAsync(restaurantId, startTime, durationMinutes);
        return availableNumbers.Any();
    }

    public async Task UpdateReservationAsync(Reservation reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        dbContext.Reservations.Update(reservation);
        await dbContext.SaveChangesAsync();
    }
}
