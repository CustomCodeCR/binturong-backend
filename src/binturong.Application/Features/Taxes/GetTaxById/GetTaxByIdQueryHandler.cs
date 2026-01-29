using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.MasterData;
using Domain.Taxes;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Taxes.GetTaxById;

internal sealed class GetTaxByIdQueryHandler : IQueryHandler<GetTaxByIdQuery, TaxReadModel>
{
    private readonly IMongoDatabase _db;

    public GetTaxByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<TaxReadModel>> Handle(GetTaxByIdQuery query, CancellationToken ct)
    {
        var col = _db.GetCollection<TaxReadModel>(MongoCollections.Taxes);
        var id = $"tax:{query.TaxId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<TaxReadModel>(TaxErrors.NotFound(query.TaxId));

        return Result.Success(doc);
    }
}
