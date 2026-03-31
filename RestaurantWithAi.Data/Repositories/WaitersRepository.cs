using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Data.Models;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Data.Repositories;

public class WaitersRepository(AppDbContext dbContext) : IWaitersRepository
{
    public async Task<IEnumerable<WaiterDto>> GetAllAsync()
    {
        var waiters = await dbContext.Waiters.ToListAsync();
        return waiters.Select(ToDto);
    }

    public async Task<WaiterDto?> GetByUserIdAsync(string userId)
    {
        var waiter = await dbContext.Waiters
            .FirstOrDefaultAsync(w => w.UserId == userId);

        return waiter is null ? null : ToDto(waiter);
    }

    public async Task<WaiterDto> CreateAsync(string userId)
    {
        var waiter = new Waiter { UserId = userId };
        dbContext.Waiters.Add(waiter);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException($"A waiter with userId '{userId}' already exists.");
        }

        return ToDto(waiter);
    }

    public async Task<WaiterDto?> AssignRestaurantAsync(string userId, string restaurantId)
    {
        var waiter = await dbContext.Waiters
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (waiter is null)
        {
            return null;
        }

        waiter.RestaurantId = restaurantId;
        await dbContext.SaveChangesAsync();

        return ToDto(waiter);
    }

    private static WaiterDto ToDto(Waiter waiter) => new()
    {
        Id = waiter.Id,
        UserId = waiter.UserId,
        RestaurantId = waiter.RestaurantId
    };
}
