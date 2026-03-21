using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Auth;

[ExcludeFromCodeCoverage]
public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}