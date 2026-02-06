using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserReadModel>
{
    private const string Module = "Users";
    private const string Entity = "User";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetUserByIdQueryHandler(
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

    public async Task<Result<UserReadModel>> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var id = $"user:{query.UserId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    _currentUser.UserId,
                    Module,
                    Entity,
                    query.UserId,
                    "USER_READ_NOT_FOUND",
                    string.Empty,
                    $"userId={query.UserId}",
                    _request.IpAddress,
                    _request.UserAgent
                ),
                ct
            );

            return Result.Failure<UserReadModel>(
                Error.NotFound("Users.NotFound", $"User '{query.UserId}' not found")
            );
        }

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                query.UserId,
                "USER_READ",
                string.Empty,
                $"userId={query.UserId}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(doc);
    }
}
