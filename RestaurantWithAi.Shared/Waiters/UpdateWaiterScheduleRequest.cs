using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Waiters;

public class UpdateWaiterScheduleRequest
{
    [Required]
    public TimeOnly ShiftStart { get; set; }

    [Required]
    public TimeOnly ShiftEnd { get; set; }
}

