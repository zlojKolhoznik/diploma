using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IRestaurantRepository
{
    Task<IEnumerable<Restaurant>> GetAllRestaurantsAsync(string? city = null, DateTime? time = null, int? durationMinutes = null);
    Task<Restaurant> GetRestaurantByIdAsync(Guid id, DateTime? time = null, int? durationMinutes = null);
    Task AddRestaurantAsync(Restaurant restaurant);
    Task UpdateRestaurantAsync(Restaurant restaurant);
    Task DeleteRestaurantAsync(Guid id);
}

