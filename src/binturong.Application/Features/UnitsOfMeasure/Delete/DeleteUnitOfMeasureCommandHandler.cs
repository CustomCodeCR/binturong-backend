using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.Delete;

internal sealed class DeleteUnitOfMeasureCommandHandler
    : ICommandHandler<DeleteUnitOfMeasureCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteUnitOfMeasureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteUnitOfMeasureCommand command, CancellationToken ct)
    {
        var uom = await _db
            .UnitsOfMeasure.Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == command.UomId, ct);

        if (uom is null)
            return Result.Failure(UnitOfMeasureErrors.NotFound(command.UomId));

        if (uom.Products.Count > 0)
            return Result.Failure(UnitOfMeasureErrors.CannotDeleteInUse);

        uom.RaiseDeleted();

        _db.UnitsOfMeasure.Remove(uom);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
