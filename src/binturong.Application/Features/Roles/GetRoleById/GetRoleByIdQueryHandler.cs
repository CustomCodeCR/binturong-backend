using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Roles.GetRoleById;

internal sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetRoleByIdQueryHandler(
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

    public async Task<Result<RoleReadModel>> Handle(GetRoleByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);
        var id = $"role:{query.RoleId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<RoleReadModel>(
                Error.NotFound("Roles.NotFound", $"Role '{query.RoleId}' not found")
            );

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Roles",
            "Role",
            query.RoleId,
            "ROLE_READ",
            string.Empty,
            $"roleId={query.RoleId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
