using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Restaurant))]
    public Guid RestaurantId { get; set; }

    [ForeignKey(nameof(Reservation))]
    public Guid ReservationId { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = OrderStatuses.Created;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    public Restaurant Restaurant { get; set; } = null!;
    public Reservation Reservation { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

