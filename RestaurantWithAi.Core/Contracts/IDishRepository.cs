using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IDishRepository
{
    Task<IEnumerable<Dish>> GetAllDishesAsync();
    Task<Dish> GetDishByIdAsync(Guid id);
    Task AddDishAsync(Dish dish);
    Task UpdateDishAsync(Dish dish);
    Task DeleteDishAsync(Guid id);
}