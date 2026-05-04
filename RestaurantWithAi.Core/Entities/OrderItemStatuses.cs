namespace RestaurantWithAi.Core.Entities;

public static class OrderItemStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly IReadOnlyList<string> AllStatuses = [Pending, Approved, Rejected];
}

