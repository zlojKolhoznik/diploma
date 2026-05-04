using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;

namespace RestaurantWithAi.Core.Services;

public class AdminService(IAdminAssignmentRepository adminAssignmentRepository, IWaiterRepository waiterRepository) : IAdminService
{
    public async Task AppointAdminAsync(string appointerId, string appointeeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointeeId);

        if (appointerId == appointeeId)
            throw new InvalidOperationException("An admin cannot appoint themselves.");

        // Verify both users exist as waiters
        var appointer = await waiterRepository.GetWaiterByUserIdAsync(appointerId);
        if (appointer == null)
            throw new KeyNotFoundException($"Appointer with ID '{appointerId}' not found");

        var appointee = await waiterRepository.GetWaiterByUserIdAsync(appointeeId);
        if (appointee == null)
            throw new KeyNotFoundException($"Appointee with ID '{appointeeId}' not found");

        // Check if the appointer has a restricted restaurant assignment
        if (appointer.RestaurantId.HasValue)
            throw new UnauthorizedAccessException("Admins with assigned restaurants cannot appoint other admins.");

        // Check if there's already an assignment
        var existingAssignment = await adminAssignmentRepository.GetAssignmentAsync(appointerId, appointeeId);
        if (existingAssignment != null)
            throw new InvalidOperationException($"Admin '{appointeeId}' is already appointed by '{appointerId}'");

        var assignment = new AdminAssignment
        {
            AppointedById = appointerId,
            AppointedUserId = appointeeId,
            AssignedAtUtc = DateTime.UtcNow
        };

        await adminAssignmentRepository.AddAssignmentAsync(assignment);
    }

    public async Task DemoteAdminAsync(string appointerId, string adminToDemoteId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminToDemoteId);

        // Find the assignment
        var assignment = await adminAssignmentRepository.GetAssignmentAsync(appointerId, adminToDemoteId)
            ?? throw new KeyNotFoundException($"No appointment relationship found between '{appointerId}' and '{adminToDemoteId}'");

        // Check if the person being demoted appointed the one trying to demote them
        var reverseAssignment = await adminAssignmentRepository.GetAssignmentAsync(adminToDemoteId, appointerId);
        if (reverseAssignment != null)
            throw new UnauthorizedAccessException("An admin cannot demote the admin who appointed them.");

        await adminAssignmentRepository.DeleteAssignmentAsync(assignment.Id);
    }

    public async Task<IEnumerable<string>> GetAppointedAdminsAsync(string appointerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointerId);
        var assignments = await adminAssignmentRepository.GetAppointedByAdminAsync(appointerId);
        return assignments.Select(aa => aa.AppointedUserId).ToList();
    }

    public async Task<string?> GetAppointedByAsync(string adminUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminUserId);
        var assignment = await adminAssignmentRepository.GetWhoAppointedAdminAsync(adminUserId);
        return assignment?.AppointedById;
    }
}

