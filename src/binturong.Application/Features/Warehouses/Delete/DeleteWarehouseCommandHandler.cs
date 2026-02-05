using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Warehouses.Delete;

internal sealed class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteWarehouseCommandHandler(IApplicationDbContext db) => _db = db;

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

        wh.RaiseDeleted();

        _db.Warehouses.Remove(wh);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
