using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationTimeRequest
{
    [Required]
    public DateTimeOffset StartTime { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DurationMinutes must be at least 1.")]
    public int DurationMinutes { get; set; }
}
