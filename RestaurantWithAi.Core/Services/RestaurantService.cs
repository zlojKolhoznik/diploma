using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Services;

public class RestaurantService(IRestaurantRepository restaurantRepository, IMapper mapper) : IRestaurantsService
{
    public async Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null, DateTimeOffset? time = null, int? durationMinutes = null)
    {
        var restaurants = await restaurantRepository.GetAllRestaurantsAsync(city);
        var result = mapper.Map<IEnumerable<RestaurantBrief>>(restaurants).ToList();

        if (time.HasValue && durationMinutes.HasValue)
        {
            foreach (var brief in result)
            {
                brief.HasAvailablePlaces = await restaurantRepository.HasAvailableTablesAsync(brief.Id, time.Value, durationMinutes.Value);
            }
        }

        return result;
    }

    public async Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id, DateTimeOffset? time = null, int? durationMinutes = null)
    {
        var restaurant = await restaurantRepository.GetRestaurantByIdAsync(id);
        var result = mapper.Map<RestaurantDetail>(restaurant);

        if (time.HasValue && durationMinutes.HasValue)
        {
            result.HasAvailablePlaces = await restaurantRepository.HasAvailableTablesAsync(id, time.Value, durationMinutes.Value);
        }

        return result;
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

