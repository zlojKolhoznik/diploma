using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationTimeRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Range(15, 720)]
    public int ApproximateDurationMinutes { get; set; }
}

