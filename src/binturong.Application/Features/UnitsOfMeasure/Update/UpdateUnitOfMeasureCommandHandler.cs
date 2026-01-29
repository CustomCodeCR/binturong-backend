using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.Update;

internal sealed class UpdateUnitOfMeasureCommandHandler
    : ICommandHandler<UpdateUnitOfMeasureCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateUnitOfMeasureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var uom = await _db.UnitsOfMeasure.FirstOrDefaultAsync(x => x.Id == command.UomId, ct);
        if (uom is null)
            return Result.Failure(UnitOfMeasureErrors.NotFound(command.UomId));

        var code = command.Code.Trim().ToUpperInvariant();
        var name = command.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(UnitOfMeasureErrors.CodeIsRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(UnitOfMeasureErrors.NameIsRequired);

        var codeExists = await _db.UnitsOfMeasure.AnyAsync(
            x => x.Id != command.UomId && x.Code.ToUpper() == code,
            ct
        );

        if (codeExists)
            return Result.Failure(UnitOfMeasureErrors.CodeNotUnique);

        uom.Code = code;
        uom.Name = name;
        uom.IsActive = command.IsActive;
        uom.UpdatedAt = DateTime.UtcNow;

        uom.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
