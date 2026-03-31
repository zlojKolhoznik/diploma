using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Dishes;

namespace RestaurantWithAi.Core.Services;

public class DishService(IDishRepository dishRepository, IMapper mapper) : IDishesService
{
    public async Task<IEnumerable<DishBrief>> GetDishesAsync()
    {
        var dishes = await dishRepository.GetAllDishesAsync();
        return mapper.Map<IEnumerable<DishBrief>>(dishes);
    }

    public async Task<DishDetail> GetDishDetailAsync(Guid id)
    {
        var dish = await dishRepository.GetDishByIdAsync(id);
        return mapper.Map<DishDetail>(dish);
    }

    public async Task CreateDishAsync(CreateDishRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var dish = mapper.Map<Dish>(request);
        await dishRepository.AddDishAsync(dish);
    }

    public async Task UpdateDishAsync(Guid id, CreateDishRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var dish = mapper.Map<Dish>(request);
        dish.Id = id;
        await dishRepository.UpdateDishAsync(dish);
    }

    public Task UpdateDishAvailabilityAsync(Guid id, IEnumerable<Guid> restaurantIds)
    {
        ArgumentNullException.ThrowIfNull(restaurantIds);
        return dishRepository.UpdateDishAvailabilityAsync(id, restaurantIds);
    }

    public Task DeleteDishAsync(Guid id)
    {
        return dishRepository.DeleteDishAsync(id);
    }
}