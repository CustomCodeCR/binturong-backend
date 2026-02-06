using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Users.GetUsers;

internal sealed class GetUsersQueryHandler
    : IQueryHandler<GetUsersQuery, IReadOnlyList<UserReadModel>>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetUsersQueryHandler(
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

    public async Task<Result<IReadOnlyList<UserReadModel>>> Handle(
        GetUsersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var filter = Builders<UserReadModel>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            filter = Builders<UserReadModel>.Filter.Or(
                Builders<UserReadModel>.Filter.Regex(
                    x => x.Username,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                ),
                Builders<UserReadModel>.Filter.Regex(
                    x => x.Email,
                    new MongoDB.Bson.BsonRegularExpression(s, "i")
                )
            );
        }

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                null,
                "USERS_READ",
                string.Empty,
                $"search={query.Search ?? ""}; skip={query.Skip}; pageSize={query.PageSize}; returned={docs.Count}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success((IReadOnlyList<UserReadModel>)docs);
    }
}
