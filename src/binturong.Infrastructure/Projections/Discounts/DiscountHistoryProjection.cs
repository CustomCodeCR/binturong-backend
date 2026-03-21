using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using Application.ReadModels.Sales;
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

    public DiscountHistoryProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(DiscountApprovalRequestedDomainEvent e, CancellationToken ct)
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = e.SalesOrderId,
                SalesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct),
                SalesOrderDetailId = e.SalesOrderDetailId,
                Scope = e.Scope,
                Action = "Requested",
                DiscountPercentage = e.RequestedPercentage,
                DiscountAmount = e.RequestedAmount,
                Reason = e.Reason,
                UserId = e.RequestedByUserId,
                UserName = await ResolveUserNameAsync(e.RequestedByUserId, ct),
                EventDateUtc = e.RequestedAtUtc,
            },
            ct
        );
    }

    public async Task ProjectAsync(DiscountApprovalApprovedDomainEvent e, CancellationToken ct)
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = Guid.Empty,
                Scope = "Approval",
                Action = "Approved",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                UserId = e.ApprovedByUserId,
                UserName = await ResolveUserNameAsync(e.ApprovedByUserId, ct),
                EventDateUtc = e.ApprovedAtUtc,
            },
            ct
        );
    }

    public async Task ProjectAsync(DiscountApprovalRejectedDomainEvent e, CancellationToken ct)
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = Guid.Empty,
                Scope = "Approval",
                Action = "Rejected",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                RejectionReason = e.RejectionReason,
                UserId = e.RejectedByUserId,
                UserName = await ResolveUserNameAsync(e.RejectedByUserId, ct),
                EventDateUtc = e.RejectedAtUtc,
            },
            ct
        );
    }

    public async Task ProjectAsync(SalesOrderLineDiscountAppliedDomainEvent e, CancellationToken ct)
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = e.SalesOrderId,
                SalesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct),
                SalesOrderDetailId = e.SalesOrderDetailId,
                Scope = "Line",
                Action = "Applied",
                DiscountPercentage = e.DiscountPerc,
                DiscountAmount = e.DiscountAmount,
                Reason = e.Reason,
                UserId = e.AppliedByUserId,
                UserName = await ResolveUserNameAsync(e.AppliedByUserId, ct),
                EventDateUtc = e.AppliedAtUtc,
            },
            ct
        );
    }

    public async Task ProjectAsync(SalesOrderLineDiscountRemovedDomainEvent e, CancellationToken ct)
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = e.SalesOrderId,
                SalesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct),
                SalesOrderDetailId = e.SalesOrderDetailId,
                Scope = "Line",
                Action = "Removed",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                UserId = e.RemovedByUserId,
                UserName = await ResolveUserNameAsync(e.RemovedByUserId, ct),
                EventDateUtc = e.RemovedAtUtc,
            },
            ct
        );
    }

    public async Task ProjectAsync(
        SalesOrderGlobalDiscountAppliedDomainEvent e,
        CancellationToken ct
    )
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = e.SalesOrderId,
                SalesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct),
                Scope = "Total",
                Action = "Applied",
                DiscountPercentage = e.DiscountPerc,
                DiscountAmount = e.DiscountAmount,
                Reason = e.Reason,
                UserId = e.AppliedByUserId,
                UserName = await ResolveUserNameAsync(e.AppliedByUserId, ct),
                EventDateUtc = e.AppliedAtUtc,
            },
            ct
        );
    }

    public async Task ProjectAsync(
        SalesOrderGlobalDiscountRemovedDomainEvent e,
        CancellationToken ct
    )
    {
        await InsertAsync(
            new DiscountHistoryReadModel
            {
                Id = $"discount_history:{Guid.NewGuid()}",
                HistoryId = Guid.NewGuid(),
                SalesOrderId = e.SalesOrderId,
                SalesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct),
                Scope = "Total",
                Action = "Removed",
                DiscountPercentage = 0,
                DiscountAmount = 0,
                UserId = e.RemovedByUserId,
                UserName = await ResolveUserNameAsync(e.RemovedByUserId, ct),
                EventDateUtc = e.RemovedAtUtc,
            },
            ct
        );
    }

    private async Task InsertAsync(DiscountHistoryReadModel doc, CancellationToken ct)
    {
        var col = _db.GetCollection<DiscountHistoryReadModel>(MongoCollections.DiscountHistory);
        await col.InsertOneAsync(doc, cancellationToken: ct);
    }

    private async Task<string?> ResolveSalesOrderCodeAsync(Guid salesOrderId, CancellationToken ct)
    {
        var salesOrders = _db.GetCollection<SalesOrderReadModel>(MongoCollections.SalesOrders);
        var so = await salesOrders
            .Find(x => x.SalesOrderId == salesOrderId)
            .FirstOrDefaultAsync(ct);
        return so?.Code;
    }

    private async Task<string?> ResolveUserNameAsync(Guid userId, CancellationToken ct)
    {
        var users = _db.GetCollection<Application.ReadModels.Security.UserReadModel>(
            MongoCollections.Users
        );
        var user = await users.Find(x => x.UserId == userId).FirstOrDefaultAsync(ct);
        if (user is null)
            return null;

        if (!string.IsNullOrWhiteSpace(user.Username))
            return user.Username;

        return user.Email;
    }
}
