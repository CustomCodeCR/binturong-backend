using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.UnitsOfMeasure.Create;

internal sealed class CreateUnitOfMeasureCommandHandler
    : ICommandHandler<CreateUnitOfMeasureCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateUnitOfMeasureCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var code = command.Code.Trim().ToUpperInvariant();
        var name = command.Name.Trim();

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(UnitOfMeasureErrors.CodeIsRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(UnitOfMeasureErrors.NameIsRequired);

        var codeExists = await _db.UnitsOfMeasure.AnyAsync(x => x.Code.ToUpper() == code, ct);
        if (codeExists)
            return Result.Failure<Guid>(UnitOfMeasureErrors.CodeNotUnique);

        var now = DateTime.UtcNow;

        var uom = new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        uom.RaiseCreated();

        _db.UnitsOfMeasure.Add(uom);
        await _db.SaveChangesAsync(ct);

        return Result.Success(uom.Id);
    }
}
