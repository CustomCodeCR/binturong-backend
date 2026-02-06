using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Warehouses.Delete;

internal sealed class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand>
{
    private const string Module = "Warehouses";
    private const string Entity = "Warehouse";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public DeleteWarehouseCommandHandler(
        IApplicationDbContext db,
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

    public async Task<Result> Handle(DeleteWarehouseCommand command, CancellationToken ct)
    {
        var wh = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == command.WarehouseId, ct);
        if (wh is null)
            return Result.Failure(
                Error.NotFound(
                    "Warehouses.NotFound",
                    $"Warehouse '{command.WarehouseId}' not found"
                )
            );

        var before =
            $"warehouseId={wh.Id}; branchId={wh.BranchId}; code={wh.Code}; name={wh.Name}; isActive={wh.IsActive}";

        wh.RaiseDeleted();

        _db.Warehouses.Remove(wh);
        await _db.SaveChangesAsync(ct);

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.WarehouseId,
                "WAREHOUSE_DELETED",
                before,
                string.Empty,
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
