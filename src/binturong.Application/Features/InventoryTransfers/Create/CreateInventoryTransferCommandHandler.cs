using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Create;

internal sealed class CreateInventoryTransferCommandHandler
    : ICommandHandler<CreateInventoryTransferCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateInventoryTransferCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        CreateInventoryTransferCommand command,
        CancellationToken ct
    )
    {
        if (command.FromBranchId == command.ToBranchId)
            return Result.Failure<Guid>(InventoryTransferErrors.InvalidBranches);

        if (command.Lines is null || command.Lines.Count == 0)
            return Result.Failure<Guid>(InventoryTransferErrors.EmptyLines);

        // Basic validation
        foreach (var l in command.Lines)
        {
            if (l.Quantity <= 0)
                return Result.Failure<Guid>(
                    Error.Validation(
                        "InventoryTransfers.InvalidLineQty",
                        "Line quantity must be > 0"
                    )
                );
        }

        // validate warehouses exist and belong to correct branches
        var whIds = command
            .Lines.SelectMany(x => new[] { x.FromWarehouseId, x.ToWarehouseId })
            .Distinct()
            .ToList();
        var warehouses = await _db.Warehouses.Where(w => whIds.Contains(w.Id)).ToListAsync(ct);

        if (warehouses.Count != whIds.Count)
            return Result.Failure<Guid>(
                Error.NotFound("Warehouses.NotFound", "One or more warehouses not found")
            );

        foreach (var l in command.Lines)
        {
            var fromWh = warehouses.First(x => x.Id == l.FromWarehouseId);
            var toWh = warehouses.First(x => x.Id == l.ToWarehouseId);

            if (fromWh.BranchId != command.FromBranchId)
                return Result.Failure<Guid>(
                    Error.Validation(
                        "InventoryTransfers.InvalidFromWarehouse",
                        "FromWarehouse not in FromBranch"
                    )
                );

            if (toWh.BranchId != command.ToBranchId)
                return Result.Failure<Guid>(
                    Error.Validation(
                        "InventoryTransfers.InvalidToWarehouse",
                        "ToWarehouse not in ToBranch"
                    )
                );
        }

        var now = DateTime.UtcNow;

        var transfer = new InventoryTransfer
        {
            Id = Guid.NewGuid(),
            FromBranchId = command.FromBranchId,
            ToBranchId = command.ToBranchId,
            Status = InventoryTransferStatus.Draft,
            Notes = command.Notes ?? "",
            CreatedByUserId = command.CreatedByUserId,
            ApprovedByUserId = null,
            CreatedAt = now,
            UpdatedAt = now,
            Lines = command
                .Lines.Select(l => new InventoryTransferLine
                {
                    Id = Guid.NewGuid(),
                    TransferId = Guid.Empty, // set by EF relationship
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    FromWarehouseId = l.FromWarehouseId,
                    ToWarehouseId = l.ToWarehouseId,
                })
                .ToList(),
        };

        // domain event for projection
        transfer.RaiseCreated();

        _db.InventoryTransfers.Add(transfer);
        await _db.SaveChangesAsync(ct);

        return Result.Success(transfer.Id);
    }
}
