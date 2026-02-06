using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.UnitsOfMeasure;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.GetUnitOfMeasureById;

internal sealed class GetUnitOfMeasureByIdQueryHandler
    : IQueryHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureReadModel>
{
    private const string Module = "UnitsOfMeasure";
    private const string Entity = "UnitOfMeasure";

    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetUnitOfMeasureByIdQueryHandler(
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

    public async Task<Result<UnitOfMeasureReadModel>> Handle(
        GetUnitOfMeasureByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);
        var id = $"uom:{query.UomId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            await _bus.Send(
                new CreateAuditLogCommand(
                    _currentUser.UserId,
                    Module,
                    Entity,
                    query.UomId,
                    "UOM_READ_NOT_FOUND",
                    string.Empty,
                    $"uomId={query.UomId}",
                    _request.IpAddress,
                    _request.UserAgent
                ),
                ct
            );

            return Result.Failure<UnitOfMeasureReadModel>(
                UnitOfMeasureErrors.NotFound(query.UomId)
            );
        }

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                query.UomId,
                "UOM_READ",
                string.Empty,
                $"uomId={query.UomId}",
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success(doc);
    }
}
