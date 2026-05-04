namespace RestaurantWithAi.Shared.Reviews;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public Guid ReservationId { get; set; }
    public int CuisineRating { get; set; }
    public string? CuisineComment { get; set; }
    public int ServiceRating { get; set; }
    public string? ServiceComment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

