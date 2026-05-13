namespace RestaurantWithAi.Core.Services.Reports;

public interface IReportRendererFactory
{
    IReportBinaryRenderer GetRenderer(string format);
}

public class ReportRendererFactory : IReportRendererFactory
{
    private readonly Dictionary<string, IReportBinaryRenderer> _renderers;

    public ReportRendererFactory()
    {
        _renderers = new(StringComparer.OrdinalIgnoreCase)
        {
            { "excel", new ExcelReportRenderer() },
            { "xlsx", new ExcelReportRenderer() },
            { "pdf", new PdfReportRenderer() }
        };
    }

    public IReportBinaryRenderer GetRenderer(string format)
    {
        if (_renderers.TryGetValue(format, out var renderer))
            return renderer;

        throw new InvalidOperationException($"No renderer found for format '{format}'. Supported formats: excel, xlsx, pdf");
    }
}

