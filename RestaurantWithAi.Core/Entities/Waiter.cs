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
    
    // Admin assignments: admins this user appointed
    public ICollection<AdminAssignment> AdminsAppointed { get; set; } = new List<AdminAssignment>();
    
    // Admin assignments: who appointed this admin
    public ICollection<AdminAssignment> AppointedBy { get; set; } = new List<AdminAssignment>();
}
