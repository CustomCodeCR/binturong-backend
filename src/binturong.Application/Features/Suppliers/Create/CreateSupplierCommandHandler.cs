using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Create;

internal sealed class CreateSupplierCommandHandler : ICommandHandler<CreateSupplierCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public CreateSupplierCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(CreateSupplierCommand command, CancellationToken ct)
    {
        var identification = command.Identification.Trim();
        var email = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(identification))
            return Result.Failure<Guid>(SupplierErrors.IdentificationIsRequired);

        if (string.IsNullOrWhiteSpace(command.LegalName))
            return Result.Failure<Guid>(SupplierErrors.LegalNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return Result.Failure<Guid>(SupplierErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Guid>(SupplierErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.Phone))
            return Result.Failure<Guid>(SupplierErrors.PhoneIsRequired);

        // Uniqueness checks
        var emailExists = await _db.Suppliers.AnyAsync(x => x.Email.ToLower() == email, ct);
        if (emailExists)
            return Result.Failure<Guid>(SupplierErrors.EmailNotUnique);

        var identificationExists = await _db.Suppliers.AnyAsync(
            x => x.Identification == identification,
            ct
        );
        if (identificationExists)
            return Result.Failure<Guid>(SupplierErrors.IdentificationNotUnique);

        var now = DateTime.UtcNow;

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            IdentificationType = command.IdentificationType.Trim(),
            Identification = identification,
            LegalName = command.LegalName.Trim(),
            TradeName = command.TradeName.Trim(),
            Email = email,
            Phone = command.Phone.Trim(),
            PaymentTerms = command.PaymentTerms?.Trim() ?? string.Empty,
            MainCurrency = command.MainCurrency?.Trim() ?? string.Empty,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        supplier.RaiseCreated();

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);

        return Result.Success(supplier.Id);
    }
}
