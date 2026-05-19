namespace RestaurantWithAi.Shared.Admins;

public class AdminUserListItemResponse
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Role { get; set; }
    public string? RestaurantId { get; set; }
    public string? RestaurantAddress { get; set; }
}
