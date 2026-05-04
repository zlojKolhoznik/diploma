using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class Review
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Reservation))]
    public Guid ReservationId { get; set; }

    [Range(1, 5)]
    public int CuisineRating { get; set; }

    [MaxLength(1000)]
    public string? CuisineComment { get; set; }

    [Range(1, 5)]
    public int ServiceRating { get; set; }

    [MaxLength(1000)]
    public string? ServiceComment { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Reservation Reservation { get; set; } = null!;
}

