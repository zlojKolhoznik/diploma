using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Waiters;

public class CreateWaiterScheduleRequest
{
    [Required]
    [MaxLength(200)]
    public required string WaiterId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly ShiftStart { get; set; }

    [Required]
    public TimeOnly ShiftEnd { get; set; }
}

