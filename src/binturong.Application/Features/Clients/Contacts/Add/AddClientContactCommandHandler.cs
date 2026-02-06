using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Web;
using Application.Features.Common.Audit;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.Add;

internal sealed class AddClientContactCommandHandler
    : ICommandHandler<AddClientContactCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICommandBus _bus;
    private readonly IRequestContext _request;
    private readonly ICurrentUser _currentUser;

    public AddClientContactCommandHandler(
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

    public async Task<Result<Guid>> Handle(AddClientContactCommand command, CancellationToken ct)
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
                null,
                "CLIENT_CONTACT_ADD_FAILED",
                string.Empty,
                $"reason=client_not_found; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientErrors.NotFound(command.ClientId));
        }

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                null,
                "CLIENT_CONTACT_ADD_FAILED",
                string.Empty,
                $"reason=email_required; clientId={command.ClientId}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientContactErrors.EmailIsRequired);
        }

        var emailExists = await _db.ClientContacts.AnyAsync(
            x => x.ClientId == command.ClientId && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
        {
            await _bus.AuditAsync(
                userId,
                "Clients",
                "ClientContact",
                null,
                "CLIENT_CONTACT_ADD_FAILED",
                string.Empty,
                $"reason=email_not_unique; clientId={command.ClientId}; email={email}",
                ip,
                ua,
                ct
            );

            return Result.Failure<Guid>(ClientContactErrors.EmailNotUniqueForClient);
        }

        var now = DateTime.UtcNow;

        if (command.IsPrimary)
        {
            var primaries = await _db
                .ClientContacts.Where(x => x.ClientId == command.ClientId && x.IsPrimary)
                .ToListAsync(ct);

            foreach (var p in primaries)
            {
                p.IsPrimary = false;
                p.UpdatedAt = now;
                p.RaiseUpdated();
            }
        }

        var contact = new ClientContact
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            Name = command.Name.Trim(),
            JobTitle = string.IsNullOrWhiteSpace(command.JobTitle) ? null : command.JobTitle.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            IsPrimary = command.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now,
        };

        contact.RaiseCreated();

        _db.ClientContacts.Add(contact);
        await _db.SaveChangesAsync(ct);

        await _bus.AuditAsync(
            userId,
            "Clients",
            "ClientContact",
            contact.Id,
            "CLIENT_CONTACT_ADDED",
            string.Empty,
            $"clientId={command.ClientId}; contactId={contact.Id}; email={contact.Email}; isPrimary={contact.IsPrimary}",
            ip,
            ua,
            ct
        );

        return Result.Success(contact.Id);
    }
}
