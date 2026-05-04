using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantWithAi.Core.Entities;

public class OrderItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(Dish))]
    public Guid DishId { get; set; }

    [MaxLength(150)]
    public string DishName { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    [MaxLength(300)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = OrderItemStatuses.Approved;

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public Order Order { get; set; } = null!;
    public Dish Dish { get; set; } = null!;
}

