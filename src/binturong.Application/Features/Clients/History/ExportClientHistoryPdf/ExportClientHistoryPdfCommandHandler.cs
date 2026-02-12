using System.Net;
using System.Text;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Clients.History.ExportClientHistoryPdf;

internal sealed class ExportClientHistoryPdfCommandHandler
    : ICommandHandler<ExportClientHistoryPdfCommand, byte[]>
{
    private readonly IMongoDatabase _db;
    private readonly IPdfGenerator _pdf;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ExportClientHistoryPdfCommandHandler(
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
        ExportClientHistoryPdfCommand cmd,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var filter = Builders<SalesOrderReadModel>.Filter.Eq(x => x.ClientId, cmd.ClientId);

        if (cmd.From is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Gte(x => x.OrderDate, cmd.From.Value);

        if (cmd.To is not null)
            filter &= Builders<SalesOrderReadModel>.Filter.Lte(x => x.OrderDate, cmd.To.Value);

        if (!string.IsNullOrWhiteSpace(cmd.Status))
            filter &= Builders<SalesOrderReadModel>.Filter.Eq(x => x.Status, cmd.Status.Trim());

        var docs = await col.Find(filter).SortByDescending(x => x.OrderDate).ToListAsync(ct);

        var html = BuildHtml(cmd.ClientId, cmd.From, cmd.To, cmd.Status, docs);

        var bytes = await _pdf.RenderHtmlToPdfAsync(html, ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Clients",
            "ClientHistory",
            cmd.ClientId,
            "CLIENT_HISTORY_PDF_EXPORTED",
            string.Empty,
            $"clientId={cmd.ClientId}; from={cmd.From}; to={cmd.To}; status={cmd.Status}; count={docs.Count}; bytes={bytes.Length}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(bytes);
    }

    private static string BuildHtml(
        Guid clientId,
        DateTime? from,
        DateTime? to,
        string? status,
        IReadOnlyList<SalesOrderReadModel> rows
    )
    {
        var sb = new StringBuilder();

        sb.Append("<html><head><meta charset='utf-8'></head><body>");
        sb.Append("<h2>Client History</h2>");
        sb.Append("<div>");
        sb.Append($"<div>ClientId: {WebUtility.HtmlEncode(clientId.ToString())}</div>");
        sb.Append($"<div>From: {WebUtility.HtmlEncode(from?.ToString("O") ?? "")}</div>");
        sb.Append($"<div>To: {WebUtility.HtmlEncode(to?.ToString("O") ?? "")}</div>");
        sb.Append($"<div>Status: {WebUtility.HtmlEncode(status ?? "")}</div>");
        sb.Append("</div>");
        sb.Append("<br/>");

        if (rows.Count == 0)
        {
            sb.Append("<div>No hay registros previos</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        sb.Append("<table border='1' cellspacing='0' cellpadding='6'>");
        sb.Append("<thead><tr>");
        sb.Append("<th>Code</th><th>OrderDate</th><th>Status</th><th>Currency</th><th>Total</th>");
        sb.Append("</tr></thead><tbody>");

        foreach (var r in rows)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.Code)}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.OrderDate.ToString("O"))}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.Status)}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.Currency)}</td>");
            sb.Append($"<td>{WebUtility.HtmlEncode(r.Total.ToString("0.##"))}</td>");
            sb.Append("</tr>");
        }

        sb.Append("</tbody></table>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
