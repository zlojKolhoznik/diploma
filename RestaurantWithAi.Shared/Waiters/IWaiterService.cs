namespace RestaurantWithAi.Shared.Waiters;

public interface IWaiterService
{
    Task<IEnumerable<WaiterResponse>> GetAllWaitersAsync();
    Task AssignWaiterRoleAsync(string userId);
    Task AssignWaiterToRestaurantAsync(string restaurantId, string userId);
}
