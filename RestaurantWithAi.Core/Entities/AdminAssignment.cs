using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Core.Entities;

public class AdminAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public required string AppointedById { get; set; }

    [MaxLength(200)]
    public required string AppointedUserId { get; set; }

    /// <summary>
    /// When set, this admin was appointed with access restricted to the specified restaurant.
    /// Mirrors the custom:restaurantId Cognito attribute that was set during appointment.
    /// </summary>
    public Guid? RestaurantId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
}



