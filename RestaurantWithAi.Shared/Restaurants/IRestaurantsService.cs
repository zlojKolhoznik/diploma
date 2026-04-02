namespace RestaurantWithAi.Shared.Restaurants;

public interface IRestaurantsService
{
    Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null, DateTimeOffset? time = null, int? durationMinutes = null);
    Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id, DateTimeOffset? time = null, int? durationMinutes = null);
    Task CreateRestaurantAsync(CreateRestaurantRequest request);
    Task UpdateRestaurantAsync(Guid id, CreateRestaurantRequest request);
    Task DeleteRestaurantAsync(Guid id);
}

