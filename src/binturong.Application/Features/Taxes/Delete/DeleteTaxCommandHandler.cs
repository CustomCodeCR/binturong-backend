using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Taxes;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Taxes.Delete;

internal sealed class DeleteTaxCommandHandler : ICommandHandler<DeleteTaxCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteTaxCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteTaxCommand command, CancellationToken ct)
    {
        var tax = await _db
            .Taxes.Include(x => x.Products) // para validar "in use"
            .FirstOrDefaultAsync(x => x.Id == command.TaxId, ct);

        if (tax is null)
            return Result.Failure(TaxErrors.NotFound(command.TaxId));

        // Si quieres bloquear borrado cuando está en uso:
        if (tax.Products.Count > 0)
            return Result.Failure(TaxErrors.CannotDeleteInUse);

        tax.RaiseDeleted();

        _db.Taxes.Remove(tax);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
