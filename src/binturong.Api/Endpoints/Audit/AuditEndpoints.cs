using System.Net;
using System.Text;
using Api.Security;
using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Audit.GetAuditLogs;
using Application.ReadModels.Audit;
using Application.Security.Scopes;

namespace Api.Endpoints.Audit;

public sealed class AuditEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit").WithTags("Audit");

        // GET /api/audit?from&to&module&action
        group
            .MapGet(
                "/",
                async (
                    DateTime? from,
                    DateTime? to,
                    string? module,
                    string? action,
                    IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogReadModel>> handler,
                    CancellationToken ct
                ) =>
                {
                    var query = new GetAuditLogsQuery(from, to, null, module, action);
                    var result = await handler.Handle(query, ct);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            )
            .RequireScope(SecurityScopes.AuditRead);

        // GET /api/audit/export/pdf?from&to&module&action
        group
            .MapGet(
                "/export/pdf",
                async (
                    DateTime? from,
                    DateTime? to,
                    string? module,
                    string? action,
                    IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogReadModel>> handler,
                    IPdfGenerator pdf,
                    CancellationToken ct
                ) =>
                {
                    var query = new GetAuditLogsQuery(from, to, null, module, action);
                    var result = await handler.Handle(query, ct);

                    if (result.IsFailure)
                        return Results.BadRequest(result.Error);

                    var html = BuildAuditHtml(result.Value, from, to, module, action);
                    var bytes = await pdf.RenderHtmlToPdfAsync(html, ct);

                    var name = $"audit_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                    return Results.File(bytes, "application/pdf", name);
                }
            )
            .RequireScope(SecurityScopes.AuditExport);

        // GET /api/audit/export/excel?from&to&module&action
        group
            .MapGet(
                "/export/excel",
                async (
                    DateTime? from,
                    DateTime? to,
                    string? module,
                    string? action,
                    IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogReadModel>> handler,
                    IExcelExporter excel,
                    CancellationToken ct
                ) =>
                {
                    var query = new GetAuditLogsQuery(from, to, null, module, action);
                    var result = await handler.Handle(query, ct);

                    if (result.IsFailure)
                        return Results.BadRequest(result.Error);

                    // Exporta exactamente lo que estás viendo (filtrado)
                    var bytes = excel.Export(result.Value, sheetName: "Audit");

                    var name = $"audit_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                    return Results.File(
                        bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        name
                    );
                }
            )
            .RequireScope(SecurityScopes.AuditExport);
    }

    private static string BuildAuditHtml(
        IReadOnlyList<AuditLogReadModel> rows,
        DateTime? from,
        DateTime? to,
        string? module,
        string? action
    )
    {
        static string E(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

        var sb = new StringBuilder();
        sb.AppendLine("<html><head><meta charset='utf-8'/>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Arial,Helvetica,sans-serif;font-size:12px;}");
        sb.AppendLine("h1{font-size:16px;margin:0 0 10px 0;}");
        sb.AppendLine("table{width:100%;border-collapse:collapse;}");
        sb.AppendLine("th,td{border:1px solid #ddd;padding:6px;vertical-align:top;}");
        sb.AppendLine("th{background:#f3f3f3;}");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<h1>Audit Log Export</h1>");
        sb.AppendLine("<div>");
        sb.AppendLine($"<b>From:</b> {E(from?.ToString("u"))}<br/>");
        sb.AppendLine($"<b>To:</b> {E(to?.ToString("u"))}<br/>");
        sb.AppendLine($"<b>Module:</b> {E(module)}<br/>");
        sb.AppendLine($"<b>Action:</b> {E(action)}<br/>");
        sb.AppendLine($"<b>Count:</b> {rows.Count}<br/>");
        sb.AppendLine("</div><br/>");

        sb.AppendLine("<table>");
        sb.AppendLine("<thead><tr>");
        sb.AppendLine(
            "<th>EventDate</th><th>Module</th><th>Entity</th><th>EntityId</th><th>Action</th><th>UserId</th><th>Ip</th><th>UserAgent</th><th>DataAfter</th>"
        );
        sb.AppendLine("</tr></thead><tbody>");

        foreach (var r in rows)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{E(r.EventDate.ToString("u"))}</td>");
            sb.AppendLine($"<td>{E(r.Module)}</td>");
            sb.AppendLine($"<td>{E(r.Entity)}</td>");
            sb.AppendLine($"<td>{E(r.EntityId?.ToString())}</td>");
            sb.AppendLine($"<td>{E(r.Action)}</td>");
            sb.AppendLine($"<td>{E(r.UserId?.ToString())}</td>");
            sb.AppendLine($"<td>{E(r.Ip)}</td>");
            sb.AppendLine($"<td>{E(r.UserAgent)}</td>");
            sb.AppendLine($"<td>{E(r.DataAfter)}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        return sb.ToString();
    }
}
