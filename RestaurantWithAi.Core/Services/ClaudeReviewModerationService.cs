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
            "Review the draft below and return ONLY a compact JSON object (no markdown, no extra text) with exactly these fields:\n" +
            "- \"approved\": boolean — true if the review is acceptable as-is\n" +
            "- \"reason\": string — short explanation when the review should be rejected or flagged, null otherwise\n" +
            "- \"suggestedRephrasing\": string — a single plain-text string with friendlier wording when the review needs changes, null otherwise. " +
            "This field MUST be a JSON string (not an object or array).\n\n" +
            "Example output: {\"approved\":false,\"reason\":\"Contains offensive language.\",\"suggestedRephrasing\":\"The food was below expectations and the service felt rushed.\"}\n\n" +
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



