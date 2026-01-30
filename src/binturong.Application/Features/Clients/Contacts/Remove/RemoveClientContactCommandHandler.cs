using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.ClientContacts;
using Domain.Clients;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Features.Clients.Contacts.Remove;

internal sealed class RemoveClientContactCommandHandler
    : ICommandHandler<RemoveClientContactCommand>
{
    private readonly IApplicationDbContext _db;

    public RemoveClientContactCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveClientContactCommand command, CancellationToken ct)
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

        if (contact.IsPrimary)
            return Result.Failure(ClientContactErrors.CannotDeletePrimary);

        contact.RaiseDeleted();

        _db.ClientContacts.Remove(contact);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
