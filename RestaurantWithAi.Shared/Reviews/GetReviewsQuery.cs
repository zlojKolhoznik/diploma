namespace RestaurantWithAi.Shared.Reviews;

public class GetReviewsQuery
{
    public string? Component { get; set; }   // "cuisine" | "service" | null (both)
    public string? SortBy { get; set; }       // "date" | "rating"
    public string? SortOrder { get; set; }    // "asc" | "desc"
}

