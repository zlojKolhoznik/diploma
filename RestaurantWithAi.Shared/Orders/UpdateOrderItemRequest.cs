using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Orders;

public class UpdateOrderItemRequest
{
    [Range(1, 100)]
    public int Quantity { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }
}

