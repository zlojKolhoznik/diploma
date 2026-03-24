using Microsoft.Extensions.Logging;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class DishRepository(RestaurantDbContext dbContext, ILogger<DishRepository> logger) : IDishRepository
{
    public Task<IEnumerable<Dish>> GetAllDishesAsync() => Task.FromResult<IEnumerable<Dish>>(dbContext.Dishes);

    public async Task<Dish> GetDishByIdAsync(Guid id) => await dbContext.Dishes.FindAsync(id) 
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

    public async Task DeleteDishAsync(Guid id)
    {
        var dish = await dbContext.Dishes.FindAsync(id) 
                   ?? throw new KeyNotFoundException($"Dish with ID {id} not found");
        dbContext.Dishes.Remove(dish);
        await dbContext.SaveChangesAsync();
    }
}