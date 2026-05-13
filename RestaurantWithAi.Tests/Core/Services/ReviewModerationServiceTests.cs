using System.Diagnostics.CodeAnalysis;
using Moq;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Reviews;

namespace RestaurantWithAi.Tests.Core.Services;

[ExcludeFromCodeCoverage]
public class ReviewModerationServiceTests
{
    [Fact]
    public async Task ModerateAsync_WhenClientReturnsWrappedJson_ParsesResult()
    {
        var client = new Mock<ITextGenerationClient>();
        string? capturedPrompt = null;

        client.Setup(c => c.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, _) => capturedPrompt = prompt)
            .ReturnsAsync("```json\n{\"approved\":false,\"reason\":\"Contains personal attacks\",\"suggestedRephrasing\":\"Please keep the feedback focused on the food and service.\"}\n```");

        var sut = new ClaudeReviewModerationService(client.Object);

        var result = await sut.ModerateAsync(new CreateReviewRequest
        {
            CuisineRating = 2,
            CuisineComment = "The food was awful.",
            ServiceRating = 1,
            ServiceComment = "The waiter ignored us."
        });

        Assert.NotNull(capturedPrompt);
        Assert.Contains("Cuisine rating: 2", capturedPrompt);
        Assert.Contains("Service comment: The waiter ignored us.", capturedPrompt);
        Assert.False(result.Approved);
        Assert.Equal("Contains personal attacks", result.Reason);
        Assert.Equal("Please keep the feedback focused on the food and service.", result.SuggestedRephrasing);
    }

    [Fact]
    public async Task ModerateAsync_WhenClientReturnsInvalidResponse_ThrowsInvalidOperationException()
    {
        var client = new Mock<ITextGenerationClient>();
        client.Setup(c => c.GenerateTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("not valid json");

        var sut = new ClaudeReviewModerationService(client.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ModerateAsync(new CreateReviewRequest
        {
            CuisineRating = 5,
            ServiceRating = 5
        }));
    }
}

