using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class RestaurantRepository(RestaurantDbContext dbContext) : IRestaurantRepository
{
    public async Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync(string? city = null)
    {
        var restaurantsQuery = dbContext.Restaurants
            .Include(r => r.AvailableDishes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim();
            var normalizedCityUpper = normalizedCity.ToUpper();
            restaurantsQuery = restaurantsQuery.Where(r => r.City.Equals(normalizedCityUpper, StringComparison.InvariantCultureIgnoreCase));
        }

        return await restaurantsQuery.ToListAsync();
    }

    public async Task<Restaurant> GetRestaurantByIdAsync(Guid id) => await dbContext.Restaurants
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
}

