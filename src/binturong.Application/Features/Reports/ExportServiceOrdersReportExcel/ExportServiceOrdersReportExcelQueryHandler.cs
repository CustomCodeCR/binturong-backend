using Application.Abstractions.Documents;
using Application.Abstractions.Messaging;
using Application.Features.Reports.GetServiceOrdersReport;
using SharedKernel;

namespace Application.Features.Reports.ExportServiceOrdersReportExcel;

internal sealed class ExportServiceOrdersReportExcelQueryHandler
    : IQueryHandler<ExportServiceOrdersReportExcelQuery, ExportServiceOrdersReportExcelResponse>
{
    private readonly IQueryHandler<
        GetServiceOrdersReportQuery,
        Application.ReadModels.Reports.ServiceOrdersReportReadModel
    > _handler;
    private readonly IExcelExporter _excel;

    public ExportServiceOrdersReportExcelQueryHandler(
        IQueryHandler<
            GetServiceOrdersReportQuery,
            Application.ReadModels.Reports.ServiceOrdersReportReadModel
        > handler,
        IExcelExporter excel
    )
    {
        _handler = handler;
        _excel = excel;
    }

    public async Task<Result<ExportServiceOrdersReportExcelResponse>> Handle(
        ExportServiceOrdersReportExcelQuery q,
        CancellationToken ct
    )
    {
        var result = await _handler.Handle(
            new GetServiceOrdersReportQuery(q.FromUtc, q.ToUtc, q.EmployeeId),
            ct
        );

        if (result.IsFailure)
            return Result.Failure<ExportServiceOrdersReportExcelResponse>(result.Error);

        var rows = result
            .Value.Items.Select(x => new
            {
                x.Code,
                x.ClientName,
                x.ScheduledDate,
                x.Status,
                x.ServiceAddress,
                Technicians = string.Join(" | ", x.Technicians),
                Services = string.Join(" | ", x.Services),
            })
            .ToList();

        var bytes = _excel.Export(rows, "ServiceOrders");

        return Result.Success(
            new ExportServiceOrdersReportExcelResponse(
                "service-orders-report.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes
            )
        );
    }
}
