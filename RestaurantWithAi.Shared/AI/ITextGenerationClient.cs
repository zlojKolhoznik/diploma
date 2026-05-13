namespace RestaurantWithAi.Shared.AI;

public interface ITextGenerationClient
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}

