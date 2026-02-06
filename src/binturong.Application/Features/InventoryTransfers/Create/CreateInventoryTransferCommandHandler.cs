using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.InventoryTransfers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.InventoryTransfers.Create;

internal sealed class CreateInventoryTransferCommandHandler
    : ICommandHandler<CreateInventoryTransferCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;

    public CreateInventoryTransferCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
    }

    public async Task<Result<Guid>> Handle(
        CreateInventoryTransferCommand command,
        CancellationToken ct
    )
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        if (command.FromBranchId == command.ToBranchId)
            return await Fail("invalid_branches", InventoryTransferErrors.InvalidBranches);

        if (command.Lines is null || command.Lines.Count == 0)
            return await Fail("empty_lines", InventoryTransferErrors.EmptyLines);

        foreach (var l in command.Lines)
        {
            if (l.Quantity <= 0)
                return await Fail(
                    "invalid_line_qty",
                    Error.Validation(
                        "InventoryTransfers.InvalidLineQty",
                        "Line quantity must be > 0"
                    )
                );
        }

        var whIds = command
            .Lines.SelectMany(x => new[] { x.FromWarehouseId, x.ToWarehouseId })
            .Distinct()
            .ToList();

        var warehouses = await _db.Warehouses.Where(w => whIds.Contains(w.Id)).ToListAsync(ct);

        if (warehouses.Count != whIds.Count)
            return await Fail(
                "warehouses_not_found",
                Error.NotFound("Warehouses.NotFound", "One or more warehouses not found")
            );

        foreach (var l in command.Lines)
        {
            var fromWh = warehouses.First(x => x.Id == l.FromWarehouseId);
            var toWh = warehouses.First(x => x.Id == l.ToWarehouseId);

            if (fromWh.BranchId != command.FromBranchId)
                return await Fail(
                    "invalid_from_warehouse",
                    Error.Validation(
                        "InventoryTransfers.InvalidFromWarehouse",
                        "FromWarehouse not in FromBranch"
                    )
                );

            if (toWh.BranchId != command.ToBranchId)
                return await Fail(
                    "invalid_to_warehouse",
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
                    TransferId = Guid.Empty,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    FromWarehouseId = l.FromWarehouseId,
                    ToWarehouseId = l.ToWarehouseId,
                })
                .ToList(),
        };

        transfer.RaiseCreated();

        _db.InventoryTransfers.Add(transfer);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            command.CreatedByUserId,
            "InventoryTransfers",
            "InventoryTransfer",
            transfer.Id,
            "TRANSFER_CREATED",
            string.Empty,
            $"transferId={transfer.Id}; fromBranchId={transfer.FromBranchId}; toBranchId={transfer.ToBranchId}; status={transfer.Status}; lines={transfer.Lines.Count}",
            ip,
            ua,
            ct
        );

        return Result.Success(transfer.Id);

        async Task<Result<Guid>> Fail(string reason, Error error)
        {
            await _bus.AuditAsync(
                command.CreatedByUserId,
                "InventoryTransfers",
                "InventoryTransfer",
                null,
                "TRANSFER_CREATE_FAILED",
                string.Empty,
                $"reason={reason}; fromBranchId={command.FromBranchId}; toBranchId={command.ToBranchId}; lines={command.Lines?.Count ?? 0}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(error);
        }
    }
}
