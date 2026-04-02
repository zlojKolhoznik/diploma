using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reservations;

public class CreateReservationRequest
{
    [Required]
    public Guid RestaurantId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "TableNumber must be a positive integer.")]
    public int TableNumber { get; set; }

    /// <summary>
    /// Guest name is optional for Customer-initiated reservations (the guest's name can be inferred from their account)
    /// and required for Admin-initiated reservations (walk-in guests or on behalf of guests).
    /// The controller enforces this distinction based on the caller's role.
    /// </summary>
    public string? GuestName { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DurationMinutes must be a positive integer.")]
    public int DurationMinutes { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "NumberOfGuests must be a positive integer.")]
    public int NumberOfGuests { get; set; }
}
