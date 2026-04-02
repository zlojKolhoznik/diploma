using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RestaurantWithAi.Shared.Reservations;

namespace RestaurantWithAi.Core.Entities;

public class Reservation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Restaurant))]
    public Guid RestaurantId { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public int TableNumber { get; set; }

    public Table Table { get; set; } = null!;

    public string? GuestUserId { get; set; }

    [Required]
    public required string GuestName { get; set; }

    public string? AssignedWaiterId { get; set; }

    public Waiter? AssignedWaiter { get; set; }

    public DateTime StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public int NumberOfGuests { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Created;
}
