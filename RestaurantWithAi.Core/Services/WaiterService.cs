using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Data;
using RestaurantWithAi.Data.Entities;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Core.Services;

public class WaiterService(AppDbContext dbContext) : IWaiterService
{
    public async Task<IEnumerable<WaiterDto>> GetAllWaitersAsync()
    {
        return await dbContext.Waiters
            .Select(w => new WaiterDto { UserId = w.UserId, RestaurantId = w.RestaurantId })
            .ToListAsync();
    }

    public async Task<WaiterDto> AssignWaiterRoleAsync(string userId, int restaurantId)
    {
        var waiter = new Waiter
        {
            UserId = userId,
            RestaurantId = restaurantId
        };

        dbContext.Waiters.Add(waiter);
        await dbContext.SaveChangesAsync();

        return new WaiterDto { UserId = waiter.UserId, RestaurantId = waiter.RestaurantId };
    }

    public async Task<WaiterDto> AssignWaiterToRestaurantAsync(string userId, int restaurantId)
    {
        var waiter = await dbContext.Waiters.FirstOrDefaultAsync(w => w.UserId == userId)
            ?? throw new WaiterNotFoundException($"Waiter with user ID '{userId}' was not found.");

        waiter.RestaurantId = restaurantId;
        await dbContext.SaveChangesAsync();

        return new WaiterDto { UserId = waiter.UserId, RestaurantId = waiter.RestaurantId };
    }
}
