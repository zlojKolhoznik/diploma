using Microsoft.EntityFrameworkCore;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Data.Repositories;

public class OrderRepository(RestaurantDbContext dbContext) : IOrderRepository
{
    public async Task<Reservation> GetReservationForScopeAsync(Guid restaurantId, Guid reservationId) =>
        await dbContext.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.RestaurantId == restaurantId)
        ?? throw new KeyNotFoundException($"Reservation with ID {reservationId} for restaurant {restaurantId} not found.");

    public async Task<IEnumerable<Order>> GetOrdersForReservationAsync(Guid restaurantId, Guid reservationId) =>
        await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.RestaurantId == restaurantId && o.ReservationId == reservationId)
            .OrderBy(o => o.CreatedAtUtc)
            .ToListAsync();

    public async Task<Order> GetOrderByIdAsync(Guid restaurantId, Guid reservationId, Guid orderId) =>
        await dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId &&
                o.RestaurantId == restaurantId &&
                o.ReservationId == reservationId)
        ?? throw new KeyNotFoundException($"Order with ID {orderId} not found for reservation {reservationId}.");

    public async Task<OrderItem> GetOrderItemByIdAsync(Guid restaurantId, Guid reservationId, Guid orderId, Guid itemId) =>
        await dbContext.OrderItems
            .Include(i => i.Order)
            .FirstOrDefaultAsync(i =>
                i.Id == itemId &&
                i.OrderId == orderId &&
                i.Order.RestaurantId == restaurantId &&
                i.Order.ReservationId == reservationId)
        ?? throw new KeyNotFoundException($"Order item with ID {itemId} not found for order {orderId}.");

    public Task<bool> HasOpenOrderForReservationAsync(Guid restaurantId, Guid reservationId) =>
        dbContext.Orders.AnyAsync(o =>
            o.RestaurantId == restaurantId &&
            o.ReservationId == reservationId &&
            OrderStatuses.OpenStatuses.Contains(o.Status));

    public async Task<bool> IsDishAvailableAtRestaurantAsync(Guid restaurantId, Guid dishId) =>
        await dbContext.Restaurants
            .AsNoTracking()
            .Where(r => r.Id == restaurantId)
            .AnyAsync(r => r.AvailableDishes.Any(d => d.Id == dishId));

    public async Task<Dish> GetDishByIdAsync(Guid dishId) =>
        await dbContext.Dishes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dishId)
        ?? throw new KeyNotFoundException($"Dish with ID {dishId} not found.");

    public async Task AddOrderAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }
}

