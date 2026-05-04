using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class AdminAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(AppointedBy))]
    [MaxLength(200)]
    public required string AppointedById { get; set; }

    [ForeignKey(nameof(AppointedUser))]
    [MaxLength(200)]
    public required string AppointedUserId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Waiter? AppointedBy { get; set; }
    public Waiter? AppointedUser { get; set; }
}

