using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Contracts;

public interface IAdminAssignmentRepository
{
    Task<IEnumerable<AdminAssignment>> GetAppointedByAdminAsync(string appoininterId);
    Task<AdminAssignment?> GetAssignmentAsync(string appointedById, string appointedUserId);
    Task<AdminAssignment?> GetWhoAppointedAdminAsync(string adminUserId);
    Task AddAssignmentAsync(AdminAssignment assignment);
    Task DeleteAssignmentAsync(Guid assignmentId);
}

