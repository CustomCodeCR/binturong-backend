using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.GetUnitsOfMeasure;

internal sealed class GetUnitsOfMeasureQueryHandler
    : IQueryHandler<GetUnitsOfMeasureQuery, IReadOnlyList<UnitOfMeasureReadModel>>
{
    private const string Module = "UnitsOfMeasure";
    private const string Entity = "UnitOfMeasure";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetUnitsOfMeasureQueryHandler(
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

    public async Task<Result<IReadOnlyList<UnitOfMeasureReadModel>>> Handle(
        GetUnitsOfMeasureQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortBy(x => x.Code)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null, // list query => no entityId
                "UOMS_READ",
                string.Empty,
                $"search={query.Search ?? ""}; skip={query.Skip}; take={query.Take}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success<IReadOnlyList<UnitOfMeasureReadModel>>(docs);
    }

    private static FilterDefinition<UnitOfMeasureReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<UnitOfMeasureReadModel>.Filter.Empty;

        var s = search.Trim();
        var re = new BsonRegularExpression(s, "i");

        return Builders<UnitOfMeasureReadModel>.Filter.Or(
            Builders<UnitOfMeasureReadModel>.Filter.Regex(x => x.Code, re),
            Builders<UnitOfMeasureReadModel>.Filter.Regex(x => x.Name, re)
        );
    }
}
