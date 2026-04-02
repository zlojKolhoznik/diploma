namespace RestaurantWithAi.Shared.Orders;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid ReservationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public IReadOnlyCollection<OrderItemResponse> Items { get; set; } = [];
    public decimal TotalAmount => Items.Sum(i => i.LineTotal);
}

