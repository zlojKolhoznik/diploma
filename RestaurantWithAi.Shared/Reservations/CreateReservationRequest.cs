using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class CreateReservationRequest
{
    [Required]
    public Guid RestaurantId { get; set; }

    [MaxLength(200)]
    public string? GuestId { get; set; }

    [MaxLength(200)]
    public string? GuestName { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Range(30, 600)]
    public int ApproximateDurationMinutes { get; set; }

    [Range(1, 100)]
    public int NumberOfGuests { get; set; }
}

