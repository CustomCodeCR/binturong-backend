using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Audit.Create;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Warehouses.Update;

internal sealed class UpdateWarehouseCommandHandler : ICommandHandler<UpdateWarehouseCommand>
{
    private const string Module = "Warehouses";
    private const string Entity = "Warehouse";

    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateWarehouseCommandHandler(
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

    public async Task<Result> Handle(UpdateWarehouseCommand command, CancellationToken ct)
    {
        var wh = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == command.WarehouseId, ct);
        if (wh is null)
            return Result.Failure(
                Error.NotFound(
                    "Warehouses.NotFound",
                    $"Warehouse '{command.WarehouseId}' not found"
                )
            );

        var branch = await _db.Branches.FirstOrDefaultAsync(x => x.Id == wh.BranchId, ct);
        if (branch is null)
            return Result.Failure(
                Error.NotFound("Branches.NotFound", $"Branch '{wh.BranchId}' not found")
            );

        var before =
            $"warehouseId={wh.Id}; branchId={wh.BranchId}; code={wh.Code}; name={wh.Name}; description={wh.Description}; isActive={wh.IsActive}";

        var code = command.Code.Trim();
        var name = command.Name.Trim();
        var description = string.IsNullOrWhiteSpace(command.Description)
            ? null
            : command.Description.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(
                Error.Validation("Warehouses.CodeRequired", "Warehouse code is required")
            );

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(
                Error.Validation("Warehouses.NameRequired", "Warehouse name is required")
            );

        var codeExists = await _db.Warehouses.AnyAsync(
            x =>
                x.Id != command.WarehouseId
                && x.BranchId == wh.BranchId
                && x.Code.ToLower() == code.ToLower(),
            ct
        );
        if (codeExists)
            return Result.Failure(
                Error.Conflict("Warehouses.CodeNotUnique", "Warehouse code is not unique in branch")
            );

        wh.Code = code;
        wh.Name = name;
        wh.Description = description ?? "";
        wh.IsActive = command.IsActive;
        wh.UpdatedAt = DateTime.UtcNow;

        wh.RaiseUpdated(branch.Code, branch.Name);

        await _db.SaveChangesAsync(ct);

        var after =
            $"warehouseId={wh.Id}; branchId={wh.BranchId}; code={wh.Code}; name={wh.Name}; description={wh.Description}; isActive={wh.IsActive}";

        await _bus.Send(
            new CreateAuditLogCommand(
                _currentUser.UserId,
                Module,
                Entity,
                command.WarehouseId,
                "WAREHOUSE_UPDATED",
                before,
                after,
                _request.IpAddress,
                _request.UserAgent
            ),
            ct
        );

        return Result.Success();
    }
}
