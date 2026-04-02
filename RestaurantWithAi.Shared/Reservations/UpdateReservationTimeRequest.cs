using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class UpdateReservationTimeRequest
{
    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DurationMinutes must be a positive integer.")]
    public int DurationMinutes { get; set; }
}
