using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Core.Services;

public class ReportAnalysisService(ITextGenerationClient textGenerationClient) : IReportAnalysisService
{
    public async Task<ReportAnalysisResponse> AnalyzeReportAsync(ReportAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ReportContent))
            throw new ArgumentException("Report content is required for analysis.", nameof(request.ReportContent));

        var prompt = BuildAnalysisPrompt(request.ReportContent);
        var analysisText = await textGenerationClient.GenerateTextAsync(prompt, cancellationToken);

        return new ReportAnalysisResponse
        {
            Analysis = analysisText,
            AnalyzedAtUtc = DateTime.UtcNow
        };
    }

    private static string BuildAnalysisPrompt(string reportContent)
    {
        return
            "You are a business intelligence analyst. Analyze the following report data and provide key insights and recommendations. " +
            "Focus on trends, anomalies, and actionable next steps. Keep the analysis concise and structured with bullet points.\n\n" +
            $"Report Data:\n{reportContent}\n\n" +
            "Please provide your analysis:";
    }
}

