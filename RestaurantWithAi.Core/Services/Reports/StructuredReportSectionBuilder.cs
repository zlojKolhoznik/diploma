using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Shared.Reports;

namespace RestaurantWithAi.Core.Services.Reports;

public class StructuredReportSectionBuilder(IReportingDataRepository reportingData) : IReportSectionBuilder
{
    public async Task<IReadOnlyCollection<ReportSectionResponse>> BuildSectionsAsync(
        GenerateReportRequest request, CancellationToken cancellationToken = default)
    {
        return request.Type.ToLowerInvariant() switch
        {
            "profitability" => await BuildProfitabilitySectionsAsync(request, cancellationToken),
            "menu"          => await BuildMenuSectionsAsync(request, cancellationToken),
            "waiter"        => await BuildWaiterSectionsAsync(request, cancellationToken),
            _               => BuildUnknownTypeSection(request)
        };
    }

    // ── Profitability ─────────────────────────────────────────────────────────

    private async Task<IReadOnlyCollection<ReportSectionResponse>> BuildProfitabilitySectionsAsync(
        GenerateReportRequest request, CancellationToken ct)
    {
        var data = await reportingData.GetProfitabilityAsync(
            request.FromUtc, request.ToUtc, request.RestaurantId, ct);

        var sections = new List<ReportSectionResponse>();

        // Summary section
        var totalRevenue = data.Sum(r => r.TotalRevenue);
        var totalOrders  = data.Sum(r => r.TotalOrders);
        sections.Add(new ReportSectionResponse
        {
            Title = "Network Summary",
            Metrics =
            [
                new() { Name = "Period",               Value = FormatPeriod(request) },
                new() { Name = "Total Locations",      Value = data.Count.ToString() },
                new() { Name = "Total Revenue",        Value = FormatCurrency(totalRevenue) },
                new() { Name = "Total Orders",         Value = totalOrders.ToString() },
                new() { Name = "Avg Revenue/Location", Value = data.Count > 0 ? FormatCurrency(totalRevenue / data.Count) : "N/A" }
            ]
        });

        // Per-restaurant section
        foreach (var r in data)
        {
            var avgOrder = r.TotalOrders > 0 ? r.TotalRevenue / r.TotalOrders : 0m;
            sections.Add(new ReportSectionResponse
            {
                Title = $"{r.City} — {r.Address}",
                Metrics =
                [
                    new() { Name = "Restaurant ID",      Value = r.RestaurantId.ToString() },
                    new() { Name = "Total Revenue",      Value = FormatCurrency(r.TotalRevenue) },
                    new() { Name = "Total Orders",       Value = r.TotalOrders.ToString() },
                    new() { Name = "Total Reservations", Value = r.TotalReservations.ToString() },
                    new() { Name = "Avg Order Value",    Value = FormatCurrency(avgOrder) },
                    new() { Name = "Average Rating",     Value = r.AverageRating.HasValue ? $"{r.AverageRating:N1}/5" : "No ratings yet" }
                ]
            });
        }

        if (data.Count == 0)
            sections.Add(NoDataSection(request));

        return sections;
    }

    // ── Menu ─────────────────────────────────────────────────────────────────

    private async Task<IReadOnlyCollection<ReportSectionResponse>> BuildMenuSectionsAsync(
        GenerateReportRequest request, CancellationToken ct)
    {
        var data = await reportingData.GetDishOrderStatsAsync(
            request.FromUtc, request.ToUtc, request.RestaurantId, ct);

        var sections = new List<ReportSectionResponse>();

        var totalQty     = data.Sum(d => d.TotalQuantityOrdered);
        var totalRevenue = data.Sum(d => d.TotalRevenue);
        sections.Add(new ReportSectionResponse
        {
            Title = "Menu Summary",
            Metrics =
            [
                new() { Name = "Period",            Value = FormatPeriod(request) },
                new() { Name = "Distinct Dishes",   Value = data.Count.ToString() },
                new() { Name = "Total Items Sold",  Value = totalQty.ToString() },
                new() { Name = "Total Revenue",     Value = FormatCurrency(totalRevenue) }
            ]
        });

        // Top 10 best-sellers
        var top = data.Take(10).ToList();
        if (top.Count > 0)
        {
            sections.Add(new ReportSectionResponse
            {
                Title = "Top-Selling Dishes",
                Metrics = top.Select(d => new ReportMetricResponse
                {
                    Name  = d.DishName,
                    Value = $"{d.TotalQuantityOrdered} sold | {FormatCurrency(d.TotalRevenue)} revenue | avg {FormatCurrency(d.AverageUnitPrice)}"
                }).ToList()
            });
        }

        // Bottom 5 worst-sellers (if enough data)
        if (data.Count > 10)
        {
            var bottom = data.TakeLast(5).ToList();
            sections.Add(new ReportSectionResponse
            {
                Title = "Least-Selling Dishes",
                Metrics = bottom.Select(d => new ReportMetricResponse
                {
                    Name  = d.DishName,
                    Value = $"{d.TotalQuantityOrdered} sold | {FormatCurrency(d.TotalRevenue)} revenue"
                }).ToList()
            });
        }

        if (data.Count == 0)
            sections.Add(NoDataSection(request));

        return sections;
    }

    // ── Waiter ────────────────────────────────────────────────────────────────

    private async Task<IReadOnlyCollection<ReportSectionResponse>> BuildWaiterSectionsAsync(
        GenerateReportRequest request, CancellationToken ct)
    {
        var data = await reportingData.GetWaiterPerformanceAsync(
            request.FromUtc, request.ToUtc, request.RestaurantId, ct);

        var sections = new List<ReportSectionResponse>();

        sections.Add(new ReportSectionResponse
        {
            Title = "Waiter Performance Summary",
            Metrics =
            [
                new() { Name = "Period",         Value = FormatPeriod(request) },
                new() { Name = "Total Waiters",  Value = data.Count.ToString() },
                new() { Name = "Avg Cuisine Rating", Value = data.Any(w => w.AverageCuisineRating.HasValue)
                    ? $"{data.Where(w => w.AverageCuisineRating.HasValue).Average(w => w.AverageCuisineRating!.Value):N1}/5"
                    : "N/A" },
                new() { Name = "Avg Service Rating", Value = data.Any(w => w.AverageServiceRating.HasValue)
                    ? $"{data.Where(w => w.AverageServiceRating.HasValue).Average(w => w.AverageServiceRating!.Value):N1}/5"
                    : "N/A" }
            ]
        });

        foreach (var w in data)
        {
            var metrics = new List<ReportMetricResponse>
            {
                new() { Name = "Waiter ID",           Value = w.WaiterId },
                new() { Name = "Total Reservations",  Value = w.TotalReservations.ToString() },
                new() { Name = "Avg Cuisine Rating",  Value = w.AverageCuisineRating.HasValue ? $"{w.AverageCuisineRating:N1}/5" : "No reviews" },
                new() { Name = "Avg Service Rating",  Value = w.AverageServiceRating.HasValue ? $"{w.AverageServiceRating:N1}/5" : "No reviews" }
            };

            foreach (var comment in w.RecentComments)
                metrics.Add(new ReportMetricResponse { Name = "Review Comment", Value = comment });

            sections.Add(new ReportSectionResponse { Title = $"Waiter: {w.WaiterId}", Metrics = metrics });
        }

        if (data.Count == 0)
            sections.Add(NoDataSection(request));

        return sections;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IReadOnlyCollection<ReportSectionResponse> BuildUnknownTypeSection(GenerateReportRequest request)
    {
        return
        [
            new ReportSectionResponse
            {
                Title = "Overview",
                Metrics =
                [
                    new() { Name = "Type",   Value = request.Type },
                    new() { Name = "Format", Value = request.Format },
                    new() { Name = "Note",   Value = $"Unknown report type '{request.Type}'. Supported: profitability, menu, waiter." }
                ]
            }
        ];
    }

    private static ReportSectionResponse NoDataSection(GenerateReportRequest request) =>
        new()
        {
            Title = "No Data",
            Metrics = [new() { Name = "Note", Value = $"No data found for the selected period ({FormatPeriod(request)})." }]
        };

    private static string FormatPeriod(GenerateReportRequest request)
    {
        var from = request.FromUtc?.ToString("yyyy-MM-dd") ?? "all time";
        var to   = request.ToUtc?.ToString("yyyy-MM-dd")   ?? "present";
        return $"{from} → {to}";
    }

    private static string FormatCurrency(decimal amount) => $"₴{amount:N2}";
}

