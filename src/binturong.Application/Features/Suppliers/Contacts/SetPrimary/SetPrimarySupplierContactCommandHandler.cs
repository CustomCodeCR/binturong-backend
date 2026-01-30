using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.SetPrimary;

internal sealed class SetPrimarySupplierContactCommandHandler
    : ICommandHandler<SetPrimarySupplierContactCommand>
{
    private readonly IApplicationDbContext _db;

    public SetPrimarySupplierContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetPrimarySupplierContactCommand command, CancellationToken ct)
    {
        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == command.SupplierId, ct);
        if (!supplierExists)
            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));

        var contact = await _db.SupplierContacts.FirstOrDefaultAsync(
            x => x.Id == command.ContactId && x.SupplierId == command.SupplierId,
            ct
        );

        if (contact is null)
            return Result.Failure(SupplierContactErrors.NotFound(command.ContactId));

        var now = DateTime.UtcNow;

        var others = await _db
            .SupplierContacts.Where(x =>
                x.SupplierId == command.SupplierId && x.IsPrimary && x.Id != command.ContactId
            )
            .ToListAsync(ct);

        foreach (var o in others)
        {
            o.IsPrimary = false;
            o.UpdatedAt = now;
            o.RaiseUpdated();
        }

        contact.IsPrimary = true;
        contact.UpdatedAt = now;

        contact.RaiseUpdated();
        contact.RaisePrimarySet();

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
