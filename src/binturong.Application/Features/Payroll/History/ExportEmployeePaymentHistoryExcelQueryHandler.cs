using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Features.Payroll.History;

internal sealed class ExportEmployeePaymentHistoryExcelQueryHandler
    : IQueryHandler<ExportEmployeePaymentHistoryExcelQuery, byte[]>
{
    private readonly IQueryHandler<
        GetEmployeePaymentHistoryQuery,
        IReadOnlyList<EmployeePaymentHistoryRow>
    > _history;
    private readonly IExcelExporter _excel;

    public ExportEmployeePaymentHistoryExcelQueryHandler(
        IQueryHandler<
            GetEmployeePaymentHistoryQuery,
            IReadOnlyList<EmployeePaymentHistoryRow>
        > history,
        IExcelExporter excel
    )
    {
        _history = history;
        _excel = excel;
    }

    public async Task<Result<byte[]>> Handle(
        ExportEmployeePaymentHistoryExcelQuery q,
        CancellationToken ct
    )
    {
        var res = await _history.Handle(
            new GetEmployeePaymentHistoryQuery(q.EmployeeId, q.FromUtc, q.ToUtc),
            ct
        );
        if (res.IsFailure)
            return Result.Failure<byte[]>(res.Error);

        var bytes = _excel.Export(res.Value, "Payments");
        return Result.Success(bytes);
    }
}
