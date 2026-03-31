using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class WaiterRepository(RestaurantDbContext dbContext) : IWaiterRepository
{
    public async Task<IEnumerable<Waiter>> GetAllWaitersAsync() => await dbContext.Waiters
        .AsNoTracking()
        .Include(w => w.Restaurant)
        .ToListAsync();

    public async Task<Waiter> GetWaiterByUserIdAsync(string userId) =>
        await dbContext.Waiters
            .AsNoTracking()
            .Include(w => w.Restaurant)
            .FirstOrDefaultAsync(w => w.UserId == userId)
        ?? throw new KeyNotFoundException($"Waiter with user ID '{userId}' not found");

    public async Task AddWaiterAsync(Waiter waiter)
    {
        ArgumentNullException.ThrowIfNull(waiter);
        await dbContext.Waiters.AddAsync(waiter);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateWaiterRestaurantAsync(string userId, Guid? restaurantId)
    {
        var waiter = await dbContext.Waiters.FindAsync(userId)
            ?? throw new KeyNotFoundException($"Waiter with user ID '{userId}' not found");

        waiter.RestaurantId = restaurantId;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteWaiterAsync(string userId)
    {
        var waiter = await dbContext.Waiters.FindAsync(userId)
            ?? throw new KeyNotFoundException($"Waiter with user ID '{userId}' not found");

        dbContext.Waiters.Remove(waiter);
        await dbContext.SaveChangesAsync();
    }
}
