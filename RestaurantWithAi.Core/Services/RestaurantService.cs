using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Services;

public class RestaurantService(IRestaurantRepository restaurantRepository, ITableRepository tableRepository, IMapper mapper) : IRestaurantsService
{
    public async Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null, DateTime? time = null, int? durationMinutes = null)
    {
        ValidateAvailabilityArguments(time, durationMinutes);

        var restaurants = (await restaurantRepository.GetAllRestaurantsAsync(city, time, durationMinutes)).ToList();
        var result = mapper.Map<List<RestaurantBrief>>(restaurants);

        if (time.HasValue && durationMinutes.HasValue)
        {
            for (var index = 0; index < restaurants.Count; index++)
            {
                result[index].HasAvailablePlaces = await tableRepository.HasAvailableTablesAsync(
                    restaurants[index].Id,
                    EnsureUtc(time.Value),
                    durationMinutes.Value);
            }
        }

        return result;
    }

    public async Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id, DateTime? time = null, int? durationMinutes = null)
    {
        ValidateAvailabilityArguments(time, durationMinutes);

        var restaurant = await restaurantRepository.GetRestaurantByIdAsync(id, time, durationMinutes);
        var result = mapper.Map<RestaurantDetail>(restaurant);

        if (time.HasValue && durationMinutes.HasValue)
        {
            result.HasAvailablePlaces = await tableRepository.HasAvailableTablesAsync(
                id,
                EnsureUtc(time.Value),
                durationMinutes.Value);
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

    private static void ValidateAvailabilityArguments(DateTime? time, int? durationMinutes)
    {
        if (time.HasValue && !durationMinutes.HasValue)
            throw new ArgumentException("Query parameter 'duration' is required when 'time' is provided.");

        if (!time.HasValue && durationMinutes.HasValue)
            throw new ArgumentException("Query parameter 'time' is required when 'duration' is provided.");
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime.ToUniversalTime()
        };
    }
}

