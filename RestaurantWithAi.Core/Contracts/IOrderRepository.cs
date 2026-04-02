using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IOrderRepository
{
    Task<Reservation> GetReservationForScopeAsync(Guid restaurantId, Guid reservationId);
    Task<IEnumerable<Order>> GetOrdersForReservationAsync(Guid restaurantId, Guid reservationId);
    Task<Order> GetOrderByIdAsync(Guid restaurantId, Guid reservationId, Guid orderId);
    Task<OrderItem> GetOrderItemByIdAsync(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId);
    Task<bool> HasOpenOrderForReservationAsync(Guid restaurantId, Guid reservationId);
    Task<bool> IsDishAvailableAtRestaurantAsync(Guid restaurantId, Guid dishId);
    Task<Dish> GetDishByIdAsync(Guid dishId);
    Task AddOrderAsync(Order order);
    Task SaveChangesAsync();
}

