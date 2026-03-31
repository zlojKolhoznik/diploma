using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using AutoMapper;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Options;
using RestaurantWithAi.Shared.Waiters;

namespace RestaurantWithAi.Core.Services;

public class WaiterService(
    IAmazonCognitoIdentityProvider cognito,
    IOptions<AwsCognitoOptions> options,
    IMapper mapper) : IWaiterService
{
    private const string WaitersGroupName = "Waiters";
    private const string RestaurantIdAttributeName = "custom:restaurantId";

    private readonly IAmazonCognitoIdentityProvider _cognito = cognito;
    private readonly AwsCognitoOptions _options = options.Value;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<WaiterResponse>> GetAllWaitersAsync()
    {
        var request = new ListUsersInGroupRequest
        {
            UserPoolId = _options.UserPoolId,
            GroupName = WaitersGroupName
        };

        var response = await _cognito.ListUsersInGroupAsync(request);

        return _mapper.Map<IEnumerable<WaiterResponse>>(response.Users);
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

}
