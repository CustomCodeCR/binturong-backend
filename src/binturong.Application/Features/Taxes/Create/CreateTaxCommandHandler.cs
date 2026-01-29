using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Taxes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Taxes.Create;

internal sealed class CreateTaxCommandHandler : ICommandHandler<CreateTaxCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateTaxCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<Guid>> Handle(CreateTaxCommand command, CancellationToken ct)
    {
        var name = command.Name.Trim();
        var code = command.Code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(TaxErrors.NameIsRequired);

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<Guid>(TaxErrors.CodeIsRequired);

        if (command.Percentage < 0 || command.Percentage > 100)
            return Result.Failure<Guid>(TaxErrors.InvalidPercentage);

        var codeExists = await _db.Taxes.AnyAsync(x => x.Code.ToUpper() == code, ct);
        if (codeExists)
            return Result.Failure<Guid>(TaxErrors.CodeNotUnique);

        var now = DateTime.UtcNow;

        var tax = new Tax
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Percentage = command.Percentage,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        tax.RaiseCreated();

        _db.Taxes.Add(tax);
        await _db.SaveChangesAsync(ct);

        return Result.Success(tax.Id);
    }
}
