namespace RestaurantWithAi.Core.Contracts;

public interface IAdminService
{
    Task AppointAdminAsync(string appointerId, string appointeeId);
    Task DemoteAdminAsync(string appointerId, string adminToDemoteId);
    Task<IEnumerable<string>> GetAppointedAdminsAsync(string appointerId);
    Task<string?> GetAppointedByAsync(string adminUserId);
}

