namespace RestaurantWithAi.Shared.Waiters;

public interface IWaitersRepository
{
    Task<IEnumerable<WaiterDto>> GetAllAsync();
    Task<WaiterDto?> GetByUserIdAsync(string userId);
    Task<WaiterDto> CreateAsync(string userId);
    Task<WaiterDto?> AssignRestaurantAsync(string userId, string restaurantId);
}
