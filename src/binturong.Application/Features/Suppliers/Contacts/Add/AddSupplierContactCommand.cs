using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.SupplierContacts;
using Domain.Suppliers;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Suppliers.Contacts.Add;

internal sealed class AddSupplierContactCommandHandler
    : ICommandHandler<AddSupplierContactCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AddSupplierContactCommandHandler(
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

    public async Task<Result<Guid>> Handle(AddSupplierContactCommand command, CancellationToken ct)
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
                "SUPPLIER_CONTACT_ADD_FAILED",
                string.Empty,
                $"reason=supplier_not_found; supplierId={command.SupplierId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(SupplierErrors.NotFound(command.SupplierId));
        }

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Guid>(SupplierContactErrors.EmailIsRequired);

        var emailExists = await _db.SupplierContacts.AnyAsync(
            x => x.SupplierId == command.SupplierId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
        {
            await _bus.AuditAsync(
                userId,
                "Suppliers",
                "SupplierContact",
                command.SupplierId,
                "SUPPLIER_CONTACT_ADD_FAILED",
                string.Empty,
                $"reason=email_not_unique; supplierId={command.SupplierId}; email={email}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(SupplierContactErrors.EmailNotUniqueForSupplier);
        }

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
            JobTitle = string.IsNullOrWhiteSpace(command.JobTitle) ? "" : command.JobTitle.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(command.Phone) ? "" : command.Phone.Trim(),
            IsPrimary = command.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now,
        };

        contact.RaiseCreated();

        _db.SupplierContacts.Add(contact);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Suppliers",
            "SupplierContact",
            contact.Id,
            "SUPPLIER_CONTACT_ADDED",
            string.Empty,
            $"supplierId={contact.SupplierId}; contactId={contact.Id}; name={contact.Name}; email={contact.Email}; isPrimary={contact.IsPrimary}",
            ip,
            ua,
            ct
        );

        return Result.Success(contact.Id);
    }
}
