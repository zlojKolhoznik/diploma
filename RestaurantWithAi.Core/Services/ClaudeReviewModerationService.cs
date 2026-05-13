using System.Text.Json;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Core.Services;

public class ClaudeReviewModerationService(ITextGenerationClient textGenerationClient) : IReviewModerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ReviewModerationResult> ModerateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prompt = BuildPrompt(request);
        var rawResponse = await textGenerationClient.GenerateTextAsync(prompt, cancellationToken);
        return ParseResponse(rawResponse);
    }

    private static string BuildPrompt(CreateReviewRequest request)
    {
        return
            "You are a restaurant review moderation assistant.\n" +
            "Review the draft below and return only a compact JSON object with these fields:\n" +
            "- approved: boolean\n" +
            "- reason: short explanation when the review should be rejected or flagged\n" +
            "- suggestedRephrasing: optional friendlier wording when the review needs changes\n\n" +
            "Draft review:\n" +
            $"Cuisine rating: {request.CuisineRating}\n" +
            $"Cuisine comment: {request.CuisineComment ?? "(none)"}\n" +
            $"Service rating: {request.ServiceRating}\n" +
            $"Service comment: {request.ServiceComment ?? "(none)"}";
    }

    private static ReviewModerationResult ParseResponse(string? rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            throw new InvalidOperationException("Review moderation response was empty.");

        var candidate = ExtractJsonCandidate(rawResponse);
        try
        {
            var result = JsonSerializer.Deserialize<ReviewModerationResult>(candidate, JsonOptions);
            if (result is not null)
                return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Review moderation response could not be parsed.", ex);
        }

        throw new InvalidOperationException("Review moderation response could not be parsed.");
    }

    private static string ExtractJsonCandidate(string rawResponse)
    {
        var startIndex = rawResponse.IndexOf('{');
        var endIndex = rawResponse.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
            return rawResponse[startIndex..(endIndex + 1)];

        return rawResponse.Trim();
    }
}



