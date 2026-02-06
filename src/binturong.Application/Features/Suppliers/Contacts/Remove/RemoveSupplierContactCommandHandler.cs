using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.Remove;

internal sealed class RemoveSupplierContactCommandHandler
    : ICommandHandler<RemoveSupplierContactCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RemoveSupplierContactCommandHandler(
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

    public async Task<Result> Handle(RemoveSupplierContactCommand command, CancellationToken ct)
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
                "SUPPLIER_CONTACT_REMOVE_FAILED",
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
                "SUPPLIER_CONTACT_REMOVE_FAILED",
                string.Empty,
                $"reason=contact_not_found; supplierId={command.SupplierId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierContactErrors.NotFound(command.ContactId));
        }

        if (contact.IsPrimary)
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierContact",
                contact.Id,
                "SUPPLIER_CONTACT_REMOVE_FAILED",
                $"supplierId={contact.SupplierId}; contactId={contact.Id}; email={contact.Email}; isPrimary={contact.IsPrimary}",
                "reason=cannot_delete_primary",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierContactErrors.CannotDeletePrimary);
        }

        var before =
            $"supplierId={contact.SupplierId}; contactId={contact.Id}; name={contact.Name}; email={contact.Email}; isPrimary={contact.IsPrimary}";

        contact.RaiseDeleted();

        _db.SupplierContacts.Remove(contact);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierContact",
            contact.Id,
            "SUPPLIER_CONTACT_REMOVED",
            before,
            $"contactId={contact.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
