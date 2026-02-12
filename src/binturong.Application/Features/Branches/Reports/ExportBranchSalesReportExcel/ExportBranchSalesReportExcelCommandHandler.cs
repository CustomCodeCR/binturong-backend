using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.Reports.ExportBranchSalesReportExcel;

internal sealed class ExportBranchSalesReportExcelCommandHandler
    : ICommandHandler<ExportBranchSalesReportExcelCommand, byte[]>
{
    private readonly IMongoDatabase _db;
    private readonly IExcelExporter _excel;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public ExportBranchSalesReportExcelCommandHandler(
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
        ExportBranchSalesReportExcelCommand cmd,
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

        var docs = await col.Find(filter).SortByDescending(x => x.OrderDate).ToListAsync(ct);

        var rows = docs.Select(x => new BranchSalesOrderRow(
                x.Code,
                x.OrderDate,
                x.Status,
                x.Currency,
                x.Total
            ))
            .ToList();

        var bytes = _excel.Export(rows, "BranchSalesOrders");

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Branches",
            "BranchReport",
            cmd.BranchId,
            "BRANCH_SALES_REPORT_EXCEL_EXPORTED",
            string.Empty,
            $"branchId={cmd.BranchId}; from={cmd.From}; to={cmd.To}; status={cmd.Status}; count={docs.Count}; bytes={bytes.Length}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(bytes);
    }

    private sealed record BranchSalesOrderRow(
        string Code,
        DateTime OrderDate,
        string Status,
        string Currency,
        decimal Total
    );
}
