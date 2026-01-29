using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserReadModel>
{
    private readonly IMongoDatabase _db;

    public GetUserByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<UserReadModel>> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<UserReadModel>(MongoCollections.Users);
        var id = $"user:{query.UserId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<UserReadModel>(
                SharedKernel.Error.NotFound("Users.NotFound", $"User '{query.UserId}' not found")
            );

        return Result.Success(doc);
    }
}
