using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Auth;

[ExcludeFromCodeCoverage]
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}