using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationAssignedWaiterRequest
{
    [Required]
    [MaxLength(200)]
    public string AssignedWaiterId { get; set; } = string.Empty;
}

