using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.InventoryTransfers.GetInventoryTransferById;

internal sealed class GetInventoryTransferByIdQueryHandler
    : IQueryHandler<GetInventoryTransferByIdQuery, InventoryTransferReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetInventoryTransferByIdQueryHandler(
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

    public async Task<Result<InventoryTransferReadModel>> Handle(
        GetInventoryTransferByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );
        var id = $"transfer:{query.TransferId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
            return Result.Failure<InventoryTransferReadModel>(
                Error.NotFound(
                    "InventoryTransfers.NotFound",
                    $"Transfer '{query.TransferId}' not found"
                )
            );

        await _bus.AuditAsync(
            _currentUser.UserId,
            "InventoryTransfers",
            "InventoryTransfer",
            query.TransferId,
            "TRANSFER_READ",
            string.Empty,
            $"transferId={query.TransferId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
