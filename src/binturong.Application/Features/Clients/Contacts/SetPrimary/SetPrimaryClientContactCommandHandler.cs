using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.SetPrimary;

internal sealed class SetPrimaryClientContactCommandHandler
    : ICommandHandler<SetPrimaryClientContactCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public SetPrimaryClientContactCommandHandler(
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

    public async Task<Result> Handle(SetPrimaryClientContactCommand command, CancellationToken ct)
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
                "CLIENT_CONTACT_PRIMARY_SET_FAILED",
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
                "CLIENT_CONTACT_PRIMARY_SET_FAILED",
                string.Empty,
                $"reason=not_found; clientId={command.ClientId}; contactId={command.ContactId}",
                ip,
                ua,
                ct
            );

            return Result.Failure(ClientContactErrors.NotFound(command.ContactId));
        }

        var now = DateTime.UtcNow;

        var others = await _db
            .ClientContacts.Where(x =>
                x.ClientId == command.ClientId && x.IsPrimary && x.Id != command.ContactId
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

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientContact",
            contact.Id,
            "CLIENT_CONTACT_PRIMARY_SET",
            string.Empty,
            $"clientId={command.ClientId}; contactId={contact.Id}; isPrimary=true",
            ip,
            ua,
            ct
        );

        return Result.Success();
    }
}
