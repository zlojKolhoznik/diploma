namespace RestaurantWithAi.Shared.Options;

public class AwsCognitoOptions
{
    public required string Region { get; set; }
    public required string UserPoolId { get; set; }
    public required string ClientId { get; set; }
    public required string Authority { get; set; }
    public required string ClientSecret { get; set; }
}