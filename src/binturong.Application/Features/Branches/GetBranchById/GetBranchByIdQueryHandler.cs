using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.GetBranchById;

internal sealed class GetBranchByIdQueryHandler : IQueryHandler<GetBranchByIdQuery, BranchReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetBranchByIdQueryHandler(
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

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Branches",
            "Branch",
            query.BranchId,
            "BRANCH_READ",
            string.Empty,
            $"branchId={query.BranchId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(doc);
    }
}
