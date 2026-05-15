using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class Waiter
{
    [Key]
    public required string UserId { get; set; }

    [ForeignKey(nameof(Restaurant))]
    public Guid? RestaurantId { get; set; }

    public decimal? AverageRating { get; set; }

    public Restaurant? Restaurant { get; set; }

    public ICollection<WaiterSchedule> Schedules { get; set; } = new List<WaiterSchedule>();
}

