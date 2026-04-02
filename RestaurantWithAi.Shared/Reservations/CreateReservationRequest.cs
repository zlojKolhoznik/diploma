using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class CreateReservationRequest
{
    [Required]
    public Guid RestaurantId { get; set; }

    public string? GuestId { get; set; }

    public string? GuestName { get; set; }

    [Required]
    public DateTimeOffset StartTime { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DurationMinutes must be at least 1.")]
    public int DurationMinutes { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "NumberOfGuests must be at least 1.")]
    public int NumberOfGuests { get; set; }
}
