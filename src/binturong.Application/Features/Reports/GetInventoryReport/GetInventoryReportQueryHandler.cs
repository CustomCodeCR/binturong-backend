using Application.Abstractions.Messaging;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Application.ReadModels.Reports;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Reports.GetInventoryReport;

internal sealed class GetInventoryReportQueryHandler
    : IQueryHandler<GetInventoryReportQuery, InventoryReportReadModel>
{
    private readonly IMongoDatabase _mongo;

    public GetInventoryReportQueryHandler(IMongoDatabase mongo) => _mongo = mongo;

    public async Task<Result<InventoryReportReadModel>> Handle(
        GetInventoryReportQuery q,
        CancellationToken ct
    )
    {
        var col = _mongo.GetCollection<ProductStockReadModel>(MongoCollections.ProductStocks);

        var builder = Builders<ProductStockReadModel>.Filter;
        var filter = builder.Empty;

        var docs = await col.Find(filter).SortBy(x => x.ProductName).ToListAsync(ct);

        if (docs.Count == 0)
        {
            return Result.Success(
                new InventoryReportReadModel
                {
                    CategoryId = q.CategoryId,
                    HasData = false,
                    Message = "Sin información disponible",
                    Items = [],
                }
            );
        }

        return Result.Success(
            new InventoryReportReadModel
            {
                HasData = true,
                Message = null,
                Items = docs.Select(x => new InventoryReportItemReadModel
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                        TotalStock = x.TotalStock,
                        UpdatedAt = x.UpdatedAt,
                    })
                    .ToArray(),
            }
        );
    }
}
