using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class DishRepository(RestaurantDbContext dbContext) : IDishRepository
{
    public async Task<IEnumerable<Dish>> GetAllDishesAsync() => await dbContext.Dishes
        .Include(d => d.AvailableAtRestaurants)
        .ToListAsync();

    public async Task<Dish> GetDishByIdAsync(Guid id) => await dbContext.Dishes
                                                            .Include(d => d.AvailableAtRestaurants)
                                                            .FirstOrDefaultAsync(d => d.Id == id)
                                                        ?? throw new KeyNotFoundException($"Dish with ID {id} not found");

    public async Task AddDishAsync(Dish dish)
    {
        await dbContext.Dishes.AddAsync(dish);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateDishAsync(Dish dish)
    {
        ArgumentNullException.ThrowIfNull(dish);
        var dishFromDb = await dbContext.Dishes.FindAsync(dish.Id)
            ?? throw new KeyNotFoundException($"Dish with ID {dish.Id} not found");
        
        dishFromDb.Name = dish.Name;
        dishFromDb.Description = dish.Description;
        dishFromDb.Price = dish.Price;
        dishFromDb.ImageUrl = dish.ImageUrl;
        
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateDishAvailabilityAsync(Guid dishId, IEnumerable<Guid> restaurantIds)
    {
        var distinctRestaurantIds = restaurantIds.Distinct().ToList();
        var dish = await dbContext.Dishes
                       .Include(d => d.AvailableAtRestaurants)
                       .FirstOrDefaultAsync(d => d.Id == dishId)
                   ?? throw new KeyNotFoundException($"Dish with ID {dishId} not found");

        var restaurants = await dbContext.Restaurants
            .Where(r => distinctRestaurantIds.Contains(r.Id))
            .ToListAsync();

        if (restaurants.Count != distinctRestaurantIds.Count)
        {
            var foundRestaurantIds = restaurants.Select(r => r.Id).ToHashSet();
            var missingRestaurantIds = distinctRestaurantIds.Where(id => !foundRestaurantIds.Contains(id));
            throw new KeyNotFoundException($"Restaurant IDs not found: {string.Join(", ", missingRestaurantIds)}");
        }

        dish.AvailableAtRestaurants = restaurants;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteDishAsync(Guid id)
    {
        var dish = await dbContext.Dishes.FindAsync(id) 
                   ?? throw new KeyNotFoundException($"Dish with ID {id} not found");
        dbContext.Dishes.Remove(dish);
        await dbContext.SaveChangesAsync();
    }
}