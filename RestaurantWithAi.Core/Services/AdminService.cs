using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Entities;
using RestaurantWithAi.Shared.Admins;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Options;

namespace RestaurantWithAi.Core.Services;

public class AdminService(
    IAdminAssignmentRepository adminAssignmentRepository,
    IAmazonCognitoIdentityProvider cognito,
    IOptions<AwsCognitoOptions> options,
    IRestaurantRepository restaurantRepository) : IAdminService
{
    private const string AdminGroupName = "Admin";
    private const string WaiterGroupName = "Waiter";
    private const string CustomerGroupName = "Customer";
    private const string RestaurantIdAttributeName = "custom:restaurantId";

    private readonly AwsCognitoOptions _options = options.Value;

    public async Task AppointAdminAsync(string appointerId, string appointeeId, Guid? restaurantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appointerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(appointeeId);

        if (appointerId == appointeeId)
            throw new InvalidOperationException("An admin cannot appoint themselves.");

        // Verify both users exist in Cognito
        await EnsureCognitoUserExistsAsync(appointerId);
        await EnsureCognitoUserExistsAsync(appointeeId);

        // Restaurant-scoped admins cannot appoint other admins:
        // check if the appointer was themselves appointed with a restaurant scope.
        var appointerAssignment = await adminAssignmentRepository.GetWhoAppointedAdminAsync(appointerId);
        if (appointerAssignment?.RestaurantId.HasValue == true)
            throw new UnauthorizedAccessException("Admins with assigned restaurants cannot appoint other admins.");

        // Check if there's already an assignment
        var existingAssignment = await adminAssignmentRepository.GetAssignmentAsync(appointerId, appointeeId);
        if (existingAssignment != null)
            throw new InvalidOperationException($"Admin '{appointeeId}' is already appointed by '{appointerId}'.");

        // Add user to Cognito Admin group
        await AddUserToAdminGroupAsync(appointeeId);

        // If restaurant scoping is requested, set the custom:restaurantId attribute in Cognito
        if (restaurantId.HasValue)
            await SetCognitoRestaurantAttributeAsync(appointeeId, restaurantId.Value.ToString());

        // Track the assignment in DB
        var assignment = new AdminAssignment
        {
            AppointedById = appointerId,
            AppointedUserId = appointeeId,
            RestaurantId = restaurantId,
            AssignedAtUtc = DateTime.UtcNow
        };

        await adminAssignmentRepository.AddAssignmentAsync(assignment);
    }

    public async Task DemoteAdminAsync(string demoterId, string adminToDemoteId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(demoterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminToDemoteId);

        // Find the assignment created by the demoter
        var assignment = await adminAssignmentRepository.GetAssignmentAsync(demoterId, adminToDemoteId)
            ?? throw new KeyNotFoundException($"No appointment relationship found between '{demoterId}' and '{adminToDemoteId}'.");

        // An admin cannot demote the admin who appointed them
        var reverseAssignment = await adminAssignmentRepository.GetAssignmentAsync(adminToDemoteId, demoterId);
        if (reverseAssignment != null)
            throw new UnauthorizedAccessException("An admin cannot demote the admin who appointed them.");

        // Remove from Cognito Admin group
        await RemoveUserFromAdminGroupAsync(adminToDemoteId);

        // Clear the custom:restaurantId Cognito attribute if the appointment was restaurant-scoped.
        if (assignment.RestaurantId.HasValue)
            await ClearCognitoRestaurantAttributeAsync(adminToDemoteId);

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

    public async Task<PagedAdminUsersResponse> GetUsersByRoleAsync(UserGroup role, int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be at least 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");

        var groupName = role switch
        {
            UserGroup.Admin => AdminGroupName,
            UserGroup.Waiter => WaiterGroupName,
            _ => CustomerGroupName
        };

        var usersInRole = await GetAllUsersInGroupAsync(groupName);
        var sortedUsers = usersInRole
            .OrderBy(u => GetAttributeValue(u.Attributes, "email"), StringComparer.OrdinalIgnoreCase)
            .ToList();

        var restaurantAddressById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (role is UserGroup.Admin or UserGroup.Waiter)
        {
            var restaurants = await restaurantRepository.GetAllRestaurantsAsync();
            restaurantAddressById = restaurants.ToDictionary(r => r.Id.ToString(), r => r.Address, StringComparer.OrdinalIgnoreCase);
        }

        var totalCount = sortedUsers.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = sortedUsers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user =>
            {
                var restaurantId = GetAttributeValue(user.Attributes, RestaurantIdAttributeName);
                restaurantAddressById.TryGetValue(restaurantId ?? string.Empty, out var restaurantAddress);

                return new AdminUserListItemResponse
                {
                    UserId = user.Username,
                    Email = GetAttributeValue(user.Attributes, "email") ?? string.Empty,
                    FirstName = GetAttributeValue(user.Attributes, "given_name"),
                    LastName = GetAttributeValue(user.Attributes, "family_name"),
                    Role = groupName,
                    RestaurantId = restaurantId,
                    RestaurantAddress = restaurantAddress
                };
            })
            .ToList();

        return new PagedAdminUsersResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private async Task EnsureCognitoUserExistsAsync(string userId)
    {
        try
        {
            await cognito.AdminGetUserAsync(new AdminGetUserRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = userId
            });
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException)
        {
            throw new KeyNotFoundException($"User with ID '{userId}' was not found.");
        }
    }

    private async Task AddUserToAdminGroupAsync(string userId)
    {
        try
        {
            await cognito.AdminAddUserToGroupAsync(new AdminAddUserToGroupRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = userId,
                GroupName = AdminGroupName
            });
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidOperationException($"Failed to add user to Admin group: {ex.Message}", ex);
        }
    }

    private async Task RemoveUserFromAdminGroupAsync(string userId)
    {
        try
        {
            await cognito.AdminRemoveUserFromGroupAsync(new AdminRemoveUserFromGroupRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = userId,
                GroupName = AdminGroupName
            });
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException)
        {
            throw new KeyNotFoundException($"User with ID '{userId}' was not found in Cognito.");
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidOperationException($"Failed to remove user from Admin group: {ex.Message}", ex);
        }
    }

    private async Task SetCognitoRestaurantAttributeAsync(string userId, string restaurantId)
    {
        try
        {
            await cognito.AdminUpdateUserAttributesAsync(new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = userId,
                UserAttributes =
                [
                    new AttributeType { Name = RestaurantIdAttributeName, Value = restaurantId }
                ]
            });
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            throw new InvalidOperationException($"Failed to set restaurant attribute for user '{userId}': {ex.Message}", ex);
        }
    }

    private async Task ClearCognitoRestaurantAttributeAsync(string userId)
    {
        try
        {
            await cognito.AdminDeleteUserAttributesAsync(new AdminDeleteUserAttributesRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = userId,
                UserAttributeNames = [RestaurantIdAttributeName]
            });
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            // Best-effort: ignore if attribute doesn't exist or deletion fails
        }
    }

    private async Task<List<UserType>> GetAllUsersInGroupAsync(string groupName)
    {
        var users = new List<UserType>();
        string? nextToken = null;

        do
        {
            var response = await cognito.ListUsersInGroupAsync(new ListUsersInGroupRequest
            {
                UserPoolId = _options.UserPoolId,
                GroupName = groupName,
                NextToken = nextToken,
                Limit = 60
            });

            users.AddRange(response.Users);
            nextToken = response.NextToken;
        } while (!string.IsNullOrWhiteSpace(nextToken));

        return users;
    }

    private static string? GetAttributeValue(List<AttributeType>? attributes, string name)
    {
        return attributes?.FirstOrDefault(a => a.Name == name)?.Value;
    }
}
