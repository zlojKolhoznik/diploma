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

        var prompt = BuildAnalysisPrompt(request);
        var analysisText = await textGenerationClient.GenerateTextAsync(prompt, cancellationToken);

        return new ReportAnalysisResponse
        {
            Analysis = analysisText,
            AnalyzedAtUtc = DateTime.UtcNow
        };
    }

    private static string BuildAnalysisPrompt(ReportAnalysisRequest request)
    {
        var context = $"Report Data:\n{request.ReportContent}\n\n";

        return request.AnalysisType.ToLowerInvariant() switch
        {
            "menu" =>
                "You are a restaurant menu consultant. Analyze the following order statistics for a restaurant network. " +
                "Identify the best-selling and worst-selling dishes. Provide specific, actionable recommendations on pricing adjustments, " +
                "promotional offers, and seasonal menu changes. Structure your response with clear sections and bullet points.\n\n" +
                context + "Please provide your menu optimization analysis:",

            "waiter" =>
                "You are a hospitality service quality analyst. Analyze the following waiter performance data including numeric ratings " +
                "and customer review text for a specific restaurant location. Identify the top-performing waiter and provide individual " +
                "recommendations for each waiter on how to improve customer satisfaction scores. " +
                "Structure your response with a summary, individual assessments, and a highlighted top performer section.\n\n" +
                context + "Please provide your waiter performance analysis:",

            "profitability" =>
                "You are a senior business analyst specializing in restaurant network profitability. Analyze the following financial " +
                "performance data across all network locations. Answer these strategic questions: " +
                "(1) Should new locations be opened, and if so, where? " +
                "(2) How should existing locations adjust their strategy to maximize profit? " +
                "(3) Are there unprofitable locations that should be closed or restructured? " +
                "Provide concrete, data-driven recommendations for top management. Use structured sections and bullet points.\n\n" +
                context + "Please provide your profitability analysis:",

            _ =>
                "You are a business intelligence analyst. Analyze the following report data and provide key insights and recommendations. " +
                "Focus on trends, anomalies, and actionable next steps. Keep the analysis concise and structured with bullet points.\n\n" +
                context + "Please provide your analysis:"
        };
    }
}

