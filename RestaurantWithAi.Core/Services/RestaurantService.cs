using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Services;

public class RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper) : IRestaurantsService
{
    public async Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null)
    {
        var restaurants = await restaurantRepository.GetAllRestaurantsAsync(city);
        return mapper.Map<IEnumerable<RestaurantBrief>>(restaurants);
    }

    public async Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id)
    {
        var restaurant = await restaurantRepository.GetRestaurantByIdAsync(id);
        return mapper.Map<RestaurantDetail>(restaurant);
    }

    public async Task CreateRestaurantAsync(CreateRestaurantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var restaurant = mapper.Map<Restaurant>(request);
        await restaurantRepository.AddRestaurantAsync(restaurant);
    }

    public async Task UpdateRestaurantAsync(Guid id, CreateRestaurantRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var restaurant = mapper.Map<Restaurant>(request);
        restaurant.Id = id;
        await restaurantRepository.UpdateRestaurantAsync(restaurant);
    }

    public Task DeleteRestaurantAsync(Guid id)
    {
        return restaurantRepository.DeleteRestaurantAsync(id);
    }
}

