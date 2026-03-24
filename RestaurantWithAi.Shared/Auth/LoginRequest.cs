using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace RestaurantWithAi.Shared.Auth;

[ExcludeFromCodeCoverage]
public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public required string Email { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public required string Password { get; set; }
}