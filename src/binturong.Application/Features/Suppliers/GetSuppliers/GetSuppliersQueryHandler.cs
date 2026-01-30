using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.CRM;
using MongoDB.Bson;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Suppliers.GetSuppliers;

internal sealed class GetSuppliersQueryHandler
    : IQueryHandler<GetSuppliersQuery, IReadOnlyList<SupplierReadModel>>
{
    private readonly IMongoDatabase _db;

    public GetSuppliersQueryHandler(IMongoDatabase db) => _db = db;

    public async Task<Result<IReadOnlyList<SupplierReadModel>>> Handle(
        GetSuppliersQuery query,
        CancellationToken ct
    )
    {
        var col = _db.GetCollection<SupplierReadModel>(MongoCollections.Suppliers);

        var filter = BuildFilter(query.Search);

        var docs = await col.Find(filter)
            .SortByDescending(x => x.UpdatedAt)
            .Skip(query.Skip)
            .Limit(query.Take)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<SupplierReadModel>>(docs);
    }

    private static FilterDefinition<SupplierReadModel> BuildFilter(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Builders<SupplierReadModel>.Filter.Empty;

        var s = search.Trim();

        var re = new BsonRegularExpression(s, "i");

        return Builders<SupplierReadModel>.Filter.Or(
            Builders<SupplierReadModel>.Filter.Regex(x => x.TradeName, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.LegalName, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.Email, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.Identification, re),
            Builders<SupplierReadModel>.Filter.Regex(x => x.Phone, re)
        );
    }
}
