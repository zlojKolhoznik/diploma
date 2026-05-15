using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Options;

namespace RestaurantWithAi.Core.Services;

public sealed class ClaudeTextGenerationClient(
    HttpClient httpClient,
    IOptions<ClaudeOptions> options,
    ILogger<ClaudeTextGenerationClient> logger)
    : ITextGenerationClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        var requestBody = new ClaudeRequest(
            opts.Model,
            opts.MaxTokens,
            [new ClaudeMessage("user", prompt)]
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages");
        request.Content = JsonContent.Create(requestBody, options: SerializerOptions);
        request.Headers.Add("x-api-key", opts.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        logger.LogDebug("Sending request to Claude API, model={Model}", opts.Model);
        var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Claude API returned {StatusCode}: {Body}", (int)response.StatusCode, errorBody);
            throw new HttpRequestException(
                $"Claude API error {(int)response.StatusCode}: {errorBody}",
                null,
                response.StatusCode);
        }

        var claudeResponse = await response.Content
            .ReadFromJsonAsync<ClaudeResponse>(SerializerOptions, cancellationToken);

        var text = claudeResponse?.Content?.FirstOrDefault(c => c.Type == "text")?.Text;

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Claude API returned an empty response.");

        return text;
    }

    // ---- Internal request/response models ----

    private sealed record ClaudeRequest(
        string Model,
        int MaxTokens,
        IReadOnlyList<ClaudeMessage> Messages);

    private sealed record ClaudeMessage(string Role, string Content);

    private sealed record ClaudeResponse(
        IReadOnlyList<ClaudeContentBlock>? Content);

    private sealed record ClaudeContentBlock(string Type, string? Text);
}

