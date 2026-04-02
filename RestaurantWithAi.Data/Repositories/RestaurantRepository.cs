using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Data.Repositories;

public class RestaurantRepository(RestaurantDbContext dbContext) : IRestaurantRepository
{
    private static readonly TimeSpan GapBuffer = TimeSpan.FromMinutes(15);

    public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync(string? city = null)
    {
        var restaurantsQuery = dbContext.Restaurants
            .AsNoTracking()
            .Include(r => r.AvailableDishes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim();

            restaurantsQuery = dbContext.Database.IsSqlServer()
                ? restaurantsQuery.Where(r => EF.Functions.Collate(r.City, "SQL_Latin1_General_CP1_CI_AS") == normalizedCity)
                : restaurantsQuery.Where(r => string.Equals(r.City, normalizedCity, StringComparison.OrdinalIgnoreCase));
        }

        return await restaurantsQuery.ToListAsync();
    }

    public async Task<Restaurant> GetRestaurantByIdAsync(Guid id) => await dbContext.Restaurants
                                                                    .AsNoTracking()
                                                                    .Include(r => r.AvailableDishes)
                                                                    .FirstOrDefaultAsync(r => r.Id == id)
                                                                ?? throw new KeyNotFoundException($"Restaurant with ID {id} not found");

    public async Task AddRestaurantAsync(Restaurant restaurant)
    {
        ArgumentNullException.ThrowIfNull(restaurant);
        await dbContext.Restaurants.AddAsync(restaurant);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateRestaurantAsync(Restaurant restaurant)
    {
        ArgumentNullException.ThrowIfNull(restaurant);
        var restaurantFromDb = await dbContext.Restaurants.FindAsync(restaurant.Id)
                               ?? throw new KeyNotFoundException($"Restaurant with ID {restaurant.Id} not found");

        restaurantFromDb.City = restaurant.City;
        restaurantFromDb.Address = restaurant.Address;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteRestaurantAsync(Guid id)
    {
        var restaurant = await dbContext.Restaurants.FindAsync(id)
                         ?? throw new KeyNotFoundException($"Restaurant with ID {id} not found");
        dbContext.Restaurants.Remove(restaurant);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> HasAvailableTablesAsync(Guid restaurantId, DateTimeOffset startTime, int durationMinutes)
    {
        var hasTables = await dbContext.Tables.AnyAsync(t => t.RestaurantId == restaurantId);
        if (!hasTables)
            return false;

        var endTime = startTime.AddMinutes(durationMinutes);

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

        var totalTableCount = await dbContext.Tables.CountAsync(t => t.RestaurantId == restaurantId);
        return occupiedTableNumbers.Count < totalTableCount;
    }
}

