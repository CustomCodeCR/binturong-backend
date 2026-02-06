using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.Remove;

internal sealed class RemoveClientContactCommandHandler
    : ICommandHandler<RemoveClientContactCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public RemoveClientContactCommandHandler(
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

    public async Task<Result> Handle(RemoveClientContactCommand command, CancellationToken ct)
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
                "CLIENT_CONTACT_REMOVE_FAILED",
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
                "CLIENT_CONTACT_REMOVE_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientContactErrors.NotFound(command.ContactId));
        }

        if (contact.IsPrimary)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                contact.Id,
                "CLIENT_CONTACT_REMOVE_FAILED",
                string.Empty,
                $"reason=cannot_delete_primary; clientId={command.ClientId}; contactId={contact.Id}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientContactErrors.CannotDeletePrimary);
        }

        var before =
            $"name={contact.Name}; email={contact.Email}; phone={contact.Phone}; isPrimary={contact.IsPrimary}";

        contact.RaiseDeleted();

        _db.ClientContacts.Remove(contact);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientContact",
            contact.Id,
            "CLIENT_CONTACT_REMOVED",
            before,
            $"clientId={command.ClientId}; contactId={contact.Id}",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
