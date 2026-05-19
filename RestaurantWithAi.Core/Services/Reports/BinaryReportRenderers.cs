using System.IO;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace RestaurantWithAi.Core.Services.Reports;

public class ExcelReportRenderer : IReportBinaryRenderer
{
    public Task<byte[]> RenderAsync(ReportStructuredData data, string format, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        worksheet.Cell("A1").Value = data.Title;
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;

        worksheet.Cell("A2").Value = $"Type: {data.Type}";
        worksheet.Cell("A3").Value = $"Generated (UTC): {data.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}";

        var row = 5;
        foreach (var section in data.Sections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            worksheet.Cell(row, 1).Value = section.Title;
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
            worksheet.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            row++;

            foreach (var metric in section.Metrics)
            {
                worksheet.Cell(row, 1).Value = metric.Name;
                worksheet.Cell(row, 2).Value = metric.Value;
                worksheet.Range(row, 1, row, 2).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;
            }

            row++;
        }

        worksheet.Columns(1, 2).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}

public class PdfReportRenderer : IReportBinaryRenderer
{
    public Task<byte[]> RenderAsync(ReportStructuredData data, string format, CancellationToken cancellationToken = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text(data.Title).FontSize(18).Bold();
                        column.Item().Text($"Type: {data.Type}").FontColor(Colors.Grey.Darken1);
                        column.Item().Text($"Generated (UTC): {data.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}").FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingVertical(16)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        foreach (var section in data.Sections)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(sectionColumn =>
                            {
                                sectionColumn.Item().Text(section.Title).SemiBold().FontSize(13);
                                sectionColumn.Item().PaddingTop(6).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(3);
                                    });

                                    foreach (var metric in section.Metrics)
                                    {
                                        table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                            .Text(metric.Name);
                                        table.Cell().PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                                            .Text(metric.Value).AlignRight();
                                    }
                                });
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();

        return Task.FromResult(bytes);
    }
}

