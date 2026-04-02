using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class Reservation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Restaurant))]
    public Guid RestaurantId { get; set; }

    public int? TableNumber { get; set; }

    [MaxLength(200)]
    public string? GuestId { get; set; }

    [MaxLength(200)]
    public string? GuestName { get; set; }

    public DateTime StartTime { get; set; }

    public int ApproximateDurationMinutes { get; set; }

    public int NumberOfGuests { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = ReservationStatuses.Created;

    [MaxLength(200)]
    public string? AssignedWaiterId { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public Table? Table { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
