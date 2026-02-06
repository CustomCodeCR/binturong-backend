using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
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
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetInventoryTransfersQueryHandler(
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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "InventoryTransfers",
            "InventoryTransfer",
            null,
            "TRANSFER_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<InventoryTransferReadModel>>(docs);
    }

    private static FilterDefinition<InventoryTransferReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<InventoryTransferReadModel>.Filter.Empty;

        var s = search.Trim();

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
