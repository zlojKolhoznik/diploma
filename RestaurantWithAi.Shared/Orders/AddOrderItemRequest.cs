using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Orders;

public class AddOrderItemRequest
{
    [Required]
    public Guid DishId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }
}

