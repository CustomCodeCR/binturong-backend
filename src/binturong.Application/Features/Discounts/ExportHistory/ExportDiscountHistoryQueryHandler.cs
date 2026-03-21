using System.Text;
using Application.Abstractions.Messaging;
using Application.Features.Discounts.GetDiscountHistory;
using SharedKernel;

namespace Application.Features.Discounts.ExportHistory;

internal sealed class ExportDiscountHistoryQueryHandler
    : IQueryHandler<ExportDiscountHistoryQuery, ExportDiscountHistoryResponse>
{
    private readonly IQueryHandler<
        GetDiscountHistoryQuery,
        IReadOnlyList<Application.ReadModels.Discounts.DiscountHistoryReadModel>
    > _historyHandler;

    public ExportDiscountHistoryQueryHandler(
        IQueryHandler<
            GetDiscountHistoryQuery,
            IReadOnlyList<Application.ReadModels.Discounts.DiscountHistoryReadModel>
        > historyHandler
    )
    {
        _historyHandler = historyHandler;
    }

    public async Task<Result<ExportDiscountHistoryResponse>> Handle(
        ExportDiscountHistoryQuery q,
        CancellationToken ct
    )
    {
        var history = await _historyHandler.Handle(
            new GetDiscountHistoryQuery(1, 5000, q.Search, q.UserId, q.FromUtc, q.ToUtc),
            ct
        );

        if (history.IsFailure)
            return Result.Failure<ExportDiscountHistoryResponse>(history.Error);

        var sb = new StringBuilder();
        sb.AppendLine(
            "SalesOrderCode,Scope,Action,DiscountPercentage,DiscountAmount,Reason,RejectionReason,UserName,EventDateUtc"
        );

        foreach (var row in history.Value)
        {
            sb.AppendLine(
                $"\"{row.SalesOrderCode}\",\"{row.Scope}\",\"{row.Action}\",\"{row.DiscountPercentage}\",\"{row.DiscountAmount}\",\"{row.Reason}\",\"{row.RejectionReason}\",\"{row.UserName}\",\"{row.EventDateUtc:O}\""
            );
        }

        return Result.Success(
            new ExportDiscountHistoryResponse(
                "discount-history.csv",
                "text/csv",
                Encoding.UTF8.GetBytes(sb.ToString())
            )
        );
    }
}
