using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.Branches.Inventory.GetBranchInventory;

internal sealed class GetBranchInventoryQueryHandler
    : IQueryHandler<GetBranchInventoryQuery, IReadOnlyList<BranchInventoryItemReadModel>>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetBranchInventoryQueryHandler(
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

    public async Task<Result<IReadOnlyList<BranchInventoryItemReadModel>>> Handle(
        GetBranchInventoryQuery query,
        CancellationToken ct
    )
    {
        var branches = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var branch = await branches
            .Find(x => x.Id == $"branch:{query.BranchId}")
            .FirstOrDefaultAsync(ct);

        if (branch is null)
            return Result.Failure<IReadOnlyList<BranchInventoryItemReadModel>>(
                Error.NotFound("Branches.NotFound", $"Branch '{query.BranchId}' not found")
            );

        var warehouseIds = branch.Warehouses.Select(w => w.WarehouseId).ToHashSet();
        if (warehouseIds.Count == 0)
        {
            // Audit success (empty inventory is still a valid read)
            await _bus.AuditAsync(
                _currentUser.UserId,
                "Inventory",
                "Branch",
                query.BranchId,
                "INVENTORY_BY_BRANCH_READ",
                string.Empty,
                $"branchId={query.BranchId}; items=0",
                _request.IpAddress,
                _request.UserAgent,
                ct
            );

            return Result.Success<IReadOnlyList<BranchInventoryItemReadModel>>(
                Array.Empty<BranchInventoryItemReadModel>()
            );
        }

        var stocks = _db.GetCollection<ProductStockReadModel>(MongoCollections.ProductStocks);
        var docs = await stocks.Find(_ => true).ToListAsync(ct);

        var result = docs.Select(d =>
            {
                var qty = d
                    .Warehouses.Where(w => warehouseIds.Contains(w.WarehouseId))
                    .Sum(w => w.CurrentStock);

                return new BranchInventoryItemReadModel
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Stock = qty,
                };
            })
            .Where(x => x.Stock != 0m)
            .OrderByDescending(x => x.Stock)
            .ToList();

        await _bus.AuditAsync(
            _currentUser.UserId,
            "Inventory",
            "Branch",
            query.BranchId,
            "INVENTORY_BY_BRANCH_READ",
            string.Empty,
            $"branchId={query.BranchId}; items={result.Count}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success<IReadOnlyList<BranchInventoryItemReadModel>>(result);
    }
}
