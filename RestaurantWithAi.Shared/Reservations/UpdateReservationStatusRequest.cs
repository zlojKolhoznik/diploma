using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationStatusRequest
{
    [Required]
    public ReservationStatus Status { get; set; }
}
