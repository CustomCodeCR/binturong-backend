using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Roles.GetRoleById;

internal sealed class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleReadModel>
{
    private readonly IMongoDatabase _db;

    public GetRoleByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<RoleReadModel>> Handle(GetRoleByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<RoleReadModel>(MongoCollections.Roles);
        var id = $"role:{query.RoleId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<RoleReadModel>(
                Error.NotFound("Roles.NotFound", $"Role '{query.RoleId}' not found")
            );

        return Result.Success(doc);
    }
}
