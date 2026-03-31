using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IWaiterRepository
{
    Task<IEnumerable<Waiter>> GetAllWaitersAsync();
    Task<Waiter> GetWaiterByUserIdAsync(string userId);
    Task AddWaiterAsync(Waiter waiter);
    Task UpdateWaiterRestaurantAsync(string userId, Guid? restaurantId);
    Task DeleteWaiterAsync(string userId);
}
