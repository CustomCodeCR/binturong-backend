using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.UnitsOfMeasure;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.GetUnitOfMeasureById;

internal sealed class GetUnitOfMeasureByIdQueryHandler
    : IQueryHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureReadModel>
{
    private readonly IMongoDatabase _db;

    public GetUnitOfMeasureByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<UnitOfMeasureReadModel>> Handle(
        GetUnitOfMeasureByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<UnitOfMeasureReadModel>(MongoCollections.UnitsOfMeasure);
        var id = $"uom:{query.UomId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<UnitOfMeasureReadModel>(
                UnitOfMeasureErrors.NotFound(query.UomId)
            );

        return Result.Success(doc);
    }
}
