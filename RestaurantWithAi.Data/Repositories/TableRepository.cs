using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Data.Repositories;

public class TableRepository(RestaurantDbContext dbContext) : ITableRepository
{
    private static readonly TimeSpan GapBuffer = TimeSpan.FromMinutes(15);

    public async Task<IEnumerable<Table>> GetTablesByRestaurantIdAsync(Guid restaurantId)
    {
        var restaurantExists = await dbContext.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

        return await dbContext.Tables
            .AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Table>> GetAvailableTablesAsync(Guid restaurantId, DateTimeOffset startTime, int durationMinutes)
    {
        var restaurantExists = await dbContext.Restaurants.AnyAsync(r => r.Id == restaurantId);
        if (!restaurantExists)
            throw new KeyNotFoundException($"Restaurant with ID {restaurantId} not found");

        var endTime = startTime.AddMinutes(durationMinutes);

        var occupiedTableNumbers = await dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.RestaurantId == restaurantId
                        && r.TableNumber != null
                        && r.Status != ReservationStatus.Cancelled
                        && startTime < r.StartTime.AddMinutes(r.DurationMinutes).Add(GapBuffer)
                        && endTime > r.StartTime.Subtract(GapBuffer))
            .Select(r => r.TableNumber!.Value)
            .Distinct()
            .ToListAsync();

        return await dbContext.Tables
            .AsNoTracking()
            .Where(t => t.RestaurantId == restaurantId && !occupiedTableNumbers.Contains(t.TableNumber))
            .ToListAsync();
    }

    public async Task AddTableAsync(Table table)
    {
        ArgumentNullException.ThrowIfNull(table);
        var restaurantExists = await dbContext.Restaurants.AnyAsync(r => r.Id == table.RestaurantId);
        if (!restaurantExists)
            throw new KeyNotFoundException($"Restaurant with ID {table.RestaurantId} not found");

        await dbContext.Tables.AddAsync(table);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteTableAsync(Guid restaurantId, int tableNumber)
    {
        var table = await dbContext.Tables
                        .FirstOrDefaultAsync(t => t.RestaurantId == restaurantId && t.TableNumber == tableNumber)
                    ?? throw new KeyNotFoundException($"Table {tableNumber} for restaurant {restaurantId} not found");

        dbContext.Tables.Remove(table);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateTableSeatsAsync(Guid restaurantId, int tableNumber, int seats)
    {
        var table = await dbContext.Tables
                        .FirstOrDefaultAsync(t => t.RestaurantId == restaurantId && t.TableNumber == tableNumber)
                    ?? throw new KeyNotFoundException($"Table {tableNumber} for restaurant {restaurantId} not found");

        table.Seats = seats;
        await dbContext.SaveChangesAsync();
    }
}
