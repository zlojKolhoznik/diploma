using RestaurantWithAi.Shared.AI;

namespace RestaurantWithAi.Core.Services;

public sealed class UnconfiguredTextGenerationClient : ITextGenerationClient
{
    public Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("{\"approved\":true}");
    }
}

