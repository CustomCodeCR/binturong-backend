using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.Update;

internal sealed class UpdateSupplierContactCommandHandler
    : ICommandHandler<UpdateSupplierContactCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateSupplierContactCommandHandler(
        IApplicationDbContext db,
        ICommandBus bus,
        IRequestContext request,
        ICurrentUser currentUser
    )
    {
        _db = db;
        _bus = bus;
        _request = request;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateSupplierContactCommand command, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;

        var supplierExists = await _db.Suppliers.AnyAsync(x => x.Id == command.SupplierId, ct);
        if (!supplierExists)
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierContact",
                command.SupplierId,
                "SUPPLIER_CONTACT_UPDATE_FAILED",
                string.Empty,
                $"reason=supplier_not_found; supplierId={command.SupplierId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierErrors.NotFound(command.SupplierId));
        }

        var contact = await _db.SupplierContacts.FirstOrDefaultAsync(
            x => x.Id == command.ContactId && x.SupplierId == command.SupplierId,
            ct
        );

        if (contact is null)
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierContact",
                command.ContactId,
                "SUPPLIER_CONTACT_UPDATE_FAILED",
                string.Empty,
                $"reason=contact_not_found; supplierId={command.SupplierId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierContactErrors.NotFound(command.ContactId));
        }

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
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierContact",
                contact.Id,
                "SUPPLIER_CONTACT_UPDATE_FAILED",
                $"supplierId={contact.SupplierId}; contactId={contact.Id}; email={contact.Email}",
                $"reason=email_not_unique; attemptedEmail={email}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierContactErrors.EmailNotUniqueForSupplier);
        }

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

        var before =
            $"supplierId={contact.SupplierId}; contactId={contact.Id}; name={contact.Name}; jobTitle={contact.JobTitle}; email={contact.Email}; phone={contact.Phone}; isPrimary={contact.IsPrimary}";

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

        var after =
            $"supplierId={contact.SupplierId}; contactId={contact.Id}; name={contact.Name}; jobTitle={contact.JobTitle}; email={contact.Email}; phone={contact.Phone}; isPrimary={contact.IsPrimary}";

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierContact",
            contact.Id,
            "SUPPLIER_CONTACT_UPDATED",
            before,
            after,
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
