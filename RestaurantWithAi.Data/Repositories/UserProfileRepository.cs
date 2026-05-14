using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class UserProfileRepository(RestaurantDbContext dbContext) : IUserProfileRepository
{
    public async Task<UserProfile?> GetByUserIdAsync(string userId) =>
        await dbContext.Set<UserProfile>().AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task UpsertAsync(UserProfile profile)
    {
        var existing = await dbContext.Set<UserProfile>().FindAsync(profile.UserId);
        if (existing is null)
        {
            await dbContext.Set<UserProfile>().AddAsync(profile);
        }
        else
        {
            existing.PhotoStorageKey = profile.PhotoStorageKey;
            dbContext.Set<UserProfile>().Update(existing);
        }
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}

