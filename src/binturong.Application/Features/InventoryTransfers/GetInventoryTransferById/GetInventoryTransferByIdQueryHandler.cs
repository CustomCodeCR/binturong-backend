using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.InventoryTransfers.GetInventoryTransferById;

internal sealed class GetInventoryTransferByIdQueryHandler
    : IQueryHandler<GetInventoryTransferByIdQuery, InventoryTransferReadModel>
{
    private readonly IMongoDatabase _db;

    public GetInventoryTransferByIdQueryHandler(IMongoDatabase db) => _db = db;

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

        return Result.Success(doc);
    }
}
