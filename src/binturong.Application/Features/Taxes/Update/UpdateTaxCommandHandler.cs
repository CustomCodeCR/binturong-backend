using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Taxes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Taxes.Update;

internal sealed class UpdateTaxCommandHandler : ICommandHandler<UpdateTaxCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateTaxCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateTaxCommand command, CancellationToken ct)
    {
        var tax = await _db.Taxes.FirstOrDefaultAsync(x => x.Id == command.TaxId, ct);
        if (tax is null)
            return Result.Failure(TaxErrors.NotFound(command.TaxId));

        var name = command.Name.Trim();
        var code = command.Code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(TaxErrors.NameIsRequired);

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(TaxErrors.CodeIsRequired);

        if (command.Percentage < 0 || command.Percentage > 100)
            return Result.Failure(TaxErrors.InvalidPercentage);

        // Unique code (excluding self)
        var codeExists = await _db.Taxes.AnyAsync(
            x => x.Id != command.TaxId && x.Code.ToUpper() == code,
            ct
        );

        if (codeExists)
            return Result.Failure(TaxErrors.CodeNotUnique);

        tax.Name = name;
        tax.Code = code;
        tax.Percentage = command.Percentage;
        tax.IsActive = command.IsActive;
        tax.UpdatedAt = DateTime.UtcNow;

        tax.RaiseUpdated();

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
