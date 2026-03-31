using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Exceptions;

[ExcludeFromCodeCoverage]
public class RegistrationFailedException(string message) : Exception(message);