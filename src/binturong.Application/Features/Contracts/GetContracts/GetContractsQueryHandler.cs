using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Sales;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Contracts.GetContracts;

internal sealed class GetContractsQueryHandler
    : IQueryHandler<GetContractsQuery, IReadOnlyList<ContractReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetContractsQueryHandler(
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

    public async Task<Result<IReadOnlyList<ContractReadModel>>> Handle(
        GetContractsQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ContractReadModel>(MongoCollections.Contracts);
        var filter = BuildFilter(query.Search);

        // No UpdatedAt => sort by StartDate
        var docs = await col.Find(filter)
            .SortByDescending(x => x.StartDate)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Contracts",
            "Contract",
            null,
            "CONTRACT_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<ContractReadModel>>(docs);
    }

    private static FilterDefinition<ContractReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<ContractReadModel>.Filter.Empty;

        var s = search.Trim();

        return Builders<ContractReadModel>.Filter.Or(
            Builders<ContractReadModel>.Filter.Regex(
                x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<ContractReadModel>.Filter.Regex(
                x => x.ClientName,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<ContractReadModel>.Filter.Regex(
                x => x.Status,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            )
        );
    }
}
