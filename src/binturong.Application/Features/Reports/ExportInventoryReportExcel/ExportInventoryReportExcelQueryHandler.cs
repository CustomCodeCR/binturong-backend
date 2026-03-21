using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Reports.GetInventoryReport;
using SharedKernel;

namespace Application.Features.Reports.ExportInventoryReportExcel;

internal sealed class ExportInventoryReportExcelQueryHandler
    : IQueryHandler<ExportInventoryReportExcelQuery, ExportInventoryReportExcelResponse>
{
    private readonly IQueryHandler<
        GetInventoryReportQuery,
        Application.ReadModels.Reports.InventoryReportReadModel
    > _handler;
    private readonly IExcelExporter _excel;

    public ExportInventoryReportExcelQueryHandler(
        IQueryHandler<
            GetInventoryReportQuery,
            Application.ReadModels.Reports.InventoryReportReadModel
        > handler,
        IExcelExporter excel
    )
    {
        _handler = handler;
        _excel = excel;
    }

    public async Task<Result<ExportInventoryReportExcelResponse>> Handle(
        ExportInventoryReportExcelQuery q,
        CancellationToken ct
    )
    {
        var result = await _handler.Handle(new GetInventoryReportQuery(q.CategoryId), ct);
        if (result.IsFailure)
            return Result.Failure<ExportInventoryReportExcelResponse>(result.Error);

        var rows = result.Value.Items;
        var bytes = _excel.Export(rows, "Inventory");

        return Result.Success(
            new ExportInventoryReportExcelResponse(
                "inventory-report.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes
            )
        );
    }
}
