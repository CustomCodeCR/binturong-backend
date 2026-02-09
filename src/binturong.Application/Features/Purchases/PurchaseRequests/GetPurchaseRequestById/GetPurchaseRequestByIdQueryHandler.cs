using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Purchases;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Purchases.PurchaseRequests.GetPurchaseRequestById;

internal sealed class GetPurchaseRequestByIdQueryHandler
    : IQueryHandler<GetPurchaseRequestByIdQuery, PurchaseRequestReadModel>
{
    private const string CollectionName = "purchase_requests";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetPurchaseRequestByIdQueryHandler(
        IMongoDatabase db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result<PurchaseRequestReadModel>> Handle(
        GetPurchaseRequestByIdQuery q,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<PurchaseRequestReadModel>(CollectionName);

        var doc = await col.Find(x => x.RequestId == q.RequestId).FirstOrDefaultAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Purchases",
            "PurchaseRequest",
            q.RequestId,
            "PURCHASE_REQUEST_READ",
            string.Empty,
            $"requestId={q.RequestId}; found={(doc is not null)}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return doc is null
            ? Result.Failure<PurchaseRequestReadModel>(
                Error.NotFound(
                    "PurchaseRequests.NotFound",
                    $"Purchase request '{q.RequestId}' not found"
                )
            )
            : Result.Success(doc);
    }
}
