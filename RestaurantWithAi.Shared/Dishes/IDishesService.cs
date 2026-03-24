namespace RestaurantWithAi.Shared.Dishes;

public interface IDishesService
{
    Task<IEnumerable<DishBrief>> GetDishesAsync();
    Task<DishDetail> GetDishDetailAsync(Guid id);
    Task CreateDishAsync(CreateDishRequest request);
    Task UpdateDishAsync(Guid id, CreateDishRequest request);
    Task DeleteDishAsync(Guid id);
}