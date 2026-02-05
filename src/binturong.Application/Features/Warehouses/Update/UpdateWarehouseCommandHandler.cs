using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Warehouses.Update;

internal sealed class UpdateWarehouseCommandHandler : ICommandHandler<UpdateWarehouseCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateWarehouseCommandHandler(IApplicationDbContext db) => _db = db;

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
        return Result.Success();
    }
}
