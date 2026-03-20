using System.Text;
using Application.Abstractions.Messaging;
using Application.Features.Employees.GetWorkHistory;
using SharedKernel;

namespace Application.Features.Employees.ExportWorkHistory;

internal sealed class ExportEmployeeWorkHistoryQueryHandler
    : IQueryHandler<ExportEmployeeWorkHistoryQuery, ExportEmployeeWorkHistoryResponse>
{
    private readonly IQueryHandler<
        GetEmployeeWorkHistoryQuery,
        Application.ReadModels.Services.EmployeeWorkHistoryReadModel
    > _historyHandler;

    public ExportEmployeeWorkHistoryQueryHandler(
        IQueryHandler<
            GetEmployeeWorkHistoryQuery,
            Application.ReadModels.Services.EmployeeWorkHistoryReadModel
        > historyHandler
    )
    {
        _historyHandler = historyHandler;
    }

    public async Task<Result<ExportEmployeeWorkHistoryResponse>> Handle(
        ExportEmployeeWorkHistoryQuery q,
        CancellationToken ct
    )
    {
        var history = await _historyHandler.Handle(
            new GetEmployeeWorkHistoryQuery(q.EmployeeId),
            ct
        );
        if (history.IsFailure)
            return Result.Failure<ExportEmployeeWorkHistoryResponse>(history.Error);

        var sb = new StringBuilder();
        sb.AppendLine(
            "ServiceOrderCode,ScheduledDate,ClosedDate,Status,ClientName,ServiceAddress,Services"
        );

        foreach (var item in history.Value.Entries)
        {
            var services = string.Join(" | ", item.Services);
            sb.AppendLine(
                $"\"{item.ServiceOrderCode}\",\"{item.ScheduledDate:O}\",\"{item.ClosedDate:O}\",\"{item.Status}\",\"{item.ClientName}\",\"{item.ServiceAddress}\",\"{services}\""
            );
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());

        return Result.Success(
            new ExportEmployeeWorkHistoryResponse(
                $"employee-work-history-{q.EmployeeId}.csv",
                "text/csv",
                bytes
            )
        );
    }
}
