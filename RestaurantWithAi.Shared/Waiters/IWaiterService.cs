namespace RestaurantWithAi.Shared.Waiters;

public interface IWaiterService
{
    Task<IEnumerable<WaiterDto>> GetAllWaitersAsync();
    Task<WaiterDto> AssignWaiterRoleAsync(string userId, int restaurantId);
    Task<WaiterDto> AssignWaiterToRestaurantAsync(string userId, int restaurantId);
}
