namespace RestaurantWithAi.Shared.Reservations;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public int? TableNumber { get; set; }
    public string? GuestId { get; set; }
    public string? GuestName { get; set; }
    public DateTime StartTime { get; set; }
    public int ApproximateDurationMinutes { get; set; }
    public int NumberOfGuests { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedWaiterId { get; set; }
}

