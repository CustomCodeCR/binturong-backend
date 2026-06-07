using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using Application.ReadModels.Sales;
using Application.ReadModels.Security;
using Domain.Discounts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Discounts;

internal sealed class DiscountHistoryProjection
    : IProjector<DiscountApprovalRequestedDomainEvent>,
        IProjector<DiscountApprovalApprovedDomainEvent>,
        IProjector<DiscountApprovalRejectedDomainEvent>,
        IProjector<SalesOrderLineDiscountAppliedDomainEvent>,
        IProjector<SalesOrderLineDiscountRemovedDomainEvent>,
        IProjector<SalesOrderGlobalDiscountAppliedDomainEvent>,
        IProjector<SalesOrderGlobalDiscountRemovedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public DiscountHistoryProjection(IMongoDatabase db)
    {
        _db = db;
    }

    public async Task ProjectAsync(DiscountApprovalRequestedDomainEvent e, CancellationToken ct)
    {
        var salesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct);
        var userName = await ResolveUserNameAsync(e.RequestedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: e.SalesOrderId,
            salesOrderCode: salesOrderCode,
            salesOrderDetailId: e.SalesOrderDetailId,
            scope: e.Scope,
            action: DiscountHistoryActions.Requested,
            discountPercentage: e.RequestedPercentage,
            discountAmount: e.RequestedAmount,
            reason: e.Reason,
            rejectionReason: null,
            userId: e.RequestedByUserId,
            userName: userName,
            eventDateUtc: e.RequestedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    public async Task ProjectAsync(DiscountApprovalApprovedDomainEvent e, CancellationToken ct)
    {
        var userName = await ResolveUserNameAsync(e.ApprovedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: null,
            salesOrderCode: null,
            salesOrderDetailId: null,
            scope: DiscountHistoryScopes.Approval,
            action: DiscountHistoryActions.Approved,
            discountPercentage: null,
            discountAmount: null,
            reason: null,
            rejectionReason: null,
            userId: e.ApprovedByUserId,
            userName: userName,
            eventDateUtc: e.ApprovedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    public async Task ProjectAsync(DiscountApprovalRejectedDomainEvent e, CancellationToken ct)
    {
        var userName = await ResolveUserNameAsync(e.RejectedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: null,
            salesOrderCode: null,
            salesOrderDetailId: null,
            scope: DiscountHistoryScopes.Approval,
            action: DiscountHistoryActions.Rejected,
            discountPercentage: null,
            discountAmount: null,
            reason: null,
            rejectionReason: e.RejectionReason,
            userId: e.RejectedByUserId,
            userName: userName,
            eventDateUtc: e.RejectedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    public async Task ProjectAsync(SalesOrderLineDiscountAppliedDomainEvent e, CancellationToken ct)
    {
        var salesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct);
        var userName = await ResolveUserNameAsync(e.AppliedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: e.SalesOrderId,
            salesOrderCode: salesOrderCode,
            salesOrderDetailId: e.SalesOrderDetailId,
            scope: DiscountHistoryScopes.Line,
            action: DiscountHistoryActions.Applied,
            discountPercentage: e.DiscountPerc,
            discountAmount: e.DiscountAmount,
            reason: e.Reason,
            rejectionReason: null,
            userId: e.AppliedByUserId,
            userName: userName,
            eventDateUtc: e.AppliedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    public async Task ProjectAsync(SalesOrderLineDiscountRemovedDomainEvent e, CancellationToken ct)
    {
        var salesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct);
        var userName = await ResolveUserNameAsync(e.RemovedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: e.SalesOrderId,
            salesOrderCode: salesOrderCode,
            salesOrderDetailId: e.SalesOrderDetailId,
            scope: DiscountHistoryScopes.Line,
            action: DiscountHistoryActions.Removed,
            discountPercentage: null,
            discountAmount: null,
            reason: null,
            rejectionReason: null,
            userId: e.RemovedByUserId,
            userName: userName,
            eventDateUtc: e.RemovedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    public async Task ProjectAsync(
        SalesOrderGlobalDiscountAppliedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct);
        var userName = await ResolveUserNameAsync(e.AppliedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: e.SalesOrderId,
            salesOrderCode: salesOrderCode,
            salesOrderDetailId: null,
            scope: DiscountHistoryScopes.Total,
            action: DiscountHistoryActions.Applied,
            discountPercentage: e.DiscountPerc,
            discountAmount: e.DiscountAmount,
            reason: e.Reason,
            rejectionReason: null,
            userId: e.AppliedByUserId,
            userName: userName,
            eventDateUtc: e.AppliedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    public async Task ProjectAsync(
        SalesOrderGlobalDiscountRemovedDomainEvent e,
        CancellationToken ct
    )
    {
        var salesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct);
        var userName = await ResolveUserNameAsync(e.RemovedByUserId, ct);

        var doc = CreateHistory(
            salesOrderId: e.SalesOrderId,
            salesOrderCode: salesOrderCode,
            salesOrderDetailId: null,
            scope: DiscountHistoryScopes.Total,
            action: DiscountHistoryActions.Removed,
            discountPercentage: null,
            discountAmount: null,
            reason: null,
            rejectionReason: null,
            userId: e.RemovedByUserId,
            userName: userName,
            eventDateUtc: e.RemovedAtUtc
        );

        await InsertAsync(doc, ct);
    }

    private async Task InsertAsync(DiscountHistoryReadModel doc, CancellationToken ct)
    {
        var col = _db.GetCollection<DiscountHistoryReadModel>(MongoCollections.DiscountHistory);
        await col.InsertOneAsync(doc, cancellationToken: ct);
    }

    private static DiscountHistoryReadModel CreateHistory(
        Guid? salesOrderId,
        string? salesOrderCode,
        Guid? salesOrderDetailId,
        string scope,
        string action,
        decimal? discountPercentage,
        decimal? discountAmount,
        string? reason,
        string? rejectionReason,
        Guid userId,
        string? userName,
        DateTime eventDateUtc
    )
    {
        var historyId = Guid.NewGuid();

        return new DiscountHistoryReadModel
        {
            Id = $"discount_history:{historyId}",
            HistoryId = historyId,

            SalesOrderId = salesOrderId,
            SalesOrderCode = salesOrderCode,
            SalesOrderDetailId = salesOrderDetailId,

            QuoteId = null,
            QuoteCode = null,
            QuoteDetailId = null,

            Scope = scope,
            Action = action,

            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,

            Reason = reason,
            RejectionReason = rejectionReason,

            UserId = userId,
            UserName = userName,

            EventDateUtc = eventDateUtc,
        };
    }

    private async Task<string?> ResolveSalesOrderCodeAsync(Guid salesOrderId, CancellationToken ct)
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);

        var salesOrder = await salesOrders
            .Find(x => x.SalesOrderId == salesOrderId)
            .FirstOrDefaultAsync(ct);

        return salesOrder?.Code;
    }

    private async Task<string?> ResolveUserNameAsync(Guid userId, CancellationToken ct)
    {
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var user = await users.Find(x => x.UserId == userId).FirstOrDefaultAsync(ct);

        if (user is null)
            return null;

        if (!string.IsNullOrWhiteSpace(user.Username))
            return user.Username;

        return user.Email;
    }
}

internal static class DiscountHistoryScopes
{
    public const string Line = "Line";
    public const string Total = "Total";
    public const string Approval = "Approval";
}

internal static class DiscountHistoryActions
{
    public const string Applied = "Applied";
    public const string Removed = "Removed";
    public const string Requested = "Requested";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
