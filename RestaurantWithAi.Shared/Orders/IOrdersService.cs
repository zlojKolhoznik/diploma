namespace RestaurantWithAi.Shared.Orders;

public interface IOrdersService
{
    Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid restaurantId, Guid reservationId, string currentUserId, bool isAdmin);
    Task<OrderResponse> GetOrderByIdAsync(Guid restaurantId, Guid reservationId, Guid orderId, string currentUserId, bool isAdmin);
    Task CreateOrderAsync(Guid restaurantId, Guid reservationId, CreateOrderRequest request, string currentUserId, bool isAdmin);
    Task UpdateOrderStatusAsync(Guid restaurantId, Guid reservationId, Guid orderId, UpdateOrderStatusRequest request, string currentUserId, bool isAdmin);
    Task AddOrderItemAsync(Guid restaurantId, Guid reservationId, Guid orderId, AddOrderItemRequest request, string currentUserId, bool isAdmin);
    Task UpdateOrderItemAsync(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId, UpdateOrderItemRequest request, string currentUserId, bool isAdmin);
    Task RemoveOrderItemAsync(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId, string currentUserId, bool isAdmin);
    Task CloseOrderAsync(Guid restaurantId, Guid reservationId, Guid orderId, string currentUserId, bool isAdmin);
}

