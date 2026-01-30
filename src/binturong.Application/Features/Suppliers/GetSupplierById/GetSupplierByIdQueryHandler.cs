using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using Domain.Suppliers;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.GetSupplierById;

internal sealed class GetSupplierByIdQueryHandler
    : IQueryHandler<GetSupplierByIdQuery, SupplierReadModel>
{
    private readonly IMongoDatabase _db;

    public GetSupplierByIdQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<SupplierReadModel>> Handle(
        GetSupplierByIdQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);
        var id = $"supplier:{query.SupplierId}";

        var doc = await col.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (doc is null)
            return Result.Failure<SupplierReadModel>(SupplierErrors.NotFound(query.SupplierId));

        return Result.Success(doc);
    }
}
