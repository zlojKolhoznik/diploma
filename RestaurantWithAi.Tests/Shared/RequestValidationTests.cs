using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Dishes;
using RestaurantWithAi.Shared.Options;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Tests.Shared;

[ExcludeFromCodeCoverage]
public class RequestValidationTests
{
    [Fact]
    public void RegisterRequest_WhenEmailIsInvalid_FailsValidation()
    {
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Fact]
    public void CreateDishRequest_WhenPriceIsNotPositive_FailsValidation()
    {
        var request = new CreateDishRequest
        {
            Name = "Soup",
            Description = "Warm",
            Price = 0m,
            ImageUrl = "https://example.com/soup.jpg"
        };

        var results = Validate(request);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateDishRequest.Price)));
    }

    [Fact]
    public void CreateRestaurantRequest_WhenRequiredValuesProvided_PassesValidation()
    {
        var request = new CreateRestaurantRequest
        {
            City = "Kyiv",
            Address = "Address 1"
        };

        var results = Validate(request);

        Assert.Empty(results);
    }

    [Fact]
    public void AwsCognitoOptions_WhenAuthorityIsInvalid_FailsValidation()
    {
        var options = new AwsCognitoOptions
        {
            Region = "us-east-1",
            UserPoolId = "pool-id",
            ClientId = "client-id",
            Authority = "invalid-authority",
            ClientSecret = "secret"
        };

        var results = Validate(options);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AwsCognitoOptions.Authority)));
    }

    [Fact]
    public void AwsCognitoOptions_WhenClientSecretMissing_FailsValidation()
    {
        var options = new AwsCognitoOptions
        {
            Region = "us-east-1",
            UserPoolId = "pool-id",
            ClientId = "client-id",
            Authority = "https://cognito-idp.us-east-1.amazonaws.com/pool-id",
            ClientSecret = string.Empty
        };

        var results = Validate(options);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AwsCognitoOptions.ClientSecret)));
    }

    private static List<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}

