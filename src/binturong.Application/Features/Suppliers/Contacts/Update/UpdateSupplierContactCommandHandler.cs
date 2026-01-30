using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.Update;

internal sealed class UpdateSupplierContactCommandHandler
    : ICommandHandler<UpdateSupplierContactCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateSupplierContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateSupplierContactCommand command, CancellationToken ct)
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

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(SupplierContactErrors.EmailIsRequired);

        var emailExists = await _db.SupplierContacts.AnyAsync(
            x =>
                x.SupplierId == command.SupplierId
                && x.Id != command.ContactId
                && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure(SupplierContactErrors.EmailNotUniqueForSupplier);

        var now = DateTime.UtcNow;

        if (command.IsPrimary)
        {
            var primaries = await _db
                .SupplierContacts.Where(x =>
                    x.SupplierId == command.SupplierId && x.IsPrimary && x.Id != command.ContactId
                )
                .ToListAsync(ct);

            foreach (var p in primaries)
            {
                p.IsPrimary = false;
                p.UpdatedAt = now;
                p.RaiseUpdated();
            }
        }

        contact.Name = command.Name.Trim();
        contact.JobTitle = string.IsNullOrWhiteSpace(command.JobTitle)
            ? ""
            : command.JobTitle.Trim();
        contact.Email = email;
        contact.Phone = string.IsNullOrWhiteSpace(command.Phone) ? "" : command.Phone.Trim();
        contact.IsPrimary = command.IsPrimary;
        contact.UpdatedAt = now;

        contact.RaiseUpdated();

        if (command.IsPrimary)
            contact.RaisePrimarySet();

        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
