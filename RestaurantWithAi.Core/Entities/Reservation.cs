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

    public int? TableNumber { get; set; }

    public string? GuestId { get; set; }

    public string? GuestName { get; set; }

    public string? WaiterId { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public int NumberOfGuests { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Created;
}
