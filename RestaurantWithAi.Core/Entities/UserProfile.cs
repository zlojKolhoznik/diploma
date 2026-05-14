using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Core.Entities;

public class UserProfile
{
    [Key]
    public string UserId { get; set; } = string.Empty;

    // S3 storage key for the user's profile image
    public string? PhotoStorageKey { get; set; }
}

