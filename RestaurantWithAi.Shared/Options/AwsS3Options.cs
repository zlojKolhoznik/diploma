using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Options;

[ExcludeFromCodeCoverage]
public class AwsS3Options
{
    public const string SectionName = "AwsS3";

    [Required]
    public required string BucketName { get; set; }

    [Required]
    public required string AccessKey { get; set; }

    [Required]
    public required string SecretKey { get; set; }

    [Required]
    public required string Region { get; set; }
}

