using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.Update;

internal sealed class UpdateClientContactCommandHandler
    : ICommandHandler<UpdateClientContactCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public UpdateClientContactCommandHandler(
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

    public async Task<Result> Handle(UpdateClientContactCommand command, CancellationToken ct)
    {
        var ip = _request.IpAddress;
        var ua = _request.UserAgent;
        var userId = _currentUser.UserId;

        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                command.ContactId,
                "CLIENT_CONTACT_UPDATE_FAILED",
                string.Empty,
                $"reason=client_not_found; clientId={command.ClientId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientErrors.NotFound(command.ClientId));
        }

        var contact = await _db.ClientContacts.FirstOrDefaultAsync(
            x => x.Id == command.ContactId && x.ClientId == command.ClientId,
            ct
        );

        if (contact is null)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                command.ContactId,
                "CLIENT_CONTACT_UPDATE_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientContactErrors.NotFound(command.ContactId));
        }

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                contact.Id,
                "CLIENT_CONTACT_UPDATE_FAILED",
                string.Empty,
                $"reason=email_required; clientId={command.ClientId}; contactId={contact.Id}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientContactErrors.EmailIsRequired);
        }

        var emailExists = await _db.ClientContacts.AnyAsync(
            x =>
                x.ClientId == command.ClientId
                && x.Id != command.ContactId
                && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                contact.Id,
                "CLIENT_CONTACT_UPDATE_FAILED",
                string.Empty,
                $"reason=email_not_unique; clientId={command.ClientId}; contactId={contact.Id}; email={email}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientContactErrors.EmailNotUniqueForClient);
        }

        var before =
            $"name={contact.Name}; jobTitle={contact.JobTitle}; email={contact.Email}; phone={contact.Phone}; isPrimary={contact.IsPrimary}";

        var now = DateTime.UtcNow;

        if (command.IsPrimary)
        {
            var primaries = await _db
                .ClientContacts.Where(x =>
                    x.ClientId == command.ClientId && x.IsPrimary && x.Id != command.ContactId
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
            ? null
            : command.JobTitle.Trim();
        contact.Email = email;
        contact.Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
        contact.IsPrimary = command.IsPrimary;
        contact.UpdatedAt = now;

        contact.RaiseUpdated();

        if (command.IsPrimary)
            contact.RaisePrimarySet();

        await _db.SaveChangesAsync(ct);

        var after =
            $"name={contact.Name}; jobTitle={contact.JobTitle}; email={contact.Email}; phone={contact.Phone}; isPrimary={contact.IsPrimary}";

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientContact",
            contact.Id,
            "CLIENT_CONTACT_UPDATED",
            before,
            after,
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
