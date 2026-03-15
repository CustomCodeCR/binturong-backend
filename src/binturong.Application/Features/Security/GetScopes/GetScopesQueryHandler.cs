using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Security.GetScopes;

internal sealed class GetScopesQueryHandler
    : IQueryHandler<GetScopesQuery, IReadOnlyList<ScopeCatalogReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetScopesQueryHandler(
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

    public async Task<Result<IReadOnlyList<ScopeCatalogReadModel>>> Handle(
        GetScopesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ScopeCatalogReadModel>(MongoCollections.Scopes);

        var filter = Builders<ScopeCatalogReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            filter = Builders<ScopeCatalogReadModel>.Filter.Or(
                Builders<ScopeCatalogReadModel>.Filter.Regex(
                    x => x.Code,
                    new BsonRegularExpression(search, "i")
                ),
                Builders<ScopeCatalogReadModel>.Filter.Regex(
                    x => x.Description,
                    new BsonRegularExpression(search, "i")
                ),
                Builders<ScopeCatalogReadModel>.Filter.Regex(
                    x => x.Id,
                    new BsonRegularExpression(search, "i")
                )
            );
        }

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Security",
            "Scope",
            null,
            "SCOPE_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<ScopeCatalogReadModel>>(docs);
    }
}
