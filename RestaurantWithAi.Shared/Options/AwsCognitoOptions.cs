using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Options;

[ExcludeFromCodeCoverage]
public class AwsCognitoOptions
{
    public required string Region { get; set; }
    public required string UserPoolId { get; set; }
    public required string ClientId { get; set; }
    public required string Authority { get; set; }
    public required string ClientSecret { get; set; }
}