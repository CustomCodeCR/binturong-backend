using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Reports.GetClientReport;
using SharedKernel;

namespace Application.Features.Reports.ExportClientReportExcel;

internal sealed class ExportClientReportExcelQueryHandler
    : IQueryHandler<ExportClientReportExcelQuery, ExportClientReportExcelResponse>
{
    private readonly IQueryHandler<
        GetClientReportQuery,
        Application.ReadModels.Reports.ClientReportReadModel
    > _handler;
    private readonly IExcelExporter _excel;

    public ExportClientReportExcelQueryHandler(
        IQueryHandler<
            GetClientReportQuery,
            Application.ReadModels.Reports.ClientReportReadModel
        > handler,
        IExcelExporter excel
    )
    {
        _handler = handler;
        _excel = excel;
    }

    public async Task<Result<ExportClientReportExcelResponse>> Handle(
        ExportClientReportExcelQuery q,
        CancellationToken ct
    )
    {
        var result = await _handler.Handle(new GetClientReportQuery(q.ClientId), ct);
        if (result.IsFailure)
            return Result.Failure<ExportClientReportExcelResponse>(result.Error);

        var rows = result
            .Value.Purchases.Select(x => new
            {
                Type = "Purchase",
                Code = x.Code,
                Date = x.OrderDate,
                Amount = x.Total,
                Status = x.Status,
            })
            .Concat(
                result.Value.Services.Select(x => new
                {
                    Type = "Service",
                    Code = x.Code,
                    Date = x.ScheduledDate,
                    Amount = 0m,
                    Status = x.Status,
                })
            )
            .Concat(
                result.Value.Invoices.Select(x => new
                {
                    Type = "Invoice",
                    Code = x.Consecutive ?? string.Empty,
                    Date = x.IssueDate,
                    Amount = x.Total,
                    Status = x.TaxStatus,
                })
            )
            .ToList();

        var bytes = _excel.Export(rows, "ClientHistory");

        return Result.Success(
            new ExportClientReportExcelResponse(
                "client-history.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes
            )
        );
    }
}
