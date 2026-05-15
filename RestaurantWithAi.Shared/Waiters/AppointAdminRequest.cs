using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Waiters;

public class AppointAdminRequest
{
    [Required]
    [MaxLength(200)]
    public required string AdminUserIdToAppoint { get; set; }

    /// <summary>
    /// Optional. When provided, the appointed admin will only have admin access
    /// to the specified restaurant (stored as custom:restaurantId in Cognito).
    /// </summary>
    public Guid? RestaurantId { get; set; }
}

