using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class WaiterSchedule
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Waiter))]
    [MaxLength(200)]
    public required string WaiterId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly ShiftStart { get; set; }

    public TimeOnly ShiftEnd { get; set; }

    public Waiter Waiter { get; set; } = null!;
}

