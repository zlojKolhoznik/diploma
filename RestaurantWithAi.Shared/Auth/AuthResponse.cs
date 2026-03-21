using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Auth;

[ExcludeFromCodeCoverage]
public class AuthResponse
{
    public required string AccessToken { get; set; }
    public required string IdToken { get; set; }
    public required string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}