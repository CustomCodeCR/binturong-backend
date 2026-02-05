using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Warehouses.Create;

internal sealed class CreateWarehouseCommandHandler : ICommandHandler<CreateWarehouseCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateWarehouseCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateWarehouseCommand command, CancellationToken ct)
    {
        if (command.BranchId == Guid.Empty)
            return Result.Failure<Guid>(
                Error.Validation("Warehouses.BranchRequired", "BranchId is required")
            );

        var branch = await _db.Branches.FirstOrDefaultAsync(x => x.Id == command.BranchId, ct);
        if (branch is null)
            return Result.Failure<Guid>(
                Error.NotFound("Branches.NotFound", $"Branch '{command.BranchId}' not found")
            );

        var code = command.Code.Trim();
        var name = command.Name.Trim();
        var description = string.IsNullOrWhiteSpace(command.Description)
            ? null
            : command.Description.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(
                Error.Validation("Warehouses.CodeRequired", "Warehouse code is required")
            );

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(
                Error.Validation("Warehouses.NameRequired", "Warehouse name is required")
            );

        var codeExists = await _db.Warehouses.AnyAsync(
            x => x.BranchId == command.BranchId && x.Code.ToLower() == code.ToLower(),
            ct
        );
        if (codeExists)
            return Result.Failure<Guid>(
                Error.Conflict("Warehouses.CodeNotUnique", "Warehouse code is not unique in branch")
            );

        var now = DateTime.UtcNow;

        var wh = new Warehouse
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            Code = code,
            Name = name,
            Description = description ?? "",
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        wh.RaiseCreated(branch.Code, branch.Name);

        _db.Warehouses.Add(wh);
        await _db.SaveChangesAsync(ct);

        return Result.Success(wh.Id);
    }
}
