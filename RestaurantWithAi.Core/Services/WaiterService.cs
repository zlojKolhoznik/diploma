using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Options;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Core.Services;

public class WaiterService(
    IAmazonCognitoIdentityProvider cognito,
    IOptions<AwsCognitoOptions> options) : IWaiterService
{
    private const string WaitersGroupName = "Waiters";
    private const string RestaurantIdAttributeName = "custom:restaurantId";

    private readonly IAmazonCognitoIdentityProvider _cognito = cognito;
    private readonly AwsCognitoOptions _options = options.Value;

    public async Task<IEnumerable<WaiterResponse>> GetAllWaitersAsync()
    {
        var request = new ListUsersInGroupRequest
        {
            UserPoolId = _options.UserPoolId,
            GroupName = WaitersGroupName
        };

        var response = await _cognito.ListUsersInGroupAsync(request);

        return response.Users.Select(u => new WaiterResponse
        {
            UserId = u.Username,
            Email = GetAttributeValue(u.Attributes, "email") ?? string.Empty,
            FirstName = GetAttributeValue(u.Attributes, "given_name"),
            LastName = GetAttributeValue(u.Attributes, "family_name"),
            RestaurantId = GetAttributeValue(u.Attributes, RestaurantIdAttributeName)
        });
    }

    public async Task AssignWaiterRoleAsync(string userId)
    {
        await EnsureUserExistsAsync(userId);

        var request = new AdminAddUserToGroupRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = userId,
            GroupName = WaitersGroupName
        };

        await _cognito.AdminAddUserToGroupAsync(request);
    }

    public async Task AssignWaiterToRestaurantAsync(string restaurantId, string userId)
    {
        await EnsureUserExistsAsync(userId);

        var request = new AdminUpdateUserAttributesRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = userId,
            UserAttributes =
            [
                new AttributeType
                {
                    Name = RestaurantIdAttributeName,
                    Value = restaurantId
                }
            ]
        };

        await _cognito.AdminUpdateUserAttributesAsync(request);
    }

    private async Task EnsureUserExistsAsync(string userId)
    {
        try
        {
            var request = new AdminGetUserRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = userId
            };

            await _cognito.AdminGetUserAsync(request);
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException)
        {
            throw new Shared.Exceptions.UserNotFoundException($"User with id '{userId}' was not found.");
        }
    }

    private static string? GetAttributeValue(List<AttributeType> attributes, string name)
    {
        return attributes.FirstOrDefault(a => a.Name == name)?.Value;
    }
}
