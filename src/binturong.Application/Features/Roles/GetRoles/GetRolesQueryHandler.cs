using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Roles.GetRoles;

internal sealed class GetRolesQueryHandler
    : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetRolesQueryHandler(
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

    public async Task<Result<IReadOnlyList<RoleReadModel>>> Handle(
        GetRolesQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);

        var f = Builders<RoleReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            f = Builders<RoleReadModel>.Filter.Or(
                Builders<RoleReadModel>.Filter.Regex(
                    x => x.Name,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<RoleReadModel>.Filter.Regex(
                    x => x.Description,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var docs = await col.Find(f).Skip(query.Skip).Limit(query.Take).ToListAsync(ct);

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Roles",
            "Role",
            null,
            "ROLE_LIST_READ",
            string.Empty,
            $"search={query.Search}; skip={query.Skip}; take={query.Take}; count={docs.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<RoleReadModel>>(docs);
    }
}
