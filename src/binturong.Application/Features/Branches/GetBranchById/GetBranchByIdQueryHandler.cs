using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.GetBranchById;

internal sealed class GetBranchByIdQueryHandler : IQueryHandler<GetBranchByIdQuery, BranchReadModel>
{
    private readonly IMongoDatabase _db;

    public GetBranchByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<BranchReadModel>> Handle(
        GetBranchByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var id = $"branch:{query.BranchId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
            return Result.Failure<BranchReadModel>(
                Error.NotFound("Branches.NotFound", $"Branch '{query.BranchId}' not found")
            );

        return Result.Success(doc);
    }
}
