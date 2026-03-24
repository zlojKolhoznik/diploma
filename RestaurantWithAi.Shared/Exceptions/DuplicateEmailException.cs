using System.Diagnostics.CodeAnalysis;

namespace RestaurantWithAi.Shared.Exceptions;

[ExcludeFromCodeCoverage]
public class DuplicateEmailException(string message) : Exception(message);

