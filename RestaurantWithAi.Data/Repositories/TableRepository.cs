using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class TableRepository(RestaurantDbContext dbContext) : ITableRepository
{
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
