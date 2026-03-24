using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Auth;

[ExcludeFromCodeCoverage]
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public required string Password { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }
}