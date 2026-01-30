using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.SetPrimary;

internal sealed class SetPrimaryClientContactCommandHandler
    : ICommandHandler<SetPrimaryClientContactCommand>
{
    private readonly IApplicationDbContext _db;

    public SetPrimaryClientContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(SetPrimaryClientContactCommand command, CancellationToken ct)
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

        return Result.Success();
    }
}
