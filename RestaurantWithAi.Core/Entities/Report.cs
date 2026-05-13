using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Core.Entities;

public class Report
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Format { get; set; } = string.Empty;

    public Guid? RestaurantId { get; set; }

    [MaxLength(200)]
    public string GeneratedById { get; set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? StorageKey { get; set; }

    public string? AnalysisText { get; set; }
}

