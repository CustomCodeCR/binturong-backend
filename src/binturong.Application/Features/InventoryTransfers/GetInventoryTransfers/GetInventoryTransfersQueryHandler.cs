using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.InventoryTransfers.GetInventoryTransfers;

internal sealed class GetInventoryTransfersQueryHandler
    : IQueryHandler<GetInventoryTransfersQuery, IReadOnlyList<InventoryTransferReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetInventoryTransfersQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<InventoryTransferReadModel>>> Handle(
        GetInventoryTransfersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<InventoryTransferReadModel>>(docs);
    }

    private static FilterDefinition<InventoryTransferReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<InventoryTransferReadModel>.Filter.Empty;

        var s = search.Trim();

        // Search in status + notes + ids (as strings)
        return Builders<InventoryTransferReadModel>.Filter.Or(
            Builders<InventoryTransferReadModel>.Filter.Regex(
                x => x.Status,
                new BsonRegularExpression(s, "i")
            ),
            Builders<InventoryTransferReadModel>.Filter.Regex(
                x => x.Notes,
                new BsonRegularExpression(s, "i")
            ),
            Builders<InventoryTransferReadModel>.Filter.Regex(
                x => x.Id,
                new BsonRegularExpression(s, "i")
            )
        );
    }
}
