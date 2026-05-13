using System.Text;
using System.Text.Json;

namespace RestaurantWithAi.Core.Services.Reports;

public class ExcelReportRenderer : IReportBinaryRenderer
{
    public Task<byte[]> RenderAsync(ReportStructuredData data, string format, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        var withHeader = Encoding.UTF8.GetBytes("Excel Report (Placeholder)\r\n\r\n").Concat(bytes).ToArray();
        return Task.FromResult(withHeader);
    }
}

public class PdfReportRenderer : IReportBinaryRenderer
{
    public Task<byte[]> RenderAsync(ReportStructuredData data, string format, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        var withHeader = Encoding.UTF8.GetBytes("PDF Report (Placeholder)\r\n\r\n").Concat(bytes).ToArray();
        return Task.FromResult(withHeader);
    }
}

