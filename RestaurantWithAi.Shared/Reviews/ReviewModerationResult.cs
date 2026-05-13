using System.Text.Json.Serialization;

namespace RestaurantWithAi.Shared.Reviews;

public class ReviewModerationResult
{
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("suggestedRephrasing")]
    public string? SuggestedRephrasing { get; set; }
}

