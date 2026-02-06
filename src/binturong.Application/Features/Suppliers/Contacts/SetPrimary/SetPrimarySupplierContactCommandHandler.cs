using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.SetPrimary;

internal sealed class SetPrimarySupplierContactCommandHandler
    : ICommandHandler<SetPrimarySupplierContactCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public SetPrimarySupplierContactCommandHandler(
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

    public async Task<Result> Handle(SetPrimarySupplierContactCommand command, CancellationToken ct)
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
                "SUPPLIER_CONTACT_SET_PRIMARY_FAILED",
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
                "SUPPLIER_CONTACT_SET_PRIMARY_FAILED",
                string.Empty,
                $"reason=contact_not_found; supplierId={command.SupplierId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(SupplierContactErrors.NotFound(command.ContactId));
        }

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

        var before =
            $"supplierId={contact.SupplierId}; contactId={contact.Id}; isPrimary={contact.IsPrimary}";

        contact.IsPrimary = true;
        contact.UpdatedAt = now;

        contact.RaiseUpdated();
        contact.RaisePrimarySet();

        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierContact",
            contact.Id,
            "SUPPLIER_CONTACT_SET_PRIMARY",
            before,
            $"supplierId={contact.SupplierId}; contactId={contact.Id}; isPrimary={contact.IsPrimary}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
