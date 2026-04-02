using AutoMapper;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Orders;

namespace RestaurantWithAi.Core.Services;

public class OrderService(IOrderRepository orderRepository, IMapper mapper) : IOrdersService
{
    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid restaurantId, Guid reservationId, string currentUserId, bool isAdmin)
    {
        _ = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);
        var orders = await orderRepository.GetOrdersForReservationAsync(restaurantId, reservationId);
        return mapper.Map<IEnumerable<OrderResponse>>(orders);
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid restaurantId, Guid reservationId, Guid orderId, string currentUserId, bool isAdmin)
    {
        _ = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);
        var order = await orderRepository.GetOrderByIdAsync(restaurantId, reservationId, orderId);
        return mapper.Map<OrderResponse>(order);
    }

    public async Task CreateOrderAsync(Guid restaurantId, Guid reservationId, CreateOrderRequest request, string currentUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);
        EnsureReservationCanAcceptOrders(reservation);

        if (await orderRepository.HasOpenOrderForReservationAsync(restaurantId, reservationId))
            throw new InvalidOperationException("An active order already exists for this reservation.");

        var order = mapper.Map<Order>(request);
        order.RestaurantId = restaurantId;
        order.ReservationId = reservationId;
        order.Status = OrderStatuses.Created;
        order.CreatedAtUtc = DateTime.UtcNow;

        foreach (var item in request.Items)
            order.Items.Add(await BuildOrderItemAsync(restaurantId, item));

        await orderRepository.AddOrderAsync(order);
    }

    public async Task UpdateOrderStatusAsync(Guid restaurantId, Guid reservationId, Guid orderId, UpdateOrderStatusRequest request, string currentUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        _ = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);
        var order = await orderRepository.GetOrderByIdAsync(restaurantId, reservationId, orderId);

        var targetStatus = request.Status.Trim();
        if (!OrderStatuses.Flow.Contains(targetStatus, StringComparer.Ordinal) &&
            !string.Equals(targetStatus, OrderStatuses.Cancelled, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Unsupported status '{targetStatus}'.", nameof(request));
        }

        if (!OrderStatuses.CanTransition(order.Status, targetStatus))
            throw new InvalidOperationException($"Invalid status transition from '{order.Status}' to '{targetStatus}'.");

        order.Status = targetStatus;

        if (string.Equals(targetStatus, OrderStatuses.Closed, StringComparison.Ordinal) ||
            string.Equals(targetStatus, OrderStatuses.Cancelled, StringComparison.Ordinal))
        {
            order.ClosedAtUtc = DateTime.UtcNow;
        }

        await orderRepository.SaveChangesAsync();
    }

    public async Task AddOrderItemAsync(Guid restaurantId, Guid reservationId, Guid orderId, AddOrderItemRequest request, string currentUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservation = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);
        EnsureReservationCanAcceptOrders(reservation);

        var order = await orderRepository.GetOrderByIdAsync(restaurantId, reservationId, orderId);
        EnsureOrderIsEditable(order);

        order.Items.Add(await BuildOrderItemAsync(restaurantId, request));
        await orderRepository.SaveChangesAsync();
    }

    public async Task UpdateOrderItemAsync(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId, UpdateOrderItemRequest request, string currentUserId, bool isAdmin)
    {
        ArgumentNullException.ThrowIfNull(request);

        _ = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);

        var order = await orderRepository.GetOrderByIdAsync(restaurantId, reservationId, orderId);
        EnsureOrderIsEditable(order);

        var item = await orderRepository.GetOrderItemByIdAsync(restaurantId, reservationId, orderId, itemId);
        item.Quantity = request.Quantity;
        item.Notes = request.Notes?.Trim();

        await orderRepository.SaveChangesAsync();
    }

    public async Task RemoveOrderItemAsync(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId, string currentUserId, bool isAdmin)
    {
        _ = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);

        var order = await orderRepository.GetOrderByIdAsync(restaurantId, reservationId, orderId);
        EnsureOrderIsEditable(order);

        var item = await orderRepository.GetOrderItemByIdAsync(restaurantId, reservationId, orderId, itemId);
        order.Items.Remove(item);

        await orderRepository.SaveChangesAsync();
    }

    public async Task CloseOrderAsync(Guid restaurantId, Guid reservationId, Guid orderId, string currentUserId, bool isAdmin)
    {
        _ = await GetAccessibleReservationAsync(restaurantId, reservationId, currentUserId, isAdmin);

        var order = await orderRepository.GetOrderByIdAsync(restaurantId, reservationId, orderId);
        if (!OrderStatuses.CanTransition(order.Status, OrderStatuses.Closed))
            throw new InvalidOperationException($"Order must be in '{OrderStatuses.Served}' status before closing.");

        order.Status = OrderStatuses.Closed;
        order.ClosedAtUtc = DateTime.UtcNow;

        await orderRepository.SaveChangesAsync();
    }

    private async Task<Reservation> GetAccessibleReservationAsync(Guid restaurantId, Guid reservationId, string currentUserId, bool isAdmin)
    {
        var reservation = await orderRepository.GetReservationForScopeAsync(restaurantId, reservationId);

        if (!isAdmin)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currentUserId);

            if (!string.IsNullOrWhiteSpace(reservation.AssignedWaiterId) &&
                !string.Equals(reservation.AssignedWaiterId, currentUserId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("You can only manage orders for your assigned reservations.");
            }
        }

        return reservation;
    }

    private static void EnsureReservationCanAcceptOrders(Reservation reservation)
    {
        if (!ReservationStatuses.OpenStatuses.Contains(reservation.Status))
            throw new InvalidOperationException("Reservation is closed and cannot accept order changes.");
    }

    private static void EnsureOrderIsEditable(Order order)
    {
        if (!OrderStatuses.EditableStatuses.Contains(order.Status))
            throw new InvalidOperationException("Order is not editable in its current status.");
    }

    private async Task<OrderItem> BuildOrderItemAsync(Guid restaurantId, AddOrderItemRequest request)
    {
        if (!await orderRepository.IsDishAvailableAtRestaurantAsync(restaurantId, request.DishId))
            throw new KeyNotFoundException($"Dish with ID {request.DishId} is not available for restaurant {restaurantId}.");

        var dish = await orderRepository.GetDishByIdAsync(request.DishId);

        return new OrderItem
        {
            DishId = dish.Id,
            DishName = dish.Name,
            Quantity = request.Quantity,
            UnitPrice = dish.Price,
            Notes = request.Notes?.Trim()
        };
    }
}

