namespace RestaurantWithAi.Shared.Reservations;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public int? TableNumber { get; set; }
    public string? GuestId { get; set; }
    public string? GuestName { get; set; }
    public string? WaiterId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public int NumberOfGuests { get; set; }
    public ReservationStatus Status { get; set; }
}
