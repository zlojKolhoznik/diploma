using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Options;
using System.Security.Cryptography;
using System.Text;

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
        var secretHash = ComputeSecretHash(request.Email);

        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _options.ClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", request.Email },
                { "PASSWORD", request.Password },
                { "SECRET_HASH", secretHash }
            }
        };

        InitiateAuthResponse response;
        try
        {
            response = await _cognito.InitiateAuthAsync(authRequest);
        }
        catch (NotAuthorizedException)
        {
            throw new AuthenticationFailedException("Invalid email or password.");
        }
        catch (Amazon.CognitoIdentityProvider.Model.UserNotFoundException)
        {
            throw new AuthenticationFailedException("Invalid email or password.");
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            throw new AuthenticationFailedException("Authentication failed.");
        }

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

    public async Task RegisterAsync(RegisterRequest request, UserGroup group = UserGroup.Customer)
    {
        await SignUpAsync(request);
        await ConfirmUserAsync(request.Email);

        try
        {
            await AddUserToGroup(group, request.Email);
        }
        catch (RegistrationFailedException)
        {
            await RollbackConfirmedUserAsync(request.Email);
            throw new RegistrationFailedException("Failed to add user to group. User creation was rolled back.");
        }
    }

    private async Task SignUpAsync(RegisterRequest request)
    {
        var signUpRequest = new SignUpRequest
        {
            ClientId = _options.ClientId,
            Username = request.Email,
            Password = request.Password,
            SecretHash = ComputeSecretHash(request.Email),
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

        SignUpResponse signUpResponse;
        try
        {
            signUpResponse = await _cognito.SignUpAsync(signUpRequest);
        }
        catch (UsernameExistsException)
        {
            throw new DuplicateEmailException("A user with this email already exists.");
        }
        catch (InvalidPasswordException)
        {
            throw;
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            throw new RegistrationFailedException("User registration failed.");
        }

        if (signUpResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new RegistrationFailedException("User registration failed.");
        }
    }
    
    private async Task ConfirmUserAsync(string email)
    {
        var request = new AdminConfirmSignUpRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = email
        };
        
        var response = await _cognito.AdminConfirmSignUpAsync(request);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new RegistrationFailedException("User registration was successful, but user confirmation failed.");
        }
    }

    private async Task AddUserToGroup(UserGroup group, string email)
    {
        var request = new AdminAddUserToGroupRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = email,
            GroupName = group.ToString()
        };
        try
        {
            await _cognito.AdminAddUserToGroupAsync(request);
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            throw new RegistrationFailedException("Failed to add user to group.");
        }
    }

    private async Task RollbackConfirmedUserAsync(string email)
    {
        var request = new AdminDeleteUserRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = email
        };

        try
        {
            var response = await _cognito.AdminDeleteUserAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new RegistrationFailedException("Failed to add user to group and rollback deletion failed.");
            }
        }
        catch (AmazonCognitoIdentityProviderException)
        {
            throw new RegistrationFailedException("Failed to add user to group and rollback deletion failed.");
        }
    }

    private string ComputeSecretHash(string username)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("AWS Cognito ClientId/ClientSecret is not configured.");
        }

        var message = username + _options.ClientId;
        var key = Encoding.UTF8.GetBytes(_options.ClientSecret);
        var payload = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(payload);
        return Convert.ToBase64String(hash);
    }
}