using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.Update;

internal sealed class UpdateClientContactCommandHandler
    : ICommandHandler<UpdateClientContactCommand>
{
    private readonly IApplicationDbContext _db;

    public UpdateClientContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateClientContactCommand command, CancellationToken ct)
    {
        var clientExists = await _db.Clients.AnyAsync(x => x.Id == command.ClientId, ct);
        if (!clientExists)
            return Result.Failure(ClientErrors.NotFound(command.ClientId));

        var contact = await _db.ClientContacts.FirstOrDefaultAsync(
            x => x.Id == command.ContactId && x.ClientId == command.ClientId,
            ct
        );

        if (contact is null)
            return Result.Failure(ClientContactErrors.NotFound(command.ContactId));

        var email = command.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(ClientContactErrors.EmailIsRequired);

        var emailExists = await _db.ClientContacts.AnyAsync(
            x =>
                x.ClientId == command.ClientId
                && x.Id != command.ContactId
                && x.Email.ToLower() == email,
            ct
        );
        if (emailExists)
            return Result.Failure(ClientContactErrors.EmailNotUniqueForClient);

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

        return Result.Success();
    }
}
