using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.Remove;

internal sealed class RemoveSupplierContactCommandHandler
    : ICommandHandler<RemoveSupplierContactCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveSupplierContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveSupplierContactCommand command, CancellationToken ct)
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

        if (contact.IsPrimary)
            return Result.Failure(SupplierContactErrors.CannotDeletePrimary);

        contact.RaiseDeleted();

        _db.SupplierContacts.Remove(contact);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
