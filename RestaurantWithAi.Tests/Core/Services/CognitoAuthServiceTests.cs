using System.Diagnostics.CodeAnalysis;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using Moq;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Exceptions;
using RestaurantWithAi.Shared.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class CognitoAuthServiceTests
{
    private static readonly AwsCognitoOptions ValidOptions = new()
    {
        Region = "eu-central-1",
        UserPoolId = "pool-id",
        ClientId = "client-id",
        Authority = "authority",
        ClientSecret = "client-secret"
    };

    [Fact]
    public async Task LoginAsync_WhenCognitoReturnsOkWithTokens_ReturnsMappedAuthResponseAndExpectedRequest()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        InitiateAuthRequest? capturedRequest = null;

        cognitoMock
            .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
            .Callback<InitiateAuthRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new InitiateAuthResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                AuthenticationResult = new AuthenticationResultType
                {
                    AccessToken = "access-token",
                    IdToken = "id-token",
                    RefreshToken = "refresh-token",
                    ExpiresIn = 3600
                }
            });

        var service = CreateService(cognitoMock.Object, ValidOptions);
        var loginRequest = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await service.LoginAsync(loginRequest);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(AuthFlowType.USER_PASSWORD_AUTH, capturedRequest!.AuthFlow);
        Assert.Equal(ValidOptions.ClientId, capturedRequest.ClientId);
        Assert.Equal(loginRequest.Email, capturedRequest.AuthParameters["USERNAME"]);
        Assert.Equal(loginRequest.Password, capturedRequest.AuthParameters["PASSWORD"]);
        Assert.Equal(ComputeSecretHash(loginRequest.Email, ValidOptions), capturedRequest.AuthParameters["SECRET_HASH"]);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("id-token", response.IdToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal(3600, response.ExpiresIn);
    }

    [Fact]
    public async Task LoginAsync_WhenAuthResultIsNull_ThrowsAuthenticationFailedException()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        cognitoMock
            .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InitiateAuthResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                AuthenticationResult = null
            });

        var service = CreateService(cognitoMock.Object, ValidOptions);

        // Act + Assert
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        }));
    }

    [Fact]
    public async Task LoginAsync_WhenStatusCodeIsNotOk_ThrowsAuthenticationFailedException()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        cognitoMock
            .Setup(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InitiateAuthResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                AuthenticationResult = new AuthenticationResultType
                {
                    AccessToken = "access-token",
                    IdToken = "id-token",
                    RefreshToken = "refresh-token",
                    ExpiresIn = 3600
                }
            });

        var service = CreateService(cognitoMock.Object, ValidOptions);

        // Act + Assert
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        }));
    }

    [Fact]
    public async Task LoginAsync_WhenClientSecretIsMissing_ThrowsInvalidOperationExceptionWithoutCallingCognito()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>(MockBehavior.Strict);
        var options = new AwsCognitoOptions
        {
            Region = ValidOptions.Region,
            UserPoolId = ValidOptions.UserPoolId,
            ClientId = ValidOptions.ClientId,
            Authority = ValidOptions.Authority,
            ClientSecret = ""
        };

        var service = CreateService(cognitoMock.Object, options);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        }));

        cognitoMock.Verify(c => c.InitiateAuthAsync(It.IsAny<InitiateAuthRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenAllCognitoCallsSucceed_UsesDefaultCustomerGroupAndExpectedRequests()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        SignUpRequest? capturedSignUp = null;
        AdminConfirmSignUpRequest? capturedConfirm = null;
        AdminAddUserToGroupRequest? capturedAddToGroup = null;

        cognitoMock
            .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SignUpRequest, CancellationToken>((request, _) => capturedSignUp = request)
            .ReturnsAsync(new SignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AdminConfirmSignUpRequest, CancellationToken>((request, _) => capturedConfirm = request)
            .ReturnsAsync(new AdminConfirmSignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AdminAddUserToGroupRequest, CancellationToken>((request, _) => capturedAddToGroup = request)
            .ReturnsAsync(new AdminAddUserToGroupResponse { HttpStatusCode = HttpStatusCode.OK });

        var service = CreateService(cognitoMock.Object, ValidOptions);
        var registerRequest = new RegisterRequest
        {
            Email = "new.user@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        await service.RegisterAsync(registerRequest);

        // Assert
        Assert.NotNull(capturedSignUp);
        Assert.Equal(ValidOptions.ClientId, capturedSignUp!.ClientId);
        Assert.Equal(registerRequest.Email, capturedSignUp.Username);
        Assert.Equal(registerRequest.Password, capturedSignUp.Password);
        Assert.Equal(ComputeSecretHash(registerRequest.Email, ValidOptions), capturedSignUp.SecretHash);
        Assert.Contains(capturedSignUp.UserAttributes, a => a.Name == "email" && a.Value == registerRequest.Email);
        Assert.Contains(capturedSignUp.UserAttributes, a => a.Name == "given_name" && a.Value == registerRequest.FirstName);
        Assert.Contains(capturedSignUp.UserAttributes, a => a.Name == "family_name" && a.Value == registerRequest.LastName);

        Assert.NotNull(capturedConfirm);
        Assert.Equal(ValidOptions.UserPoolId, capturedConfirm!.UserPoolId);
        Assert.Equal(registerRequest.Email, capturedConfirm.Username);

        Assert.NotNull(capturedAddToGroup);
        Assert.Equal(ValidOptions.UserPoolId, capturedAddToGroup!.UserPoolId);
        Assert.Equal(registerRequest.Email, capturedAddToGroup.Username);
        Assert.Equal(UserGroup.Customer.ToString(), capturedAddToGroup.GroupName);
    }

    [Fact]
    public async Task RegisterAsync_WhenSignUpStatusIsNotOk_ThrowsRegistrationFailedExceptionAndStopsFlow()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        cognitoMock
            .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignUpResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        var service = CreateService(cognitoMock.Object, ValidOptions);
        var request = new RegisterRequest
        {
            Email = "new.user@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act + Assert
        await Assert.ThrowsAsync<RegistrationFailedException>(() => service.RegisterAsync(request, UserGroup.Waiter));

        cognitoMock.Verify(c => c.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        cognitoMock.Verify(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenConfirmStatusIsNotOk_ThrowsRegistrationFailedExceptionAndDoesNotAddToGroup()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        cognitoMock
            .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminConfirmSignUpResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        var service = CreateService(cognitoMock.Object, ValidOptions);
        var request = new RegisterRequest
        {
            Email = "new.user@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act + Assert
        await Assert.ThrowsAsync<RegistrationFailedException>(() => service.RegisterAsync(request, UserGroup.Admin));

        cognitoMock.Verify(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenAddToGroupThrowsCognitoException_ThrowsRegistrationFailedException()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();
        AdminAddUserToGroupRequest? capturedAddToGroup = null;

        cognitoMock
            .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminConfirmSignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AdminAddUserToGroupRequest, CancellationToken>((request, _) => capturedAddToGroup = request)
            .ThrowsAsync(new AmazonCognitoIdentityProviderException("cognito failure"));

        cognitoMock
            .Setup(c => c.AdminDeleteUserAsync(It.IsAny<AdminDeleteUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminDeleteUserResponse { HttpStatusCode = HttpStatusCode.OK });

        var service = CreateService(cognitoMock.Object, ValidOptions);
        var request = new RegisterRequest
        {
            Email = "new.user@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act + Assert
        var exception = await Assert.ThrowsAsync<RegistrationFailedException>(() => service.RegisterAsync(request, UserGroup.Waiter));
        Assert.NotNull(capturedAddToGroup);
        Assert.Equal(UserGroup.Waiter.ToString(), capturedAddToGroup!.GroupName);
        Assert.Equal("Failed to add user to group. User creation was rolled back.", exception.Message);
        cognitoMock.Verify(c => c.AdminDeleteUserAsync(It.IsAny<AdminDeleteUserRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenRollbackDeleteFails_ThrowsRegistrationFailedExceptionWithRollbackFailureMessage()
    {
        // Arrange
        var cognitoMock = new Mock<IAmazonCognitoIdentityProvider>();

        cognitoMock
            .Setup(c => c.SignUpAsync(It.IsAny<SignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminConfirmSignUpAsync(It.IsAny<AdminConfirmSignUpRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdminConfirmSignUpResponse { HttpStatusCode = HttpStatusCode.OK });

        cognitoMock
            .Setup(c => c.AdminAddUserToGroupAsync(It.IsAny<AdminAddUserToGroupRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonCognitoIdentityProviderException("cognito failure"));

        cognitoMock
            .Setup(c => c.AdminDeleteUserAsync(It.IsAny<AdminDeleteUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonCognitoIdentityProviderException("delete failure"));

        var service = CreateService(cognitoMock.Object, ValidOptions);
        var request = new RegisterRequest
        {
            Email = "new.user@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act + Assert
        var exception = await Assert.ThrowsAsync<RegistrationFailedException>(() => service.RegisterAsync(request, UserGroup.Admin));
        Assert.Equal("Failed to add user to group and rollback deletion failed.", exception.Message);
        cognitoMock.Verify(c => c.AdminDeleteUserAsync(It.IsAny<AdminDeleteUserRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CognitoAuthService CreateService(IAmazonCognitoIdentityProvider cognito, AwsCognitoOptions options)
    {
        return new CognitoAuthService(cognito, Options.Create(options));
    }

    private static string ComputeSecretHash(string username, AwsCognitoOptions options)
    {
        var message = username + options.ClientId;
        var key = Encoding.UTF8.GetBytes(options.ClientSecret);
        var payload = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(payload);
        return Convert.ToBase64String(hash);
    }
}