using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Purchases;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.History.ExportSupplierPurchaseHistoryExcel;

internal sealed class ExportSupplierPurchaseHistoryExcelCommandHandler
    : ICommandHandler<ExportSupplierPurchaseHistoryExcelCommand, byte[]>
{
    private readonly IMongoDatabase _db;
    private readonly IExcelExporter _excel;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ExportSupplierPurchaseHistoryExcelCommandHandler(
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
        ExportSupplierPurchaseHistoryExcelCommand cmd,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseOrderReadModel>(MongoCollections.PurchaseOrders);

        var filter = Builders<PurchaseOrderReadModel>.Filter.Eq(x => x.SupplierId, cmd.SupplierId);

        if (cmd.From is not null)
            filter &= Builders<PurchaseOrderReadModel>.Filter.Gte(x => x.OrderDate, cmd.From.Value);

        if (cmd.To is not null)
            filter &= Builders<PurchaseOrderReadModel>.Filter.Lte(x => x.OrderDate, cmd.To.Value);

        if (!string.IsNullOrWhiteSpace(cmd.Status))
            filter &= Builders<PurchaseOrderReadModel>.Filter.Eq(x => x.Status, cmd.Status.Trim());

        var docs = await col.Find(filter).SortByDescending(x => x.OrderDate).ToListAsync(ct);

        var rows = docs.Select(x => new SupplierPurchaseHistoryRow(
                x.Code,
                x.OrderDate,
                x.Status,
                x.Currency,
                x.Total
            ))
            .ToList();

        var bytes = _excel.Export(rows, "SupplierPurchaseHistory");

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Suppliers",
            "SupplierPurchaseHistory",
            cmd.SupplierId,
            "SUPPLIER_PURCHASE_HISTORY_EXCEL_EXPORTED",
            string.Empty,
            $"supplierId={cmd.SupplierId}; from={cmd.From}; to={cmd.To}; status={cmd.Status}; count={docs.Count}; bytes={bytes.Length}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(bytes);
    }

    private sealed record SupplierPurchaseHistoryRow(
        string Code,
        DateTime OrderDate,
        string Status,
        string Currency,
        decimal Total
    );
}
