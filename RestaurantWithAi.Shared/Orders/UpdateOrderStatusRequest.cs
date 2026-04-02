using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Orders;

public class UpdateOrderStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

