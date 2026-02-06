using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Clients.GetClients;

internal sealed class GetClientsQueryHandler
    : IQueryHandler<GetClientsQuery, IReadOnlyList<ClientReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetClientsQueryHandler(
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

    public async Task<Result<IReadOnlyList<ClientReadModel>>> Handle(
        GetClientsQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<ClientReadModel>(MongoCollections.Clients);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Clients",
            "Client",
            null,
            "CLIENT_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<ClientReadModel>>(docs);
    }

    private static FilterDefinition<ClientReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<ClientReadModel>.Filter.Empty;

        var s = search.Trim();
        var re = new BsonRegularExpression(s, "i");

        return Builders<ClientReadModel>.Filter.Or(
            Builders<ClientReadModel>.Filter.Regex(x => x.TradeName, re),
            Builders<ClientReadModel>.Filter.Regex(x => x.ContactName, re),
            Builders<ClientReadModel>.Filter.Regex(x => x.Email, re),
            Builders<ClientReadModel>.Filter.Regex(x => x.Identification, re),
            Builders<ClientReadModel>.Filter.Regex(x => x.PrimaryPhone, re)
        );
    }
}
