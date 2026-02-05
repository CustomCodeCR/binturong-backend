using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.SetCreditConditions;

internal sealed class SetSupplierCreditConditionsCommandHandler
    : ICommandHandler<SetSupplierCreditConditionsCommand>
{
    private readonly IApplicationDbContext _db;

    public SetSupplierCreditConditionsCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(
        SetSupplierCreditConditionsCommand command,
        CancellationToken ct
    )
    {
        if (!command.HasPermission)
            return Result.Failure(SupplierCreditErrors.Unauthorized);

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.SupplierId, ct);

        if (supplier is null)
            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));

        // Example business rule
        if (command.CreditLimit > 50_000_000)
            return Result.Failure(SupplierCreditErrors.CreditExceeded);

        supplier.SetCreditConditions(command.CreditLimit, command.CreditDays);

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
