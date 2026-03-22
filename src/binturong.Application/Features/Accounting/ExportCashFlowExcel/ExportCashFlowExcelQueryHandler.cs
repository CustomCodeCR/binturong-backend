using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Accounting.GetCashFlow;
using SharedKernel;

namespace Application.Features.Accounting.ExportCashFlowExcel;

internal sealed class ExportCashFlowExcelQueryHandler
    : IQueryHandler<ExportCashFlowExcelQuery, ExportCashFlowExcelResponse>
{
    private readonly IQueryHandler<
        GetCashFlowQuery,
        Application.ReadModels.Accounting.CashFlowReadModel
    > _handler;
    private readonly IExcelExporter _excel;

    public ExportCashFlowExcelQueryHandler(
        IQueryHandler<
            GetCashFlowQuery,
            Application.ReadModels.Accounting.CashFlowReadModel
        > handler,
        IExcelExporter excel
    )
    {
        _handler = handler;
        _excel = excel;
    }

    public async Task<Result<ExportCashFlowExcelResponse>> Handle(
        ExportCashFlowExcelQuery q,
        CancellationToken ct
    )
    {
        var result = await _handler.Handle(new GetCashFlowQuery(q.FromUtc, q.ToUtc), ct);
        if (result.IsFailure)
            return Result.Failure<ExportCashFlowExcelResponse>(result.Error);

        var bytes = _excel.Export(result.Value.Points, "CashFlow");

        return Result.Success(
            new ExportCashFlowExcelResponse(
                "cash-flow.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes
            )
        );
    }
}
