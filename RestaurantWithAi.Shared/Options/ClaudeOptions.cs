using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Options;

public sealed class ClaudeOptions
{
    public const string SectionName = "Claude";

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-3-5-haiku-20241022";

    public int MaxTokens { get; set; } = 1024;
}

