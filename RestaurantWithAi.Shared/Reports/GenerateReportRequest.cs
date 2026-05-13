using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reports;

public class GenerateReportRequest
{
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Format { get; set; } = string.Empty;

    public Guid? RestaurantId { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }
}

