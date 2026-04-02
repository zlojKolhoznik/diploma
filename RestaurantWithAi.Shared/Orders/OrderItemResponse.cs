namespace RestaurantWithAi.Shared.Orders;

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid DishId { get; set; }
    public string DishName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

