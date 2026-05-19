using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantWithAi.Shared.Reviews;

public class ReviewModerationResult
{
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("suggestedRephrasing")]
    [JsonConverter(typeof(StringOrObjectConverter))]
    public string? SuggestedRephrasing { get; set; }
}

/// <summary>
/// Tolerant converter: accepts a JSON string as-is, but if the AI returns an object or array
/// it serialises it back to a compact JSON string rather than throwing.
/// </summary>
internal sealed class StringOrObjectConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            _ => ReadRawElement(ref reader)
        };
    }

    private static string ReadRawElement(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return doc.RootElement.GetRawText();
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}

