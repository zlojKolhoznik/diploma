using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Options;

namespace RestaurantWithAi.Core.Services;
public class CognitoAuthService : IAuthService
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly AwsCognitoOptions _options;

    public CognitoAuthService(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<AwsCognitoOptions> options)
    {
        _cognito = cognito;
        _options = options.Value;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _options.ClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", request.Email },
                { "PASSWORD", request.Password }
            }
        };

        var response = await _cognito.InitiateAuthAsync(authRequest);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK || response.AuthenticationResult is null)
        {
            throw new AuthenticationFailedException("Authentication failed.");
        }

        return new AuthResponse
        {
            AccessToken = response.AuthenticationResult.AccessToken,
            IdToken = response.AuthenticationResult.IdToken,
            RefreshToken = response.AuthenticationResult.RefreshToken,
            ExpiresIn = response.AuthenticationResult.ExpiresIn ?? 0
        };
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var signUpRequest = new SignUpRequest
        {
            ClientId = _options.ClientId,
            Username = request.Email,
            Password = request.Password,
            UserAttributes =
            [
                new AttributeType
                {
                    Name = "email",
                    Value = request.Email
                },
                new AttributeType
                {
                    Name = "given_name",
                    Value = request.FirstName
                },
                new AttributeType
                {
                    Name = "family_name",
                    Value = request.LastName
                }
            ]
        };

        var signUpResponse = await _cognito.SignUpAsync(signUpRequest);
        if (signUpResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new RegistrationFailedException("User registration failed.");
        }
    }
}