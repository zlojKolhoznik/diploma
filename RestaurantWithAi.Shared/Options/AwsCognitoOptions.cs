using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Options;

[ExcludeFromCodeCoverage]
public class AwsCognitoOptions
{
    public const string SectionName = "AWS";

    [Required]
    public required string Region { get; set; }

    [Required]
    public required string UserPoolId { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    [Url]
    public required string Authority { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}