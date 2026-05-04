using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Orders;

public class RejectOrderItemRequest
{
    [Required]
    [MaxLength(500)]
    public required string RejectionReason { get; set; }
}

