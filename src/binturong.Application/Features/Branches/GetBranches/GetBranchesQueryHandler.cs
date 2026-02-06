using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.GetBranches;

internal sealed class GetBranchesQueryHandler
    : IQueryHandler<GetBranchesQuery, IReadOnlyList<BranchReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetBranchesQueryHandler(
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

    public async Task<Result<IReadOnlyList<BranchReadModel>>> Handle(
        GetBranchesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Branches",
            "Branch",
            null,
            "BRANCH_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<BranchReadModel>>(docs);
    }

    private static FilterDefinition<BranchReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<BranchReadModel>.Filter.Empty;

        var s = search.Trim();

        return Builders<BranchReadModel>.Filter.Or(
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Code,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Name,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Address,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            ),
            Builders<BranchReadModel>.Filter.Regex(
                x => x.Phone,
                new MongoDB.Bson.BsonRegularExpression(s, "i")
            )
        );
    }
}
