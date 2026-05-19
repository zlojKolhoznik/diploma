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
            Analysis = analysisText.Replace("```html", "").Replace("```", "").Trim(),
            AnalyzedAtUtc = DateTime.UtcNow
        };
    }

    private static string BuildAnalysisPrompt(ReportAnalysisRequest request)
    {
        var context = $"Report Data:\n{request.ReportContent}\n\n";
        var htmlInstructions = BuildHtmlFormattingInstructions();

        return request.AnalysisType.ToLowerInvariant() switch
        {
            "menu" =>
                "You are a restaurant menu consultant. Analyze the following order statistics for a restaurant network. " +
                "Identify the best-selling and worst-selling dishes. Provide specific, actionable recommendations on pricing adjustments, " +
                "promotional offers, and seasonal menu changes.\n\n" +
                htmlInstructions + "\n\n" +
                context + "Provide a menu optimization analysis as HTML only:",

            "waiter" =>
                "You are a hospitality service quality analyst. Analyze the following waiter performance data including numeric ratings " +
                "and customer review text for a specific restaurant location. Identify the top-performing waiter and provide individual " +
                "recommendations for each waiter on how to improve customer satisfaction scores. " +
                "Include a summary, individual assessments, and a highlighted top performer section.\n\n" +
                htmlInstructions + "\n\n" +
                context + "Provide a waiter performance analysis as HTML only:",

            "profitability" =>
                "You are a senior business analyst specializing in restaurant network profitability. Analyze the following financial " +
                "performance data across all network locations. Answer these strategic questions: " +
                "(1) Should new locations be opened, and if so, where? " +
                "(2) How should existing locations adjust their strategy to maximize profit? " +
                "(3) Are there unprofitable locations that should be closed or restructured? " +
                "Provide concrete, data-driven recommendations for top management.\n\n" +
                htmlInstructions + "\n\n" +
                context + "Provide a profitability analysis as HTML only:",

            _ =>
                "You are a business intelligence analyst. Analyze the following report data and provide key insights and recommendations. " +
                "Focus on trends, anomalies, and actionable next steps.\n\n" +
                htmlInstructions + "\n\n" +
                context + "Provide a concise analysis as HTML only:"
        };
    }

    private static string BuildHtmlFormattingInstructions()
    {
        return "Return valid HTML only. Do not use Markdown, code fences, or plain text outside HTML. " +
               "Do not include <html>, <head>, <body>, <style>, <script>, inline styles, or external resources. " +
               "Use this structure and classes: " +
               "<article class='ai-report'> as root; " +
               "<header class='ai-report__header'> with <h3 class='ai-report__title'> and optional <p class='ai-report__subtitle'>; " +
               "<p class='ai-report__summary'> for executive summary; " +
               "multiple <section class='ai-report__section'> blocks with <h4 class='ai-report__section-title'>; " +
               "<ul class='ai-report__list'><li>...</li></ul> for recommendations; " +
               "<div class='ai-report__grid'> containing <div class='ai-report__card'> with <p class='ai-report__card-title'> and <p class='ai-report__card-value'> for KPI highlights; " +
               "for tabular comparisons use <div class='ai-report__table-wrap'><table class='ai-report__table'><thead>...</thead><tbody>...</tbody></table></div>; " +
               "use emphasis callouts with <div class='ai-report__callout ai-report__callout--success|warning|danger'> when relevant. " +
               "Keep output concise, data-driven, and visually scannable.";
    }
}

