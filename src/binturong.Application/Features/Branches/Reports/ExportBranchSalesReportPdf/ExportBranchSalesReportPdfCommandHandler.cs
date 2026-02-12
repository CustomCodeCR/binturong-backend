using System.Net;
using System.Text;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Reports;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.Reports.ExportBranchSalesReportPdf;

internal sealed class ExportBranchSalesReportPdfCommandHandler
    : ICommandHandler<ExportBranchSalesReportPdfCommand, byte[]>
{
    private readonly IMongoDatabase _db;
    private readonly IPdfGenerator _pdf;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ExportBranchSalesReportPdfCommandHandler(
        IMongoDatabase db,
        IPdfGenerator pdf,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _pdf = pdf;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<byte[]>> Handle(
        ExportBranchSalesReportPdfCommand cmd,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.BranchId, cmd.BranchId);

        if (cmd.From is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Gte(x => x.OrderDate, cmd.From.Value);

        if (cmd.To is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Lte(x => x.OrderDate, cmd.To.Value);

        if (!string.IsNullOrWhiteSpace(cmd.Status))
            filter &= Builders<SalesOrderReadModel>.Filter.Eq(x => x.Status, cmd.Status.Trim());

        var docs = await col.Find(filter).ToListAsync(ct);

        var ordersCount = docs.Count;
        var totalSales = docs.Sum(x => x.Total);
        var avg = ordersCount == 0 ? 0m : totalSales / ordersCount;

        var byDay = docs.GroupBy(x => new DateTime(
                x.OrderDate.Year,
                x.OrderDate.Month,
                x.OrderDate.Day,
                0,
                0,
                0,
                DateTimeKind.Utc
            ))
            .OrderBy(g => g.Key)
            .Select(g => new BranchSalesByDayReadModel
            {
                DayUtc = g.Key,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(x => x.Total),
            })
            .ToList();

        var byCurrency = docs.GroupBy(x => (x.Currency ?? string.Empty).Trim())
            .OrderBy(g => g.Key)
            .Select(g => new BranchSalesByCurrencyReadModel
            {
                Currency = string.IsNullOrWhiteSpace(g.Key) ? "N/A" : g.Key,
                OrdersCount = g.Count(),
                TotalSales = g.Sum(x => x.Total),
            })
            .ToList();

        var rm = new BranchSalesReportReadModel
        {
            BranchId = cmd.BranchId,
            From = cmd.From,
            To = cmd.To,
            OrdersCount = ordersCount,
            TotalSales = totalSales,
            AverageOrder = avg,
            ByDay = byDay,
            ByCurrency = byCurrency,
        };

        var html = BuildHtml(rm);
        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Branches",
            "BranchReport",
            cmd.BranchId,
            "BRANCH_SALES_REPORT_PDF_EXPORTED",
            string.Empty,
            $"branchId={cmd.BranchId}; from={cmd.From}; to={cmd.To}; status={cmd.Status}; orders={ordersCount}; total={totalSales}; bytes={bytes.Length}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(bytes);
    }

    private static string BuildHtml(BranchSalesReportReadModel rm)
    {
        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'></head><body>");
        sb.Append("<h2>Branch Sales Report</h2>");
        sb.Append("<div>");
        sb.Append($"<div>BranchId: {WebUtility.HtmlEncode(rm.BranchId.ToString())}</div>");
        sb.Append($"<div>From: {WebUtility.HtmlEncode(rm.From?.ToString("O") ?? "")}</div>");
        sb.Append($"<div>To: {WebUtility.HtmlEncode(rm.To?.ToString("O") ?? "")}</div>");
        sb.Append($"<div>Orders: {WebUtility.HtmlEncode(rm.OrdersCount.ToString())}</div>");
        sb.Append(
            $"<div>TotalSales: {WebUtility.HtmlEncode(rm.TotalSales.ToString("0.##"))}</div>"
        );
        sb.Append(
            $"<div>AverageOrder: {WebUtility.HtmlEncode(rm.AverageOrder.ToString("0.##"))}</div>"
        );
        sb.Append("</div><br/>");

        sb.Append("<h3>By Currency</h3>");
        sb.Append(
            "<table border='1' cellspacing='0' cellpadding='6'><thead><tr><th>Currency</th><th>Orders</th><th>Total</th></tr></thead><tbody>"
        );
        foreach (var r in rm.ByCurrency)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.Currency)}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.OrdersCount.ToString())}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.TotalSales.ToString("0.##"))}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table><br/>");

        sb.Append("<h3>By Day</h3>");
        sb.Append(
            "<table border='1' cellspacing='0' cellpadding='6'><thead><tr><th>Day</th><th>Orders</th><th>Total</th></tr></thead><tbody>"
        );
        foreach (var r in rm.ByDay)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.DayUtc.ToString("yyyy-MM-dd"))}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.OrdersCount.ToString())}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.TotalSales.ToString("0.##"))}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table>");

        sb.Append("</body></html>");
        return sb.ToString();
    }
}
