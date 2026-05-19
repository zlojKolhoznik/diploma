namespace RestaurantWithAi.Shared.Admins;

public class PagedAdminUsersResponse
{
    public required IReadOnlyList<AdminUserListItemResponse> Items { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalCount { get; set; }
    public required int TotalPages { get; set; }
}
