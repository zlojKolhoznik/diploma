namespace RestaurantWithAi.Shared.Reservations;

public class ReservationResponse
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public int TableNumber { get; set; }
    public string? GuestUserId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? AssignedWaiterId { get; set; }
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public int NumberOfGuests { get; set; }
    public ReservationStatus Status { get; set; }
}
