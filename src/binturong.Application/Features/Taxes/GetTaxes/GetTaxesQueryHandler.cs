using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Taxes.GetTaxes;

internal sealed class GetTaxesQueryHandler
    : IQueryHandler<GetTaxesQuery, IReadOnlyList<TaxReadModel>>
{
    private const string Module = "Taxes";
    private const string Entity = "Tax";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetTaxesQueryHandler(
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

    public async Task<Result<IReadOnlyList<TaxReadModel>>> Handle(
        GetTaxesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);

        var filter = BuildFilter(query.Search);

        var items = await col.Find(filter)
            .SortBy(x => x.Code)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null, // list query -> no single entityId
                "TAXES_READ",
                string.Empty,
                $"search={query.Search ?? ""}; skip={query.Skip}; take={query.Take}; returned={items.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<TaxReadModel>>(items);
    }

    private static FilterDefinition<TaxReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<TaxReadModel>.Filter.Empty;

        var s = search.Trim();

        var name = Builders<TaxReadModel>.Filter.Regex(
            x => x.Name,
            new MongoDB.Bson.BsonRegularExpression(s, "i")
        );

        var code = Builders<TaxReadModel>.Filter.Regex(
            x => x.Code,
            new MongoDB.Bson.BsonRegularExpression(s, "i")
        );

        return Builders<TaxReadModel>.Filter.Or(name, code);
    }
}
