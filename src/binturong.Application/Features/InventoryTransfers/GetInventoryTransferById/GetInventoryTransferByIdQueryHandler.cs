using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Application.ReadModels.Common;
using Application.ReadModels.Inventory;
using Application.ReadModels.MasterData;
using Application.ReadModels.Security;
using MongoDB.Driver;
using SharedKernel;

namespace Application.Features.InventoryTransfers.GetInventoryTransferById;

internal sealed class GetInventoryTransferByIdQueryHandler
    : IQueryHandler<GetInventoryTransferByIdQuery, InventoryTransferReadModel>
{
    private readonly IMongoDatabase _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public GetInventoryTransferByIdQueryHandler(
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

    public async Task<Result<InventoryTransferReadModel>> Handle(
        GetInventoryTransferByIdQuery query,
        CancellationToken ct
    )
    {
        var transfers = _db.GetCollection<InventoryTransferReadModel>(
            MongoCollections.InventoryTransfers
        );
        var branches = _db.GetCollection<BranchReadModel>(MongoCollections.Branches);
        var warehouses = _db.GetCollection<WarehouseReadModel>(MongoCollections.Warehouses);
        var products = _db.GetCollection<ProductReadModel>(MongoCollections.Products);
        var users = _db.GetCollection<UserReadModel>(MongoCollections.Users);

        var id = $"transfer:{query.TransferId}";

        var doc = await transfers.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

        if (doc is null)
        {
            return Result.Failure<InventoryTransferReadModel>(
                Error.NotFound(
                    "InventoryTransfers.NotFound",
                    $"Transfer '{query.TransferId}' not found"
                )
            );
        }

        var branchIds = new HashSet<Guid> { doc.FromBranchId, doc.ToBranchId };

        var warehouseIds = doc
            .Lines.SelectMany(x => new[] { x.FromWarehouseId, x.ToWarehouseId })
            .Distinct()
            .ToHashSet();

        var productIds = doc.Lines.Select(x => x.ProductId).Distinct().ToHashSet();

        var userIds = new HashSet<Guid> { doc.CreatedByUserId };
        if (doc.ApprovedByUserId.HasValue)
        {
            userIds.Add(doc.ApprovedByUserId.Value);
        }

        var branchDocs = await branches.Find(x => branchIds.Contains(x.BranchId)).ToListAsync(ct);

        var warehouseDocs = await warehouses
            .Find(x => warehouseIds.Contains(x.WarehouseId))
            .ToListAsync(ct);

        var productDocs = await products
            .Find(x => productIds.Contains(x.ProductId))
            .ToListAsync(ct);

        var userDocs = await users.Find(x => userIds.Contains(x.UserId)).ToListAsync(ct);

        var branchById = branchDocs.ToDictionary(x => x.BranchId);
        var warehouseById = warehouseDocs.ToDictionary(x => x.WarehouseId);
        var productById = productDocs.ToDictionary(x => x.ProductId);
        var userById = userDocs.ToDictionary(x => x.UserId);

        branchById.TryGetValue(doc.FromBranchId, out var fromBranch);
        branchById.TryGetValue(doc.ToBranchId, out var toBranch);

        userById.TryGetValue(doc.CreatedByUserId, out var createdByUser);

        UserReadModel? approvedByUser = null;
        if (doc.ApprovedByUserId.HasValue)
        {
            userById.TryGetValue(doc.ApprovedByUserId.Value, out approvedByUser);
        }

        var enriched = new InventoryTransferReadModel
        {
            Id = doc.Id,
            TransferId = doc.TransferId,

            FromBranchId = doc.FromBranchId,
            ToBranchId = doc.ToBranchId,

            FromBranchCode = fromBranch?.Code,
            FromBranchName = fromBranch?.Name,

            ToBranchCode = toBranch?.Code,
            ToBranchName = toBranch?.Name,

            Status = doc.Status,
            Notes = doc.Notes,

            CreatedByUserId = doc.CreatedByUserId,
            CreatedByUsername = createdByUser?.Username,
            CreatedByEmail = createdByUser?.Email,

            ApprovedByUserId = doc.ApprovedByUserId,
            ApprovedByUsername = approvedByUser?.Username,
            ApprovedByEmail = approvedByUser?.Email,

            RejectionReason = doc.RejectionReason,

            CreatedAt = doc.CreatedAt,
            UpdatedAt = doc.UpdatedAt,

            Lines = doc
                .Lines.Select(line =>
                {
                    productById.TryGetValue(line.ProductId, out var product);
                    warehouseById.TryGetValue(line.FromWarehouseId, out var fromWarehouse);
                    warehouseById.TryGetValue(line.ToWarehouseId, out var toWarehouse);

                    return new InventoryTransferLineReadModel
                    {
                        LineId = line.LineId,

                        ProductId = line.ProductId,
                        ProductSku = product?.SKU,
                        ProductName = product?.Name,

                        Quantity = line.Quantity,

                        FromWarehouseId = line.FromWarehouseId,
                        FromWarehouseCode = fromWarehouse?.Code,
                        FromWarehouseName = fromWarehouse?.Name,

                        ToWarehouseId = line.ToWarehouseId,
                        ToWarehouseCode = toWarehouse?.Code,
                        ToWarehouseName = toWarehouse?.Name,
                    };
                })
                .ToList(),
        };

        await _bus.AuditAsync(
            _currentUser.UserId,
            "InventoryTransfers",
            "InventoryTransfer",
            query.TransferId,
            "TRANSFER_READ",
            string.Empty,
            $"transferId={query.TransferId}",
            _request.IpAddress,
            _request.UserAgent,
            ct
        );

        return Result.Success(enriched);
    }
}
