using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Exceptions;

[ExcludeFromCodeCoverage]
public class AuthenticationFailedException(string message) : Exception(message);