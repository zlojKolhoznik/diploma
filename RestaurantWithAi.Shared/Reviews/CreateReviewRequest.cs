using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Reviews;

public class CreateReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int CuisineRating { get; set; }

    [MaxLength(1000)]
    public string? CuisineComment { get; set; }

    [Required]
    [Range(1, 5)]
    public int ServiceRating { get; set; }

    [MaxLength(1000)]
    public string? ServiceComment { get; set; }
}

