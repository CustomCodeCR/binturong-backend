using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Payments.Reports.ExportPaymentHistoryExcel;

internal sealed class ExportPaymentHistoryExcelCommandHandler
    : ICommandHandler<ExportPaymentHistoryExcelCommand, byte[]>
{
    private readonly IMongoDatabase _db;
    private readonly IExcelExporter _excel;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ExportPaymentHistoryExcelCommandHandler(
        IMongoDatabase db,
        IExcelExporter excel,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _excel = excel;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<byte[]>> Handle(
        ExportPaymentHistoryExcelCommand cmd,
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

        var rows = docs.Select(ToRow).ToList();

        var bytes = _excel.Export(rows, "PaymentHistory");

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Payments",
            "PaymentReport",
            null,
            "PAYMENT_HISTORY_EXCEL_EXPORTED",
            string.Empty,
            $"from={cmd.From}; to={cmd.To}; clientId={cmd.ClientId}; paymentMethodId={cmd.PaymentMethodId}; search={cmd.Search}; count={docs.Count}; bytes={bytes.Length}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(bytes);
    }

    private static PaymentHistoryRow ToRow(PaymentReadModel x)
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

        return new PaymentHistoryRow(
            x.PaymentDate,
            x.PaymentId,
            x.ClientId,
            x.ClientName ?? string.Empty,
            x.PaymentMethodId,
            method,
            x.TotalAmount,
            x.Reference ?? string.Empty,
            x.Notes ?? string.Empty,
            applied
        );
    }

    private sealed record PaymentHistoryRow(
        DateTime PaymentDateUtc,
        Guid PaymentId,
        Guid ClientId,
        string ClientName,
        Guid PaymentMethodId,
        string PaymentMethod,
        decimal TotalAmount,
        string Reference,
        string Notes,
        string AppliedInvoices
    );
}
