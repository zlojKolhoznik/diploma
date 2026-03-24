using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IRestaurantRepository
{
    Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync(string? city = null);
    Task<Restaurant> GetRestaurantByIdAsync(Guid id);
    Task AddRestaurantAsync(Restaurant restaurant);
    Task UpdateRestaurantAsync(Restaurant restaurant);
    Task DeleteRestaurantAsync(Guid id);
}

