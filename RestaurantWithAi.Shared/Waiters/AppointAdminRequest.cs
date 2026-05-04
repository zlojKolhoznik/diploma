using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Waiters;

public class AppointAdminRequest
{
    [Required]
    [MaxLength(200)]
    public required string AdminUserIdToAppoint { get; set; }
}

