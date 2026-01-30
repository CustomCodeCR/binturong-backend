using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.Add;

internal sealed class AddSupplierContactCommandHandler
    : ICommandHandler<AddSupplierContactCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    public AddSupplierContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(AddSupplierContactCommand command, CancellationToken ct)
    {
        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == command.SupplierId, ct);
        if (!supplierExists)
            return Result.Failure<Guid>(SupplierErrors.NotFound(command.SupplierId));

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Guid>(SupplierContactErrors.EmailIsRequired);

        // email unique per supplier
        var emailExists = await _db.SupplierContacts.AnyAsync(
            x => x.SupplierId == command.SupplierId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure<Guid>(SupplierContactErrors.EmailNotUniqueForSupplier);

        var now = DateTime.UtcNow;

        if (command.IsPrimary)
        {
            var primaries = await _db
                .SupplierContacts.Where(x => x.SupplierId == command.SupplierId && x.IsPrimary)
                .ToListAsync(ct);

            foreach (var p in primaries)
            {
                p.IsPrimary = false;
                p.UpdatedAt = now;
                p.RaiseUpdated();
            }
        }

        var contact = new SupplierContact
        {
            Id = Guid.NewGuid(),
            SupplierId = command.SupplierId,
            Name = command.Name.Trim(),
            JobTitle = string.IsNullOrWhiteSpace(command.JobTitle!) ? "" : command.JobTitle.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(command.Phone!) ? "" : command.Phone.Trim(),
            IsPrimary = command.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now,
        };

        contact.RaiseCreated();

        _db.SupplierContacts.Add(contact);
        await _db.SaveChangesAsync(ct);

        return Result.Success(contact.Id);
    }
}
