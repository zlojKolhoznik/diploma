using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Services;

public class RestaurantService(IRestaurantRepository restaurantRepository, ITableRepository tableRepository, IMapper mapper) : IRestaurantsService
{
    public async Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null, DateTime? time = null, int? durationMinutes = null)
    {
        var restaurants = await restaurantRepository.GetAllRestaurantsAsync(city);
        var briefs = mapper.Map<IEnumerable<RestaurantBrief>>(restaurants).ToList();

        if (time.HasValue && durationMinutes.HasValue)
        {
            foreach (var brief in briefs)
            {
                var available = await tableRepository.GetAvailableTablesAsync(brief.Id, time.Value, durationMinutes.Value);
                brief.HasAvailablePlaces = available.Any();
            }
        }

        return briefs;
    }

    public async Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id, DateTime? time = null, int? durationMinutes = null)
    {
        var restaurant = await restaurantRepository.GetRestaurantByIdAsync(id);
        var detail = mapper.Map<RestaurantDetail>(restaurant);

        if (time.HasValue && durationMinutes.HasValue)
        {
            var available = await tableRepository.GetAvailableTablesAsync(id, time.Value, durationMinutes.Value);
            detail.HasAvailablePlaces = available.Any();
        }

        return detail;
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

