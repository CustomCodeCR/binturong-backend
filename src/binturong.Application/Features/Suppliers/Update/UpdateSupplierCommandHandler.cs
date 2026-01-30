using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Update;

internal sealed class UpdateSupplierCommandHandler : ICommandHandler<UpdateSupplierCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateSupplierCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateSupplierCommand command, CancellationToken ct)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.SupplierId, ct);
        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));

        var email = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(command.LegalName))
            return Result.Failure(SupplierErrors.LegalNameIsRequired);

        if (string.IsNullOrWhiteSpace(command.TradeName))
            return Result.Failure(SupplierErrors.TradeNameIsRequired);

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(SupplierErrors.EmailIsRequired);

        if (string.IsNullOrWhiteSpace(command.Phone))
            return Result.Failure(SupplierErrors.PhoneIsRequired);

        // Email unique excluding self
        var emailExists = await _db.Suppliers.AnyAsync(
            x => x.Id != command.SupplierId && x.Email.ToLower() == email,
            ct
        );

        if (emailExists)
            return Result.Failure(SupplierErrors.EmailNotUnique);

        supplier.UpdateBasicInfo(
            legalName: command.LegalName.Trim(),
            tradeName: command.TradeName.Trim(),
            email: email,
            phone: command.Phone.Trim(),
            paymentTerms: string.IsNullOrWhiteSpace(command.PaymentTerms)
                ? null
                : command.PaymentTerms.Trim(),
            mainCurrency: string.IsNullOrWhiteSpace(command.MainCurrency)
                ? null
                : command.MainCurrency.Trim(),
            isActive: command.IsActive
        );

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
