using Application.Abstractions.Projections;
using Application.ReadModels.Common;
using Application.ReadModels.Discounts;
using Application.ReadModels.Sales;
using Application.ReadModels.Security;
using Domain.Discounts;
using MongoDB.Driver;

namespace Infrastructure.Projections.Discounts;

internal sealed class DiscountApprovalProjection
    : IProjector<DiscountApprovalRequestedDomainEvent>,
        IProjector<DiscountApprovalApprovedDomainEvent>,
        IProjector<DiscountApprovalRejectedDomainEvent>
{
    private readonly IMongoDatabase _db;

    public DiscountApprovalProjection(IMongoDatabase db) => _db = db;

    public async Task ProjectAsync(DiscountApprovalRequestedDomainEvent e, CancellationToken ct)
    {
        var approvals = _db.GetCollection<DiscountApprovalRequestReadModel>(
            MongoCollections.DiscountApprovalRequests
        );

        var id = $"discount_approval:{e.ApprovalRequestId}";
        var filter = Builders<DiscountApprovalRequestReadModel>.Filter.Eq(x => x.Id, id);

        var salesOrderCode = await ResolveSalesOrderCodeAsync(e.SalesOrderId, ct);
        var requestedByName = await ResolveUserNameAsync(e.RequestedByUserId, ct);

        var update = Builders<DiscountApprovalRequestReadModel>
            .Update.SetOnInsert(x => x.Id, id)
            .SetOnInsert(x => x.ApprovalRequestId, e.ApprovalRequestId)
            .Set(x => x.SalesOrderId, e.SalesOrderId)
            .Set(x => x.SalesOrderCode, salesOrderCode)
            .Set(x => x.SalesOrderDetailId, e.SalesOrderDetailId)
            .Set(x => x.Scope, e.Scope)
            .Set(x => x.RequestedPercentage, e.RequestedPercentage)
            .Set(x => x.RequestedAmount, e.RequestedAmount)
            .Set(x => x.Reason, e.Reason)
            .Set(x => x.RequestedByUserId, e.RequestedByUserId)
            .Set(x => x.RequestedByUserName, requestedByName)
            .Set(x => x.RequestedAtUtc, e.RequestedAtUtc)
            .Set(x => x.Status, "Pending")
            .Set(x => x.ResolvedByUserId, null)
            .Set(x => x.ResolvedByUserName, null)
            .Set(x => x.ResolvedAtUtc, null)
            .Set(x => x.RejectionReason, null);

        await approvals.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(DiscountApprovalApprovedDomainEvent e, CancellationToken ct)
    {
        var approvals = _db.GetCollection<DiscountApprovalRequestReadModel>(
            MongoCollections.DiscountApprovalRequests
        );

        var id = $"discount_approval:{e.ApprovalRequestId}";
        var filter = Builders<DiscountApprovalRequestReadModel>.Filter.Eq(x => x.Id, id);
        var approverName = await ResolveUserNameAsync(e.ApprovedByUserId, ct);

        var update = Builders<DiscountApprovalRequestReadModel>
            .Update.Set(x => x.Status, "Approved")
            .Set(x => x.ResolvedByUserId, e.ApprovedByUserId)
            .Set(x => x.ResolvedByUserName, approverName)
            .Set(x => x.ResolvedAtUtc, e.ApprovedAtUtc)
            .Set(x => x.RejectionReason, null);

        await approvals.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }

    public async Task ProjectAsync(DiscountApprovalRejectedDomainEvent e, CancellationToken ct)
    {
        var approvals = _db.GetCollection<DiscountApprovalRequestReadModel>(
            MongoCollections.DiscountApprovalRequests
        );

        var id = $"discount_approval:{e.ApprovalRequestId}";
        var filter = Builders<DiscountApprovalRequestReadModel>.Filter.Eq(x => x.Id, id);
        var userName = await ResolveUserNameAsync(e.RejectedByUserId, ct);

        var update = Builders<DiscountApprovalRequestReadModel>
            .Update.Set(x => x.Status, "Rejected")
            .Set(x => x.ResolvedByUserId, e.RejectedByUserId)
            .Set(x => x.ResolvedByUserName, userName)
            .Set(x => x.ResolvedAtUtc, e.RejectedAtUtc)
            .Set(x => x.RejectionReason, e.RejectionReason);

        await approvals.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
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
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var user = await users.Find(x => x.UserId == userId).FirstOrDefaultAsync(ct);
        if (user is null)
            return null;

        if (!string.IsNullOrWhiteSpace(user.Username))
            return user.Username;

        return user.Email;
    }
}
