using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationTimeRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Range(30, 600)]
    public int ApproximateDurationMinutes { get; set; }
}

