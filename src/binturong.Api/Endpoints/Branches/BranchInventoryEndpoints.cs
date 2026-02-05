using Application.Abstractions.Messaging;
using Application.Features.Branches.Inventory.GetBranchInventory;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using MongoDB.Driver;

namespace Api.Endpoints.Branches;

public sealed class BranchInventoryEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/branches").WithTags("Branches");

        group.MapGet(
            "/{id:guid}/inventory",
            async (
                Guid id,
                IQueryHandler<
                    GetBranchInventoryQuery,
                    IReadOnlyList<BranchInventoryItemReadModel>
                > handler,
                CancellationToken ct
            ) =>
            {
                var result = await handler.Handle(new GetBranchInventoryQuery(id), ct);
                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );

        // Consolidated (admin)
        group.MapGet(
            "/inventory",
            async (IMongoDatabase db, CancellationToken ct) =>
            {
                var col = db.GetCollection<ProductStockReadModel>(MongoCollections.ProductStocks);
                var docs = await col.Find(_ => true).ToListAsync(ct);

                var resp = docs.Select(d => new BranchInventoryItemReadModel
                    {
                        ProductId = d.ProductId,
                        ProductName = d.ProductName,
                        Stock = d.TotalStock,
                    })
                    .OrderByDescending(x => x.Stock)
                    .ToList();

                return Results.Ok(resp);
            }
        );
    }
}
