namespace RestaurantWithAi.Shared.Restaurants;

public interface IRestaurantsService
{
    Task<IEnumerable<RestaurantBrief>> GetRestaurantsAsync(string? city = null);
    Task<RestaurantDetail> GetRestaurantDetailAsync(Guid id);
    Task CreateRestaurantAsync(CreateRestaurantRequest request);
    Task UpdateRestaurantAsync(Guid id, CreateRestaurantRequest request);
    Task DeleteRestaurantAsync(Guid id);
}

