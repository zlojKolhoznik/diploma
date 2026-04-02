using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationWaiterRequest
{
    [Required]
    public required string WaiterId { get; set; }
}
