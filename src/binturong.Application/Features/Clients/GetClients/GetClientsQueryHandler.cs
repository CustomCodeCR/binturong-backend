using Application.Abstractions.Messaging;
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

    public GetClientsQueryHandler(IMongoDatabase db) => _db = db;

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
