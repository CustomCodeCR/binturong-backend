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

namespace Application.Features.Payments.Reports.ExportPaymentHistoryPdf;

internal sealed class ExportPaymentHistoryPdfCommandHandler
    : ICommandHandler<ExportPaymentHistoryPdfCommand, byte[]>
{
    private readonly IMongoDatabase _db;
    private readonly IPdfGenerator _pdf;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ExportPaymentHistoryPdfCommandHandler(
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
        ExportPaymentHistoryPdfCommand cmd,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PaymentReadModel>(MongoCollections.Payments);

        var filter = Builders<PaymentReadModel>.Filter.Empty;

        if (cmd.From is not null)
            filter &= Builders<PaymentReadModel>.Filter.Gte(x => x.PaymentDate, cmd.From.Value);

        if (cmd.To is not null)
            filter &= Builders<PaymentReadModel>.Filter.Lte(x => x.PaymentDate, cmd.To.Value);

        if (cmd.ClientId.HasValue && cmd.ClientId.Value != Guid.Empty)
            filter &= Builders<PaymentReadModel>.Filter.Eq(x => x.ClientId, cmd.ClientId.Value);

        if (cmd.PaymentMethodId.HasValue && cmd.PaymentMethodId.Value != Guid.Empty)
            filter &= Builders<PaymentReadModel>.Filter.Eq(
                x => x.PaymentMethodId,
                cmd.PaymentMethodId.Value
            );

        if (!string.IsNullOrWhiteSpace(cmd.Search))
        {
            var s = cmd.Search.Trim();
            filter &= Builders<PaymentReadModel>.Filter.Or(
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.Reference,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.Notes,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.ClientName,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.PaymentMethodCode,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<PaymentReadModel>.Filter.Regex(
                    x => x.PaymentMethodDescription,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var docs = await col.Find(filter).SortByDescending(x => x.PaymentDate).ToListAsync(ct);

        var count = docs.Count;
        var total = docs.Sum(x => x.TotalAmount);
        var avg = count == 0 ? 0m : total / count;

        var items = docs.Select(ToItem).ToList();

        var byMethod = docs.GroupBy(x =>
            {
                var method = string.IsNullOrWhiteSpace(x.PaymentMethodDescription)
                    ? (x.PaymentMethodCode ?? string.Empty)
                    : $"{x.PaymentMethodCode} - {x.PaymentMethodDescription}";
                return string.IsNullOrWhiteSpace(method) ? "N/A" : method.Trim();
            })
            .OrderBy(g => g.Key)
            .Select(g => new PaymentHistoryByMethodReadModel
            {
                PaymentMethod = g.Key,
                PaymentsCount = g.Count(),
                TotalCollected = g.Sum(x => x.TotalAmount),
            })
            .ToList();

        var rm = new PaymentHistoryReportReadModel
        {
            From = cmd.From,
            To = cmd.To,
            ClientId = cmd.ClientId,
            PaymentMethodId = cmd.PaymentMethodId,
            Search = cmd.Search,
            PaymentsCount = count,
            TotalCollected = total,
            AveragePayment = avg,
            Items = items,
            ByMethod = byMethod,
        };

        var html = BuildHtml(rm);
        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "PaymentReport",
            null,
            "PAYMENT_HISTORY_PDF_EXPORTED",
            string.Empty,
            $"from={cmd.From}; to={cmd.To}; clientId={cmd.ClientId}; paymentMethodId={cmd.PaymentMethodId}; search={cmd.Search}; count={count}; total={total}; bytes={bytes.Length}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(bytes);
    }

    private static PaymentHistoryItemReadModel ToItem(PaymentReadModel x)
    {
        var method = string.IsNullOrWhiteSpace(x.PaymentMethodDescription)
            ? (x.PaymentMethodCode ?? string.Empty)
            : $"{x.PaymentMethodCode} - {x.PaymentMethodDescription}";

        var applied =
            x.AppliedInvoices is null || x.AppliedInvoices.Count == 0
                ? string.Empty
                : string.Join(
                    ", ",
                    x.AppliedInvoices.Select(ai =>
                        $"{(ai.InvoiceConsecutive ?? ai.InvoiceId.ToString())} ({ai.AppliedAmount:0.##})"
                    )
                );

        return new PaymentHistoryItemReadModel
        {
            PaymentId = x.PaymentId,
            PaymentDateUtc = x.PaymentDate,

            ClientId = x.ClientId,
            ClientName = x.ClientName ?? string.Empty,

            PaymentMethodId = x.PaymentMethodId,
            PaymentMethod = method,

            TotalAmount = x.TotalAmount,
            Reference = string.IsNullOrWhiteSpace(x.Reference) ? null : x.Reference,
            Notes = string.IsNullOrWhiteSpace(x.Notes) ? null : x.Notes,

            AppliedInvoices = applied,
        };
    }

    private static string BuildHtml(PaymentHistoryReportReadModel rm)
    {
        static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='utf-8'></head><body>");
        sb.Append("<h2>Payment History Report</h2>");

        sb.Append("<div>");
        sb.Append($"<div>From: {E(rm.From?.ToString("O"))}</div>");
        sb.Append($"<div>To: {E(rm.To?.ToString("O"))}</div>");
        sb.Append($"<div>ClientId: {E(rm.ClientId?.ToString())}</div>");
        sb.Append($"<div>PaymentMethodId: {E(rm.PaymentMethodId?.ToString())}</div>");
        sb.Append($"<div>Search: {E(rm.Search)}</div>");
        sb.Append($"<div>Payments: {E(rm.PaymentsCount.ToString())}</div>");
        sb.Append($"<div>TotalCollected: {E(rm.TotalCollected.ToString("0.##"))}</div>");
        sb.Append($"<div>AveragePayment: {E(rm.AveragePayment.ToString("0.##"))}</div>");
        sb.Append("</div><br/>");

        sb.Append("<h3>By Payment Method</h3>");
        sb.Append("<table border='1' cellspacing='0' cellpadding='6'>");
        sb.Append("<thead><tr><th>Method</th><th>Payments</th><th>Total</th></tr></thead><tbody>");
        foreach (var r in rm.ByMethod)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{E(r.PaymentMethod)}</td>");
            sb.Append($"<td>{E(r.PaymentsCount.ToString())}</td>");
            sb.Append($"<td>{E(r.TotalCollected.ToString("0.##"))}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table><br/>");

        sb.Append("<h3>Payments</h3>");
        sb.Append("<table border='1' cellspacing='0' cellpadding='6'>");
        sb.Append("<thead><tr>");
        sb.Append(
            "<th>Date (UTC)</th><th>Client</th><th>Method</th><th>Total</th><th>Reference</th><th>Applied</th>"
        );
        sb.Append("</tr></thead><tbody>");

        foreach (var p in rm.Items)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{E(p.PaymentDateUtc.ToString("yyyy-MM-dd HH:mm:ss"))}</td>");
            sb.Append($"<td>{E(p.ClientName)}</td>");
            sb.Append($"<td>{E(p.PaymentMethod)}</td>");
            sb.Append($"<td>{E(p.TotalAmount.ToString("0.##"))}</td>");
            sb.Append($"<td>{E(p.Reference)}</td>");
            sb.Append($"<td>{E(p.AppliedInvoices)}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</tbody></table>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
