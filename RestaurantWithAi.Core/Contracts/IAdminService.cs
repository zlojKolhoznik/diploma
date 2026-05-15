namespace RestaurantWithAi.Core.Contracts;

public interface IAdminService
{
    /// <summary>
    /// Appoints a user as admin. Adds them to the Cognito Admin group.
    /// When <paramref name="restaurantId"/> is provided, the admin's access is scoped
    /// to that restaurant by setting the custom:restaurantId Cognito attribute.
    /// </summary>
    Task AppointAdminAsync(string appointerId, string appointeeId, Guid? restaurantId = null);
    Task DemoteAdminAsync(string demoterId, string adminToDemoteId);
    Task<IEnumerable<string>> GetAppointedAdminsAsync(string appointerId);
    Task<string?> GetAppointedByAsync(string adminUserId);
}

