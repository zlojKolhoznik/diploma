using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Orders;

public class CreateOrderRequest
{
    [MaxLength(500)]
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<AddOrderItemRequest> Items { get; set; } = [];
}

